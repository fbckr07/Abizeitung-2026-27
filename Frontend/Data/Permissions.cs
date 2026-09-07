namespace Frontend.Data;

public static class Permissions
{
    public const string Steckbrief = "steckbrief";
    public const string RankingSchueler = "rankingschueler";
    public const string RankingLehrer = "rankinglehrer";
    public const string Lehrerzitate = "lehrerzitate";
    public const string Bestellungen = "bestellungen";
    public const string SchülerKommentare = "kommentare";
    
    public static readonly IReadOnlyList<(string Value, string DisplayName)> Alle =
    [
        (Steckbrief, "Steckbrief"),
        (RankingSchueler, "Ranking Schüler"),
        (RankingLehrer, "Ranking Lehrer"),
        (Lehrerzitate, "Lehrerzitate"),
        (Bestellungen, "Bestellungen"),
        (SchülerKommentare, "Schüler Kommentare")
    ];
}
