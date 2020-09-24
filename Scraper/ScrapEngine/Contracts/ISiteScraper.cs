using System.Threading.Tasks;

namespace Scraper.ScrapEngine.Contracts
{
    public interface ISiteScraper
    {
        Task ScrapAsync();
    }
}