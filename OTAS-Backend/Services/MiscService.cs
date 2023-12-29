using OTAS.Interfaces.IService;
using OTAS.Models;
using System.Text;

namespace OTAS.Services
{
    public class MiscService : IMiscService
    {
        public decimal CalculateExpensesEstimatedTotal(List<Expense> expenses)
        {
            decimal estimatedTotal = 0;
            foreach (Expense expense in expenses)
            {
                estimatedTotal += expense.EstimatedFee;
            }

            return estimatedTotal;
        }

        public decimal CalculateTripEstimatedFee(Trip trip)
        {
            decimal estimatedFee = 0;
            const decimal MILEAGE_ALLOWANCE = 2.5m; // 2.5DH PER KM
            if (trip.Unit == "KM")
            {
                estimatedFee += trip.HighwayFee + (trip.Value * MILEAGE_ALLOWANCE);
            }
            else
            {
                estimatedFee += trip.Value;
            }


            return estimatedFee;
        }

        public decimal CalculateTripsEstimatedTotal(List<Trip> trips)
        {
            decimal estimatedTotal = 0;
            const decimal MILEAGE_ALLOWANCE = 2.5m; // 2.5DH PER KM
            foreach (Trip trip in trips)
            {
                if (trip.Unit == "KM")
                {
                    estimatedTotal += trip.HighwayFee + (trip.Value * MILEAGE_ALLOWANCE);
                }
                else
                {
                    estimatedTotal += trip.Value;
                }
            }

            return estimatedTotal;
        }


        public int GenerateRandomNumber(int length)
        {
            int[] _decimalNumbers = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Random _random = new();

            var sb = new StringBuilder(length);

            for (int i = 0; i < length; i++)
                sb.Append(_decimalNumbers[_random.Next(10)]);

            return int.Parse(sb.ToString());
        }

        public string GenerateRandomString(int length)
        {
            char[] _base62chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();

            Random _random = new();

            var sb = new StringBuilder(length);

            for (int i = 0; i < length; i++)
                sb.Append(_base62chars[_random.Next(36)]); //Base36 will only include numbers and capital letters, if you want to include small letters change it to 62

            return sb.ToString();
        }
    }
}
