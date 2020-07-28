using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HtmlAgilityPack;

namespace FantasyDraft
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Scrape web for latest player data?
            // if FootballPlayer table SQL records == null, then webscrape... if not, don't?
            WebScrape();
        }


        /// <summary>
        /// Scrape the fantasy website for football player ranks/info... should be in another class?
        /// </summary>
        private void WebScrape()
        {
            var webGet = new HtmlWeb();
            var document = webGet.Load("https://www.fantasypros.com/nfl/rankings/consensus-cheatsheets.php");
            var espn = webGet.Load("https://fantasy.espn.com/football/players/projections");

            var metaTags = document.DocumentNode.SelectNodes("//span[@class='full-name']");
            var espnMetaTags = document.DocumentNode.SelectNodes("//span[@class='position-eligibility']");

            if (metaTags != null)
            {
                foreach (var tag in metaTags)
                {
                    if (tag != null)
                    {
                        //MessageBox.Show(tag.Attributes["full-name"].Value);
                        MessageBox.Show(tag.InnerText);
                    }
                }
            }
        }

        private void BtnDraftPlayer_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
