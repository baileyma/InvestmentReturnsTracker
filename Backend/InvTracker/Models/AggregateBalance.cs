namespace InvTracker.Models
{
    public class AggregateBalance
    {
        public double EndBalance { get; set; }

        public double StartBalance { get; set; }

        public double PercentChange => Math.Round(((EndBalance/StartBalance * 100) - 100), 2);
    }
}
