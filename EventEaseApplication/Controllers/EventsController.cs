using EventEaseApplication.Data;
using EventEaseApplication.Models;
using EventEaseApplication.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventEaseApplication.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;

        public EventsController(ApplicationDbContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var events = _context.Events
                .Include(e => e.Venue)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                events = events.Where(e =>
                    e.EventName.Contains(searchString) ||
                    (e.Description != null && e.Description.Contains(searchString)) ||
                    (e.Venue != null && e.Venue.VenueName.Contains(searchString)));
            }

            ViewData["CurrentFilter"] = searchString;

            return View(await events.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ev = await _context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null)
            {
                return NotFound();
            }

            return View(ev);
        }

        public IActionResult Create()
        {
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event ev, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null)
                    {
                        ev.ImageUrl = await _imageService.UploadImageAsync(imageFile, "events");
                    }
                    else
                    {
                        ev.ImageUrl = "https://placehold.co/600x400?text=Event";
                    }

                    _context.Add(ev);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", ev.VenueId);
            return View(ev);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ev = await _context.Events.FindAsync(id);

            if (ev == null)
            {
                return NotFound();
            }

            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", ev.VenueId);
            return View(ev);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event ev, IFormFile? imageFile)
        {
            if (id != ev.EventId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingEvent = await _context.Events
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.EventId == id);

                    if (existingEvent == null)
                    {
                        return NotFound();
                    }

                    if (imageFile != null)
                    {
                        if (!string.IsNullOrWhiteSpace(existingEvent.ImageUrl) &&
                            !existingEvent.ImageUrl.Contains("placehold.co"))
                        {
                            await _imageService.DeleteImageAsync(existingEvent.ImageUrl);
                        }

                        ev.ImageUrl = await _imageService.UploadImageAsync(imageFile, "events");
                    }
                    else
                    {
                        ev.ImageUrl = existingEvent.ImageUrl;
                    }

                    _context.Update(ev);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(ev.EventId))
                    {
                        return NotFound();
                    }

                    throw;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", ev.VenueId);
            return View(ev);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ev = await _context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null)
            {
                return NotFound();
            }

            return View(ev);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hasBookings = await _context.Bookings.AnyAsync(b => b.EventId == id);

            if (hasBookings)
            {
                TempData["ErrorMessage"] = "This event cannot be deleted because it has existing bookings.";
                return RedirectToAction(nameof(Index));
            }

            var ev = await _context.Events.FindAsync(id);

            if (ev != null)
            {
                if (!string.IsNullOrWhiteSpace(ev.ImageUrl) &&
                    !ev.ImageUrl.Contains("placehold.co"))
                {
                    await _imageService.DeleteImageAsync(ev.ImageUrl);
                }

                _context.Events.Remove(ev);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }
    }
}