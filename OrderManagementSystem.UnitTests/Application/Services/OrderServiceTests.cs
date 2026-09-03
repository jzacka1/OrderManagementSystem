using Moq;
using OrderManagementSystem.Application.DTOs;
using OrderManagementSystem.Application.Interfaces;
using OrderManagementSystem.Application.Services;
using OrderManagementSystem.Domain.Entities;
using OrderManagementSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.UnitTests.Application.Services
{
    public class OrderServiceTests
    {
        [Fact]
        public async Task CreateOrderAsync_WithValidRequest_ReturnsOrderResponse()
        {
            // Arrange
            var repository = new Mock<IOrderRepository>();

            var service = new OrderService(repository.Object);

            var request = new CreateOrderRequest
            {
                CustomerName = "James",
                Email = "james@example.com",
                Items = new List<OrderItemRequest>
                {
                    new OrderItemRequest
                    {
                        ProductId = "P001",
                        ProductName = "Keyboard",
                        Quantity = 2,
                        UnitPrice = 50m
                    }
                }
            };

            // Act
            var result = await service.CreateOrderAsync(request);

            // Assert
            Assert.NotNull(result);

            Assert.NotEqual(
                Guid.Empty,
                result.Id);

            Assert.Equal(
                "James",
                result.CustomerName);

            Assert.Equal(
                "james@example.com",
                result.Email);

            Assert.Equal(
                OrderStatus.Pending,
                result.Status);

            Assert.Single(result.Items);

            Assert.Equal(
                100m,
                result.TotalAmount);

            repository.Verify(
                x => x.AddAsync(
                    It.IsAny<Order>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            repository.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_WithNullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var repository = new Mock<IOrderRepository>();

            var service = new OrderService(repository.Object);

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                () => service.CreateOrderAsync(null!));

            // Assert
            Assert.Equal(
                "request",
                exception.ParamName);

            repository.Verify(
                x => x.AddAsync(
                    It.IsAny<Order>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            repository.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateOrderAsync_WithEmptyItems_ThrowsArgumentException()
        {
            // Arrange
            var repository = new Mock<IOrderRepository>();

            var service = new OrderService(repository.Object);

            var request = new CreateOrderRequest
            {
                CustomerName = "James",
                Email = "james@example.com",
                Items = new List<OrderItemRequest>()
            };

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => service.CreateOrderAsync(request));

            // Assert
            Assert.Equal(
                "At least one order item is required.",
                exception.Message);

            repository.Verify(
                x => x.AddAsync(
                    It.IsAny<Order>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            repository.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateOrderAsync_WithNullItems_ThrowsArgumentException()
        {
            // Arrange
            var repository = new Mock<IOrderRepository>();

            var service = new OrderService(repository.Object);

            var request = new CreateOrderRequest
            {
                CustomerName = "James",
                Email = "james@example.com",
                Items = null!
            };

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => service.CreateOrderAsync(request));

            // Assert
            Assert.Equal(
                "At least one order item is required.",
                exception.Message);

            repository.Verify(
                x => x.AddAsync(
                    It.IsAny<Order>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            repository.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task GetOrderByIdAsync_WhenOrderExists_ReturnsOrderResponse()
        {
            // Arrange
            var repository = new Mock<IOrderRepository>();

            var order = new Order(
                "James",
                "james@example.com",
                new List<OrderItem>
                {
            new OrderItem(
                "P001",
                "Keyboard",
                2,
                50m)
                });

            repository
                .Setup(x => x.GetByIdAsync(
                    order.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            var service = new OrderService(repository.Object);

            // Act
            var result = await service.GetOrderByIdAsync(order.Id);

            // Assert
            Assert.NotNull(result);

            Assert.Equal(
                order.Id,
                result.Id);

            Assert.Equal(
                "James",
                result.CustomerName);

            Assert.Equal(
                "james@example.com",
                result.Email);

            Assert.Equal(
                OrderStatus.Pending,
                result.Status);

            Assert.Single(result.Items);

            Assert.Equal(
                "P001",
                result.Items[0].ProductId);

            Assert.Equal(
                "Keyboard",
                result.Items[0].ProductName);

            Assert.Equal(
                2,
                result.Items[0].Quantity);

            Assert.Equal(
                50m,
                result.Items[0].UnitPrice);

            Assert.Equal(
                100m,
                result.TotalAmount);

            repository.Verify(
                x => x.GetByIdAsync(
                    order.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetOrderByIdAsync_WhenOrderDoesNotExist_ReturnsNull()
        {
            // Arrange
            var repository = new Mock<IOrderRepository>();

            var orderId = Guid.NewGuid();

            repository
                .Setup(x => x.GetByIdAsync(
                    orderId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Order?)null);

            var service = new OrderService(repository.Object);

            // Act
            var result = await service.GetOrderByIdAsync(orderId);

            // Assert
            Assert.Null(result);

            repository.Verify(
                x => x.GetByIdAsync(
                    orderId,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetOrdersAsync_WhenOrdersExist_ReturnsMappedOrderResponses()
        {
            // Arrange
            var repository = new Mock<IOrderRepository>();

            var orders = new List<Order>
    {
        new Order(
            "James",
            "james@example.com",
            new List<OrderItem>
            {
                new OrderItem(
                    "P001",
                    "Keyboard",
                    2,
                    50m)
            }),

        new Order(
            "John",
            "john@example.com",
            new List<OrderItem>
            {
                new OrderItem(
                    "P002",
                    "Mouse",
                    1,
                    25m)
            })
    };

            repository
                .Setup(x => x.GetAllAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(orders);

            var service = new OrderService(repository.Object);

            // Act
            var result = await service.GetOrdersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            Assert.Equal(
                "James",
                result[0].CustomerName);

            Assert.Equal(
                "John",
                result[1].CustomerName);

            Assert.Equal(
                100m,
                result[0].TotalAmount);

            Assert.Equal(
                25m,
                result[1].TotalAmount);

            Assert.Single(result[0].Items);
            Assert.Single(result[1].Items);

            repository.Verify(
                x => x.GetAllAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ProcessOrderAsync_WhenOrderExists_ProcessesAndSavesOrder()
        {
            // Arrange
            var repository = new Mock<IOrderRepository>();

            var order = new Order(
                "James",
                "james@example.com",
                new List<OrderItem>
                {
            new OrderItem(
                "P001",
                "Keyboard",
                1,
                50m)
                });

            repository
                .Setup(x => x.GetByIdAsync(
                    order.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            var service = new OrderService(repository.Object);

            // Act
            await service.ProcessOrderAsync(order.Id);

            // Assert
            Assert.Equal(
                OrderStatus.Processing,
                order.Status);

            repository.Verify(
                x => x.GetByIdAsync(
                    order.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            repository.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ProcessOrderAsync_WhenOrderDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            var repository = new Mock<IOrderRepository>();

            var orderId = Guid.NewGuid();

            repository
                .Setup(x => x.GetByIdAsync(
                    orderId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Order?)null);

            var service = new OrderService(repository.Object);

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.ProcessOrderAsync(orderId));

            // Assert
            Assert.Equal(
                $"Order with ID '{orderId}' was not found.",
                exception.Message);

            repository.Verify(
                x => x.GetByIdAsync(
                    orderId,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            repository.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CompleteOrderAsync_WhenOrderIsProcessing_CompletesAndSavesOrder()
        {
            // Arrange
            var repository = new Mock<IOrderRepository>();

            var order = new Order(
                "James",
                "james@example.com",
                new List<OrderItem>
                {
            new OrderItem(
                "P001",
                "Keyboard",
                1,
                50m)
                });

            order.StartProcessing();

            repository
                .Setup(x => x.GetByIdAsync(
                    order.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            var service = new OrderService(repository.Object);

            // Act
            await service.CompleteOrderAsync(order.Id);

            // Assert
            Assert.Equal(
                OrderStatus.Completed,
                order.Status);

            repository.Verify(
                x => x.GetByIdAsync(
                    order.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            repository.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CompleteOrderAsync_WhenOrderDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            var repository = new Mock<IOrderRepository>();

            var orderId = Guid.NewGuid();

            repository
                .Setup(x => x.GetByIdAsync(
                    orderId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Order?)null);

            var service = new OrderService(repository.Object);

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.CompleteOrderAsync(orderId));

            // Assert
            Assert.Equal(
                $"Order with ID '{orderId}' was not found.",
                exception.Message);

            repository.Verify(
                x => x.GetByIdAsync(
                    orderId,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            repository.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CancelOrderAsync_WhenOrderExists_CancelsAndSavesOrder()
        {
            // Arrange
            var repository = new Mock<IOrderRepository>();

            var order = new Order(
                "James",
                "james@example.com",
                new List<OrderItem>
                {
            new OrderItem(
                "P001",
                "Keyboard",
                1,
                50m)
                });

            repository
                .Setup(x => x.GetByIdAsync(
                    order.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            var service = new OrderService(repository.Object);

            // Act
            await service.CancelOrderAsync(order.Id);

            // Assert
            Assert.Equal(
                OrderStatus.Cancelled,
                order.Status);

            repository.Verify(
                x => x.GetByIdAsync(
                    order.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            repository.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CancelOrderAsync_WhenOrderDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            var repository = new Mock<IOrderRepository>();

            var orderId = Guid.NewGuid();

            repository
                .Setup(x => x.GetByIdAsync(
                    orderId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Order?)null);

            var service = new OrderService(repository.Object);

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.CancelOrderAsync(orderId));

            // Assert
            Assert.Equal(
                $"Order with ID '{orderId}' was not found.",
                exception.Message);

            repository.Verify(
                x => x.GetByIdAsync(
                    orderId,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            repository.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteOrderAsync_WhenOrderExists_SoftDeletesAndSavesOrder()
        {
            // Arrange
            var repository = new Mock<IOrderRepository>();

            var order = new Order(
                "James",
                "james@example.com",
                new List<OrderItem>
                {
            new OrderItem(
                "P001",
                "Keyboard",
                1,
                50m)
                });

            repository
                .Setup(x => x.GetByIdAsync(
                    order.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            var service = new OrderService(repository.Object);

            // Act
            await service.DeleteOrderAsync(order.Id);

            // Assert
            Assert.True(order.IsDeleted);
            Assert.NotNull(order.DeletedDate);

            repository.Verify(
                x => x.GetByIdAsync(
                    order.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            repository.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteOrderAsync_WhenOrderDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            var repository = new Mock<IOrderRepository>();

            var orderId = Guid.NewGuid();

            repository
                .Setup(x => x.GetByIdAsync(
                    orderId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Order?)null);

            var service = new OrderService(repository.Object);

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.DeleteOrderAsync(orderId));

            // Assert
            Assert.Equal(
                $"Order with ID '{orderId}' was not found.",
                exception.Message);

            repository.Verify(
                x => x.GetByIdAsync(
                    orderId,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            repository.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task GetOrderByIdAsync_PassesCancellationTokenToRepository()
        {
            // Arrange
            var repository = new Mock<IOrderRepository>();

            var order = new Order(
                "James",
                "james@example.com",
                new List<OrderItem>
                {
            new OrderItem(
                "P001",
                "Keyboard",
                1,
                50m)
                });

            var cancellationToken = new CancellationTokenSource().Token;

            repository
                .Setup(x => x.GetByIdAsync(
                    order.Id,
                    cancellationToken))
                .ReturnsAsync(order);

            var service = new OrderService(repository.Object);

            // Act
            var result = await service.GetOrderByIdAsync(
                order.Id,
                cancellationToken);

            // Assert
            Assert.NotNull(result);

            repository.Verify(
                x => x.GetByIdAsync(
                    order.Id,
                    cancellationToken),
                Times.Once);
        }
    }
}
