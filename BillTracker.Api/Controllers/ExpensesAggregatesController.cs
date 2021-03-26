﻿using System;
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
    [Route("expenses/aggregates")]
    public class ExpensesAggregatesController : ControllerBase
    {
        private readonly SaveExpenseAggregate _saveExpenseAggregate;
        private readonly ExpensesQuery _expensesQuery;

        public ExpensesAggregatesController(SaveExpenseAggregate saveExpenseAggregate, ExpensesQuery expensesQuery)
        {
            _saveExpenseAggregate = saveExpenseAggregate;
            _expensesQuery = expensesQuery;
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ExpenseAggregateModel>> Put(SaveExpenseAggregateRequest request)
        {
            var result = await _saveExpenseAggregate.Handle(
                new SaveExpenseAggregateParameters(
                    request.Id,
                    this.GetUserId(),
                    request.Name,
                    request.AddedDate,
                    request.IsDraft));

            return result.Match<ActionResult>(
                success => Ok(success),
                error => BadRequest(error));
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ExpenseAggregateModel>> Get(Guid id)
        {
            var result = await _expensesQuery.GetExpensesAggregate(id);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PagedResult<ExpenseAggregateModel>>> GetMany(
            [FromQuery][Required][Range(1, int.MaxValue)] int? pageNumber,
            [FromQuery][Required][Range(5, 50)] int? pageSize,
            [FromQuery] DateTimeOffset? fromDate,
            [FromQuery] DateTimeOffset? toDate)
        {
            var result = await _expensesQuery.GetManyExpensesAggregate(
                this.GetUserId(),
                pageNumber: pageNumber.Value,
                pageSize: pageSize.Value,
                fromDate: fromDate,
                toDate: toDate);

            return result.Match<ActionResult>(
                success => Ok(success),
                error => BadRequest(error));
        }
    }
}
