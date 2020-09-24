using System.ComponentModel.DataAnnotations;

namespace Scraper.Database.Entities
{
    public class Operation : BaseEntity
    {
        [Required]
        public OperationType Type { get; set; }
        
        [Required]
        public bool IsCompleted { get; set; }
    }

    public enum OperationType
    {
        Scrap
    }
}