using Frontend.Data.DTOs;
using Frontend.Data.Entities;

namespace Frontend.Services;

public interface IProfileValidator
{
    Dictionary<Guid, string> Validate(List<ProfileField> fields, List<FieldValueInput> values);
}