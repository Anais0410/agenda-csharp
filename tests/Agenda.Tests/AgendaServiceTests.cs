using Agenda.Core.Data;
using Agenda.Core.Models;
using Agenda.Core.Services;

namespace Agenda.Tests;

public class AgendaServiceTests
{
    private readonly AgendaService _service = new(new EvenementRepositoryMemoire());

    private Evenement Ajouter(string titre, DateTime debut, double heures,
                              Categorie categorie = Categorie.Autre, string lieu = "")
    {
        var e = new Evenement(titre, debut, debut.AddHours(heures), categorie, lieu);
        _service.Ajouter(e);
        return e;
    }

    [Fact]
    public void Tous_RetourneLesEvenementsTriesParDate()
    {
        Ajouter("Deuxième", new DateTime(2026, 10, 14, 10, 0, 0), 1);
        Ajouter("Premier", new DateTime(2026, 10, 13, 10, 0, 0), 1);

        var tous = _service.Tous();

        Assert.Equal(new[] { "Premier", "Deuxième" }, tous.Select(e => e.Titre));
    }

    [Fact]
    public void DuJour_RetourneSeulementLesEvenementsDuJour()
    {
        Ajouter("Mardi", new DateTime(2026, 10, 13, 9, 0, 0), 2);
        Ajouter("Mercredi", new DateTime(2026, 10, 14, 9, 0, 0), 2);

        var resultat = _service.DuJour(new DateOnly(2026, 10, 13));

        Assert.Single(resultat);
        Assert.Equal("Mardi", resultat[0].Titre);
    }

    [Theory]
    [InlineData(2026, 10, 12)] // lundi
    [InlineData(2026, 10, 15)] // jeudi
    [InlineData(2026, 10, 18)] // dimanche
    public void LundiDeLaSemaine_RetourneLeLundi(int annee, int mois, int jour)
    {
        DateOnly lundi = AgendaService.LundiDeLaSemaine(new DateOnly(annee, mois, jour));
        Assert.Equal(new DateOnly(2026, 10, 12), lundi);
    }

    [Fact]
    public void DeLaSemaine_RetourneDuLundiAuDimanche()
    {
        Ajouter("Dimanche d'avant", new DateTime(2026, 10, 11, 10, 0, 0), 1);
        Ajouter("Lundi", new DateTime(2026, 10, 12, 10, 0, 0), 1);
        Ajouter("Dimanche", new DateTime(2026, 10, 18, 10, 0, 0), 1);
        Ajouter("Lundi d'après", new DateTime(2026, 10, 19, 10, 0, 0), 1);

        var semaine = _service.DeLaSemaine(new DateOnly(2026, 10, 15));

        Assert.Equal(new[] { "Lundi", "Dimanche" }, semaine.Select(e => e.Titre));
    }

    [Fact]
    public void Prochains_IgnoreLesEvenementsTermines()
    {
        var maintenant = new DateTime(2026, 10, 13, 12, 0, 0);
        Ajouter("Passé", maintenant.AddHours(-3), 1);
        Ajouter("En cours", maintenant.AddHours(-1), 2);
        Ajouter("Futur", maintenant.AddHours(2), 1);

        var prochains = _service.Prochains(maintenant, 10);

        Assert.Equal(new[] { "En cours", "Futur" }, prochains.Select(e => e.Titre));
    }

    [Fact]
    public void Prochains_RespecteLeNombreDemande()
    {
        var maintenant = new DateTime(2026, 10, 13, 8, 0, 0);
        for (int i = 1; i <= 5; i++)
        {
            Ajouter($"Événement {i}", maintenant.AddDays(i), 1);
        }

        Assert.Equal(3, _service.Prochains(maintenant, 3).Count);
    }

    [Fact]
    public void Rechercher_TrouveDansTitreEtLieuSansTenirCompteDesMajuscules()
    {
        Ajouter("Cours de PYTHON", new DateTime(2026, 10, 13, 9, 0, 0), 2);
        Ajouter("Footing", new DateTime(2026, 10, 13, 18, 0, 0), 1, Categorie.Sport, "Parc Monceau");
        Ajouter("Réunion", new DateTime(2026, 10, 14, 9, 0, 0), 1);

        Assert.Single(_service.Rechercher("python"));
        Assert.Single(_service.Rechercher("monceau"));
        Assert.Empty(_service.Rechercher("java"));
        Assert.Empty(_service.Rechercher("   "));
    }

    [Fact]
    public void TrouverConflits_DetecteLesChevauchements()
    {
        Ajouter("Cours", new DateTime(2026, 10, 13, 9, 0, 0), 2);         // 9h - 11h
        Ajouter("Déjeuner", new DateTime(2026, 10, 13, 12, 0, 0), 1);     // 12h - 13h

        var nouveau = new Evenement("Rendez-vous", new DateTime(2026, 10, 13, 10, 30, 0), new DateTime(2026, 10, 13, 12, 30, 0));
        var conflits = _service.TrouverConflits(nouveau);

        Assert.Equal(new[] { "Cours", "Déjeuner" }, conflits.Select(e => e.Titre));
    }

    [Fact]
    public void Modifier_ChangeLEvenement()
    {
        var e = Ajouter("Ancien titre", new DateTime(2026, 10, 13, 9, 0, 0), 1);

        var modifie = new Evenement("Nouveau titre", e.Debut, e.Fin) { Id = e.Id };
        _service.Modifier(modifie);

        Assert.Equal("Nouveau titre", _service.Tous().Single().Titre);
    }

    [Fact]
    public void Modifier_EvenementInvalide_LeveUneErreurEtNeChangeRien()
    {
        var e = Ajouter("Cours", new DateTime(2026, 10, 13, 9, 0, 0), 1);
        var invalide = new Evenement("Cours", e.Debut, e.Fin) { Id = e.Id };
        invalide.Fin = invalide.Debut.AddHours(-1);

        Assert.Throws<ArgumentException>(() => _service.Modifier(invalide));
        Assert.Equal(e.Fin, _service.Tous().Single().Fin);
    }

    [Fact]
    public void Supprimer_RetireLEvenement()
    {
        var e = Ajouter("À supprimer", new DateTime(2026, 10, 13, 9, 0, 0), 1);

        Assert.True(_service.Supprimer(e.Id));
        Assert.Empty(_service.Tous());
        Assert.False(_service.Supprimer(e.Id)); // déjà supprimé
    }

    [Fact]
    public void TempsParCategorie_AdditionneLesDurees()
    {
        Ajouter("Cours 1", new DateTime(2026, 10, 13, 9, 0, 0), 2, Categorie.Cours);
        Ajouter("Cours 2", new DateTime(2026, 10, 14, 9, 0, 0), 1.5, Categorie.Cours);
        Ajouter("Footing", new DateTime(2026, 10, 14, 18, 0, 0), 1, Categorie.Sport);

        var stats = _service.TempsParCategorie();

        Assert.Equal(TimeSpan.FromHours(3.5), stats[Categorie.Cours]);
        Assert.Equal(TimeSpan.FromHours(1), stats[Categorie.Sport]);
        Assert.False(stats.ContainsKey(Categorie.Travail));
    }
}
