using OrderManagementSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Application.DTOs
{
    public class OrderResponse
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemResponse> Items { get; set; }
    }
}
