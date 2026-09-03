using OrderManagementSystem.Application.Interfaces;
using OrderManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Application.Orders.CreateOrder
{
    public sealed class CreateOrderHandler
    {
        private readonly IOrderRepository _orderRepository;

        public CreateOrderHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<Guid> Handle(
            CreateOrderCommand command,
            CancellationToken cancellationToken = default)
        {
            var items = command.Items
                .Select(item => new OrderItem(
                    item.ProductId,
                    item.ProductName,
                    item.Quantity,
                    item.UnitPrice))
                .ToList();

            var order = new Order(
                command.CustomerName,
                command.Email,
                items);

            await _orderRepository.AddAsync(
                order,
                cancellationToken);

            return order.Id;
        }
    }
}
