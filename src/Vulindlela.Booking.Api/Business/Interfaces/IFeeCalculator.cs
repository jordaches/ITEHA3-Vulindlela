namespace Vulindlela.Booking.Api.Business.Interfaces
{
    public interface IFeeCalculator
    {
        bool TryCalculate(string serviceType, out decimal fee);
    }
}
