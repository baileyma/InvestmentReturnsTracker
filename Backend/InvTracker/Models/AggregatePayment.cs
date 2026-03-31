namespace InvTracker.Models
{
    public class AggregatePayment
    {
        public double NetDeposits { get; set; }

        public double TotalDeposits { get; set; }

        public double TotalWithdrawals { get; set; }

        public double StartingBalance { get; set; }

        public double PercentageDeposits => Math.Round(((TotalDeposits / StartingBalance * 100)), 2);

        public double PercentageWithdrawals => Math.Round(((TotalWithdrawals / StartingBalance * 100)), 2);

        public double PercentageNetDeposits => Math.Round(((NetDeposits / StartingBalance * 100)), 2);
    }
}
