using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scraper.Database;
using Scraper.Database.Entities;
using Scraper.Options;
using Scraper.ScrapEngine.Contracts;

namespace Scraper.ScrapEngine.Impl
{
    public class PureSiteScraper : ISiteScraper
    {
        private readonly ScraperDataContext _context;
        private readonly ILogger<PureSiteScraper> _logger;
        private readonly Url _siteUrl;

        public PureSiteScraper(
            IOptions<AppSettingsOptions> options, 
            ScraperDataContext context, 
            ILogger<PureSiteScraper> logger)
        {
            _context = context;
            _logger = logger;
            _siteUrl = new Url(options.Value.SiteUrl);
        }

        public async Task ScrapAsync()
        {
            if (_context.Operations.Any(x => x.Type == OperationType.Scrap && x.IsCompleted))
                return;

            //4 testing purposes
            //await CleanDatabaseAsync();

            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            
            var document = await context.OpenAsync(_siteUrl);
            var menuItemsNodes = document.All.First(m => m.LocalName == "nav" && m.ClassList.Contains("nav-menus"))
                .Children.First();

            await ScrapMenuItems();

            async Task ScrapMenuItems()
            {
                foreach (var itemsNode in menuItemsNodes.Children)
                {
                    if (string.Equals(itemsNode.TextContent, "Wellbeing Boxes", StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    var link = itemsNode.QuerySelectorAll("a").First().GetAttribute("href");
                    var nodeUrl = new Url($"{_siteUrl.Origin}{link}");

                    var menuDocument = await context.OpenAsync(nodeUrl);
                    var description = menuDocument.All
                        .First(x => x.LocalName == "header" && x.ClassList.Contains("menu-header"))
                        .Children
                        .Last(x => x.LocalName == "p")
                        .TextContent;

                    var menuItem = new MenuItem()
                    {
                        Title = itemsNode.TextContent.Trim(),
                        Type = MenuItemType.Global,
                        Description = description.Trim()
                    };
                    _context.MenuItems.Add(menuItem);

                    var submenuItemNodes =
                        menuDocument.All.Where(x => x.LocalName == "h4" && x.ClassList.Contains("menu-title"));

                    await ScrapMenuSections(submenuItemNodes, menuItem);
                }
            }

            async Task ScrapMenuSections(IEnumerable<IElement> submenuItemNodes, MenuItem menuItemEntity)
            {
                foreach (var submenuItemNode in submenuItemNodes)
                {
                    var subMenuItem = new MenuItem()
                    {
                        Parent = menuItemEntity,
                        Type = MenuItemType.Section,
                        Title = submenuItemNode.QuerySelectorAll("span").First().InnerHtml.Trim(),
                    };
                    _context.MenuItems.Add(subMenuItem);


                    var dishListNode = submenuItemNode.NextElementSibling.Children.First();
                    await ScrapDishes(dishListNode, subMenuItem);
                }
            }

            async Task ScrapDishes(IElement element, MenuItem subMenuItemEntity)
            {
                foreach (var dishNode in element.Children)
                {
                    var dishANode = dishNode.Children.First();
                    var dishUrl = new Url($"{_siteUrl.Origin}{dishANode.GetAttribute("href")}");

                    var dishDocument = await context.OpenAsync(dishUrl);
                    var details = dishDocument.All.First(x => x.ClassName == "menu-item-details");
                    var dishDescription = details.Children.First(x => x.LocalName == "div").TextContent;

                    var dish = new Dish()
                    {
                        Title = dishANode.GetAttribute("title").Trim(),
                        Description = dishDescription.Trim(),
                        Section = subMenuItemEntity
                    };
                    _context.Dishes.Add(dish);

                    _logger.LogInformation($"Successfully scraped dish \"{dish.Title}\" under section \"{subMenuItemEntity.Title}\"");
                }
            }

            await _context.SaveChangesAsync();

            _context.Operations.Add(new Operation() {Type = OperationType.Scrap, IsCompleted = true});
            await _context.SaveChangesAsync();
        }

        private async Task CleanDatabaseAsync()
        {
            _context.MenuItems.RemoveRange(_context.MenuItems.Where(x => x.Parent != null));
            _context.MenuItems.RemoveRange(_context.MenuItems.Where(x => x.Parent == null));
            await _context.SaveChangesAsync();
        }
    }
}