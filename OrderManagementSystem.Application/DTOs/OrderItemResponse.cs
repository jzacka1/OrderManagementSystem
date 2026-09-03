using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Application.DTOs
{
    public class OrderItemResponse
    {
        public Guid Id { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }
}
