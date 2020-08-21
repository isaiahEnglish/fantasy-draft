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
using System.Windows.Media.Animation;

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

            //Load football player data from CSV
            LoadData();

            LeftToRightMarqueeText();

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

        private void LoadData()
        {
            // NOTE: Changed data csv file to Embedded Resource... Was initially NONE for build action, may have to change it back if issues arise

            //Get the project file path
            string filePath = System.IO.Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
            //Step back the file path to the root FantasyDraft folder
            filePath = Directory.GetParent(Directory.GetParent(Directory.GetParent(filePath).FullName).FullName).FullName;
            //Add on the "\Data" subfolder and then the "\[name].csv" CSV file
            filePath += "\\Data\\FantasyPros_2020_Draft_Overall_Rankings.csv";

            //Load the data into temp for now... should be made into 2D array later?
            List<string> temp = Data.LoadCSVFile(filePath);

        }

        /// <summary>
        /// Creates the ticker that shows the draft picks
        /// </summary>
        void LeftToRightMarqueeText()
        {
            string Copy = " " + TextBoxMarquee.Text;
            double TextGraphicalWidth = new FormattedText(Copy, System.Globalization.CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight, new Typeface(TextBoxMarquee.FontFamily.Source), TextBoxMarquee.FontSize, TextBoxMarquee.Foreground).WidthIncludingTrailingWhitespace;
            double TextLenghtGraphicalWidth = 0;
            //BorderTextBoxMarquee.Width = TextGraphicalWidth + 5;
            while (TextLenghtGraphicalWidth < TextBoxMarquee.ActualWidth)
            {
                TextBoxMarquee.Text += Copy;
                TextLenghtGraphicalWidth = new FormattedText(TextBoxMarquee.Text, System.Globalization.CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight, new Typeface(TextBoxMarquee.FontFamily.Source), TextBoxMarquee.FontSize, TextBoxMarquee.Foreground).WidthIncludingTrailingWhitespace;
            }
            TextBoxMarquee.Text += " " + TextBoxMarquee.Text;
            ThicknessAnimation ThickAnimation = new ThicknessAnimation();
            ThickAnimation.From = new Thickness(0, 0, 0, 0);
            ThickAnimation.To = new Thickness(-TextGraphicalWidth, 0, 0, 0);
            ThickAnimation.RepeatBehavior = RepeatBehavior.Forever;
            ThickAnimation.Duration = new Duration(TimeSpan.FromSeconds(2.5)); //Speed of ticker -> Less seconds, faster ticker | More seconds, slower animation
            TextBoxMarquee.BeginAnimation(TextBox.PaddingProperty, ThickAnimation);
        }

        private void SelectPlayer(object selectedPlayer)
        {
            //Find the player in the list of players
            //Add them to team's roster
            //Remove player from list of players
            //Add player to list of already selected players?
        }

        private void BtnDraftPlayer_Click(object sender, RoutedEventArgs e)
        {
            // when a player clicks the draft button, timer count is reset
            SelectionTime = 91;

            SelectPlayer(GrdBigBoard.SelectedItem);
        }

        private void GrdBigBoard_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
