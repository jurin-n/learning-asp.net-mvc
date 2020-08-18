using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class Order
    {
        public string OrderId { get; set; }
        public string OrderDescription { get; set; }
        public List<Item> items { get; set; }
        public string BulkRegistration { get; set; }
        public bool isBulk { get; set; }
    }
}