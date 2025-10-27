using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using ERP.Domain.Entities;
using ERP.Domain.Entities.RequestFeatures;
using ERP.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // tüm endpoint'ler login gerektirir
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _svc;
   
    public CustomersController(ICustomerService svc) 
    {
        _svc = svc; 
    }
    // Her kullanýcý tüm müþterileri görebilir
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerReadDto>>> GetAll()
        => Ok(await _svc.GetAllAsync());

    // Her kullanýcý kendi detayýný görebilir; admin tümünü görebilir
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerReadDto>> Get(int id)
    {
        var e = await _svc.GetAsync(id);
        return e is null ? NotFound() : Ok(e);
    }

    // Yalnýzca Admin ekleyebilir
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CustomerReadDto>> Create(CustomerCreateDto dto)
    {
        var res = await _svc.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = res.Id }, res);
    }

    // Yalnýzca Admin güncelleyebilir
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, CustomerUpdateDto dto)
    {
        await _svc.UpdateAsync(id, dto);
        return NoContent();
    }

    // Yalnýzca Admin silebilir
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _svc.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("Parameters")]

    public async Task<IActionResult> Get([FromQuery] QueryParams queryParams)
    {
        try
        {
            var result = await _svc.GetCustomersAsync(queryParams);
            Response.Headers.Append("X-Total-Count", result.TotalRecords.ToString());
            Response.Headers.Append("X-Total-Pages", result.TotalPages.ToString());
            Response.Headers.Append("X-Current-Page", result.PageNumber.ToString());
            Response.Headers.Append("X-Page-Size", result.PageSize.ToString());
            return Ok(result.Data);
        }
        catch (Exception ex)
        {

            return BadRequest(new { eror = ex.Message });
        }



    }


}
