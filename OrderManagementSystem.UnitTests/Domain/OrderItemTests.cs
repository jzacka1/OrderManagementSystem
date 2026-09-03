using OrderManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.UnitTests.Domain
{
    public class OrderItemTests
    {
        [Fact]
        public void Constructor_With_Valid_Data_Creates_OrderItem()
        {
            // Act
            var item = new OrderItem(
                "P100",
                "Keyboard",
                2,
                50m);

            // Assert
            Assert.NotEqual(
                Guid.Empty,
                item.Id);

            Assert.Equal(
                "P100",
                item.ProductId);

            Assert.Equal(
                "Keyboard",
                item.ProductName);

            Assert.Equal(
                2,
                item.Quantity);

            Assert.Equal(
                50m,
                item.UnitPrice);

            Assert.Equal(
                100m,
                item.Subtotal);
        }

        [Fact]
        public void Constructor_With_Invalid_Quantity_Throws_ArgumentException()
        {
            // Act
            var exception = Assert.Throws<ArgumentException>(() =>
                new OrderItem(
                    "P100",
                    "Keyboard",
                    0,
                    50m));

            // Assert
            Assert.Equal(
                "Quantity must be a positive value. (Parameter 'quantity')",
                exception.Message);
        }

        [Fact]
        public void Constructor_With_Invalid_UnitPrice_Throws_ArgumentException()
        {
            // Act
            var exception = Assert.Throws<ArgumentException>(() =>
                new OrderItem(
                    "P100",
                    "Keyboard",
                    1,
                    0m));

            // Assert
            Assert.Equal(
                "Unit price must be a positive value. (Parameter 'unitPrice')",
                exception.Message);
        }

        [Fact]
        public void Constructor_With_Empty_ProductId_Throws_ArgumentException()
        {
            // Act
            var exception = Assert.Throws<ArgumentException>(() =>
                new OrderItem(
                    "",
                    "Keyboard",
                    1,
                    50m));

            // Assert
            Assert.Equal(
                "Product ID must be a non-empty value. (Parameter 'productId')",
                exception.Message);
        }

        [Fact]
        public void Constructor_With_Empty_ProductName_Throws_ArgumentException()
        {
            // Act
            var exception = Assert.Throws<ArgumentException>(() =>
                new OrderItem(
                    "P100",
                    "",
                    1,
                    50m));

            // Assert
            Assert.Equal(
                "Product name must be a non-empty value. (Parameter 'productName')",
                exception.Message);
        }
    }
}
