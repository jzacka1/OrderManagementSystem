using OrderManagementSystem.Domain.Entities;
using OrderManagementSystem.Domain.Enums;
using System.Threading;

namespace OrderManagementSystem.UnitTests.Domain
{
    public class OrderTests
    {
        //CREATION TESTS
        [Fact]
        public void Valid_Order_Starts_As_Pending()
        {
            var order = CreateValidOrder();

            Assert.Equal(OrderStatus.Pending, order.Status);
        }

        [Fact]
        public void Invalid_CustomerName_Should_Throw_Exception()
        {
            var item = new OrderItem(
                "P001",
                "Product 1",
                2,
                50);

            Assert.Throws<ArgumentException>(() => new Order(
                null,
                "james@example.com",
                new List<OrderItem> { item }));
        }

        [Fact]
        public void Invalid_Email_Fails_Validation()
        {
            Assert.Throws<ArgumentException>(
                () => new Order(
                    "James",
                    "no-an-email",
                    new List<OrderItem>() { new OrderItem(
                        "P001",
                        "Product 1",
                        2,
                        50)
                    }));
        }

        [Fact]
        public void Invalid_Order_With_Zero_Items()
        {
            Assert.Throws<ArgumentException>(() => new Order(
                "James",
                "james@example.com",
                new List<OrderItem>()));
        }

        //ITEMS
        [Fact]
        public void Adding_Item_Works()
        {
            var order = CreateValidOrder();
            var item = new OrderItem(
                "P001",
                "Product 1",
                2,
                50
            );
            Assert.Equal(1, order.Items.Count);
            order.AddItem(item);
            Assert.Equal(2, order.Items.Count);
        }

        [Fact]
        public void Removing_Item_Works()
        {
            var order = CreateValidOrder();
            var item = new OrderItem(
                "P001",
                "Product 1",
                2,
                50
            );
            order.AddItem(item);
            order.RemoveItem(item);
            Assert.True(order.Items.Count == 1);
        }

        [Fact]
        public void Total_Is_Calculated()
        {
            var order = CreateValidOrder();
            var item1 = new OrderItem(
                "P001",
                "Product 1",
                2,
                50
            );

            var item2 = new OrderItem(
                "P002",
                "Product 2",
                1,
                30
            );

            order.AddItem(item1);
            order.AddItem(item2);
            Assert.Equal(230, order.TotalAmount);
        }

        [Fact]
        public void Subtotal_Is_Calculated()
        {
            var item = new OrderItem(
                "P001",
                "Product 1",
                2,
                50
            );
            Assert.Equal(100, item.Subtotal);
        }

        [Fact]
        public void Cannot_Modify_Completed_Order()
        {
            var order = CreateValidOrder();
            order.StartProcessing();
            order.Complete();
            var item = new OrderItem(
                "P001",
                "Product 1",
                2,
                50
            );

            Assert.Throws<InvalidOperationException>(() => order.AddItem(item));
        }

        [Fact]
        public void Cannot_Modify_Cancelled_Order() { 
            var order = CreateValidOrder();
            order.Cancel();
            var item = new OrderItem(
                "P001",
                "Product 1",
                2,
                50
            );
            Assert.Throws<InvalidOperationException>(() => order.AddItem(item));
        }

        //STATE TRANSITIONS
        [Fact]
        public void Pending_Processing_Order_Succeeds()
        {
            var order = CreateValidOrder();
            order.StartProcessing();
            Assert.Equal(OrderStatus.Processing, order.Status);
        }

        [Fact]
        public void Processing_Completed_Order_Succeeds()
        {
            var order = CreateValidOrder();
            order.StartProcessing();
            order.Complete();
            Assert.Equal(OrderStatus.Completed, order.Status);
        }

        [Fact]
        public void Pending_Cancelled_Order_Succeeds()
        {
            var order = CreateValidOrder();
            order.Cancel();
            Assert.Equal(OrderStatus.Cancelled, order.Status);
        }

        [Fact]
        public void Processing_Cancelled_Order_Succeeds()
        {
            var order = CreateValidOrder();
            order.StartProcessing();
            order.Cancel();
            Assert.Equal(OrderStatus.Cancelled, order.Status);
        }

        [Fact]
        public void Completed_Cancelled_Order_Fails()
        {
            var order = CreateValidOrder();
            order.StartProcessing();
            order.Complete();
            Assert.Throws<InvalidOperationException>(() => order.Cancel());
        }

        [Fact]
        public void Completed_Processing_Order_Fails()
        {
            var order = CreateValidOrder();
            order.StartProcessing();
            order.Complete();
            Assert.Throws<InvalidOperationException>(() => order.StartProcessing());
        }

        [Fact]
        public void Cancelled_Processing_Order_Fails()
        {
            var order = CreateValidOrder();
            order.StartProcessing();
            order.Cancel();
            Assert.Throws<InvalidOperationException>(() => order.StartProcessing());
        }

        [Fact]
        public void Cancelled_Completed_Order_Fails()
        {
            var order = CreateValidOrder();
            order.StartProcessing();
            order.Cancel();
            Assert.Throws<InvalidOperationException>(() => order.Complete());
        }

        //SOFT DELETION
        [Fact]
        public void Order_Can_Be_Soft_Deleted()
        {
            var order = CreateValidOrder();
            order.SoftDelete();
            Assert.True(order.IsDeleted);
        }

        [Fact]
        public void Order_Cannot_Be_Soft_Deleted_Twice()
        {
            var order = CreateValidOrder();
            order.SoftDelete();
            Assert.Throws<InvalidOperationException>(() => order.SoftDelete());
        }

        [Fact]
        public void Order_Delete_Date_Is_Populated()
        {
            var order = CreateValidOrder();

            Assert.False(order.IsDeleted);
            Assert.Null(order.DeletedDate);

            order.SoftDelete();

            Assert.True(order.IsDeleted);
            Assert.NotNull(order.DeletedDate);
        }

        [Fact]
        public void Cannot_Remove_Last_Item_From_Order()
        {
            var order = CreateValidOrder();
            var item = order.Items.Single();

            Assert.Throws<InvalidOperationException>(
                () => order.RemoveItem(item));
        }

        [Fact]
        public void New_Order_Should_Have_Same_Created_And_Updated_Date()
        {
            var order = CreateValidOrder();

            Assert.Equal(
                order.CreatedDate,
                order.UpdatedDate);
        }

        [Fact]
        public void Null_Order_Items_Should_Throw_ArgumentNullException()
        {
            // Act
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new Order(
                    "James",
                    "james@example.com",
                    null));

            // Assert
            Assert.Equal(
                "Order items cannot be null. (Parameter 'items')",
                exception.Message);
        }

        [Fact]
        public void Cannot_Remove_Item_That_Does_Not_Belong_To_Order()
        {
            // Arrange
            var order = CreateValidOrder();

            var itemNotInOrder = new OrderItem(
                "P999",
                "Product 999",
                1,
                25);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => order.RemoveItem(itemNotInOrder));
        }

        [Fact]
        public void Adding_Item_Updates_UpdatedDate()
        {
            // Arrange
            var order = CreateValidOrder();
            var originalUpdatedDate = order.UpdatedDate;

            // Give the clock enough time to advance
            Thread.Sleep(10);

            var item = new OrderItem(
                "P002",
                "Product 2",
                1,
                30);

            // Act
            order.AddItem(item);

            // Assert
            Assert.True(
                order.UpdatedDate > originalUpdatedDate);
        }

        [Fact]
        public void Removing_Item_Updates_UpdatedDate()
        {
            // Arrange
            var order = CreateValidOrder();

            var itemToRemove = new OrderItem(
                "P002",
                "Product 2",
                1,
                30);

            order.AddItem(itemToRemove);

            var originalUpdatedDate = order.UpdatedDate;

            Thread.Sleep(10);

            // Act
            order.RemoveItem(itemToRemove);

            // Assert
            Assert.True(
                order.UpdatedDate > originalUpdatedDate);
        }

        [Fact]
        public void Starting_Processing_Updates_UpdatedDate()
        {
            // Arrange
            var order = CreateValidOrder();
            var originalUpdatedDate = order.UpdatedDate;

            Thread.Sleep(10);

            // Act
            order.StartProcessing();

            // Assert
            Assert.Equal(
                OrderStatus.Processing,
                order.Status);

            Assert.True(
                order.UpdatedDate > originalUpdatedDate);
        }

        [Fact]
        public void Cannot_Add_Item_To_Soft_Deleted_Order()
        {
            // Arrange
            var order = CreateValidOrder();

            order.SoftDelete();

            var item = new OrderItem(
                "P002",
                "Product 2",
                1,
                30);

            // Act
            var exception = Assert.Throws<InvalidOperationException>(
                () => order.AddItem(item));

            // Assert
            Assert.Equal(
                "Cannot add items to a deleted order.",
                exception.Message);
        }

        [Fact]
        public void Cannot_Remove_Item_From_Soft_Deleted_Order()
        {
            // Arrange
            var order = CreateValidOrder();

            var itemToRemove = new OrderItem(
                "P002",
                "Product 2",
                1,
                30);

            order.AddItem(itemToRemove);

            order.SoftDelete();

            // Act
            var exception = Assert.Throws<InvalidOperationException>(
                () => order.RemoveItem(itemToRemove));

            // Assert
            Assert.Equal(
                "Cannot remove items from a deleted order.",
                exception.Message);
        }

        private static Order CreateValidOrder()
        {
            var item = new OrderItem(
                "P001",
                "Product 1",
                2,
                50);

            return new Order(
                "James",
                "james@example.com",
                new List<OrderItem> { item });
        }
    }
}
