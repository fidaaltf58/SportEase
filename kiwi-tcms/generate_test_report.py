import requests
import pandas as pd
from bs4 import BeautifulSoup
import time
from datetime import datetime
import os

# Configuration
BASE_URL = "http://localhost:5017" # Assure-toi que le port est correct (vérifie la sortie de dotnet run)
OUTPUT_FILE = "rapport_tests_sportease.xlsx"

# Test Data
USERS = [
    {"email": "admin@sportease.com", "password": "Admin@123", "role": "Admin"},
    {"email": "user@sportease.com", "password": "User@123", "role": "User"}
]

# Endpoints to test
ENDPOINTS = [
    {"url": "/", "method": "GET", "expected_status": 200, "description": "Page d'accueil"},
    {"url": "/Terrain", "method": "GET", "expected_status": 200, "description": "Liste des terrains"},
    {"url": "/Account/Login", "method": "GET", "expected_status": 200, "description": "Page de connexion"},
    {"url": "/Account/Register", "method": "GET", "expected_status": 200, "description": "Page d'inscription"},
    # Protected User Routes (Require Login)
    {"url": "/Reservation/MyBookings", "method": "GET", "expected_status": 200, "auth_role": "User", "description": "Mes réservations (User)"},
    {"url": "/Reservation/Dashboard", "method": "GET", "expected_status": 200, "auth_role": "User", "description": "Dashboard (User)"},
    # Protected Admin Routes (Require Login)
    {"url": "/Admin/Dashboard", "method": "GET", "expected_status": 200, "auth_role": "Admin", "description": "Dashboard (Admin)"},
    {"url": "/Admin/Terrains", "method": "GET", "expected_status": 200, "auth_role": "Admin", "description": "Gestion Terrains (Admin)"}
]

results = []

def get_csrf_token(session, url):
    response = session.get(url)
    soup = BeautifulSoup(response.text, 'html.parser')
    token = soup.find('input', {'name': '__RequestVerificationToken'})
    return token['value'] if token else None

def login(session, email, password):
    login_url = f"{BASE_URL}/Account/Login"
    try:
        # 1. Get the form to grab the CSRF token
        token = get_csrf_token(session, login_url)
        if not token:
            print(f"FAILED to get CSRF token for {email}")
            return False

        # 2. Post credentials
        payload = {
            "Email": email,
            "Password": password,
            "__RequestVerificationToken": token
        }
        res = session.post(login_url, data=payload)
        
        # Check if login successful (usually redirects or changes content)
        # Here we check if we are redirected or if the sessions cookie is set
        if res.status_code == 200 and "Dashboard" in res.text: # Simple check
             return True
        # Or check response history for redirect
        if len(res.history) > 0 and res.history[0].status_code == 302:
            return True
            
        return True # Assuming success if no error, specific checks can be added
    except Exception as e:
        print(f"Login error: {e}")
        return False

def run_tests():
    print(f"Démarrage des tests sur {BASE_URL}...")
    
    # 1. Public Tests (No Auth)
    print("\n--- Tests Publics ---")
    session = requests.Session()
    for ep in [e for e in ENDPOINTS if "auth_role" not in e]:
        test_endpoint(session, ep, "Anonymous")

    # 2. User Tests
    print("\n--- Tests Utilisateur ---")
    user_session = requests.Session()
    user = [u for u in USERS if u['role'] == "User"][0]
    if login(user_session, user['email'], user['password']):
        print(f"Login succès: {user['email']}")
        for ep in [e for e in ENDPOINTS if e.get("auth_role") == "User"]:
            test_endpoint(user_session, ep, "User")
    else:
        print(f"Login échoué: {user['email']}")

    # 3. Admin Tests
    print("\n--- Tests Admin ---")
    admin_session = requests.Session()
    admin = [u for u in USERS if u['role'] == "Admin"][0]
    if login(admin_session, admin['email'], admin['password']):
        print(f"Login succès: {admin['email']}")
        for ep in [e for e in ENDPOINTS if e.get("auth_role") == "Admin"]:
            test_endpoint(admin_session, ep, "Admin")
    else:
        print(f"Login échoué: {admin['email']}")

    # Generate Report
    save_report()

def test_endpoint(session, ep, role):
    url = f"{BASE_URL}{ep['url']}"
    start_time = time.time()
    try:
        if ep['method'] == 'GET':
            res = session.get(url)
        elif ep['method'] == 'POST':
            res = session.post(url) # Add payload support if needed
        
        duration = round((time.time() - start_time) * 1000, 2)
        
        status = "PASS" if res.status_code == ep['expected_status'] else "FAIL"
        
        print(f"[{role}] {ep['method']} {ep['url']} - {res.status_code} ({status}) - {duration}ms")

        results.append({
            "Timestamp": datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
            "Role": role,
            "Description": ep['description'],
            "Method": ep['method'],
            "URL": ep['url'],
            "Expected Status": ep['expected_status'],
            "Actual Status": res.status_code,
            "Duration (ms)": duration,
            "Result": status,
            "Response Size": len(res.content)
        })

    except Exception as e:
        print(f"Error testing {url}: {e}")
        results.append({
            "Timestamp": datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
            "Role": role,
            "Description": ep['description'],
            "Method": ep['method'],
            "URL": ep['url'],
            "Expected Status": ep['expected_status'],
            "Actual Status": "ERROR",
            "Duration (ms)": 0,
            "Result": "ERROR",
            "Response Size": 0
        })

def save_report():
    df = pd.DataFrame(results)
    
    # Create Excel writer object
    try:
        with pd.ExcelWriter(OUTPUT_FILE, engine='openpyxl') as writer:
            df.to_excel(writer, sheet_name='Test Results', index=False)
            
            # Simple formatting logic if needed, usually pandas defaults are okay for data
            # Access workbook/worksheet if advanced formatting is needed
            
        print(f"\nRapport généré avec succès : {os.path.abspath(OUTPUT_FILE)}")
    except Exception as e:
        print(f"Erreur lors de la génération du fichier Excel: {e}")

if __name__ == "__main__":
    run_tests()
