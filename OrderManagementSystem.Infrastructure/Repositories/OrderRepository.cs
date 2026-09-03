using Microsoft.EntityFrameworkCore;
using OrderManagementSystem.Application.Interfaces;
using OrderManagementSystem.Domain.Entities;
using OrderManagementSystem.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _context;

        public OrderRepository(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<Order?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(
                    o => o.Id == id && !o.IsDeleted,
                    cancellationToken);
        }

        public async Task<List<Order>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .Where(o => !o.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(
            Order order,
            CancellationToken cancellationToken = default)
        {
            await _context.Orders.AddAsync(
                order,
                cancellationToken);
        }

        public async Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
