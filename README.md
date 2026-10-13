# 📅 Agenda en C#

Application d'agenda en console développée en **C# (.NET 8)** dans le cadre de mon BTS SIO option SLAM.
Elle met en pratique la **programmation orientée objet**, une **architecture en couches**, la **persistance des données**
en JSON et les **tests unitaires**, avec une **intégration continue** sur GitHub Actions.

![Compilation et tests](https://github.com/Anais0410/agenda-csharp/actions/workflows/build-et-tests.yml/badge.svg)

## ✨ Fonctionnalités

- **Ajouter, modifier et supprimer** des événements (titre, date, horaires, catégorie, lieu, description)
- **Vue du jour** et **vue de la semaine** (du lundi au dimanche)
- **Prochains événements** à venir
- **Recherche** dans le titre, le lieu et la description
- **Détection des conflits** : l'application prévient si un nouvel événement chevauche un créneau déjà pris
- **Statistiques** : temps passé par catégorie, avec un petit graphique en barres
- **6 catégories** colorées : Cours, Travail, Stage, Personnel, Sport, Autre
- **Sauvegarde automatique** dans un fichier JSON, rechargé au démarrage
- **Saisies contrôlées** : dates et heures au bon format, titre obligatoire, fin après le début

## 🏗️ Architecture

La solution est découpée en trois projets :

```
agenda-csharp/
├── Agenda.sln
├── src/
│   ├── Agenda.Core/                     # Bibliothèque : le cœur de l'application
│   │   ├── Models/
│   │   │   ├── Evenement.cs             # classe métier + règles de validation
│   │   │   └── Categorie.cs             # énumération des catégories
│   │   ├── Data/
│   │   │   ├── IEvenementRepository.cs  # interface d'accès aux données
│   │   │   ├── EvenementRepositoryMemoire.cs
│   │   │   └── EvenementRepositoryJson.cs
│   │   └── Services/
│   │       └── AgendaService.cs         # logique métier
│   └── Agenda.App/                      # Application console
│       ├── Program.cs                   # point d'entrée
│       └── MenuConsole.cs               # menus et saisies
├── tests/
│   └── Agenda.Tests/                    # 29 tests unitaires (xUnit)
└── .github/workflows/                   # intégration continue
```

| Couche | Rôle |
|---|---|
| **Modèle** (`Evenement`, `Categorie`) | Représente les données et vérifie leurs règles (titre obligatoire, fin après le début…) |
| **Accès aux données** (`IEvenementRepository`) | Lit et enregistre les événements. L'interface permet de changer de stockage sans toucher au reste |
| **Service** (`AgendaService`) | Logique métier : vues par jour et par semaine, recherche, conflits, statistiques |
| **Interface** (`MenuConsole`) | Affiche les menus, lit les saisies et appelle le service |

## 🧠 Notions de POO utilisées

- **Encapsulation** : la classe `Evenement` vérifie elle-même ses données avec `Valider()`
- **Interface** : `IEvenementRepository` définit un contrat que plusieurs classes respectent
- **Héritage et redéfinition** : `EvenementRepositoryJson` hérite de `EvenementRepositoryMemoire` et redéfinit (`override`) les méthodes d'ajout, de modification et de suppression pour enregistrer le fichier
- **Polymorphisme** : `AgendaService` reçoit un `IEvenementRepository` et fonctionne aussi bien avec le stockage en mémoire (tests) qu'avec le stockage JSON (application)
- **Injection de dépendances** : le service reçoit son stockage dans son constructeur
- **Énumération**, **propriétés calculées** (`Duree`), **LINQ** (`Where`, `OrderBy`, `GroupBy`), **gestion des exceptions**

## 🚀 Lancer l'application

Prérequis : [.NET 8 SDK](https://dotnet.microsoft.com/download), ou Visual Studio 2022.

**Avec Visual Studio** : ouvrir `Agenda.sln`, choisir `Agenda.App` comme projet de démarrage, puis appuyer sur F5.

**En ligne de commande** :

```bash
dotnet run --project src/Agenda.App
```

## 🧪 Lancer les tests

```bash
dotnet test
```

Les tests vérifient les règles de validation, le chevauchement des créneaux, les vues par jour et par semaine,
la recherche, les statistiques et l'enregistrement dans le fichier JSON. Ils sont aussi lancés automatiquement
par **GitHub Actions** à chaque envoi de code.

## 🔭 Évolutions possibles

- Remplacer le fichier JSON par une base de données **MySQL**, en ajoutant simplement une nouvelle classe qui implémente `IEvenementRepository`
- Ajouter une interface graphique (WinForms ou WPF) qui réutiliserait `Agenda.Core` sans modification
- Gérer les événements récurrents (chaque semaine, chaque mois)

## 👩‍💻 Autrice

**Anaïs Billaud**, étudiante en BTS SIO option SLAM
