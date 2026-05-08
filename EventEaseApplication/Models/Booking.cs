using System.ComponentModel.DataAnnotations;

namespace EventEaseApplication.Models
{
    public class Booking
    {
        public int BookingId { get; set; }

        [Required]
        [Display(Name = "Event")]
        public int EventId { get; set; }

        public Event? Event { get; set; }

        [Required]
        [Display(Name = "Venue")]
        public int VenueId { get; set; }

        public Venue? Venue { get; set; }

        [Required]
        [Display(Name = "Booking Date")]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; }
    }
}