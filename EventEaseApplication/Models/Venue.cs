using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace EventEaseApplication.Models
{
    public class Venue
    {
        public int VenueId { get; set; }

        [Required]
        [Display(Name = "Venue Name")]
        public string VenueName { get; set; } = string.Empty;

        [Required]
        public string Location { get; set; } = string.Empty;

        [Required]
        [Range(1, 100000)]
        public int Capacity { get; set; }

        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        public ICollection<Event> Events { get; set; } = new List<Event>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}