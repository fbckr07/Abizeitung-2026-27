namespace Frontend.Data;

public static class Permissions
{
    public const string Steckbrief = "steckbrief";
    public const string RankingSchueler = "rankingschueler";
    public const string RankingLehrer = "rankinglehrer";
    public const string Lehrerzitate = "lehrerzitate";
    public const string Bestellungen = "bestellungen";
    public const string SchülerKommentare = "kommentare";
    
    public const string AdminErgebnisse = "admin_ergebnisse";
    public const string AdminFragen = "admin_fragen";
    public const string AdminLehrer = "admin_lehrer";
    public const string AdminList = "admin_list";
    public const string AdminLehrerzitate = "admin_lehrerzitate";
    public const string AdminSchueler = "admin_schueler";
    public const string AdminSteckbriefe = "admin_steckbriefe";
    
    
    public static readonly IReadOnlyList<(string Value, string DisplayName)> Alle =
    [
        (Steckbrief, "Steckbrief"),
        (RankingSchueler, "Ranking Schüler"),
        (RankingLehrer, "Ranking Lehrer"),
        (Lehrerzitate, "Lehrerzitate"),
        (Bestellungen, "Bestellungen"),
        (SchülerKommentare, "Schüler Kommentare"),

    ];
    
    public static readonly IReadOnlyList<(string Value, string DisplayName)> AdminPermissions =
    [
        (AdminErgebnisse, "Admin Ergebnisse"),
        (AdminFragen, "Admin Fragen"),
        (AdminLehrer, "Admin Lehrer"),
        (AdminList, "Admin List"),
        (AdminLehrerzitate, "Admin Lehrerzitate"),
        (AdminSchueler, "Admin Schüler"),
        (AdminSteckbriefe, "Admin Steckbriefe")
    ];
}
