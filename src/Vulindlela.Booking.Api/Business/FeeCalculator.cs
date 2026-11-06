using Vulindlela.Booking.Api.Business.Interfaces;

namespace Vulindlela.Booking.Api.Business
{
    public sealed class FeeCalculator : IFeeCalculator
    {
        private static readonly Dictionary<string, decimal> BaseFees =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["ID_COLLECTION"] = 0m,
                ["ID_APPLICATION"] = 140m,
                ["PASSPORT"] = 400m,
                ["PASSPORT_URGENT"] = 800m,
                ["BIRTH_CERT"] = 75m,
                ["MARRIAGE_CERT"] = 75m,
                ["DEATH_CERT"] = 75m
            };

        private const decimal PassportSurcharge = 65m;

        public bool TryCalculate(string serviceType, out decimal fee)
        {
            if (!BaseFees.TryGetValue(serviceType, out fee)) return false;

            if (serviceType.Equals("PASSPORT", StringComparison.OrdinalIgnoreCase) ||
                serviceType.Equals("PASSPORT_URGENT", StringComparison.OrdinalIgnoreCase))
                fee += PassportSurcharge;

            return true;
        }
    }
}
