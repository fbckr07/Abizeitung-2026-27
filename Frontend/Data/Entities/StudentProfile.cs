namespace Frontend.Data.Entities;

public sealed class StudentProfile
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public string? FotoUrl { get; set; }

    public string? Motto { get; set; }

    public string? Hobbys { get; set; }

    public string? Zukunftswunsch { get; set; }

    public bool IstFreigegeben { get; set; }
    
    public string? AdminKommentar { get; set; }

    public DateTime ErstelltAm { get; set; }

    public DateTime? AktualisiertAm { get; set; }

    public Student Student { get; set; } = null!;
}