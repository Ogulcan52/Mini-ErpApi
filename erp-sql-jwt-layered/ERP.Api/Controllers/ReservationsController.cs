using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.WebUI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationsController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        // GET: api/reservations
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var reservations = await _reservationService.GetAllReservationsAsync();
            return Ok(reservations);
        }

        // GET: api/reservations/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null) return NotFound();
            return Ok(reservation);
        }

        // POST: api/reservations/{orderId}
        [HttpPost("{orderId}")]
        public async Task<IActionResult> Create(int orderId, [FromBody] CreateReservationRequestDto dto)
        {
            var reservationId = await _reservationService.CreateReservationAsync(orderId, dto);
            return CreatedAtAction(nameof(GetById), new { id = reservationId }, new { id = reservationId });
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateReservationRequestDto dto)
        {
            await _reservationService.UpdateReservationAsync(id, dto);
            return Ok(new { message = $"Rezervasyon {id} güncellendi." });
            
        }

        // DELETE: api/reservations/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _reservationService.DeleteReservationAsync(id);
            return NoContent();
        }

        
        [HttpPost("{reservationId}/confirm")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmReservation(int reservationId)
        {
            try
            {
                await _reservationService.ConfirmReservationAsync(reservationId);
                return Ok("Rezervasyon onaylandı.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
