namespace Vulindlela.Booking.Api.Data.Entities
{
    public class Branch
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public ICollection<Slot> Slots { get; set; } = new List<Slot>();
    }
}
