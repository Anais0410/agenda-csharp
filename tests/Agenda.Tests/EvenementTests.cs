using Agenda.Core.Models;

namespace Agenda.Tests;

public class EvenementTests
{
    private static readonly DateTime Lundi9h = new(2026, 10, 12, 9, 0, 0);

    [Fact]
    public void Constructeur_DonneesValides_CreeLEvenement()
    {
        var e = new Evenement("  Cours de C#  ", Lundi9h, Lundi9h.AddHours(2), Categorie.Cours, "CNED");

        Assert.Equal("Cours de C#", e.Titre); // les espaces autour sont retirés
        Assert.Equal(TimeSpan.FromHours(2), e.Duree);
        Assert.Equal(Categorie.Cours, e.Categorie);
        Assert.NotEqual(Guid.Empty, e.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructeur_TitreVide_LeveUneErreur(string titre)
    {
        Assert.Throws<ArgumentException>(() => new Evenement(titre, Lundi9h, Lundi9h.AddHours(1)));
    }

    [Fact]
    public void Constructeur_TitreTropLong_LeveUneErreur()
    {
        string titre = new('a', Evenement.LongueurMaxTitre + 1);
        Assert.Throws<ArgumentException>(() => new Evenement(titre, Lundi9h, Lundi9h.AddHours(1)));
    }

    [Fact]
    public void Constructeur_FinAvantDebut_LeveUneErreur()
    {
        Assert.Throws<ArgumentException>(() => new Evenement("Test", Lundi9h, Lundi9h.AddHours(-1)));
    }

    [Fact]
    public void Constructeur_FinEgaleAuDebut_LeveUneErreur()
    {
        Assert.Throws<ArgumentException>(() => new Evenement("Test", Lundi9h, Lundi9h));
    }

    [Fact]
    public void ChevaucheAvec_CreneauxQuiSeCroisent_RetourneVrai()
    {
        var a = new Evenement("A", Lundi9h, Lundi9h.AddHours(2));             // 9h - 11h
        var b = new Evenement("B", Lundi9h.AddHours(1), Lundi9h.AddHours(3)); // 10h - 12h

        Assert.True(a.ChevaucheAvec(b));
        Assert.True(b.ChevaucheAvec(a));
    }

    [Fact]
    public void ChevaucheAvec_CreneauxQuiSeSuivent_RetourneFaux()
    {
        var a = new Evenement("A", Lundi9h, Lundi9h.AddHours(2));             // 9h - 11h
        var b = new Evenement("B", Lundi9h.AddHours(2), Lundi9h.AddHours(3)); // 11h - 12h

        Assert.False(a.ChevaucheAvec(b));
    }

    [Fact]
    public void ChevaucheAvec_LuiMeme_RetourneFaux()
    {
        var a = new Evenement("A", Lundi9h, Lundi9h.AddHours(2));
        Assert.False(a.ChevaucheAvec(a));
    }

    [Fact]
    public void ALieuLe_EvenementSurDeuxJours_ApparaitLesDeuxJours()
    {
        var soiree = new Evenement("Soirée", new DateTime(2026, 10, 12, 22, 0, 0), new DateTime(2026, 10, 13, 2, 0, 0));

        Assert.True(soiree.ALieuLe(new DateOnly(2026, 10, 12)));
        Assert.True(soiree.ALieuLe(new DateOnly(2026, 10, 13)));
        Assert.False(soiree.ALieuLe(new DateOnly(2026, 10, 14)));
    }
}
