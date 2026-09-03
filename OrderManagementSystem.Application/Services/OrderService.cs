using OrderManagementSystem.Application.DTOs;
using OrderManagementSystem.Application.Interfaces;
using OrderManagementSystem.Domain.Entities;

namespace OrderManagementSystem.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<OrderResponse> CreateOrderAsync(
            CreateOrderRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var items = request.Items?
                .Select(item => new OrderItem(
                    item.ProductId,
                    item.ProductName,
                    item.Quantity,
                    item.UnitPrice))
                .ToList();

            if (items == null || items.Count == 0)
            {
                throw new ArgumentException(
                    "At least one order item is required.");
            }

            var order = new Order(
                request.CustomerName,
                request.Email,
                items);

            await _orderRepository.AddAsync(
                order,
                cancellationToken);

            await _orderRepository.SaveChangesAsync(
                cancellationToken);

            return MapToResponse(order);
        }

        public async Task<OrderResponse?> GetOrderByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var order = await _orderRepository.GetByIdAsync(
                id,
                cancellationToken);

            return order == null
                ? null
                : MapToResponse(order);
        }

        public async Task<List<OrderResponse>> GetOrdersAsync(
            CancellationToken cancellationToken = default)
        {
            var orders = await _orderRepository.GetAllAsync(
                cancellationToken);

            return orders
                .Select(MapToResponse)
                .ToList();
        }

        public async Task ProcessOrderAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var order = await GetOrderOrThrowAsync(
                id,
                cancellationToken);

            order.StartProcessing();

            await _orderRepository.SaveChangesAsync(
                cancellationToken);
        }

        public async Task CompleteOrderAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var order = await GetOrderOrThrowAsync(
                id,
                cancellationToken);

            order.Complete();

            await _orderRepository.SaveChangesAsync(
                cancellationToken);
        }

        public async Task CancelOrderAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var order = await GetOrderOrThrowAsync(
                id,
                cancellationToken);

            order.Cancel();

            await _orderRepository.SaveChangesAsync(
                cancellationToken);
        }

        public async Task DeleteOrderAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var order = await GetOrderOrThrowAsync(
                id,
                cancellationToken);

            order.SoftDelete();

            await _orderRepository.SaveChangesAsync(
                cancellationToken);
        }

        private async Task<Order> GetOrderOrThrowAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(
                id,
                cancellationToken);

            if (order == null)
            {
                throw new KeyNotFoundException(
                    $"Order with ID '{id}' was not found.");
            }

            return order;
        }

        private static OrderResponse MapToResponse(Order order)
        {
            return new OrderResponse
            {
                Id = order.Id,
                CustomerName = order.CustomerName,
                Email = order.Email,
                Status = order.Status,
                CreatedDate = order.CreatedDate,
                UpdatedDate = order.UpdatedDate,
                TotalAmount = order.TotalAmount,

                Items = order.Items
                    .Select(item => new OrderItemResponse
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        Subtotal = item.Subtotal
                    })
                    .ToList()
            };
        }
    }
}
