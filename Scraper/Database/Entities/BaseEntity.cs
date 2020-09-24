using System;
using System.ComponentModel.DataAnnotations;

namespace Scraper.Database.Entities
{
    public class BaseEntity
    {
        public BaseEntity()
        {
            Id = Guid.NewGuid();
        }

        [Key]
        public Guid Id { get; private set; }
    }
}