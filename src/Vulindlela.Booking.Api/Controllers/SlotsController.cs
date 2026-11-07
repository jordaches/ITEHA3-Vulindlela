using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vulindlela.Booking.Api.Business.Interfaces;
using Vulindlela.Shared.Contracts;

namespace Vulindlela.Booking.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/slots")]
    public class SlotsController : ControllerBase
    {
        private readonly ISlotService _slots;
        public SlotsController(ISlotService slots) => _slots = slots;

        /// <example>GET /api/slots?branchId=1&amp;date=2026-11-10&amp;serviceType=PASSPORT</example>
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<SlotDto>>> Get(
            [FromQuery] int branchId,
            [FromQuery] string date,
            [FromQuery] string? serviceType,
            CancellationToken ct)
        {
            // Parse explicitly: [FromQuery] DateTime depends on server culture.
            if (!DateTime.TryParseExact(date, "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var parsed))
                return BadRequest(new ProblemDto("bad_date", "date must be yyyy-MM-dd."));

            if (branchId <= 0)
                return BadRequest(new ProblemDto("bad_branch", "branchId is required."));

            return Ok(await _slots.GetForBranchOnDateAsync(branchId, parsed, serviceType, ct));
        }
    }
}
