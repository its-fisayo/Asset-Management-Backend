using AssetManagementBackend.DTOs;
using AssetManagementBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagemebtBackend.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class StocksController : ControllerBase
{
    private readonly StockService _stockService;
    private readonly StockMovementService _stockMovementService;

    public StocksController(
        StockService stockService,
        StockMovementService stockMovementService)
    {
        _stockService = stockService;
        _stockMovementService = stockMovementService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStocks(
        int page = 1,
        int pageSize = 16,
        string? search = null,
        bool lowStockOnly = false)
    {
        var stocks = await _stockService.GetStocks(
            page,
            pageSize,
            search,
            lowStockOnly);

        return Ok(stocks);
    }

    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetStockByProductId(Guid productId)
    {
        var stock = await _stockService.GetStockByProductId(
            productId);

        return Ok(stock);
    }

    [HttpPatch("product/{productId}/threshold")]
    public async Task<IActionResult> UpdateLowStockThreshold(
        Guid productId,
        UpdateLowStockThresholdDto dto)
    {
        var stock = await _stockService.UpdateLowStockThreshold(
            productId,
            dto.LowStockThreshold);

        return Ok(stock);
    }

    [HttpGet("movements")]
    public async Task<IActionResult> GetStockMovements(
        int page = 1,
        int pageSize = 16,
        string? search = null,
        Guid? productId = null,
        string? movementType = null)
    {
        var stockMovements = await _stockMovementService.GetStockMovements(
            page,
            pageSize,
            search,
            productId,
            movementType);

        return Ok(stockMovements);
    }

    [HttpGet("movements/{id}")]
    public async Task<IActionResult> GetStockMovement(Guid id)
    {
        var movement = await _stockMovementService.GetStockMovement(id);

        return Ok(movement);
    }
}