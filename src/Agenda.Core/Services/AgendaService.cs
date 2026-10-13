using Agenda.Core.Data;
using Agenda.Core.Models;

namespace Agenda.Core.Services;

/// <summary>
/// Logique métier de l'agenda : ajout, modification, suppression,
/// recherche, vues par jour et par semaine, détection des conflits et statistiques.
/// </summary>
public class AgendaService
{
    private readonly IEvenementRepository _repository;

    public AgendaService(IEvenementRepository repository)
    {
        _repository = repository;
    }

    // ----- Création, modification, suppression -----

    public void Ajouter(Evenement evenement)
    {
        evenement.Valider();
        _repository.Ajouter(evenement);
    }

    public void Modifier(Evenement evenement)
    {
        evenement.Valider();
        _repository.Modifier(evenement);
    }

    public bool Supprimer(Guid id)
    {
        return _repository.Supprimer(id);
    }

    // ----- Consultation -----

    public IReadOnlyList<Evenement> Tous()
    {
        return Trier(_repository.ToutRecuperer());
    }

    public IReadOnlyList<Evenement> DuJour(DateOnly jour)
    {
        return Trier(_repository.ToutRecuperer().Where(e => e.ALieuLe(jour)));
    }

    /// <summary>
    /// Événements de la semaine (du lundi au dimanche) qui contient le jour donné.
    /// </summary>
    public IReadOnlyList<Evenement> DeLaSemaine(DateOnly jour)
    {
        DateOnly lundi = LundiDeLaSemaine(jour);
        DateTime debutSemaine = lundi.ToDateTime(TimeOnly.MinValue);
        DateTime finSemaine = debutSemaine.AddDays(7);

        return Trier(_repository.ToutRecuperer()
            .Where(e => e.Debut < finSemaine && e.Fin > debutSemaine));
    }

    /// <summary>
    /// Les prochains événements à venir (ou en cours) à partir de maintenant.
    /// </summary>
    public IReadOnlyList<Evenement> Prochains(DateTime maintenant, int nombre)
    {
        return Trier(_repository.ToutRecuperer().Where(e => e.Fin > maintenant))
            .Take(nombre)
            .ToList();
    }

    /// <summary>
    /// Recherche un texte dans le titre, le lieu et la description (sans tenir compte des majuscules).
    /// </summary>
    public IReadOnlyList<Evenement> Rechercher(string texte)
    {
        if (string.IsNullOrWhiteSpace(texte))
        {
            return new List<Evenement>();
        }

        string recherche = texte.Trim();
        return Trier(_repository.ToutRecuperer().Where(e =>
            e.Titre.Contains(recherche, StringComparison.OrdinalIgnoreCase) ||
            e.Lieu.Contains(recherche, StringComparison.OrdinalIgnoreCase) ||
            e.Description.Contains(recherche, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// Liste des événements déjà présents qui chevauchent l'événement donné.
    /// </summary>
    public IReadOnlyList<Evenement> TrouverConflits(Evenement evenement)
    {
        return Trier(_repository.ToutRecuperer().Where(e => e.ChevaucheAvec(evenement)));
    }

    /// <summary>
    /// Temps total passé par catégorie (seules les catégories utilisées apparaissent).
    /// </summary>
    public Dictionary<Categorie, TimeSpan> TempsParCategorie()
    {
        return _repository.ToutRecuperer()
            .GroupBy(e => e.Categorie)
            .ToDictionary(
                groupe => groupe.Key,
                groupe => TimeSpan.FromTicks(groupe.Sum(e => e.Duree.Ticks)));
    }

    // ----- Outils -----

    public static DateOnly LundiDeLaSemaine(DateOnly jour)
    {
        // DayOfWeek : dimanche = 0, lundi = 1 ... on calcule l'écart avec le lundi.
        int ecart = ((int)jour.DayOfWeek + 6) % 7;
        return jour.AddDays(-ecart);
    }

    private static List<Evenement> Trier(IEnumerable<Evenement> evenements)
    {
        return evenements.OrderBy(e => e.Debut).ThenBy(e => e.Titre).ToList();
    }
}
