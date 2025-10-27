using Azure.Core;
using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using ERP.Domain.Entities.RequestFeatures;
using ERP.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Tüm endpoint'ler login gerektirir
public class OrdersController : ControllerBase
{
    private readonly IOrderService _svc;

    public OrdersController(IOrderService svc)
    {
        _svc = svc;
    }

    // Her kullanýcý kendi sipariþlerini görebilir, admin tümünü görebilir
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderReadDto>>> GetAll()
        => Ok(await _svc.GetAllAsync());

    // Sipariþ detaylarý
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderReadDto>> Get(int id)
    {
        var e = await _svc.GetAsync(id);
        return e is null ? NotFound() : Ok(e);
    }

    // Sipariþ oluþturma, login olmuþ kullanýcýlar yapabilir
    [HttpPost]
    public async Task<ActionResult> Create(OrderCreateDto dto)
    {
        var id = await _svc.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    // Sipariþi tamamlamak sadece admin yetkisiyle
    [HttpPost("{id:int}/complete")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Complete(int id)
    {
        await _svc.CompleteAsync(id);
        return NoContent();
    }

    // Sipariþi silmek sadece admin yetkisiyle
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _svc.DeleteAsync(id);
        return NoContent();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult>Update(int id, [FromBody] OrderItemUpdateDto request)
    {
        var result = await _svc.UpdateAsync(id, request);

        if (!result)
            return BadRequest("Sipariþ bulunamadý.");

        return Ok(new { message = $"Sipariþ {id} güncellendi." });

    }


}
