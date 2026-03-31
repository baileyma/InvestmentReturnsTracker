namespace InvTracker.Models
{
    public class Account
    {
        public int Id { get; set; }

        public required string Name { get; set; }
    }

    public class AccountReturn
    {
        public required int AccountId { get; set; }
        public required string? AccountName { get; set; }

        public required Dictionary<int, BalanceAndXIRR> BalancesAndReturnsByYear { get; set; }

        public double CumulativeReturn => GetCumulativeReturn();


        public double GetCumulativeReturn()
        {
            double index = 100;

            foreach (var yearlyReturn in BalancesAndReturnsByYear.Values.Select(x => x.XIRR))
            {
                index = index * (1 + yearlyReturn / 100);
            }

            return index;
        }
    }

    public class AccountToAdd
    {
        public required string Name { get; set; }
    }
}
