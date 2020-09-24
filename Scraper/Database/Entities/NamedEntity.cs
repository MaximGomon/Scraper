using System.ComponentModel.DataAnnotations;

namespace Scraper.Database.Entities
{
    public class NamedEntity : BaseEntity
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }

    }
}