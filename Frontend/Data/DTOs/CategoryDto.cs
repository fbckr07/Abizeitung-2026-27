using Frontend.Data.Enums;

namespace Frontend.Data.DTOs;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public List<FieldDto> Fields { get; set; } = new();
}

public class FieldDto
{
    public Guid Id { get; set; }
    public string Label { get; set; } = default!;
    public string? Placeholder { get; set; }
    public FieldType Type { get; set; }
    public bool IsRequired { get; set; }
    public List<FieldOptionDto> Options { get; set; } = new();
    
    public string? Value { get; set; }
    public List<Guid> SelectedOptionIds { get; set; } = new();
}

public class FieldOptionDto
{
    public Guid Id { get; set; }
    public string Value { get; set; } = default!;
    public string Label { get; set; } = default!;
}
    