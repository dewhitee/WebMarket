﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebMarket.Models
{
    public class AddProductViewModel : Product
    {
        public IFormFile ZipFile { get; set; }
    }
}