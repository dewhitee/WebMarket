﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebMarket.Controllers
{
    public class ComparisonController : Controller
    {
        public IActionResult Comparison()
        {
            return View();
        }
    }
}