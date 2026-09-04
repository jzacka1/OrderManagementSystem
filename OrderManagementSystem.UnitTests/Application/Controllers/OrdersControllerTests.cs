using Microsoft.AspNetCore.Mvc;
using Moq;
using OrderManagementSystem.API.Controllers;
using OrderManagementSystem.Application.DTOs;
using OrderManagementSystem.Application.Interfaces;
using OrderManagementSystem.Application.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace OrderManagementSystem.UnitTests.Application.Controllers
{
    public class OrdersControllerTests
    {
        [Fact]
        public async Task CreateOrder_WithValidRequest_ReturnsCreatedAtAction()
        {
            // Arrange
            var repository = new Mock<IOrderRepository>();

            var service = new Mock<IOrderService>();

            var orderId = Guid.NewGuid();

            var request = new CreateOrderRequest
            {
                CustomerName = "James Zacka",
                Email = "james@example.com",
                Items = new List<OrderItemRequest>
                {
                    new OrderItemRequest
                    {
                        ProductId = "P001",
                        ProductName = "Laptop",
                        Quantity = 1,
                        UnitPrice = 1000m
                    }
                }
            };

            var expectedResponse = new OrderResponse
            {
                Id = orderId,
                CustomerName = request.CustomerName,
                Email = request.Email
            };

            service
                .Setup(x => x.CreateOrderAsync(
                    request,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var controller = new OrdersController(service.Object);

            // Act
            var result = await controller.CreateOrder(
                request,
                CancellationToken.None);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(
                result.Result);

            Assert.Equal(
                nameof(OrdersController.GetOrderById),
                createdResult.ActionName);

            Assert.Equal(
                orderId,
                createdResult.RouteValues["id"]);

            Assert.Same(
                expectedResponse,
                createdResult.Value);

            service.Verify(
                x => x.CreateOrderAsync(
                    request,
                    CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task GetOrderById_WhenOrderExists_ReturnsOk()
        {
            // Arrange
            var service = new Mock<IOrderService>();

            var orderId = Guid.NewGuid();

            var expectedResponse = new OrderResponse
            {
                Id = orderId,
                CustomerName = "James Zacka",
                Email = "james@example.com"
            };

            service
                .Setup(x => x.GetOrderByIdAsync(
                    orderId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var controller = new OrdersController(service.Object);

            // Act
            var result = await controller.GetOrderById(
                orderId,
                CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(
                result.Result);

            Assert.Same(
                expectedResponse,
                okResult.Value);

            service.Verify(
                x => x.GetOrderByIdAsync(
                    orderId,
                    CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task GetOrderById_WhenOrderDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            var service = new Mock<IOrderService>();

            var orderId = Guid.NewGuid();

            service
                .Setup(x => x.GetOrderByIdAsync(
                    orderId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((OrderResponse?)null);

            var controller = new OrdersController(service.Object);

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => controller.GetOrderById(
                    orderId,
                    CancellationToken.None));

            // Assert
            Assert.Equal(
                $"Order with ID '{orderId}' was not found.",
                exception.Message);

            service.Verify(
                x => x.GetOrderByIdAsync(
                    orderId,
                    CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task GetOrders_WhenOrdersExist_ReturnsOk()
        {
            // Arrange
            var service = new Mock<IOrderService>();

            var expectedOrders = new List<OrderResponse>
            {
                new OrderResponse
                {
                    Id = Guid.NewGuid(),
                    CustomerName = "James Zacka",
                    Email = "james@example.com"
                },
                new OrderResponse
                {
                    Id = Guid.NewGuid(),
                    CustomerName = "John Smith",
                    Email = "john@example.com"
                }
            };

            service
                .Setup(x => x.GetOrdersAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedOrders);

            var controller = new OrdersController(service.Object);

            // Act
            var result = await controller.GetOrders(
                CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(
                result.Result);

            Assert.Same(
                expectedOrders,
                okResult.Value);

            service.Verify(
                x => x.GetOrdersAsync(
                    CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task GetOrders_WhenNoOrdersExist_ReturnsOkWithEmptyList()
        {
            // Arrange
            var service = new Mock<IOrderService>();

            var expectedOrders = new List<OrderResponse>();

            service
                .Setup(x => x.GetOrdersAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedOrders);

            var controller = new OrdersController(service.Object);

            // Act
            var result = await controller.GetOrders(
                CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(
                result.Result);

            var returnedOrders = Assert.IsType<List<OrderResponse>>(
                okResult.Value);

            Assert.Empty(returnedOrders);

            service.Verify(
                x => x.GetOrdersAsync(
                    CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task ProcessOrder_WhenServiceSucceeds_ReturnsNoContent()
        {
            // Arrange
            var service = new Mock<IOrderService>();

            var orderId = Guid.NewGuid();

            service
                .Setup(x => x.ProcessOrderAsync(
                    orderId,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var controller = new OrdersController(service.Object);

            // Act
            var result = await controller.ProcessOrder(
                orderId,
                CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(result);

            service.Verify(
                x => x.ProcessOrderAsync(
                    orderId,
                    CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task ProcessOrder_WhenOrderDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            var service = new Mock<IOrderService>();

            var orderId = Guid.NewGuid();

            service
                .Setup(x => x.ProcessOrderAsync(
                    orderId,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(
                    new KeyNotFoundException(
                        $"Order with ID '{orderId}' was not found."));

            var controller = new OrdersController(service.Object);

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => controller.ProcessOrder(
                    orderId,
                    CancellationToken.None));

            // Assert
            Assert.Equal(
                $"Order with ID '{orderId}' was not found.",
                exception.Message);

            service.Verify(
                x => x.ProcessOrderAsync(
                    orderId,
                    CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task CompleteOrder_WhenServiceSucceeds_ReturnsNoContent()
        {
            // Arrange
            var service = new Mock<IOrderService>();

            var orderId = Guid.NewGuid();

            service
                .Setup(x => x.CompleteOrderAsync(
                    orderId,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var controller = new OrdersController(service.Object);

            // Act
            var result = await controller.CompleteOrder(
                orderId,
                CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(result);

            service.Verify(
                x => x.CompleteOrderAsync(
                    orderId,
                    CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task CompleteOrder_WhenOrderDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            var service = new Mock<IOrderService>();

            var orderId = Guid.NewGuid();

            service
                .Setup(x => x.CompleteOrderAsync(
                    orderId,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(
                    new KeyNotFoundException(
                        $"Order with ID '{orderId}' was not found."));

            var controller = new OrdersController(service.Object);

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => controller.CompleteOrder(
                    orderId,
                    CancellationToken.None));

            // Assert
            Assert.Equal(
                $"Order with ID '{orderId}' was not found.",
                exception.Message);

            service.Verify(
                x => x.CompleteOrderAsync(
                    orderId,
                    CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task CancelOrder_WhenServiceSucceeds_ReturnsNoContent()
        {
            // Arrange
            var service = new Mock<IOrderService>();

            var orderId = Guid.NewGuid();

            service
                .Setup(x => x.CancelOrderAsync(
                    orderId,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var controller = new OrdersController(service.Object);

            // Act
            var result = await controller.CancelOrder(
                orderId,
                CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(result);

            service.Verify(
                x => x.CancelOrderAsync(
                    orderId,
                    CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task CancelOrder_WhenOrderDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            var service = new Mock<IOrderService>();

            var orderId = Guid.NewGuid();

            service
                .Setup(x => x.CancelOrderAsync(
                    orderId,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(
                    new KeyNotFoundException(
                        $"Order with ID '{orderId}' was not found."));

            var controller = new OrdersController(service.Object);

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => controller.CancelOrder(
                    orderId,
                    CancellationToken.None));

            // Assert
            Assert.Equal(
                $"Order with ID '{orderId}' was not found.",
                exception.Message);

            service.Verify(
                x => x.CancelOrderAsync(
                    orderId,
                    CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task DeleteOrder_WhenServiceSucceeds_ReturnsNoContent()
        {
            // Arrange
            var service = new Mock<IOrderService>();

            var orderId = Guid.NewGuid();

            service
                .Setup(x => x.DeleteOrderAsync(
                    orderId,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var controller = new OrdersController(service.Object);

            // Act
            var result = await controller.DeleteOrder(
                orderId,
                CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(result);

            service.Verify(
                x => x.DeleteOrderAsync(
                    orderId,
                    CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task DeleteOrder_WhenOrderDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            var service = new Mock<IOrderService>();

            var orderId = Guid.NewGuid();

            service
                .Setup(x => x.DeleteOrderAsync(
                    orderId,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(
                    new KeyNotFoundException(
                        $"Order with ID '{orderId}' was not found."));

            var controller = new OrdersController(service.Object);

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => controller.DeleteOrder(
                    orderId,
                    CancellationToken.None));

            // Assert
            Assert.Equal(
                $"Order with ID '{orderId}' was not found.",
                exception.Message);

            service.Verify(
                x => x.DeleteOrderAsync(
                    orderId,
                    CancellationToken.None),
                Times.Once);
        }
    }
}
