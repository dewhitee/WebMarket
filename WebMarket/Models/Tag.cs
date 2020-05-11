﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebMarket.Models
{
    [Serializable]
    public class Tag
    {
        [Key]
        public int ID { get; set; }

        [Required, MaxLength(32)]
        public string Text { get; set; }
        
        public int TypeId { get; set; }

        [Required, MaxLength(32)]
        public string ProductID { get; set; }
    }
}
