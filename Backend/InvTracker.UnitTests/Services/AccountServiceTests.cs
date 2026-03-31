using System;
using System.Collections.Generic;
using System.Text;
using AutoFixture;
using InvTracker.Models;
using InvTracker.Repositories;
using InvTracker.Services;
using InvTracker.Utils;
using NSubstitute;

namespace InvTracker.UnitTests.Services;


public class AccountServiceTests
{
      private readonly IBalancesRepository _balancesRepository;
      private readonly IPaymentsRepository _paymentsRepository;
      private readonly IXIRRCalculator _XIRRCalculator;
      private readonly Fixture _fixture;

  public AccountServiceTests()
  {
        _balancesRepository = Substitute.For<IBalancesRepository>();
        _paymentsRepository = Substitute.For<IPaymentsRepository>();
        _XIRRCalculator = Substitute.For<IXIRRCalculator>();
        _fixture = new();
        _fixture.Customize<DateOnly>(composer => composer.FromFactory<DateTime>(DateOnly.FromDateTime));
  }
    
  [Fact]
  public async Task ReturnAccountDetails_GivenValidInput()
  {
        _balancesRepository.GetBalanceByIdAndYearAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>()).Returns(_fixture.Create<Balance>());
        _paymentsRepository.GetPaymentsByIdAndYearAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>()).Returns(_fixture.Create<List<Payment>>());
        _XIRRCalculator.CalculateXirr(Arg.Any<Balance>(), _fixture.Create<List<Payment>>(), Arg.Any<int>()).Returns(6.55d);

        var sut = new AccountService(_XIRRCalculator, _balancesRepository, _paymentsRepository);

        var result = await sut.GetAccountDetailsByYear("testUserId", 1, 2024);

        Assert.NotNull(result);
    
  }
}

