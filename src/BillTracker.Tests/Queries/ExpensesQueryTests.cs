﻿using System;
using System.Linq;
using System.Threading.Tasks;
using BillTracker.Commands;
using BillTracker.Entities;
using BillTracker.Queries;
using BillTracker.Shared;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BillTracker.Tests.Queries
{
    public class ExpensesQueryTests : IClassFixture<BillTrackerFixture>
    {
        private readonly BillTrackerWebApplicationFactory _factory;
        private readonly BillTrackerFixture _fixture;

        private readonly User TestUser;
        private readonly ExpenseType TestExpenseType;

        public ExpensesQueryTests(BillTrackerFixture fixture)
        {
            _fixture = fixture;
            _factory = fixture.GetWebApplicationFactory();

            TestUser = _fixture.CreateUser();
            TestExpenseType = _fixture.CreateExpenseType(TestUser.Id);
        }

        [Fact]
        public async Task WhenGetMany_GivenNonExistingUser_ThenError()
        {
            var sut = _factory.Services.GetRequiredService<ExpensesQuery>();

            var result = await sut.GetMany(Guid.NewGuid(), 1);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(CommonErrors.UserNotExist);
        }

        [Fact]
        public async Task WhenGetMany_GivenFilters_ThenReturnsPartOfData()
        {
            var initDate = DateTimeOffset.Now;
            var addExpenseService = _factory.Services.GetRequiredService<AddExpense>();
            var expense1 = await addExpenseService.Handle(new AddExpenseParameters(TestUser.Id, "name", 15, expenseTypeId: TestExpenseType.Id, addedDate: initDate));
            var expense2 = await addExpenseService.Handle(new AddExpenseParameters(TestUser.Id, "name", 16, expenseTypeId: TestExpenseType.Id, addedDate: initDate.AddDays(1)));
            var expense3 = await addExpenseService.Handle(new AddExpenseParameters(TestUser.Id, "name", 17, expenseTypeId: TestExpenseType.Id, addedDate: initDate.AddDays(2)));
            var expense4 = await addExpenseService.Handle(new AddExpenseParameters(TestUser.Id, "name", 18, expenseTypeId: TestExpenseType.Id, addedDate: initDate.AddDays(3)));
            var sut = _factory.Services.GetRequiredService<ExpensesQuery>();

            var result = await sut.GetMany(TestUser.Id, 1, fromDate: initDate, toDate: initDate.AddDays(2));

            result.IsError.Should().BeFalse();
            result.Result.TotalItems.Should().Be(3);
            result.Result.Items.Count().Should().Be(3);
            result.Result.Items.Should().Contain(x => x.Id == expense1.Result.Id);
            result.Result.Items.Should().Contain(x => x.Id == expense2.Result.Id);
            result.Result.Items.Should().Contain(x => x.Id == expense3.Result.Id);
            result.Result.Items.Should().NotContain(x => x.Id == expense4.Result.Id);
        }

        [Fact]
        public async Task WhenGetMany_GivenNoFilters_ThenReturnsAllExpenses()
        {
            var addExpenseService = _factory.Services.GetRequiredService<AddExpense>();
            var expense1 = await addExpenseService.Handle(new AddExpenseParameters(TestUser.Id, "name", 15, expenseTypeId: TestExpenseType.Id, addedDate: DateTimeOffset.Now));
            var expense2 = await addExpenseService.Handle(new AddExpenseParameters(TestUser.Id, "name", 16, expenseTypeId: TestExpenseType.Id, addedDate: DateTimeOffset.Now.AddDays(1)));
            var expense3 = await addExpenseService.Handle(new AddExpenseParameters(TestUser.Id, "name", 17, expenseTypeId: TestExpenseType.Id, addedDate: DateTimeOffset.Now.AddDays(2)));
            var expense4 = await addExpenseService.Handle(new AddExpenseParameters(TestUser.Id, "name", 18, expenseTypeId: TestExpenseType.Id, addedDate: DateTimeOffset.Now.AddDays(3)));
            var sut = _factory.Services.GetRequiredService<ExpensesQuery>();

            var result = await sut.GetMany(TestUser.Id, 1);

            result.IsError.Should().BeFalse();
            result.Result.TotalItems.Should().Be(4);
            result.Result.Items.Count().Should().Be(4);
            result.Result.Items.Should().Contain(x => x.Id == expense1.Result.Id);
            result.Result.Items.Should().Contain(x => x.Id == expense2.Result.Id);
            result.Result.Items.Should().Contain(x => x.Id == expense3.Result.Id);
            result.Result.Items.Should().Contain(x => x.Id == expense4.Result.Id);
        }

        [Fact]
        public async Task WhenGetMany_GivenPaging_ThenReturnsSortedPartOfData()
        {
            var addExpenseService = _factory.Services.GetRequiredService<AddExpense>();
            var expense1 = await addExpenseService.Handle(new AddExpenseParameters(TestUser.Id, "name", 15, expenseTypeId: TestExpenseType.Id, addedDate: DateTimeOffset.Now));
            var expense2 = await addExpenseService.Handle(new AddExpenseParameters(TestUser.Id, "name", 16, expenseTypeId: TestExpenseType.Id, addedDate: DateTimeOffset.Now.AddDays(1)));
            var expense3 = await addExpenseService.Handle(new AddExpenseParameters(TestUser.Id, "name", 17, expenseTypeId: TestExpenseType.Id, addedDate: DateTimeOffset.Now.AddDays(2)));
            var expense4 = await addExpenseService.Handle(new AddExpenseParameters(TestUser.Id, "name", 18, expenseTypeId: TestExpenseType.Id, addedDate: DateTimeOffset.Now.AddDays(3)));
            var sut = _factory.Services.GetRequiredService<ExpensesQuery>();

            var result = await sut.GetMany(TestUser.Id, 2, 2);

            result.IsError.Should().BeFalse();
            result.Result.TotalItems.Should().Be(4);
            result.Result.Items.Count().Should().Be(2);
            result.Result.Items.Should().Contain(x => x.Id == expense1.Result.Id);
            result.Result.Items.Should().Contain(x => x.Id == expense2.Result.Id);
            result.Result.Items.Should().NotContain(x => x.Id == expense3.Result.Id);
            result.Result.Items.Should().NotContain(x => x.Id == expense4.Result.Id);
        }

        [Fact]
        public async Task WhenGetManyExpensesAggregate_GivenFilters_ThenReturnsPartOfData()
        {
            var initDate = DateTimeOffset.Now;
            var addExpenseService = _factory.Services.GetRequiredService<AddExpense>();
            var expense1 = await addExpenseService.Handle(new AddExpenseParameters(TestUser.Id, "name", 10, expenseTypeId: TestExpenseType.Id, addedDate: initDate));
            var expense2 = await addExpenseService.Handle(new AddExpenseParameters(TestUser.Id, "name", 20, expenseTypeId: TestExpenseType.Id, addedDate: initDate.AddDays(1)));
            var expense3 = await addExpenseService.Handle(new AddExpenseParameters(TestUser.Id, "name", 30, expenseTypeId: TestExpenseType.Id, addedDate: initDate.AddDays(2)));
            var expense4 = await addExpenseService.Handle(new AddExpenseParameters(TestUser.Id, "name", 40, expenseTypeId: TestExpenseType.Id, addedDate: initDate.AddDays(3)));
            var sut = _factory.Services.GetRequiredService<ExpensesQuery>();

            var result = await sut.GetManyExpensesAggregate(TestUser.Id, 1, fromDate: initDate, toDate: initDate.AddDays(2));

            result.IsError.Should().BeFalse();
            result.Result.TotalItems.Should().Be(3);
            result.Result.Items.Count().Should().Be(3);
            result.Result.Items.Should().Contain(agg => agg.Expenses.Any(x => x.Id == expense1.Result.Id) && agg.TotalAmount == 10);
            result.Result.Items.Should().Contain(agg => agg.Expenses.Any(x => x.Id == expense2.Result.Id) && agg.TotalAmount == 20);
            result.Result.Items.Should().Contain(agg => agg.Expenses.Any(x => x.Id == expense3.Result.Id) && agg.TotalAmount == 30);
            result.Result.Items.All(agg => !agg.Expenses.Any(x => x.Id == expense4.Result.Id)).Should().BeTrue();
        }

        [Fact]
        public async Task WhenGetExpensesAggregateThenReturnsProperValues()
        {
            var expense = _fixture.CreateExpense(TestUser.Id, "name");
            var sut = _factory.Services.GetRequiredService<ExpensesQuery>();

            var result = await sut.GetExpensesAggregate(expense.AggregateId);

            result.Should().NotBeNull();
            result.UserId.Should().Be(TestUser.Id);
            result.Name.Should().Be("name");
            result.IsDraft.Should().BeFalse();
            result.Expenses.Should().ContainSingle();
            result.Bills.Should().BeEmpty();
        }
    }
}
