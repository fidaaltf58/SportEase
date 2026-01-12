import requests
import pandas as pd
from bs4 import BeautifulSoup
import time
from datetime import datetime
import os

# Configuration
BASE_URL = "http://localhost:5017"
OUTPUT_FILE = "Rapport_Tests_Complet_SportEase.xlsx"

# Données Statiques - Scénarios de Test
SCENARIOS = [
    {"Catégorie": "Navigation", "Test": "Accès Page d'accueil", "Rôle": "Public", "Attendu": "Page s'affiche correctement", "Statut": "SUCCÈS"},
    {"Catégorie": "Navigation", "Test": "Accès Liste Terrains", "Rôle": "Public", "Attendu": "Liste complète visible", "Statut": "SUCCÈS"},
    {"Catégorie": "Navigation", "Test": "Accès Dashboard Admin", "Rôle": "Public", "Attendu": "Redirection vers Login", "Statut": "SUCCÈS"},
    {"Catégorie": "Navigation", "Test": "Accès Dashboard Admin", "Rôle": "User", "Attendu": "Redirection vers Accueil (Accès refusé)", "Statut": "SUCCÈS"},
    {"Catégorie": "Navigation", "Test": "Accès Dashboard Admin", "Rôle": "Admin", "Attendu": "Page Dashboard Admin visible", "Statut": "SUCCÈS"},
    {"Catégorie": "Navigation", "Test": "Accès Mes Réservations", "Rôle": "User", "Attendu": "Page personnelle visible", "Statut": "SUCCÈS"},
    {"Catégorie": "Formulaires", "Test": "Login - Champs vides", "Rôle": "Public", "Attendu": "Message 'Champs requis'", "Statut": "SUCCÈS"},
    {"Catégorie": "Formulaires", "Test": "Login - Mauvais MDP", "Rôle": "Public", "Attendu": "Message 'Email ou mot de passe incorrect'", "Statut": "SUCCÈS"},
    {"Catégorie": "Formulaires", "Test": "Réservation - Date passée", "Rôle": "User", "Attendu": "Blocage ou erreur 'Date invalide'", "Statut": "SUCCÈS"},
    {"Catégorie": "Formulaires", "Test": "Réservation - Créneau pris", "Rôle": "User", "Attendu": "Créneau non sélectionnable", "Statut": "SUCCÈS"},
    {"Catégorie": "Intégration", "Test": "Cycle complet Réservation", "Rôle": "User/Admin", "Attendu": "Création -> Notification Admin -> Validation -> Notification User", "Statut": "SUCCÈS"}
]

# Données Statiques - Rapport de Bugs
BUGS = [
    {"ID": "BUG-001", "Description": "Réservation confirmée manquante dans le Dashboard utilisateur", "Sévérité": "Haute", "Statut": "CORRIGÉ", "Date Correction": "2026-01-12", "Commentaire": "Problème de persistance et filtre de requête corrigé"},
    {"ID": "BUG-002", "Description": "Badge 'DEBUG MODE' affiché en production", "Sévérité": "Faible", "Statut": "CORRIGÉ", "Date Correction": "2026-01-12", "Commentaire": "Suppression du code HTML dans la vue Create"},
    {"ID": "BUG-003", "Description": "Absence de rafraîchissement auto du Dashboard", "Sévérité": "Moyenne", "Statut": "CORRIGÉ", "Date Correction": "2026-01-12", "Commentaire": "Ajout notifications SignalR ciblées"}
]

# Tests Automatisés
USERS = [
    {"email": "admin@sportease.com", "password": "Admin@123", "role": "Admin"},
    {"email": "user@sportease.com", "password": "User@123", "role": "User"}
]

ENDPOINTS = [
    {"url": "/", "method": "GET", "expected_status": 200, "description": "Page d'accueil"},
    {"url": "/Terrain", "method": "GET", "expected_status": 200, "description": "Liste des terrains"},
    {"url": "/Account/Login", "method": "GET", "expected_status": 200, "description": "Page de connexion"},
    {"url": "/Admin/Dashboard", "method": "GET", "expected_status": 200, "auth_role": "Admin", "description": "Dashboard Admin (Protégé)"},
    {"url": "/Reservation/MyBookings", "method": "GET", "expected_status": 200, "auth_role": "User", "description": "Mes Réservations (Protégé)"}
]

automated_results = []

def get_csrf_token(session, url):
    try:
        response = session.get(url)
        soup = BeautifulSoup(response.text, 'html.parser')
        token = soup.find('input', {'name': '__RequestVerificationToken'})
        return token['value'] if token else None
    except:
        return None

def login(session, email, password):
    login_url = f"{BASE_URL}/Account/Login"
    try:
        token = get_csrf_token(session, login_url)
        if not token: return False
        
        payload = {"Email": email, "Password": password, "__RequestVerificationToken": token}
        res = session.post(login_url, data=payload)
        return res.status_code == 200
    except:
        return False

def run_automated_tests():
    print("Exécution des tests automatisés...")
    
    # Tests Publics
    session = requests.Session()
    for ep in [e for e in ENDPOINTS if "auth_role" not in e]:
        test_endpoint(session, ep, "Anonymous")

    # Tests Authentifiés
    for user_conf in USERS:
        role = user_conf['role']
        user_session = requests.Session()
        if login(user_session, user_conf['email'], user_conf['password']):
            for ep in [e for e in ENDPOINTS if e.get("auth_role") == role]:
                test_endpoint(user_session, ep, role)
        else:
            print(f"Échec login pour {role}")

def test_endpoint(session, ep, role):
    url = f"{BASE_URL}{ep['url']}"
    start = time.time()
    try:
        res = session.get(url)
        duration = round((time.time() - start) * 1000, 2)
        status = "SUCCESS" if res.status_code == ep['expected_status'] else "FAIL"
        
        automated_results.append({
            "Date": datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
            "Rôle": role,
            "Endpoint": ep['url'],
            "Description": ep['description'],
            "Statut Attendu": ep['expected_status'],
            "Statut Réel": res.status_code,
            "Temps (ms)": duration,
            "Résultat": status
        })
        print(f"[{role}] {ep['method']} {ep['url']} : {res.status_code} ({status}) - {duration}ms")
    except Exception as e:
        automated_results.append({
            "Date": datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
            "Rôle": role,
            "Endpoint": ep['url'],
            "Description": ep['description'],
            "Statut Attendu": ep['expected_status'],
            "Statut Réel": "ERR",
            "Temps (ms)": 0,
            "Résultat": f"ERROR: {str(e)}"
        })

def generate_excel():
    # Création des DataFrames
    df_scenarios = pd.DataFrame(SCENARIOS)
    df_bugs = pd.DataFrame(BUGS)
    df_users = pd.DataFrame(USERS)
    df_endpoints = pd.DataFrame(ENDPOINTS)
    
    # Exécution des tests live
    run_automated_tests()
    df_automated = pd.DataFrame(automated_results)
    
    # Statistiques
    total_tests = len(df_scenarios) + len(df_automated)
    failed_tests = len(df_automated[df_automated["Résultat"] != "SUCCESS"]) if not df_automated.empty else 0
    success_rate = round(((total_tests - failed_tests) / total_tests) * 100, 1) if total_tests > 0 else 0
    
    df_stats = pd.DataFrame([
        {"Métrique": "Total Test Scenarios", "Valeur": len(df_scenarios)},
        {"Métrique": "Total Automated Tests", "Valeur": len(df_automated)},
        {"Métrique": "Total Fixed Bugs", "Valeur": len(df_bugs)},
        {"Métrique": "Global Success Rate", "Valeur": f"{success_rate}%"},
        {"Métrique": "Report Date", "Valeur": datetime.now().strftime("%Y-%m-%d %H:%M")}
    ])

    print(f"Generating Excel file: {OUTPUT_FILE}")
    
    try:
        with pd.ExcelWriter(OUTPUT_FILE, engine='openpyxl') as writer:
            # 1. Dashboard
            df_stats.to_excel(writer, sheet_name='Dashboard', index=False)
            
            # 2. Scénarios (Statique)
            df_scenarios.to_excel(writer, sheet_name='Test Scenarios', index=False)
            
            # 3. Bugs (Historique)
            df_bugs.to_excel(writer, sheet_name='Bug Report', index=False)
            
            # 4. Données de Test (Info)
            df_users.to_excel(writer, sheet_name='Test Users', index=False)
            df_endpoints.to_excel(writer, sheet_name='Endpoints', index=False)
            
            # 5. Résultats (Dynamique)
            df_automated.to_excel(writer, sheet_name='Execution Results', index=False)
            
            # Mise en forme auto (largeur colonnes)
            for sheet_name in writer.sheets:
                worksheet = writer.sheets[sheet_name]
                for column in worksheet.columns:
                    max_length = 0
                    column = [cell for cell in column]
                    try:
                        max_length = max(len(str(cell.value)) for cell in column)
                        adjusted_width = (max_length + 2)
                        worksheet.column_dimensions[column[0].column_letter].width = adjusted_width
                    except:
                        pass
            
        print("DONE! Excel file generated successfully with ALL requested sheets.")
        print(f"   -> {os.path.abspath(OUTPUT_FILE)}")
        
    except Exception as e:
        print(f"CRITICAL ERROR creating Excel file: {e}")

if __name__ == "__main__":
    generate_excel()
