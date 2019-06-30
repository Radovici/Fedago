﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataBazrPeer.Models
{
    public class CustomerModel
    {
        public string Id { get; set; }

        public bool ShowId => !string.IsNullOrEmpty(Id);
    }
}
