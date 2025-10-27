using Microsoft.AspNetCore.Mvc;
using ERP.Application.Interfaces;
using ERP.Application.DTOs;
namespace ErpApı.WebUI.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _service;
        public ProductsController(IProductService service) => _service = service;

        public async Task<IActionResult> Index()
        {
            var products = await _service.GetAllAsync();
            return View(products);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            await _service.CreateAsync(dto);
            return RedirectToAction(nameof(Index));
        }
    }
}
