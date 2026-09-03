using Moq;
using OrderManagementSystem.Application.Interfaces;
using OrderManagementSystem.Application.Orders.CreateOrder;
using OrderManagementSystem.Domain.Entities;

namespace OrderManagementSystem.UnitTests.Application.Orders.CreateOrder
{
    public class CreateOrderHandlerTests
    {
        [Fact]
        public async Task Handle_Should_Create_Order_And_Save_It()
        {
            // Arrange
            var repository = new Mock<IOrderRepository>();

            var handler = new CreateOrderHandler(repository.Object);

            var command = new CreateOrderCommand(
                "James",
                "james@example.com",
                new[]
                {
                new CreateOrderItem(
                    "P001",
                    "Product 1",
                    2,
                    50)
                });

            // Act
            var orderId = await handler.Handle(command);

            // Assert
            Assert.NotEqual(Guid.Empty, orderId);

            repository.Verify(
                x => x.AddAsync(
                    It.Is<Order>(order =>
                        order.CustomerName == "James" &&
                        order.Email == "james@example.com" &&
                        order.Items.Count == 1 &&
                        order.TotalAmount == 100),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Throw_When_CustomerName_Is_Empty()
        {
            // Arrange
            var repository = new Mock<IOrderRepository>();
            var handler = new CreateOrderHandler(repository.Object);

            var command = new CreateOrderCommand(
                "",
                "james@example.com",
                new[]
                {
            new CreateOrderItem(
                "P001",
                "Product 1",
                2,
                50)
                });

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => handler.Handle(command));

            repository.Verify(
                x => x.AddAsync(
                    It.IsAny<Order>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_Should_Create_Order_With_Multiple_Items()
        {
            // Arrange
            var repository = new Mock<IOrderRepository>();
            var handler = new CreateOrderHandler(repository.Object);

            var command = new CreateOrderCommand(
                "James",
                "james@example.com",
                new[]
                {
            new CreateOrderItem(
                "P001",
                "Product 1",
                2,
                50),

            new CreateOrderItem(
                "P002",
                "Product 2",
                1,
                30)
                });

            // Act
            await handler.Handle(command);

            // Assert
            repository.Verify(
                x => x.AddAsync(
                    It.Is<Order>(order =>
                        order.Items.Count == 2 &&
                        order.TotalAmount == 130),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
