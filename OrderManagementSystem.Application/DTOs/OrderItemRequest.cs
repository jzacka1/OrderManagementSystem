using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OrderManagementSystem.Application.DTOs
{
    public class OrderItemRequest
    {
        [Required]
        public string ProductId { get; set; }

        [Required]
        public string ProductName { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
        public decimal UnitPrice { get; set; }
    }
}
