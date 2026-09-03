using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Application.Orders.CreateOrder
{
    public sealed record CreateOrderCommand(
        string CustomerName,
        string Email,
        IEnumerable<CreateOrderItem> Items);

    public sealed record CreateOrderItem(
        string ProductId,
        string ProductName,
        int Quantity,
        decimal UnitPrice);
}
