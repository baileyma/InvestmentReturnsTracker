namespace InvTracker.Models
{
    public class MinimalBalance
    {
        public int Year { get; set; }

        public double StartingBalance { get; set; }

        public double EndBalance { get; set; }

        public int Month { get; set; }

        public int Day { get; set; }

        public DateTime EndDate => new DateTime(Year, Month, Day);
    }

    public class Balance : MinimalBalance
    {
        public int Id { get; set; }

        public int AccountId { get; set; }
    }

    public class BalanceAndXIRR : MinimalBalance
    {
        public double XIRR { get; set; }

        public double PercentChange => Math.Round(((EndBalance / StartingBalance * 100) - 100), 2);

    }
}
