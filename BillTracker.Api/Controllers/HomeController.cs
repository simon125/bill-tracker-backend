﻿using Microsoft.AspNetCore.Mvc;

namespace BillTracker.Api.Controllers
{
    [Route("home")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public string Index()
        {
            return "asd";
        }
    }
}