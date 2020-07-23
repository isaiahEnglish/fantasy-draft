using System;
using System.Collections.Generic;
using System.Text;

namespace FantasyDraft
{
    class WebScraper
    {
        /*
        HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();

        public WebScraper()
        {
            HtmlAgilityPack.HtmlDocument doc = web.Load("https://www.fantasypros.com/nfl/rankings/consensus-cheatsheets.php");

            var HeaderName = doc.DocumentNode.SelectNodes("");
        }
        */

        public WebScraper()
        {
            HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = web.Load("https://www.fantasypros.com/nfl/rankings/consensus-cheatsheets.php");

            var HeaderNames = doc.DocumentNode.SelectNodes("//a[@class='full-name']");

            foreach (var item in HeaderNames)
            {
                Console.WriteLine(item.InnerText);
            }
        }
        
    }
}
