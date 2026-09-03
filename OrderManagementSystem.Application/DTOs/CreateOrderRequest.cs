using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OrderManagementSystem.Application.DTOs
{
    public class CreateOrderRequest
    {
        [Required]
        [MaxLength(200)]
        public string CustomerName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(320)]
        public string Email { get; set; }
        [Required]
        [MinLength(1)]
        public List<OrderItemRequest> Items { get; set; }
    }
}
