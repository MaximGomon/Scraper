using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Scraper.Database;
using Scraper.Models;
using Scraper.ScrapEngine.Contracts;

namespace Scraper.Controllers
{
    [ApiController]
    [Route("scrape")]
    public class ScrapeController : ControllerBase
    {
        private readonly ISiteScraper _scraper;
        private readonly ScraperDataContext _context;
        private readonly ILogger<ScrapeController> _logger;

        public ScrapeController(
            ISiteScraper scraper,
            ILogger<ScrapeController> logger, 
            ScraperDataContext context)
        {
            _scraper = scraper;
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            await _scraper.ScrapAsync();
            var dishes = _context.Dishes
                .Include(x => x.Section)
                .ThenInclude(x => x.Parent)
                .Select(x => new DishViewModel()
                {
                    DishName = x.Title,
                    DishDescription = x.Description,
                    MenuSectionTitle = x.Section.Title,
                    MenuTitle = x.Section.Parent.Title,
                    MenuDescription = x.Section.Parent.Description
                })
                .ToList();
            
            return Ok(dishes);
        }
    }
}
