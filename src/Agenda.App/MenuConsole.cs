using System.Globalization;
using Agenda.Core.Models;
using Agenda.Core.Services;

namespace Agenda.App;

/// <summary>
/// Interface en console : affiche les menus, lit et vérifie les saisies,
/// puis appelle le service de l'agenda.
/// </summary>
public class MenuConsole
{
    private const string FormatDate = "dd/MM/yyyy";
    private const string FormatHeure = "HH:mm";

    private static readonly CultureInfo Fr = CultureInfo.GetCultureInfo("fr-FR");

    private static readonly Dictionary<Categorie, ConsoleColor> Couleurs = new()
    {
        [Categorie.Cours] = ConsoleColor.Cyan,
        [Categorie.Travail] = ConsoleColor.Yellow,
        [Categorie.Stage] = ConsoleColor.Green,
        [Categorie.Personnel] = ConsoleColor.Magenta,
        [Categorie.Sport] = ConsoleColor.Red,
        [Categorie.Autre] = ConsoleColor.Gray
    };

    private readonly AgendaService _service;

    public MenuConsole(AgendaService service)
    {
        _service = service;
    }

    public void Lancer()
    {
        AfficherTitre("Mon agenda");
        bool continuer = true;

        while (continuer)
        {
            Console.WriteLine();
            Console.WriteLine("1. Voir aujourd'hui");
            Console.WriteLine("2. Voir une semaine");
            Console.WriteLine("3. Prochains événements");
            Console.WriteLine("4. Ajouter un événement");
            Console.WriteLine("5. Modifier un événement");
            Console.WriteLine("6. Supprimer un événement");
            Console.WriteLine("7. Rechercher");
            Console.WriteLine("8. Statistiques par catégorie");
            Console.WriteLine("0. Quitter");

            int choix = LireEntier("Ton choix : ", 0, 8);
            Console.WriteLine();

            try
            {
                switch (choix)
                {
                    case 1: VoirAujourdhui(); break;
                    case 2: VoirSemaine(); break;
                    case 3: VoirProchains(); break;
                    case 4: AjouterEvenement(); break;
                    case 5: ModifierEvenement(); break;
                    case 6: SupprimerEvenement(); break;
                    case 7: Rechercher(); break;
                    case 8: AfficherStatistiques(); break;
                    case 0: continuer = false; break;
                }
            }
            catch (ArgumentException erreur)
            {
                AfficherErreur(erreur.Message);
            }
        }

        Console.WriteLine("À bientôt !");
    }

    // ----- Actions du menu -----

    private void VoirAujourdhui()
    {
        DateOnly aujourdhui = DateOnly.FromDateTime(DateTime.Now);
        AfficherTitre($"Aujourd'hui, {aujourdhui.ToString("dddd d MMMM", Fr)}");
        AfficherListe(_service.DuJour(aujourdhui), numeroter: false);
    }

    private void VoirSemaine()
    {
        DateOnly jour = LireDate("Une date dans la semaine (vide = cette semaine) : ",
                                 DateOnly.FromDateTime(DateTime.Now));
        DateOnly lundi = AgendaService.LundiDeLaSemaine(jour);
        AfficherTitre($"Semaine du {lundi.ToString("d MMMM", Fr)} au {lundi.AddDays(6).ToString("d MMMM yyyy", Fr)}");

        IReadOnlyList<Evenement> evenements = _service.DeLaSemaine(jour);
        for (int i = 0; i < 7; i++)
        {
            DateOnly jourCourant = lundi.AddDays(i);
            var duJour = evenements.Where(e => e.ALieuLe(jourCourant)).ToList();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(Fr.TextInfo.ToTitleCase(jourCourant.ToString("dddd d", Fr)));
            Console.ResetColor();

            if (duJour.Count == 0)
            {
                Console.WriteLine("   -");
            }

            foreach (Evenement e in duJour)
            {
                AfficherEvenement(e, null);
            }
        }
    }

    private void VoirProchains()
    {
        AfficherTitre("Prochains événements");
        AfficherListe(_service.Prochains(DateTime.Now, 10), numeroter: false);
    }

    private void AjouterEvenement()
    {
        AfficherTitre("Nouvel événement");
        string titre = LireTexte("Titre : ", obligatoire: true);
        DateOnly date = LireDate("Date (jj/mm/aaaa) : ", null);
        TimeOnly heureDebut = LireHeure("Heure de début (hh:mm) : ", null);
        TimeOnly heureFin = LireHeure("Heure de fin (hh:mm) : ", null);
        Categorie categorie = LireCategorie(null);
        string lieu = LireTexte("Lieu (facultatif) : ", obligatoire: false);
        string description = LireTexte("Description (facultatif) : ", obligatoire: false);

        var evenement = new Evenement(titre, date.ToDateTime(heureDebut), date.ToDateTime(heureFin),
                                      categorie, lieu, description);

        if (!ConfirmerMalgreConflits(evenement))
        {
            Console.WriteLine("Ajout annulé.");
            return;
        }

        _service.Ajouter(evenement);
        AfficherSucces("Événement ajouté !");
    }

    private void ModifierEvenement()
    {
        Evenement? evenement = ChoisirEvenement("Quel événement modifier ?");
        if (evenement == null)
        {
            return;
        }

        Console.WriteLine("Appuie sur Entrée pour garder la valeur actuelle.");
        string titre = LireTexte($"Titre [{evenement.Titre}] : ", obligatoire: false);
        DateOnly date = LireDate($"Date [{evenement.Debut.ToString(FormatDate)}] : ", DateOnly.FromDateTime(evenement.Debut));
        TimeOnly heureDebut = LireHeure($"Début [{evenement.Debut.ToString(FormatHeure)}] : ", TimeOnly.FromDateTime(evenement.Debut));
        TimeOnly heureFin = LireHeure($"Fin [{evenement.Fin.ToString(FormatHeure)}] : ", TimeOnly.FromDateTime(evenement.Fin));
        Categorie categorie = LireCategorie(evenement.Categorie);
        string lieu = LireTexte($"Lieu [{evenement.Lieu}] : ", obligatoire: false);
        string description = LireTexte($"Description [{evenement.Description}] : ", obligatoire: false);

        // On crée une copie modifiée : l'original n'est remplacé que si tout est valide.
        var modifie = new Evenement(
            titre == string.Empty ? evenement.Titre : titre,
            date.ToDateTime(heureDebut),
            date.ToDateTime(heureFin),
            categorie,
            lieu == string.Empty ? evenement.Lieu : lieu,
            description == string.Empty ? evenement.Description : description)
        {
            Id = evenement.Id
        };

        if (!ConfirmerMalgreConflits(modifie))
        {
            Console.WriteLine("Modification annulée.");
            return;
        }

        _service.Modifier(modifie);
        AfficherSucces("Événement modifié !");
    }

    private void SupprimerEvenement()
    {
        Evenement? evenement = ChoisirEvenement("Quel événement supprimer ?");
        if (evenement == null)
        {
            return;
        }

        if (Confirmer($"Supprimer « {evenement.Titre} » ? (o/n) : "))
        {
            _service.Supprimer(evenement.Id);
            AfficherSucces("Événement supprimé.");
        }
    }

    private void Rechercher()
    {
        string texte = LireTexte("Mot à rechercher : ", obligatoire: true);
        AfficherTitre($"Résultats pour « {texte} »");
        AfficherListe(_service.Rechercher(texte), numeroter: false);
    }

    private void AfficherStatistiques()
    {
        AfficherTitre("Temps passé par catégorie");
        Dictionary<Categorie, TimeSpan> stats = _service.TempsParCategorie();

        if (stats.Count == 0)
        {
            Console.WriteLine("Aucun événement pour le moment.");
            return;
        }

        double total = stats.Values.Sum(t => t.TotalHours);
        foreach (var (categorie, duree) in stats.OrderByDescending(s => s.Value))
        {
            double pourcentage = duree.TotalHours / total * 100;
            int longueurBarre = (int)Math.Round(pourcentage / 5);

            Console.ForegroundColor = Couleurs[categorie];
            Console.Write($"{categorie,-10} {new string('█', longueurBarre),-20} ");
            Console.ResetColor();
            Console.WriteLine($"{duree.TotalHours:0.#} h ({pourcentage:0} %)");
        }
    }

    // ----- Affichage -----

    private static void AfficherTitre(string titre)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"=== {titre} ===");
        Console.ResetColor();
    }

    private static void AfficherListe(IReadOnlyList<Evenement> evenements, bool numeroter)
    {
        if (evenements.Count == 0)
        {
            Console.WriteLine("Aucun événement.");
            return;
        }

        for (int i = 0; i < evenements.Count; i++)
        {
            AfficherEvenement(evenements[i], numeroter ? i + 1 : null);
        }
    }

    private static void AfficherEvenement(Evenement e, int? numero)
    {
        string prefixe = numero.HasValue ? $"{numero,2}. " : "   ";
        string date = e.Debut.ToString("ddd dd/MM", Fr);
        string horaire = e.Debut.Date == e.Fin.Date
            ? $"{e.Debut.ToString(FormatHeure)} → {e.Fin.ToString(FormatHeure)}"
            : $"{e.Debut.ToString(FormatHeure)} → {e.Fin.ToString("dd/MM HH:mm")}";

        Console.Write($"{prefixe}[{date}] {horaire}  ");
        Console.ForegroundColor = Couleurs[e.Categorie];
        Console.Write($"{e.Titre}");
        Console.ResetColor();
        Console.Write($" ({e.Categorie})");
        if (!string.IsNullOrEmpty(e.Lieu))
        {
            Console.Write($" @ {e.Lieu}");
        }

        Console.WriteLine();
    }

    private static void AfficherSucces(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    private static void AfficherErreur(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Erreur : {message}");
        Console.ResetColor();
    }

    // ----- Saisies vérifiées -----

    private Evenement? ChoisirEvenement(string question)
    {
        IReadOnlyList<Evenement> evenements = _service.Tous();
        if (evenements.Count == 0)
        {
            Console.WriteLine("Aucun événement dans l'agenda.");
            return null;
        }

        AfficherTitre(question);
        AfficherListe(evenements, numeroter: true);
        int numero = LireEntier("Numéro (0 pour annuler) : ", 0, evenements.Count);
        return numero == 0 ? null : evenements[numero - 1];
    }

    private bool ConfirmerMalgreConflits(Evenement evenement)
    {
        IReadOnlyList<Evenement> conflits = _service.TrouverConflits(evenement);
        if (conflits.Count == 0)
        {
            return true;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Attention, ce créneau chevauche :");
        Console.ResetColor();
        AfficherListe(conflits, numeroter: false);
        return Confirmer("Continuer quand même ? (o/n) : ");
    }

    private static string LireTexte(string message, bool obligatoire)
    {
        while (true)
        {
            Console.Write(message);
            string saisie = (Console.ReadLine() ?? string.Empty).Trim();
            if (!obligatoire || saisie.Length > 0)
            {
                return saisie;
            }

            AfficherErreur("Ce champ est obligatoire.");
        }
    }

    private static int LireEntier(string message, int min, int max)
    {
        while (true)
        {
            Console.Write(message);
            if (int.TryParse(Console.ReadLine(), out int valeur) && valeur >= min && valeur <= max)
            {
                return valeur;
            }

            AfficherErreur($"Entre un nombre entre {min} et {max}.");
        }
    }

    private static DateOnly LireDate(string message, DateOnly? parDefaut)
    {
        while (true)
        {
            Console.Write(message);
            string saisie = (Console.ReadLine() ?? string.Empty).Trim();

            if (saisie.Length == 0 && parDefaut.HasValue)
            {
                return parDefaut.Value;
            }

            if (DateOnly.TryParseExact(saisie, FormatDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date))
            {
                return date;
            }

            AfficherErreur("Format attendu : jj/mm/aaaa (exemple : 14/10/2026).");
        }
    }

    private static TimeOnly LireHeure(string message, TimeOnly? parDefaut)
    {
        while (true)
        {
            Console.Write(message);
            string saisie = (Console.ReadLine() ?? string.Empty).Trim().Replace('h', ':');

            if (saisie.Length == 0 && parDefaut.HasValue)
            {
                return parDefaut.Value;
            }

            if (TimeOnly.TryParseExact(saisie, FormatHeure, CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly heure))
            {
                return heure;
            }

            AfficherErreur("Format attendu : hh:mm (exemple : 09:30).");
        }
    }

    private static Categorie LireCategorie(Categorie? parDefaut)
    {
        Categorie[] categories = Enum.GetValues<Categorie>();
        for (int i = 0; i < categories.Length; i++)
        {
            Console.ForegroundColor = Couleurs[categories[i]];
            Console.Write($"  {i + 1}. {categories[i]}");
            Console.ResetColor();
        }

        Console.WriteLine();

        while (true)
        {
            string invite = parDefaut.HasValue ? $"Catégorie [{parDefaut}] : " : "Catégorie : ";
            Console.Write(invite);
            string saisie = (Console.ReadLine() ?? string.Empty).Trim();

            if (saisie.Length == 0 && parDefaut.HasValue)
            {
                return parDefaut.Value;
            }

            if (int.TryParse(saisie, out int numero) && numero >= 1 && numero <= categories.Length)
            {
                return categories[numero - 1];
            }

            AfficherErreur($"Entre un nombre entre 1 et {categories.Length}.");
        }
    }

    private static bool Confirmer(string message)
    {
        Console.Write(message);
        string saisie = (Console.ReadLine() ?? string.Empty).Trim().ToLowerInvariant();
        return saisie == "o" || saisie == "oui";
    }
}
