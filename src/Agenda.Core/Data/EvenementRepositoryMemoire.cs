using Agenda.Core.Models;

namespace Agenda.Core.Data;

/// <summary>
/// Stockage des événements en mémoire uniquement (rien n'est enregistré sur le disque).
/// Utilisé pour les tests unitaires.
/// </summary>
public class EvenementRepositoryMemoire : IEvenementRepository
{
    protected readonly List<Evenement> Evenements = new();

    public IReadOnlyList<Evenement> ToutRecuperer()
    {
        return Evenements.AsReadOnly();
    }

    public Evenement? TrouverParId(Guid id)
    {
        return Evenements.FirstOrDefault(e => e.Id == id);
    }

    public virtual void Ajouter(Evenement evenement)
    {
        if (TrouverParId(evenement.Id) != null)
        {
            throw new InvalidOperationException("Un événement avec cet identifiant existe déjà.");
        }

        Evenements.Add(evenement);
    }

    public virtual void Modifier(Evenement evenement)
    {
        int index = Evenements.FindIndex(e => e.Id == evenement.Id);
        if (index < 0)
        {
            throw new KeyNotFoundException("Événement introuvable.");
        }

        Evenements[index] = evenement;
    }

    public virtual bool Supprimer(Guid id)
    {
        return Evenements.RemoveAll(e => e.Id == id) > 0;
    }
}
