namespace Vulindlela.Booking.Api.Data.Entities;

public class CitizenAccount
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string IdNumber { get; set; } = string.Empty;
    public string CellNumber { get; set; } = string.Empty;

    public string Role { get; set; } = "Citizen";      // "Citizen" or "Official"
    public byte[]? PasswordHash { get; set; }
    public byte[]? PasswordSalt { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}