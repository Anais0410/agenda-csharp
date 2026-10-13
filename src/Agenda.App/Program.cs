using System.Text;
using Agenda.App;
using Agenda.Core.Data;
using Agenda.Core.Services;

// Point d'entrée de l'application
Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

// Les événements sont enregistrés dans le dossier "data", à côté de l'exécutable
string cheminFichier = Path.Combine(AppContext.BaseDirectory, "data", "agenda.json");

var repository = new EvenementRepositoryJson(cheminFichier);
var service = new AgendaService(repository);
var menu = new MenuConsole(service);

menu.Lancer();
