using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private string DiceChangeName(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            // select random string from list
            var random = new Random();
            var names = new List<string>
            {
                "Hans Wurst", "Fritz Frosch", "Klaus Kleber", "Otto Normalverbraucher", "Peter Lustig",
                "Karl-Heinz Klammer", "Gustav Gans", "Heinz Erhardt", "Wolfgang Witzig", "Ludwig Lustig",
                "Rudi Rüssel", "Siggi Sorglos", "Berti Bratwurst", "Dieter Dosenbier", "Erwin Einhorn",
                "Franz Fuchs", "Günther Gans", "Horst Hering", "Ingo Igel", "Jürgen Jux",
                "Kurt Ketchup", "Lars Lachs", "Manfred Möhre", "Norbert Nudel", "Olaf Oktopus",
                "Paul Pinguin", "Quirin Qualle", "Ralf Rabe", "Stefan Seestern", "Thomas Tintenfisch",
                "Uwe Uhu", "Volker Vogel", "Willi Wurm", "Xaver Xylophon", "Yannik Yak",
                "Zacharias Zebra", "Albert Apfel", "Bernd Banane", "Claus Clown", "Detlef Dachs",
                "Egon Eule", "Ferdinand Frosch", "Gerd Giraffe", "Helmut Hase", "Igor Igel",
                "Jochen Jaguar", "Knut Känguru", "Lothar Löwe", "Martin Marder", "Norbert Nashorn",
                "Egon Kowalski", "Fritz Fink", "Heinz Hering"
            };
            var randomName = names[random.Next(names.Count)];
            var oldName = player.PlayerName;
            player.PlayerName = randomName;
            Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iszPlayerName");
            return $"{ChatColors.Green}{oldName}{ChatColors.Default} changed their name to {ChatColors.Green}{randomName}{ChatColors.Default}!";
        }
    }
}
