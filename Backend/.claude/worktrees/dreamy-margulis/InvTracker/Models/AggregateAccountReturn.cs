namespace InvTracker.Models
{
    public class HomePageData
    {
        public required Dictionary<int, AggregateData> AggregateData { get; set; }

        public required Dictionary<int, AccountReturn> IndividualAccountData { get; set; }
    }
}