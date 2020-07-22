using System;
using System.Linq;
using System.Reflection.Metadata;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            // this one works based off of this example: https://dev.to/rachelsoderberg/create-a-simple-web-scraper-in-c-1l1m
            HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = web.Load("https://www.yellowpages.com/search?search_terms=Software&geo_location_terms=Sydney%2C+ND");

            var HeaderNames = doc.DocumentNode
                .SelectNodes("//a[@class='business-name']").ToList();

            foreach (var item in HeaderNames)
            {
                Console.WriteLine(item.InnerText);
            }

            // this example isn't working for ESPN
            /*
            HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = web.Load("https://fantasy.espn.com/football/players/projections");

            var HeaderNames = doc.DocumentNode
                .SelectNodes("//a[@class='player-name']").ToList();

            foreach (var item in HeaderNames)
            {
                Console.WriteLine(item.InnerText);
            }
            */
        }
    }
}
