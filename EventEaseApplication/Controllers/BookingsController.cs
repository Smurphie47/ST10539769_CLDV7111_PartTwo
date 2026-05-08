
using EventEaseApplication.Data;
using EventEaseApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventEaseApplication.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var bookings = _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                bookings = bookings.Where(b =>
                    b.BookingId.ToString().Contains(searchString) ||
                    b.BookingDate.ToString().Contains(searchString) ||
                    (b.Event != null && b.Event.EventName.Contains(searchString)) ||
                    (b.Venue != null && b.Venue.VenueName.Contains(searchString)));
            }

            ViewData["CurrentFilter"] = searchString;

            return View(await bookings.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        public IActionResult Create()
        {
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName");
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            var isDoubleBooked = await _context.Bookings.AnyAsync(b =>
                b.VenueId == booking.VenueId &&
                b.BookingDate.Date == booking.BookingDate.Date);

            if (isDoubleBooked)
            {
                ModelState.AddModelError("", "This venue is already booked on the selected date.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);

            return View(booking);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null)
            {
                return NotFound();
            }

            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Booking booking)
        {
            if (id != booking.BookingId)
            {
                return NotFound();
            }

            var isDoubleBooked = await _context.Bookings.AnyAsync(b =>
                b.BookingId != booking.BookingId &&
                b.VenueId == booking.VenueId &&
                b.BookingDate.Date == booking.BookingDate.Date);

            if (isDoubleBooked)
            {
                ModelState.AddModelError("", "This venue is already booked on the selected date.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);

            return View(booking);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(b => b.BookingId == id);
        }
    }
}