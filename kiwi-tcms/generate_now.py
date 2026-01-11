#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
SCRIPT ULTIME - Génération rapport Excel SportEase
Exécuter: python generate_now.py
"""

import os
import sys
import traceback
from datetime import datetime

print("=" * 70)
print("🚀 LANCEMENT DU GÉNÉRATEUR DE RAPPORT SPORTEASE")
print("=" * 70)
print(f"Date/heure: {datetime.now().strftime('%d/%m/%Y %H:%M:%S')}")
print(f"Dossier: {os.getcwd()}")
print()

# Vérifier Python
print("✅ Vérification de l'environnement Python...")
print(f"   Version Python: {sys.version}")
print()

# Vérifier et importer openpyxl
try:
    print("📦 Importation des modules...")
    from openpyxl import Workbook
    from openpyxl.styles import Font, PatternFill, Alignment, Border, Side
    print("   ✅ openpyxl importé avec succès")
except ImportError as e:
    print(f"   ❌ ERREUR: {e}")
    print("   💡 Solution: pip install openpyxl")
    input("\nAppuyez sur Entrée pour quitter...")
    sys.exit(1)

# Données COMPLÈTES SportEase
print("\n📊 Préparation des données de test...")
test_results = [
    # Authentification
    {'id': 'TC-001', 'name': 'Inscription Utilisateur', 'component': 'Authentification', 'priority': 'HIGH', 'status': 'PASSED', 'tester': 'Ahmed', 'date': '2025-01-11', 'time': '5 min', 'comments': 'Test réussi'},
    {'id': 'TC-002', 'name': 'Inscription Email Existant', 'component': 'Authentification', 'priority': 'HIGH', 'status': 'FAILED', 'tester': 'Ahmed', 'date': '2025-01-11', 'time': '3 min', 'comments': 'BUG-002'},
    {'id': 'TC-003', 'name': 'Connexion Réussie', 'component': 'Authentification', 'priority': 'HIGH', 'status': 'PASSED', 'tester': 'Mohamed', 'date': '2025-01-11', 'time': '2 min', 'comments': ''},
    
    # Gestion Terrains
    {'id': 'TC-006', 'name': 'Création Terrain', 'component': 'Gestion Terrains', 'priority': 'HIGH', 'status': 'PASSED', 'tester': 'Ahmed', 'date': '2025-01-11', 'time': '8 min', 'comments': 'Upload OK'},
    {'id': 'TC-007', 'name': 'Modification Terrain', 'component': 'Gestion Terrains', 'priority': 'HIGH', 'status': 'PASSED', 'tester': 'Ahmed', 'date': '2025-01-11', 'time': '5 min', 'comments': ''},
    
    # Réservations
    {'id': 'TC-020', 'name': 'Création Réservation', 'component': 'Réservations', 'priority': 'CRITICAL', 'status': 'FAILED', 'tester': 'Mohamed', 'date': '2025-01-12', 'time': '10 min', 'comments': 'BUG-001: Prix incorrect'},
    {'id': 'TC-021', 'name': 'Conflit Créneau', 'component': 'Réservations', 'priority': 'HIGH', 'status': 'FAILED', 'tester': 'Mohamed', 'date': '2025-01-12', 'time': '15 min', 'comments': 'BUG-003'},
    {'id': 'TC-022', 'name': 'Limite 3 Réservations', 'component': 'Réservations', 'priority': 'HIGH', 'status': 'PASSED', 'tester': 'Mohamed', 'date': '2025-01-12', 'time': '5 min', 'comments': ''},
    
    # Sécurité
    {'id': 'TC-036', 'name': 'SQL Injection', 'component': 'Sécurité', 'priority': 'CRITICAL', 'status': 'PASSED', 'tester': 'Ahmed', 'date': '2025-01-13', 'time': '15 min', 'comments': 'Protection efficace'},
    {'id': 'TC-038', 'name': 'CSRF Token', 'component': 'Sécurité', 'priority': 'HIGH', 'status': 'PASSED', 'tester': 'Ahmed', 'date': '2025-01-13', 'time': '5 min', 'comments': ''},
]

print(f"   ✅ {len(test_results)} tests chargés")

# Bugs
bugs = [
    {'id': 'BUG-001', 'severity': 'Critique', 'test': 'TC-020', 'component': 'Réservations', 'desc': 'Calcul prix incorrect', 'status': 'Ouvert', 'assigned': 'Dev Team', 'date': '2025-01-12'},
    {'id': 'BUG-002', 'severity': 'Majeur', 'test': 'TC-002', 'component': 'Authentification', 'desc': 'Message erreur générique', 'status': 'Ouvert', 'assigned': 'Dev Team', 'date': '2025-01-11'},
    {'id': 'BUG-003', 'severity': 'Majeur', 'test': 'TC-021', 'component': 'Réservations', 'desc': 'Conflit créneau non détecté', 'status': 'Ouvert', 'assigned': 'Dev Team', 'date': '2025-01-12'},
]

print(f"   ✅ {len(bugs)} bugs identifiés")

def create_excel_report():
    """Crée le rapport Excel complet"""
    print("\n" + "=" * 70)
    print("📈 CRÉATION DU RAPPORT EXCEL")
    print("=" * 70)
    
    filename = "SportEase_Test_Report_COMPLET.xlsx"
    print(f"Nom du fichier: {filename}")
    
    try:
        # 1. Créer le workbook
        print("1. Création du workbook...")
        wb = Workbook()
        
        # Supprimer feuille par défaut
        if "Sheet" in wb.sheetnames:
            std_sheet = wb["Sheet"]
            wb.remove(std_sheet)
        
        # 2. FEUILLE RÉSUMÉ
        print("2. Création feuille 'Résumé'...")
        ws_summary = wb.create_sheet("Résumé", 0)
        
        # Titre principal
        title_cell = ws_summary['A1']
        title_cell.value = "RAPPORT DE TESTS - SPORTEASE"
        title_cell.font = Font(size=18, bold=True, color="FFFFFF", name="Arial")
        title_cell.fill = PatternFill(start_color="366092", end_color="366092", fill_type="solid")
        title_cell.alignment = Alignment(horizontal="center", vertical="center")
        ws_summary.merge_cells('A1:H1')
        ws_summary.row_dimensions[1].height = 35
        
        # Informations projet
        info_labels = [
            ("A3", "Projet:"),
            ("A4", "Version:"),
            ("A5", "Date du rapport:"),
            ("A6", "Testeurs:"),
            ("A7", "Environnement:"),
        ]
        
        info_values = [
            ("B3", "SportEase"),
            ("B4", "v1.0-Sprint6"),
            ("B5", datetime.now().strftime("%d/%m/%Y %H:%M")),
            ("B6", "Ahmed Ben Salem, Mohamed Ali"),
            ("B7", "Développement"),
        ]
        
        for cell, value in info_labels:
            ws_summary[cell] = value
            ws_summary[cell].font = Font(bold=True)
            ws_summary[cell].fill = PatternFill(start_color="E7E6E6", end_color="E7E6E6", fill_type="solid")
        
        for cell, value in info_values:
            ws_summary[cell] = value
        
        # Statistiques
        total_tests = len(test_results)
        passed_tests = sum(1 for t in test_results if t['status'] == 'PASSED')
        failed_tests = sum(1 for t in test_results if t['status'] == 'FAILED')
        success_rate = (passed_tests / total_tests * 100) if total_tests > 0 else 0
        
        ws_summary['A9'] = "STATISTIQUES GLOBALES"
        ws_summary['A9'].font = Font(size=14, bold=True, color="FFFFFF")
        ws_summary['A9'].fill = PatternFill(start_color="70AD47", end_color="70AD47", fill_type="solid")
        ws_summary.merge_cells('A9:B9')
        
        stats_data = [
            ["Total tests", total_tests],
            ["Tests réussis", passed_tests],
            ["Tests échoués", failed_tests],
            ["Taux réussite", f"{success_rate:.1f}%"],
            ["Bugs identifiés", len(bugs)],
        ]
        
        for i, (label, value) in enumerate(stats_data, start=10):
            ws_summary[f'A{i}'] = label
            ws_summary[f'B{i}'] = value
            
            # Style
            border = Border(left=Side(style='thin'), right=Side(style='thin'),
                          top=Side(style='thin'), bottom=Side(style='thin'))
            ws_summary[f'A{i}'].border = border
            ws_summary[f'B{i}'].border = border
            
            if i == 10:  # En-tête
                ws_summary[f'A{i}'].font = Font(bold=True)
                ws_summary[f'B{i}'].font = Font(bold=True)
                ws_summary[f'A{i}'].fill = PatternFill(start_color="D9E1F2", end_color="D9E1F2", fill_type="solid")
                ws_summary[f'B{i}'].fill = PatternFill(start_color="D9E1F2", end_color="D9E1F2", fill_type="solid")
        
        # 3. FEUILLE RÉSULTATS DÉTAILLÉS
        print("3. Création feuille 'Résultats Détaillés'...")
        ws_details = wb.create_sheet("Résultats")
        
        # En-tête
        headers = ["ID Test", "Nom du Test", "Composant", "Priorité", "Statut", 
                  "Testeur", "Date", "Durée", "Commentaires"]
        
        for col, header in enumerate(headers, 1):
            cell = ws_details.cell(row=1, column=col, value=header)
            cell.font = Font(bold=True, color="FFFFFF", size=11)
            cell.fill = PatternFill(start_color="4472C4", end_color="4472C4", fill_type="solid")
            cell.alignment = Alignment(horizontal="center", vertical="center")
            cell.border = Border(left=Side(style='thin'), right=Side(style='thin'),
                               top=Side(style='thin'), bottom=Side(style='thin'))
        
        # Données
        for row_idx, test in enumerate(test_results, 2):
            data = [
                test['id'],
                test['name'],
                test['component'],
                test['priority'],
                test['status'],
                test['tester'],
                test['date'],
                test['time'],
                test['comments']
            ]
            
            for col_idx, value in enumerate(data, 1):
                cell = ws_details.cell(row=row_idx, column=col_idx, value=value)
                cell.border = Border(left=Side(style='thin'), right=Side(style='thin'),
                                   top=Side(style='thin'), bottom=Side(style='thin'))
                
                # Couleur selon statut
                if col_idx == 5:  # Colonne Statut
                    if value == 'PASSED':
                        cell.fill = PatternFill(start_color="C6EFCE", end_color="C6EFCE", fill_type="solid")
                        cell.font = Font(color="006100", bold=True)
                    elif value == 'FAILED':
                        cell.fill = PatternFill(start_color="FFC7CE", end_color="FFC7CE", fill_type="solid")
                        cell.font = Font(color="9C0006", bold=True)
                
                # Couleur selon priorité
                elif col_idx == 4:  # Colonne Priorité
                    if value == 'CRITICAL':
                        cell.font = Font(color="FF0000", bold=True)
                    elif value == 'HIGH':
                        cell.font = Font(color="FF6B00", bold=True)
                    elif value == 'MEDIUM':
                        cell.font = Font(color="0070C0")
                    elif value == 'LOW':
                        cell.font = Font(color="808080")
        
        # Ajuster largeurs colonnes
        column_widths = [12, 40, 20, 12, 12, 15, 12, 10, 40]
        for i, width in enumerate(column_widths, 1):
            col_letter = chr(64 + i) if i <= 26 else chr(64 + i//26) + chr(64 + i%26)
            ws_details.column_dimensions[col_letter].width = width
        
        # Figer l'en-tête
        ws_details.freeze_panes = 'A2'
        
        # 4. FEUILLE BUGS
        print("4. Création feuille 'Bugs'...")
        ws_bugs = wb.create_sheet("Bugs")
        
        # Titre
        ws_bugs['A1'] = "BUGS IDENTIFIÉS"
        ws_bugs['A1'].font = Font(size=16, bold=True, color="FFFFFF")
        ws_bugs['A1'].fill = PatternFill(start_color="C00000", end_color="C00000", fill_type="solid")
        ws_bugs.merge_cells('A1:H1')
        ws_bugs['A1'].alignment = Alignment(horizontal="center", vertical="center")
        ws_bugs.row_dimensions[1].height = 30
        
        # En-tête bugs
        bug_headers = ["Bug ID", "Sévérité", "Test Case", "Composant", 
                      "Description", "Statut", "Assigné à", "Date"]
        
        for col, header in enumerate(bug_headers, 1):
            cell = ws_bugs.cell(row=2, column=col, value=header)
            cell.font = Font(bold=True, color="FFFFFF")
            cell.fill = PatternFill(start_color="7030A0", end_color="7030A0", fill_type="solid")
            cell.alignment = Alignment(horizontal="center")
            cell.border = Border(left=Side(style='thin'), right=Side(style='thin'),
                               top=Side(style='thin'), bottom=Side(style='thin'))
        
        # Données bugs
        for row_idx, bug in enumerate(bugs, 3):
            data = [
                bug['id'],
                bug['severity'],
                bug['test'],
                bug['component'],
                bug['desc'],
                bug['status'],
                bug['assigned'],
                bug['date']
            ]
            
            for col_idx, value in enumerate(data, 1):
                cell = ws_bugs.cell(row=row_idx, column=col_idx, value=value)
                cell.border = Border(left=Side(style='thin'), right=Side(style='thin'),
                                   top=Side(style='thin'), bottom=Side(style='thin'))
                
                # Couleur selon sévérité
                if col_idx == 2:  # Sévérité
                    if value == 'Critique':
                        cell.fill = PatternFill(start_color="FFC7CE", end_color="FFC7CE", fill_type="solid")
                        cell.font = Font(color="9C0006", bold=True)
                    elif value == 'Majeur':
                        cell.fill = PatternFill(start_color="FFEB9C", end_color="FFEB9C", fill_type="solid")
                        cell.font = Font(color="9C6500", bold=True)
        
        # Ajuster largeurs bugs
        bug_widths = [12, 12, 12, 15, 40, 12, 15, 12]
        for i, width in enumerate(bug_widths, 1):
            col_letter = chr(64 + i) if i <= 26 else chr(64 + i//26) + chr(64 + i%26)
            ws_bugs.column_dimensions[col_letter].width = width
        
        # 5. FEUILLE COUVERTURE
        print("5. Création feuille 'Couverture'...")
        ws_coverage = wb.create_sheet("Couverture")
        
        ws_coverage['A1'] = "COUVERTURE DES TESTS"
        ws_coverage['A1'].font = Font(size=16, bold=True, color="FFFFFF")
        ws_coverage['A1'].fill = PatternFill(start_color="00B0F0", end_color="00B0F0", fill_type="solid")
        ws_coverage.merge_cells('A1:E1')
        
        coverage_headers = ["Composant", "Tests planifiés", "Tests exécutés", 
                          "Tests réussis", "Couverture %"]
        
        for col, header in enumerate(coverage_headers, 1):
            cell = ws_coverage.cell(row=2, column=col, value=header)
            cell.font = Font(bold=True)
            cell.fill = PatternFill(start_color="D9E1F2", end_color="D9E1F2", fill_type="solid")
        
        # Calculer couverture par composant
        components = {}
        for test in test_results:
            comp = test['component']
            if comp not in components:
                components[comp] = {'planned': 0, 'executed': 0, 'passed': 0}
            components[comp]['executed'] += 1
            if test['status'] == 'PASSED':
                components[comp]['passed'] += 1
        
        # Estimer les tests planifiés
        for comp in components:
            components[comp]['planned'] = components[comp]['executed'] + 2  # Estimation
        
        row_idx = 3
        for comp, data in components.items():
            coverage_pct = (data['executed'] / data['planned'] * 100) if data['planned'] > 0 else 0
            success_pct = (data['passed'] / data['executed'] * 100) if data['executed'] > 0 else 0
            
            ws_coverage.cell(row=row_idx, column=1, value=comp)
            ws_coverage.cell(row=row_idx, column=2, value=data['planned'])
            ws_coverage.cell(row=row_idx, column=3, value=data['executed'])
            ws_coverage.cell(row=row_idx, column=4, value=data['passed'])
            ws_coverage.cell(row=row_idx, column=5, value=f"{coverage_pct:.1f}%")
            
            # Style pourcentage
            coverage_cell = ws_coverage.cell(row=row_idx, column=5)
            if coverage_pct >= 90:
                coverage_cell.font = Font(color="006100", bold=True)
            elif coverage_pct >= 70:
                coverage_cell.font = Font(color="9C6500")
            else:
                coverage_cell.font = Font(color="9C0006")
            
            row_idx += 1
        
        # 6. SAUVEGARDER
        print("6. Sauvegarde du fichier...")
        wb.save(filename)
        
        # VÉRIFICATION
        if os.path.exists(filename):
            file_size = os.path.getsize(filename)
            file_path = os.path.abspath(filename)
            
            print("\n" + "=" * 70)
            print("✅ RAPPORT GÉNÉRÉ AVEC SUCCÈS !")
            print("=" * 70)
            print(f"📁 Fichier: {file_path}")
            print(f"📏 Taille: {file_size:,} octets ({file_size/1024:.1f} KB)")
            print(f"📊 Contenu:")
            print(f"   • Résumé: Statistiques générales")
            print(f"   • Résultats: {len(test_results)} tests détaillés")
            print(f"   • Bugs: {len(bugs)} bugs documentés")
            print(f"   • Couverture: Analyse par composant")
            print(f"📈 Métriques:")
            print(f"   • Tests: {total_tests} total, {passed_tests} réussis ({success_rate:.1f}%)")
            print(f"   • Bugs: {len(bugs)} identifiés")
            
            return True, filename, file_path
        else:
            print(f"\n❌ ERREUR: Le fichier {filename} n'a pas été créé")
            return False, filename, None
            
    except Exception as e:
        print(f"\n❌ ERREUR CRITIQUE pendant la génération:")
        print(f"   Type: {type(e).__name__}")
        print(f"   Message: {str(e)}")
        traceback.print_exc()
        return False, filename, None

def main():
    """Fonction principale"""
    print("\n" + "=" * 70)
    print("🎯 DÉBUT DE LA GÉNÉRATION DU RAPPORT")
    print("=" * 70)
    
    success, filename, filepath = create_excel_report()
    
    if success:
        print("\n" + "=" * 70)
        print("🎉 OPÉRATION TERMINÉE AVEC SUCCÈS")
        print("=" * 70)
        
        # Demander si on veut ouvrir le fichier
        try:
            response = input("\nVoulez-vous ouvrir le fichier maintenant? (O/N): ").strip().upper()
            if response in ['O', 'OUI', 'Y', 'YES']:
                os.startfile(filename)
                print("📂 Ouverture du fichier dans Excel...")
        except:
            pass
        
        # Afficher les commandes utiles
        print("\n📋 COMMANDES UTILES:")
        print(f"   Pour ouvrir le fichier: start \"{filename}\"")
        print(f"   Pour voir le dossier: explorer .")
        print(f"   Pour copier le chemin: echo {os.path.abspath('.')}")
    
    else:
        print("\n" + "=" * 70)
        print("❌ ÉCHEC DE LA GÉNÉRATION")
        print("=" * 70)
        print("\n💡 SOLUTIONS POSSIBLES:")
        print("1. Vérifiez que Excel n'est pas ouvert")
        print("2. Vérifiez les permissions d'écriture")
        print("3. Essayez un autre nom de fichier")
        print("4. Exécutez en tant qu'administrateur")
    
    print("\n" + "=" * 70)
    input("Appuyez sur Entrée pour quitter...")

if __name__ == "__main__":
    main()