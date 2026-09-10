namespace Frontend.Data.DTOs;

public class FieldValueInput
{
    public Guid FieldId { get; set; }
    public string? Value { get; set; }
    public List<Guid>? SelectedOptionIds { get; set; }
}