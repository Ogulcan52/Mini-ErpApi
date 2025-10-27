using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERP.Application.DTOs;

namespace ERP.Application.Interfaces
{
    public interface IReservationService
    {
        Task<IEnumerable<ReservationReadDto>> GetAllReservationsAsync();
        Task<int> CreateReservationAsync(int orderId,CreateReservationRequestDto request);
        Task<ReservationReadDto?>GetReservationByIdAsync(int id);
        Task  DeleteReservationAsync(int id);
        Task UpdateReservationAsync(int id, UpdateReservationRequestDto dto);
        Task ConfirmReservationAsync(int reservationId);
    }

}
