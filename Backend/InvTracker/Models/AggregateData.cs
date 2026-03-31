namespace InvTracker.Models
{
    public class AggregateData
    {
        public double XIRR { get; set; }

        public double NetDeposits { get; set; }
        
        public double TotalDeposits { get; set; }

        public double TotalWithdrawals { get; set; }

        public double StartBalance { get; set; }
        
        public double EndBalance { get; set; }

        public double PercentChange => Math.Round(((EndBalance / StartBalance * 100) - 100), 2);

        public double PercentageDeposits => Math.Round(((TotalDeposits / StartBalance * 100)), 2);

        public double PercentageWithdrawals => Math.Round(((TotalWithdrawals / StartBalance * 100)), 2);

        public double PercentageNetDeposits => Math.Round(((NetDeposits / StartBalance * 100)), 2);
    }
}
