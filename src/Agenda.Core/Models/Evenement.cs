using System.Text.Json.Serialization;

namespace Agenda.Core.Models;

/// <summary>
/// Un événement de l'agenda : un titre, une date de début, une date de fin,
/// une catégorie et éventuellement un lieu et une description.
/// </summary>
public class Evenement
{
    public const int LongueurMaxTitre = 100;

    public Guid Id { get; set; } = Guid.NewGuid();
    public string Titre { get; set; } = string.Empty;
    public DateTime Debut { get; set; }
    public DateTime Fin { get; set; }
    public Categorie Categorie { get; set; } = Categorie.Autre;
    public string Lieu { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Constructeur vide, nécessaire pour la lecture du fichier JSON.
    /// </summary>
    public Evenement()
    {
    }

    /// <summary>
    /// Crée un événement et vérifie immédiatement que ses données sont valides.
    /// </summary>
    public Evenement(string titre, DateTime debut, DateTime fin,
                     Categorie categorie = Categorie.Autre,
                     string lieu = "", string description = "")
    {
        Titre = titre?.Trim() ?? string.Empty;
        Debut = debut;
        Fin = fin;
        Categorie = categorie;
        Lieu = lieu?.Trim() ?? string.Empty;
        Description = description?.Trim() ?? string.Empty;
        Valider();
    }

    /// <summary>Durée de l'événement (calculée, non enregistrée dans le fichier).</summary>
    [JsonIgnore]
    public TimeSpan Duree => Fin - Debut;

    /// <summary>
    /// Vérifie les règles métier. Lève une ArgumentException si une règle n'est pas respectée.
    /// </summary>
    public void Valider()
    {
        if (string.IsNullOrWhiteSpace(Titre))
        {
            throw new ArgumentException("Le titre est obligatoire.");
        }

        if (Titre.Length > LongueurMaxTitre)
        {
            throw new ArgumentException($"Le titre ne doit pas dépasser {LongueurMaxTitre} caractères.");
        }

        if (Fin <= Debut)
        {
            throw new ArgumentException("La fin doit être après le début.");
        }
    }

    /// <summary>
    /// Indique si deux événements se chevauchent dans le temps.
    /// Deux événements qui se suivent (l'un finit quand l'autre commence) ne se chevauchent pas.
    /// </summary>
    public bool ChevaucheAvec(Evenement autre)
    {
        return Id != autre.Id && Debut < autre.Fin && autre.Debut < Fin;
    }

    /// <summary>
    /// Indique si l'événement a lieu (au moins en partie) le jour donné.
    /// </summary>
    public bool ALieuLe(DateOnly jour)
    {
        DateTime debutJour = jour.ToDateTime(TimeOnly.MinValue);
        DateTime finJour = debutJour.AddDays(1);
        return Debut < finJour && Fin > debutJour;
    }

    public override string ToString()
    {
        return $"{Titre} ({Debut:dd/MM/yyyy HH:mm} → {Fin:HH:mm})";
    }
}
