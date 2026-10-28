using System.Security.Claims;
using AssetManagementBackend.DTOs;
using AssetManagementBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagementBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders(
            int page = 1,
            int pageSize = 16,
            string? search = null)
    {
        var orders = await _orderService.GetOrders(
            page,
            pageSize,
            search);

        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var order = await _orderService.GetOrder(id);

        return Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
    {
        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if(!Guid.TryParse(userIdClaim,  out var userId))
        {
            return Unauthorized(new
            {
                message = "The authenticated user's ID is missing or invalid."
            });
        }

        var order = await _orderService.CreateOrder(dto, userId);

        return CreatedAtAction(
            nameof(GetOrder),
            new { id = order.Id },
            order);
    }
}