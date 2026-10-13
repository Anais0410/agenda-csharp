using System.Text.Json;
using System.Text.Json.Serialization;
using Agenda.Core.Models;

namespace Agenda.Core.Data;

/// <summary>
/// Stockage des événements dans un fichier JSON.
/// Hérite du stockage en mémoire et ajoute la lecture au démarrage
/// et l'écriture du fichier après chaque modification (héritage + redéfinition).
/// </summary>
public class EvenementRepositoryJson : EvenementRepositoryMemoire
{
    private static readonly JsonSerializerOptions OptionsJson = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _cheminFichier;

    public EvenementRepositoryJson(string cheminFichier)
    {
        _cheminFichier = cheminFichier;
        Evenements.AddRange(Charger());
    }

    public override void Ajouter(Evenement evenement)
    {
        base.Ajouter(evenement);
        Sauvegarder();
    }

    public override void Modifier(Evenement evenement)
    {
        base.Modifier(evenement);
        Sauvegarder();
    }

    public override bool Supprimer(Guid id)
    {
        bool supprime = base.Supprimer(id);
        if (supprime)
        {
            Sauvegarder();
        }

        return supprime;
    }

    private List<Evenement> Charger()
    {
        if (!File.Exists(_cheminFichier))
        {
            return new List<Evenement>();
        }

        try
        {
            string json = File.ReadAllText(_cheminFichier);
            return JsonSerializer.Deserialize<List<Evenement>>(json, OptionsJson) ?? new List<Evenement>();
        }
        catch (JsonException)
        {
            // Fichier abîmé : on repart d'un agenda vide plutôt que de planter.
            return new List<Evenement>();
        }
    }

    private void Sauvegarder()
    {
        string? dossier = Path.GetDirectoryName(_cheminFichier);
        if (!string.IsNullOrEmpty(dossier))
        {
            Directory.CreateDirectory(dossier);
        }

        string json = JsonSerializer.Serialize(Evenements, OptionsJson);
        File.WriteAllText(_cheminFichier, json);
    }
}
