using Amazon.DynamoDBv2.DataModel;
using MenuRestaurante_RaulGonzalez.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MenuRestaurante_RaulGonzalez.Controllers
{
    [Authorize]
    public class ReservationController : Controller
    {
        private IDynamoDBContext _context;
        public ReservationController(IDynamoDBContext context)
        {
            _context = context;
        }

        // GET: /Reservation/All
        // Este metodo recupera todas las reservas hechas de todas los clientes para el Usuario admin
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> All(DateTime? startDate, DateTime? endDate)
        {
            var allReservations = await _context.ScanAsync<Reservation>(
                new List<ScanCondition>()).GetRemainingAsync();

            if (startDate.HasValue)
            {
                allReservations = allReservations
                    .Where(r => r.DateOfReservation >= startDate.Value)
                    .ToList();
            }

            if (endDate.HasValue)
            {
                allReservations = allReservations
                    .Where(r => r.DateOfReservation <= endDate.Value)
                    .ToList();
            }

            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(allReservations);
        }

        // Metodo para que un cliente, pueda cancelar su reserva
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Cancel(string id)
        {
            var reservation = await _context.LoadAsync<Reservation>(id);
            if (reservation == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (reservation.UserId != userId)
            {
                return Forbid();
            }

            await _context.DeleteAsync<Reservation>(id);
            TempData["Message"] = "Your reservation has been canceled.";
            return RedirectToAction(nameof(MyReservations));
        }



        // GET: /Reservation/Create
        public IActionResult Create() { return View(); }

        // POST: /Reservation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(Reservation reservation)
        {
            if (reservation.DateOfReservation < DateTime.Now)
            {
                ModelState.AddModelError("DateOfReservation", "You cannot make a reservation in the past.");
            }

            if (ModelState.IsValid)
            {
                reservation.ReservationId = Guid.NewGuid().ToString();
                reservation.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
                await _context.SaveAsync(reservation);
                TempData["Message"] = "Reservation created successfully.";
                return RedirectToAction(nameof(MyReservations));
            }

            return View(reservation);
        }


        // GET: /Reservation/MyReservations
        // Este metodo servira para que los usuarios puedan ver sus reservas hechas.
        public async Task<IActionResult> MyReservations()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";

                var reservations = await _context.ScanAsync<Reservation>(
                    new List<ScanCondition> { new ScanCondition("UserId", Amazon.DynamoDBv2.DocumentModel.ScanOperator.Equal, userId) }
                    ).GetRemainingAsync();

                return View(reservations);
            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }
        }

    }
}
