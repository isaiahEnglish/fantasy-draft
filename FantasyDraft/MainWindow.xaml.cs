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
using System.Windows.Threading;
using System.Threading;
using System.IO;

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

            DraftState = 1;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            //Scrape web for latest player data?
            // if FootballPlayer table SQL records == null, then webscrape... if not, don't?
            WebScrape();

            //Timer code
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromSeconds(1);
            dt.Tick += Ticker;
            dt.Start();
            ConvertTimerToMinSecFormat();
        }



        // amount of seconds each person gets per pick
        private int SelectionTime = 90;
        // Boolean that determines if the timer has hit 0 or not
        private bool IsOutOfTimeToPick = false;
        

        // Property that keeps track of the status of the draft... Predraft, Middraft, or Post-Draft
        // Pre-Draft = 0 | Mid-Draft = 1 | Post-Draft = 2... To change for testing purposes, change the "PropertyMetaData(#)" number below to whatever state you want
        public int DraftState
        {
            get { return (int)GetValue(DraftStatesProperty); }
            set { SetValue(DraftStatesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DraftState.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DraftStatesProperty =
            DependencyProperty.Register("DraftState", typeof(int), typeof(MainWindow), new PropertyMetadata(1));







        // decrements the count of the ticker
        private void Ticker(object sender, EventArgs e)
        {
            SelectionTime--;

            // test case if person didn't choose a player in time
            if(SelectionTime < 0)
            {
                SelectionTime = 0;
                IsOutOfTimeToPick = true;
            }
            ConvertTimerToMinSecFormat();
        }


        /// <summary>
        /// This gets called so we can change seconds to a Min:Sec format
        /// Ex: 90 sec -> 1:30
        /// </summary>
        private void ConvertTimerToMinSecFormat()
        {
            // Mod 60 gets seconds
            int rawSeconds = SelectionTime % 60;
            // Div function gets minutes
            int minutes = SelectionTime / 60;
            string seconds = "";

            //If there is 0 - 9 seconds..
            if (rawSeconds < 10)
            {
                //Add a '0' to the front of seconds... to get 1:09 instead of 1:9
                seconds = "0" + rawSeconds.ToString();
            }
            else
            {
                seconds = rawSeconds.ToString();
            }

            this.LblTimerMinutes.Content = minutes;
            this.LblTimerSeconds.Content = seconds;
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
                        //MessageBox.Show(tag.InnerText);
                    }
                }
            }
        }


        private static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }


        



        private void BtnDraftPlayer_Click(object sender, RoutedEventArgs e)
        {
            // when a player clicks the draft button, timer count is reset
            SelectionTime = 91;  
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void BtnLoadLogo_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".txt";
            dlg.Filter = "PNG Files (*.png)|*.png|JPEG Files (*.jpeg)|*.jpeg|JPG Files (*.jpg)|*.jpg";

            string lclogopath = "";
            //lclogopath = Setting.GetSettingPath("LOGO_PATH") + "PNG";                                      Setting does not exist...
            dlg.InitialDirectory = lclogopath;
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                byte[] img = null;
                FileStream fs = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fs);
                img = br.ReadBytes((int)fs.Length);
                //FantasyTeamDBRecord.TeamLogo = img;  <-- puts img into TeamLogo SQL field in FantasyTeam database
                //Data.Context.SaveChanges();    <-- saves changes... we may need to do this another way
                //ImgFantasyLogo.Source = LoadImage(/*FantasyTeamDBRecord.TeamLogo*/);
            }
        }
    }
}
