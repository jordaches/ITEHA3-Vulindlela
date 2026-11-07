using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vulindlela.Booking.Api.Business.Interfaces;
using Vulindlela.Shared.Contracts;

namespace Vulindlela.Booking.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/branches")]
    public class BranchesController : ControllerBase
    {
        private readonly IBranchRepository _branches;
        public BranchesController(IBranchRepository branches) => _branches = branches;

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<BranchDto>>> GetAll(CancellationToken ct)
        {
            var branches = await _branches.GetAllAsync(ct);
            return Ok(branches.Select(b => new BranchDto(b.Id, b.Code, b.Name, b.City, b.Address)).ToList());
        }
    }
}
