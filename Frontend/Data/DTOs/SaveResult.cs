namespace Frontend.Data.DTOs;

public class SaveResult
{
    public bool Success { get; set; }
    public Dictionary<Guid, string> Errors { get; set; } = new Dictionary<Guid, string>();
}