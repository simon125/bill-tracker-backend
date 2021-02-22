﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BillTracker.Api.Models;
using BillTracker.Commands;
using BillTracker.Models;
using BillTracker.Queries;
using BillTracker.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BillTracker.Api.Controllers
{
    [ApiController]
    [Route("expenses")]
    public class ExpensesController : ControllerBase
    {
        private readonly IAddExpense _addExpenseHandler;
        private readonly IExpensesQuery _expensesQuery;

        public ExpensesController(
            IAddExpense addExpenseHandler,
            IExpensesQuery expensesQuery)
        {
            _addExpenseHandler = addExpenseHandler;
            _expensesQuery = expensesQuery;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PagedResult<ExpenseModel>>> GetMany(
            [FromQuery][Required][Range(1, int.MaxValue)] int? pageNumber,
            [FromQuery][Required][Range(5, 50)] int? pageSize,
            [FromQuery] DateTimeOffset? fromDate,
            [FromQuery] DateTimeOffset? toDate)
        {
            var result = await _expensesQuery.GetMany(this.GetUserId(),
                pageNumber: pageNumber.Value,
                pageSize: pageSize.Value,
                fromDate: fromDate,
                toDate: toDate);

            return result.Match<ActionResult>(
                success => Ok(success),
                error => BadRequest(error));
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ExpenseModel>> Get(Guid id)
        {
            var result = await _expensesQuery.GetById(id);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<ExpenseModel>>> Post(AddExpenseRequest request)
        {
            var result = await _addExpenseHandler.Handle(
                new AddExpenseParameters(this.GetUserId(), request.Name, request.Amount, request.AddedAt));

            return result.Match<ActionResult>(
                success => CreatedAtAction(nameof(Get), new { id = result.Result.Id }, result.Result),
                error => BadRequest(error));
        }
    }
}
