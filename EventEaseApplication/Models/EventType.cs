using System.ComponentModel.DataAnnotations;

namespace EventEaseApplication.Models
{
    public class EventType
    {
        public int EventTypeId { get; set; }

        [Required]
        [Display(Name = "Event Type")]
        public string Name { get; set; } = string.Empty;

        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}