using OrderManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Application.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

        Task<List<Order>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task AddAsync(
            Order order,
            CancellationToken cancellationToken = default);

        Task SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
