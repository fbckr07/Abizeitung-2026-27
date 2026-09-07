using System.Text;
using Frontend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers;

[ApiController]
[Route("admin/ergebnisse")]
[Authorize(AuthenticationSchemes = "AdminAuth", Roles = "Admin")]
public sealed class AdminErgebnisseController(IResultService resultService) : ControllerBase
{
    [HttpGet("export/schueler")]
    public async Task<IActionResult> ExportSchueler(CancellationToken cancellationToken)
    {
        var csv = await resultService.ExportStudentCsvAsync(cancellationToken);
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", "ergebnisse-schueler.csv");
    }

    [HttpGet("export/lehrer")]
    public async Task<IActionResult> ExportLehrer(CancellationToken cancellationToken)
    {
        var csv = await resultService.ExportTeacherCsvAsync(cancellationToken);
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", "ergebnisse-lehrer.csv");
    }
}
