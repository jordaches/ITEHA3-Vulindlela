namespace Vulindlela.Booking.Api.Business.Security;

public interface IPasswordHasher
{
    (byte[] Hash, byte[] Salt) Create(string password);
    bool Verify(string password, byte[] hash, byte[] salt);
}