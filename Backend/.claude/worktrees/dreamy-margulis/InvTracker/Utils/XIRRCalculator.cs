using InvTracker.Models;
using Excel.FinancialFunctions;

namespace InvTracker.Utils
{
    public interface IXIRRCalculator
    {
        double CalculateXirr(MinimalBalance balance, IEnumerable<Payment> payments, int year);

    }

    public class XIRRCalculator : IXIRRCalculator
    {
        public double CalculateXirr(MinimalBalance balance, IEnumerable<Payment> payments, int year)
        {
            var mappedInput = MapCashFlowAndDates(balance, payments, year);

            var xirr = Financial.XIrr(mappedInput.cashflow, mappedInput.dates);

            int daysElapsed = (mappedInput.dates[^1] - mappedInput.dates[0]).Days;

            var yearToDateReturn = Math.Pow(1 + xirr, daysElapsed / 365.0) - 1;

            return Math.Round(yearToDateReturn * 100, 2);
        }

        private (List<DateTime> dates, List<double> cashflow) MapCashFlowAndDates(MinimalBalance balance, IEnumerable<Payment> payments, int year)
        {
            var cashflow = new List<double>()
            {
                -balance.StartingBalance
            };

            var dates = new List<DateTime>()
            {
                new DateTime(year, 1, 1)
            };

            foreach (var payment in payments)
            {
                cashflow.Add(-payment.Amount);
                var date = new DateTime(year, payment.Date.Month, payment.Date.Day);
                dates.Add(date);
            }

            cashflow.Add(balance.EndBalance);
            dates.Add(balance.EndDate);

            return (dates, cashflow);
        }

        
    }
}
