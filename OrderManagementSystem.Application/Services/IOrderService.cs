using OrderManagementSystem.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Application.Services
{
    public interface IOrderService
    {
        Task<OrderResponse> CreateOrderAsync(
            CreateOrderRequest request,
            CancellationToken cancellationToken = default);

        Task<OrderResponse?> GetOrderByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<List<OrderResponse>> GetOrdersAsync(
            CancellationToken cancellationToken = default);

        Task ProcessOrderAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task CompleteOrderAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task CancelOrderAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task DeleteOrderAsync(
            Guid id,
            CancellationToken cancellationToken = default);
    }
}
