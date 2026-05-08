using EventEaseApplication.Data;
using EventEaseApplication.Models;
using EventEaseApplication.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEaseApplication.Controllers
{
    public class VenuesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;

        public VenuesController(ApplicationDbContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var venues = from v in _context.Venues
                         select v;

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                venues = venues.Where(v =>
                    v.VenueName.Contains(searchString) ||
                    v.Location.Contains(searchString));
            }

            ViewData["CurrentFilter"] = searchString;

            return View(await venues.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .FirstOrDefaultAsync(v => v.VenueId == id);

            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Venue venue, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null)
                    {
                        venue.ImageUrl = await _imageService.UploadImageAsync(imageFile, "venues");
                    }
                    else
                    {
                        venue.ImageUrl = "https://placehold.co/600x400?text=Venue";
                    }

                    _context.Add(venue);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            return View(venue);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues.FindAsync(id);

            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Venue venue, IFormFile? imageFile)
        {
            if (id != venue.VenueId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingVenue = await _context.Venues
                        .AsNoTracking()
                        .FirstOrDefaultAsync(v => v.VenueId == id);

                    if (existingVenue == null)
                    {
                        return NotFound();
                    }

                    if (imageFile != null)
                    {
                        if (!string.IsNullOrWhiteSpace(existingVenue.ImageUrl) &&
                            !existingVenue.ImageUrl.Contains("placehold.co"))
                        {
                            await _imageService.DeleteImageAsync(existingVenue.ImageUrl);
                        }

                        venue.ImageUrl = await _imageService.UploadImageAsync(imageFile, "venues");
                    }
                    else
                    {
                        venue.ImageUrl = existingVenue.ImageUrl;
                    }

                    _context.Update(venue);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(venue.VenueId))
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

            return View(venue);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .FirstOrDefaultAsync(v => v.VenueId == id);

            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hasBookings = await _context.Bookings.AnyAsync(b => b.VenueId == id);

            if (hasBookings)
            {
                TempData["ErrorMessage"] = "This venue cannot be deleted because it has existing bookings.";
                return RedirectToAction(nameof(Index));
            }

            var venue = await _context.Venues.FindAsync(id);

            if (venue != null)
            {
                if (!string.IsNullOrWhiteSpace(venue.ImageUrl) &&
                    !venue.ImageUrl.Contains("placehold.co"))
                {
                    await _imageService.DeleteImageAsync(venue.ImageUrl);
                }

                _context.Venues.Remove(venue);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool VenueExists(int id)
        {
            return _context.Venues.Any(v => v.VenueId == id);
        }
    }
}