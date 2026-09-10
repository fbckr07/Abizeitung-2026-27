using System.Text.RegularExpressions;
using Frontend.Data.DTOs;
using Frontend.Data.Entities;
using Frontend.Data.Enums;
using Frontend.Services;

namespace Frontend;

public class ProfileValidator : IProfileValidator
{
    public Dictionary<Guid, string> Validate(List<ProfileField> fields, List<FieldValueInput> values)
    {
        var errors = new Dictionary<Guid, string>();
        var valuesByFieldId = values.ToDictionary(v => v.FieldId);

        foreach (var field in fields)
        {
            valuesByFieldId.TryGetValue(field.Id, out var input);

            var isEmpty = input is null
                || (string.IsNullOrWhiteSpace(input.Value) && (input.SelectedOptionIds is null || input.SelectedOptionIds.Count == 0));

            if (field.IsRequired && isEmpty)
            {
                errors[field.Id] = $"„{field.Label}„ ist ein Pflichtfeld.";
                continue;
            }

            if (isEmpty)
                continue; // optional und leer -> keine weitere Prüfung

            switch (field.Type)
            {
                case FieldType.Number:
                    if (!double.TryParse(input!.Value, out _))
                        errors[field.Id] = $"„{field.Label}„ muss eine Zahl sein.";
                    break;

                case FieldType.Date:
                    if (!DateTime.TryParse(input!.Value, out _))
                        errors[field.Id] = $"„{field.Label}„ muss ein gültiges Datum sein.";
                    break;

                case FieldType.Boolean:
                    if (!bool.TryParse(input!.Value, out _))
                        errors[field.Id] = $"„{field.Label}„ muss ein gültiger Wahrheitswert sein.";
                    break;

                case FieldType.Email:
                    if (!Regex.IsMatch(input!.Value ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                        errors[field.Id] = $"„{field.Label}„ muss eine gültige E-Mail-Adresse sein.";
                    break;

                case FieldType.Url:
                    if (!Uri.TryCreate(input!.Value, UriKind.Absolute, out _))
                        errors[field.Id] = $"„{field.Label}„ muss eine gültige URL sein.";
                    break;

                case FieldType.Text:
                case FieldType.TextArea:
                    var text = input!.Value ?? "";
                    break;

                case FieldType.SingleSelect:
                    if (input!.SelectedOptionIds is null || input.SelectedOptionIds.Count != 1)
                    {
                        errors[field.Id] = $"„{field.Label}„ erfordert genau eine Auswahl.";
                        break;
                    }
                    if (!field.Options.Any(o => o.Id == input.SelectedOptionIds[0]))
                        errors[field.Id] = $"Ausgewählte Option ist für „{field.Label}„ ungültig.";
                    break;

                case FieldType.MultiSelect:
                    var selected = input!.SelectedOptionIds ?? new List<Guid>();
                    var validIds = field.Options.Select(o => o.Id).ToHashSet();
                    if (selected.Any(id => !validIds.Contains(id)))
                        errors[field.Id] = $"Eine oder mehrere ausgewählte Optionen sind für „{field.Label}„ ungültig.";
                    break;
            }
        }

        return errors;
    }
}