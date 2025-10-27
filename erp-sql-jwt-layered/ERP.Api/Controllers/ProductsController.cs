using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using ERP.Domain.Entities.RequestFeatures;
using ERP.Infrastructure.Persistence;
using ERP.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _svc;
    private readonly AppDbContext _appDbContext;
    public ProductsController(IProductService svc, AppDbContext context)
    {
         _svc = svc;
        _appDbContext = context;

    }

    // Herkes ürünleri görebilir
    [HttpGet]
   
    public async Task<ActionResult<IEnumerable<ProductReadDto>>> GetAll()
        => Ok(await _svc.GetAllAsync());

    // Herkes ürün detaylarýný görebilir
    [HttpGet("{id:int}")]
    
    public async Task<ActionResult<ProductReadDto>> Get(int id)
    {
        var e = await _svc.GetAsync(id);
        return e is null ? NotFound() : Ok(e);
    }

    // Sadece admin ürün ekleyebilir
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductReadDto>> Create(ProductCreateDto dto)
    {
        var res = await _svc.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = res.Id }, res);
    }

    // Sadece admin ürün güncelleyebilir
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, ProductUpdateDto dto)
    {
        await _svc.UpdateAsync(id, dto);
        return NoContent();
    }

    // Sadece admin ürün silebilir
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
            var result = await _svc.GetProductsAsync(queryParams);
            Response.Headers.Append("X-Total-Count", result.TotalRecords.ToString());
            Response.Headers.Append("X-Total-Pages", result.TotalPages.ToString());
            Response.Headers.Append("X-Current-Page", result.PageNumber.ToString());
            Response.Headers.Append("X-Page-Size", result.PageSize.ToString());
            return Ok(result.Data);
        }
        catch (Exception ex)
        {

            return BadRequest(new {eror=ex.Message});
        }
        

        
    }



}
