using Microsoft.AspNetCore.Mvc;
using Vulindlela.Booking.Api.Business.Interfaces;
using Vulindlela.Shared.Contracts;

namespace Vulindlela.Booking.Api.Controllers
{
    [ApiController]
    [Route("api/citizens")]
    public class CitizensController : ControllerBase
    {
        private readonly ICitizenAccountRepository _citizens;
        public CitizensController(ICitizenAccountRepository citizens) => _citizens = citizens;

        // Demo convenience: lets you find seeded citizen ids without touching the db file.
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<CitizenDto>>> GetAll(CancellationToken ct)
        {
            var citizens = await _citizens.GetAllAsync(ct);
            return Ok(citizens.Select(c => new CitizenDto(c.Id, c.FullName, c.IdNumber, c.CellNumber)).ToList());
        }
    }
}
