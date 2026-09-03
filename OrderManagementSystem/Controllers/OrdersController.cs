using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderManagementSystem.Application.DTOs;
using OrderManagementSystem.Application.Services;

namespace OrderManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        [ProducesResponseType(
            typeof(OrderResponse),
            StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OrderResponse>> CreateOrder(
            [FromBody] CreateOrderRequest request,
            CancellationToken cancellationToken)
        {
            var order = await _orderService.CreateOrderAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetOrderById),
                new { id = order.Id },
                order);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(
            typeof(OrderResponse),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderResponse>> GetOrderById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var order = await _orderService.GetOrderByIdAsync(
                id,
                cancellationToken);

            if (order == null)
            {
                throw new KeyNotFoundException(
                    $"Order with ID '{id}' was not found.");
            }

            return Ok(order);
        }

        [HttpGet]
        [ProducesResponseType(
            typeof(List<OrderResponse>),
            StatusCodes.Status200OK)]
        public async Task<ActionResult<List<OrderResponse>>> GetOrders(
            CancellationToken cancellationToken)
        {
            var orders = await _orderService.GetOrdersAsync(
                cancellationToken);

            return Ok(orders);
        }

        [HttpPost("{id:guid}/process")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ProcessOrder(
            Guid id,
            CancellationToken cancellationToken)
        {
            await _orderService.ProcessOrderAsync(
                id,
                cancellationToken);

            return NoContent();
        }

        [HttpPost("{id:guid}/complete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CompleteOrder(
            Guid id,
            CancellationToken cancellationToken)
        {
            await _orderService.CompleteOrderAsync(
                id,
                cancellationToken);

            return NoContent();
        }

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CancelOrder(
            Guid id,
            CancellationToken cancellationToken)
        {
            await _orderService.CancelOrderAsync(
                id,
                cancellationToken);

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteOrder(
            Guid id,
            CancellationToken cancellationToken)
        {
            await _orderService.DeleteOrderAsync(
                id,
                cancellationToken);

            return NoContent();
        }
    }
}
