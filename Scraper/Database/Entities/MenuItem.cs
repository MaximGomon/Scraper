using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Scraper.Database.Entities
{
    public class MenuItem : NamedEntity
    {
        public MenuItem()
        {
            Sections = new List<MenuItem>();
            Dishes = new List<Dish>();
        }
        
        [Required]
        public MenuItemType Type { get; set; }

        public virtual ICollection<MenuItem> Sections { get; set; }
        public virtual ICollection<Dish> Dishes { get; set; }
        public virtual MenuItem Parent { get; set; }
    }

    public enum MenuItemType
    {
        Global,
        Section
    }
}