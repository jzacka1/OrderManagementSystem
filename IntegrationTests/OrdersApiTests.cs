
using Microsoft.AspNetCore.Mvc;
using OrderManagementSystem.Application.DTOs;
using OrderManagementSystem.Domain.Enums;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrderManagementSystem.IntegrationTests
{
    public class OrdersApiTests
    : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private static CreateOrderRequest CreateValidOrderRequest(
                                            string customerName = "Test Customer",
                                            string email = "test@example.com")
        {
            return new CreateOrderRequest
            {
                CustomerName = customerName,
                Email = email,
                Items = new List<OrderItemRequest>
                {   
                    new()
                    {
                        ProductId = "TEST-001",
                        ProductName = "Test Product",
                        Quantity = 1,
                        UnitPrice = 25.00m
                    }
                }
            };
        }
        private async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest? request = null)
        {
            request ??= CreateValidOrderRequest();

            var response = await _client.PostAsJsonAsync(
                "/api/Orders",
                request);

            Assert.Equal(
                HttpStatusCode.Created,
                response.StatusCode);

            var createdOrder =
                await response.Content.ReadFromJsonAsync<OrderResponse>(
                    _jsonOptions);

            Assert.NotNull(createdOrder);

            Assert.NotEqual(
                Guid.Empty,
                createdOrder.Id);

            return createdOrder;
        }

        private async Task<OrderResponse> GetOrderAsync(Guid id)
        {
            var response = await _client.GetAsync(
                $"/api/Orders/{id}");

            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);

            var order =
                await response.Content.ReadFromJsonAsync<OrderResponse>(
                    _jsonOptions);

            Assert.NotNull(order);

            return order;
        }

        public OrdersApiTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();

            _jsonOptions.Converters.Add(
                new JsonStringEnumConverter());
        }

        [Fact]
        public async Task Get_Orders_Returns_Success_Status_Code()
        {
            //var response = await _client.GetAsync(
            //    "/api/Orders");

            //response.EnsureSuccessStatusCode();

            var response = await _client.GetAsync("/api/Orders");

            var content = await response.Content.ReadAsStringAsync();

            Assert.True(
                response.IsSuccessStatusCode,
                $"Status: {response.StatusCode}\nResponse: {content}");
        }

        [Fact]
        public async Task Create_Order_Returns_Created_And_Can_Be_Retrieved()
        {
            var request = new CreateOrderRequest
            {
                CustomerName = "Integration Test Customer",
                Email = "integration@example.com",
                Items = new List<OrderItemRequest>
            {
                new OrderItemRequest
                {
                    ProductId = "TEST-001",
                    ProductName = "Integration Test Product",
                    Quantity = 2,
                    UnitPrice = 25.00m
                }
            }
            };

            var createResponse = await _client.PostAsJsonAsync(
                "/api/Orders",
                request);

            Assert.Equal(
                HttpStatusCode.Created,
                createResponse.StatusCode);

            Assert.NotNull(
                createResponse.Headers.Location);

            //var createdOrder =
            //    await createResponse.Content
            //        .ReadFromJsonAsync<OrderResponse>(_jsonOptions);

            var responseBody =
                await createResponse.Content.ReadAsStringAsync();

            var createdOrder =
                JsonSerializer.Deserialize<OrderResponse>(
                    responseBody,
                    _jsonOptions);

            Assert.NotNull(createdOrder);

            Assert.NotEqual(
                Guid.Empty,
                createdOrder.Id);

            Assert.NotNull(createdOrder);

            Assert.NotEqual(
                Guid.Empty,
                createdOrder.Id);

            Assert.Equal(
                "Integration Test Customer",
                createdOrder.CustomerName);

            Assert.Equal(
                "integration@example.com",
                createdOrder.Email);

            Assert.Equal(
                50.00m,
                createdOrder.TotalAmount);

            var getResponse = await _client.GetAsync(
                $"/api/Orders/{createdOrder.Id}");

            Assert.Equal(
                HttpStatusCode.OK,
                getResponse.StatusCode);

            var retrievedOrder =
                await getResponse.Content.ReadFromJsonAsync<OrderResponse>(
                    _jsonOptions);

            Assert.NotNull(retrievedOrder);

            Assert.Equal(
                createdOrder.Id,
                retrievedOrder.Id);

            Assert.Equal(
                "Integration Test Customer",
                retrievedOrder.CustomerName);

            Assert.Equal(
                50.00m,
                retrievedOrder.TotalAmount);
        }

        [Fact]
        public async Task Get_Nonexistent_Order_Returns_NotFound()
        {
            var nonexistentOrderId = Guid.NewGuid();

            var response = await _client.GetAsync(
                $"/api/Orders/{nonexistentOrderId}");

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);
        }

        [Fact]
        public async Task Create_Order_With_No_Items_Returns_BadRequest()
        {
            var request = new CreateOrderRequest
            {
                CustomerName = "Invalid Order",
                Email = "invalid@example.com",
                Items = new List<OrderItemRequest>()
            };

            var response = await _client.PostAsJsonAsync(
                "/api/Orders",
                request);

            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);

            var problemDetails =
                await response.Content
                    .ReadFromJsonAsync<ValidationProblemDetails>();

            Assert.NotNull(problemDetails);

            Assert.Equal(
                400,
                problemDetails.Status);

            Assert.Equal(
                "One or more validation errors occurred.",
                problemDetails.Title);

            Assert.True(
                problemDetails.Errors.ContainsKey("Items"));

            Assert.Contains(
                "The field Items must be a string or array type with a minimum length of '1'.",
                problemDetails.Errors["Items"]);
        }

        [Fact]
        public async Task Process_Order_Returns_NoContent_And_Changes_Status()
        {
            // Arrange
            var request = CreateValidOrderRequest(
                "Process Test Customer",
                "process@example.com");

            var createdOrder = await CreateOrderAsync(request);

            Assert.Equal(
                OrderStatus.Pending,
                createdOrder.Status);

            // Act
            var processResponse = await _client.PostAsync(
                $"/api/Orders/{createdOrder.Id}/process",
                null);

            // Assert
            Assert.Equal(
                HttpStatusCode.NoContent,
                processResponse.StatusCode);

            // Verify the status changed
            var processedOrder = await GetOrderAsync(createdOrder.Id);

            Assert.Equal(
                OrderStatus.Processing,
                processedOrder.Status);
        }

        [Fact]
        public async Task Complete_Order_Returns_NoContent_And_Changes_Status()
        {
            // Arrange - create an order
            var request = CreateValidOrderRequest(
                "Complete Test Customer",
                "complete@example.com");

            var createdOrder = await CreateOrderAsync(request);

            Assert.Equal(
                OrderStatus.Pending,
                createdOrder.Status);

            // Move Pending → Processing
            var processResponse = await _client.PostAsync(
                $"/api/Orders/{createdOrder.Id}/process",
                null);

            Assert.Equal(
                HttpStatusCode.NoContent,
                processResponse.StatusCode);

            // Act - move Processing → Completed
            var completeResponse = await _client.PostAsync(
                $"/api/Orders/{createdOrder.Id}/complete",
                null);

            // Assert
            Assert.Equal(
                HttpStatusCode.NoContent,
                completeResponse.StatusCode);

            // Verify the status changed
            var completedOrder = await GetOrderAsync(createdOrder.Id);

            Assert.Equal(
                OrderStatus.Completed,
                completedOrder.Status);
        }

        [Fact]
        public async Task Cancel_Order_Returns_NoContent_And_Changes_Status()
        {
            // Arrange
            var request = CreateValidOrderRequest(
                "Cancel Test Customer",
                "cancel@example.com");

            var createdOrder = await CreateOrderAsync(request);

            Assert.Equal(
                OrderStatus.Pending,
                createdOrder.Status);

            // Act
            var cancelResponse = await _client.PostAsync(
                $"/api/Orders/{createdOrder.Id}/cancel",
                null);

            // Assert
            Assert.Equal(
                HttpStatusCode.NoContent,
                cancelResponse.StatusCode);

            // Verify the status changed
            var cancelledOrder = await GetOrderAsync(createdOrder.Id);

            Assert.Equal(
                OrderStatus.Cancelled,
                cancelledOrder.Status);
        }

        [Fact]
        public async Task Cancel_Completed_Order_Returns_BadRequest()
        {
            // Arrange - create a Pending order
            var request = CreateValidOrderRequest(
                "Completed Order",
                "completed@example.com");

            var createdOrder = await CreateOrderAsync(request);

            Assert.Equal(
                OrderStatus.Pending,
                createdOrder.Status);

            // Pending → Processing
            var processResponse = await _client.PostAsync(
                $"/api/Orders/{createdOrder.Id}/process",
                null);

            Assert.Equal(
                HttpStatusCode.NoContent,
                processResponse.StatusCode);

            // Processing → Completed
            var completeResponse = await _client.PostAsync(
                $"/api/Orders/{createdOrder.Id}/complete",
                null);

            Assert.Equal(
                HttpStatusCode.NoContent,
                completeResponse.StatusCode);

            // Act - attempt Completed → Cancelled
            var cancelResponse = await _client.PostAsync(
                $"/api/Orders/{createdOrder.Id}/cancel",
                null);

            // Assert
            Assert.Equal(
                HttpStatusCode.BadRequest,
                cancelResponse.StatusCode);

            var problemDetails =
                await cancelResponse.Content
                    .ReadFromJsonAsync<ProblemDetails>();

            Assert.NotNull(problemDetails);

            Assert.Equal(
                400,
                problemDetails.Status);

            Assert.Equal(
                "Bad Request",
                problemDetails.Title);

            Assert.Equal(
                "Completed orders cannot be cancelled.",
                problemDetails.Detail);
        }

        [Fact]
        public async Task Delete_Order_Returns_NoContent_And_Order_Cannot_Be_Retrieved()
        {
            // Arrange
            var request = CreateValidOrderRequest(
                "Delete Test Customer",
                "delete@example.com");

            var createdOrder = await CreateOrderAsync(request);

            // Act - soft delete the order
            var deleteResponse = await _client.DeleteAsync(
                $"/api/Orders/{createdOrder.Id}");

            // Assert
            Assert.Equal(
                HttpStatusCode.NoContent,
                deleteResponse.StatusCode);

            // Verify the deleted order can no longer be retrieved
            var getResponse = await _client.GetAsync(
                $"/api/Orders/{createdOrder.Id}");

            Assert.Equal(
                HttpStatusCode.NotFound,
                getResponse.StatusCode);
        }

        [Fact]
        public async Task Get_Order_With_Invalid_Id_Returns_NotFound()
        {
            // Act
            var response = await _client.GetAsync(
                "/api/Orders/not-a-valid-guid");

            // Assert
            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);
        }

        [Fact]
        public async Task Process_Completed_Order_Returns_BadRequest()
        {
            // Arrange - create Pending order
            var createdOrder = await CreateOrderAsync(
                CreateValidOrderRequest(
                    "Process Completed Test",
                    "processcompleted@example.com"));

            Assert.Equal(
                OrderStatus.Pending,
                createdOrder.Status);

            // Pending → Processing
            var processResponse = await _client.PostAsync(
                $"/api/Orders/{createdOrder.Id}/process",
                null);

            Assert.Equal(
                HttpStatusCode.NoContent,
                processResponse.StatusCode);

            // Processing → Completed
            var completeResponse = await _client.PostAsync(
                $"/api/Orders/{createdOrder.Id}/complete",
                null);

            Assert.Equal(
                HttpStatusCode.NoContent,
                completeResponse.StatusCode);

            // Act - attempt Completed → Processing
            var secondProcessResponse = await _client.PostAsync(
                $"/api/Orders/{createdOrder.Id}/process",
                null);

            // Assert
            Assert.Equal(
                HttpStatusCode.BadRequest,
                secondProcessResponse.StatusCode);

            var problemDetails =
                await secondProcessResponse.Content
                    .ReadFromJsonAsync<ProblemDetails>();

            Assert.NotNull(problemDetails);

            Assert.Equal(
                400,
                problemDetails.Status);

            Assert.Equal(
                "Bad Request",
                problemDetails.Title);

            Assert.Equal(
                "Only pending orders can be processed.",
                problemDetails.Detail);
        }

        [Fact]
        public async Task Cancel_Cancelled_Order_Returns_BadRequest()
        {
            // Arrange
            var createdOrder = await CreateOrderAsync(
                CreateValidOrderRequest(
                    "Duplicate Cancel Test",
                    "duplicatecancel@example.com"));

            Assert.Equal(
                OrderStatus.Pending,
                createdOrder.Status);

            // Pending → Cancelled
            var firstCancelResponse = await _client.PostAsync(
                $"/api/Orders/{createdOrder.Id}/cancel",
                null);

            Assert.Equal(
                HttpStatusCode.NoContent,
                firstCancelResponse.StatusCode);

            // Act - attempt to cancel the order again
            var secondCancelResponse = await _client.PostAsync(
                $"/api/Orders/{createdOrder.Id}/cancel",
                null);

            // Assert
            Assert.Equal(
                HttpStatusCode.BadRequest,
                secondCancelResponse.StatusCode);

            var problemDetails =
                await secondCancelResponse.Content
                    .ReadFromJsonAsync<ProblemDetails>();

            Assert.NotNull(problemDetails);

            Assert.Equal(
                400,
                problemDetails.Status);

            Assert.Equal(
                "Bad Request",
                problemDetails.Title);

            Assert.Equal(
                "Cancelled orders cannot be cancelled.",
                problemDetails.Detail);
        }

        [Fact]
        public async Task Create_Order_With_Invalid_Quantity_Returns_BadRequest()
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                CustomerName = "Invalid Quantity Customer",
                Email = "invalidquantity@example.com",
                Items = new List<OrderItemRequest>
                {
                    new()
                    {
                        ProductId = "INVALID-001",
                        ProductName = "Invalid Quantity Product",
                        Quantity = 0,
                        UnitPrice = 25.00m
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync(
                "/api/Orders",
                request);

            // Assert
            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);

            var problemDetails =
                await response.Content
                    .ReadFromJsonAsync<ValidationProblemDetails>();

            Assert.NotNull(problemDetails);

            Assert.Equal(
                400,
                problemDetails.Status);

            Assert.Equal(
                "One or more validation errors occurred.",
                problemDetails.Title);

            Assert.True(
                problemDetails.Errors.ContainsKey("Items[0].Quantity"));

            Assert.Contains(
                "The field Quantity must be between 1 and 2147483647.",
                problemDetails.Errors["Items[0].Quantity"]);
        }

        [Fact]
        public async Task Create_Order_With_Invalid_UnitPrice_Returns_BadRequest()
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                CustomerName = "Invalid Price Customer",
                Email = "invalidprice@example.com",
                Items = new List<OrderItemRequest>
                {
                    new()
                    {
                        ProductId = "INVALID-PRICE-001",
                        ProductName = "Invalid Price Product",
                        Quantity = 1,
                        UnitPrice = 0m
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync(
                "/api/Orders",
                request);

            // Assert
            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);

            var problemDetails =
                await response.Content
                    .ReadFromJsonAsync<ValidationProblemDetails>();

            Assert.NotNull(problemDetails);

            Assert.Equal(
                400,
                problemDetails.Status);

            Assert.Equal(
                "One or more validation errors occurred.",
                problemDetails.Title);

            Assert.True(
                problemDetails.Errors.ContainsKey(
                    "Items[0].UnitPrice"));

            Assert.Contains(
                problemDetails.Errors["Items[0].UnitPrice"],
                message => message.Contains(
                    "The field UnitPrice must be between 0.01 and"));
                }
       
    }
}