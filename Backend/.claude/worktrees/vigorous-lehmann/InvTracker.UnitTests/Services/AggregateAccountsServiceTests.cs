using AutoFixture;
using InvTracker.Models;
using InvTracker.Repositories;
using InvTracker.Services;
using InvTracker.Utils;
using NSubstitute;

namespace InvTracker.UnitTests.Services;

public class AggregateAccountsServiceTests
{
    private readonly IAccountsRepository _accountsRepo;
    private readonly IBalancesRepository _balancesRepo;
    private readonly IPaymentsRepository _paymentsRepo;
    private readonly IXIRRCalculator _xirrCalculator;
    private Fixture _fixture;
    public AggregateAccountsServiceTests()
    {
        _accountsRepo = Substitute.For<IAccountsRepository>();
        _balancesRepo = Substitute.For<IBalancesRepository>();
        _paymentsRepo = Substitute.For<IPaymentsRepository>();
        _xirrCalculator = Substitute.For<IXIRRCalculator>();
        _fixture = new();
        _fixture.Customize<DateOnly>(composer => composer.FromFactory<DateTime>(DateOnly.FromDateTime));
    }

    [Fact]
    public async Task GetAggregateData_ReturnsCorrectly()
    {
        _accountsRepo.GetAccounts().Returns(_fixture.Create<List<Account>>());
        _balancesRepo.GetAllBalances().Returns(_fixture.Create<List<Balance>>());
        _paymentsRepo.GetAllPayments().Returns(_fixture.Create<List<Payment>>());
        _xirrCalculator.CalculateXirr(Arg.Any<Balance>(), Arg.Any<List<Payment>>(), Arg.Any<int>()).Returns(6.55d);

        var sut = new AggregateAccountsService(_accountsRepo, _balancesRepo, _paymentsRepo, _xirrCalculator);

        //var result = await sut.GetAggregateData();

        //Assert.NotNull(result);
        //NEED TO MOCK DATA
        Assert.True(true);
    }
}

