

using System.ComponentModel.DataAnnotations;

namespace EventEaseApplication.Models
{
    public class Event
    {
        public int EventId { get; set; }

        [Required]
        [Display(Name = "Event Name")]
        public string EventName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Event Date")]
        [DataType(DataType.Date)]
        public DateTime EventDate { get; set; }

        public string? Description { get; set; }

        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        [Required]
        [Display(Name = "Venue")]
        public int VenueId { get; set; }

        public Venue? Venue { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}