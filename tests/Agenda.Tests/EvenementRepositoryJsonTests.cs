using Agenda.Core.Data;
using Agenda.Core.Models;

namespace Agenda.Tests;

public class EvenementRepositoryJsonTests : IDisposable
{
    private readonly string _dossier;
    private readonly string _fichier;

    public EvenementRepositoryJsonTests()
    {
        _dossier = Path.Combine(Path.GetTempPath(), "agenda-tests-" + Guid.NewGuid());
        _fichier = Path.Combine(_dossier, "agenda.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(_dossier))
        {
            Directory.Delete(_dossier, recursive: true);
        }
    }

    [Fact]
    public void FichierAbsent_DemarreAvecUnAgendaVide()
    {
        var repo = new EvenementRepositoryJson(_fichier);
        Assert.Empty(repo.ToutRecuperer());
    }

    [Fact]
    public void Ajouter_PuisRelire_RetrouveLEvenement()
    {
        var debut = new DateTime(2026, 10, 13, 9, 0, 0);
        var original = new Evenement("Cours de SQL", debut, debut.AddHours(2), Categorie.Cours, "CNED", "Jointures");

        new EvenementRepositoryJson(_fichier).Ajouter(original);

        // Une nouvelle instance relit le fichier : on simule un redémarrage de l'application
        var relu = new EvenementRepositoryJson(_fichier).TrouverParId(original.Id);

        Assert.NotNull(relu);
        Assert.Equal("Cours de SQL", relu!.Titre);
        Assert.Equal(debut, relu.Debut);
        Assert.Equal(Categorie.Cours, relu.Categorie);
        Assert.Equal("Jointures", relu.Description);
    }

    [Fact]
    public void Supprimer_EstEnregistreDansLeFichier()
    {
        var debut = new DateTime(2026, 10, 13, 9, 0, 0);
        var e = new Evenement("Temporaire", debut, debut.AddHours(1));
        var repo = new EvenementRepositoryJson(_fichier);
        repo.Ajouter(e);

        repo.Supprimer(e.Id);

        Assert.Empty(new EvenementRepositoryJson(_fichier).ToutRecuperer());
    }

    [Fact]
    public void FichierAbime_DemarreAvecUnAgendaVide()
    {
        Directory.CreateDirectory(_dossier);
        File.WriteAllText(_fichier, "{ ceci n'est pas du JSON valide");

        var repo = new EvenementRepositoryJson(_fichier);

        Assert.Empty(repo.ToutRecuperer());
    }

    [Fact]
    public void Ajouter_DeuxFoisLeMemeEvenement_LeveUneErreur()
    {
        var debut = new DateTime(2026, 10, 13, 9, 0, 0);
        var e = new Evenement("Doublon", debut, debut.AddHours(1));
        var repo = new EvenementRepositoryJson(_fichier);
        repo.Ajouter(e);

        Assert.Throws<InvalidOperationException>(() => repo.Ajouter(e));
    }
}
