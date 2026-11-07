using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vulindlela.Booking.Api.Business.Security;
using Vulindlela.Booking.Api.Data;
using Vulindlela.Shared.Contracts;

namespace Vulindlela.Booking.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly BookingDbContext _db;
    private readonly IPasswordHasher _hasher;

    public AuthController(BookingDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    /// Called only by the Portal. Requires a valid X-Internal-Key.
    [Authorize]
    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifyPasswordRequest req, CancellationToken ct)
    {
        var account = await _db.Citizens.FirstOrDefaultAsync(c => c.IdNumber == req.IdNumber, ct);

        if (account?.PasswordHash is null || account.PasswordSalt is null ||
            !_hasher.Verify(req.Password, account.PasswordHash, account.PasswordSalt))
        {
            return Unauthorized(new ProblemDto("bad_credentials", "Invalid ID number or password."));
        }

        return Ok(new VerifyPasswordResponse(
            account.Id, account.FullName, account.Role, account.IdNumber));
    }
}