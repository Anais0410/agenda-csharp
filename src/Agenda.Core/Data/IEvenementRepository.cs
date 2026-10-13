using Agenda.Core.Models;

namespace Agenda.Core.Data;

/// <summary>
/// Contrat d'accès aux données des événements.
/// Grâce à cette interface, l'application ne dépend pas de la façon dont les données
/// sont stockées : fichier JSON, mémoire, ou plus tard base de données MySQL.
/// </summary>
public interface IEvenementRepository
{
    IReadOnlyList<Evenement> ToutRecuperer();

    Evenement? TrouverParId(Guid id);

    void Ajouter(Evenement evenement);

    void Modifier(Evenement evenement);

    bool Supprimer(Guid id);
}
