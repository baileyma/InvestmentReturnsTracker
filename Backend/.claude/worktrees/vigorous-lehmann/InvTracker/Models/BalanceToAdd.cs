namespace InvTracker.Models
{
    public class BalanceToAdd
    {
        public int AccountId { get; set; }

        public double LatestBalance { get; set; }

        public int Month { get; set; }

        public int Day { get; set; }

        public int Year { get; set; }

        public DateTime EndDate => new DateTime(Year, Month, Day);
    }
}
