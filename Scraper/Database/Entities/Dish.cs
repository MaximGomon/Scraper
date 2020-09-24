using System.ComponentModel.DataAnnotations;

namespace Scraper.Database.Entities
{
    public class Dish : NamedEntity
    {
        [Required]
        public virtual MenuItem Section { get; set; }
    }
}