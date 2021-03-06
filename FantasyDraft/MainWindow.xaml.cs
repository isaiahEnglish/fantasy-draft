﻿using System;
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
            ListOfFootballPlayers = new List<FootballPlayer>();
            FantasyDraftRecord = new FantasyDraftSettings();

            SetDataPath();

            //Scrape web for latest player data?
            // if FootballPlayer table SQL records == null, then webscrape... if not, don't?
            WebScrape();

            //Timer code
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromSeconds(1);
            dt.Tick += Ticker;
            dt.Start();
            ConvertTimerToMinSecFormat();


            //Load previous drafts into list of drafts
            SavedDraftsList = LoadPreviousDraftList();


            //Load football player data from CSV
            LoadPlayerData();
            GrdBigBoard.ItemsSource = ListOfFootballPlayers;

            LeftToRightMarqueeText();

            //New Draft
            NewDraftTeams = new List<NewTeam>();
            GrdNewDraftTeams.ItemsSource = NewDraftTeams;

        }



        // amount of seconds each person gets per pick
        private int SelectionTime = 90;
        // Boolean that determines if the timer has hit 0 or not
        private bool IsOutOfTimeToPick = false;

        public string DataPath { get; set; }

        public string DraftResultFile { get; set; }

        public List<NewTeam> NewDraftTeams { get; set; }


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



        //Rows of raw data, not parsed or split up
        public List<string> ListOfRawPlayers
        {
            get { return (List<string>)GetValue(ListOfPlayersProperty); }
            set { SetValue(ListOfPlayersProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ListOfPlayers.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ListOfPlayersProperty =
            DependencyProperty.Register("ListOfPlayers", typeof(List<string>), typeof(MainWindow));



        //Final, parsed and cleaned up list of players
        public List<FootballPlayer> ListOfFootballPlayers
        {
            get { return (List<FootballPlayer>)GetValue(ListOfFootballPlayersProperty); }
            set { SetValue(ListOfFootballPlayersProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ListOfFootballPlayers.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ListOfFootballPlayersProperty =
            DependencyProperty.Register("ListOfFootballPlayers", typeof(List<FootballPlayer>), typeof(MainWindow));

        


        public FantasyDraftSettings FantasyDraftRecord
        {
            get { return (FantasyDraftSettings)GetValue(FantasyDraftRecordProperty); }
            set { SetValue(FantasyDraftRecordProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FantasyDraftRecord.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FantasyDraftRecordProperty =
            DependencyProperty.Register("FantasyDraftRecord", typeof(FantasyDraftSettings), typeof(MainWindow));




        public List<string> SavedDraftsList
        {
            get { return (List<string>)GetValue(SavedDraftsListProperty); }
            set { SetValue(SavedDraftsListProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SavedDraftsList.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SavedDraftsListProperty =
            DependencyProperty.Register("SavedDraftsList", typeof(List<string>), typeof(MainWindow));










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


        /// <summary>
        /// Sets the DataPath property to be the Data folder in the project parent folder
        /// </summary>
        private void SetDataPath()
        {
            //Get the project file path
            string filePath = System.IO.Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
            //Step back the file path to the root FantasyDraft folder
            filePath = Directory.GetParent(Directory.GetParent(Directory.GetParent(filePath).FullName).FullName).FullName;
            //Add on the \Data subfolder
            filePath += "\\Data";

            DataPath = filePath;
        }


        private List<string> LoadPreviousDraftList()
        {
            string[] draftsRaw = System.IO.File.ReadAllLines(DataPath + "\\FantasyDrafts\\FantasyDrafts.txt");
            string[] drafts;
            List<string> fantasyDraftNamesList = new List<string>();
            fantasyDraftNamesList.Add("");

            foreach (var draft in draftsRaw)
            {
                drafts = draft.Split(',');
                fantasyDraftNamesList.Add(drafts[0]);
            }

            return fantasyDraftNamesList;
        }



        /// <summary>
        /// Loads the data from stored CSV
        /// </summary>
        private void LoadPlayerData()
        {
            // NOTE: Changed data csv file to Embedded Resource... Was initially NONE for build action, may have to change it back if issues arise

            //Get the project file path
            string filePath = DataPath;
            //Add on the "\[name].csv" CSV file
            filePath += "\\FantasyPros_2020_Draft_Overall_Rankings.csv";

            ListOfRawPlayers = Data.LoadCSVFile(filePath);
            ParseDataAndBuildPlayerList(ListOfRawPlayers);
        }

        /// <summary>
        /// Splits data into important pieces of information, parses and cleans data, creates a new instance of FootballPlayer and inserts into list
        /// </summary>
        /// <param name="playersRow"></param>
        private void ParseDataAndBuildPlayerList(List<string> playersRow)
        {
            int i = 0; //row counter, AKA player counter
            foreach (var row in playersRow)
            {
                //Ignore first header row and last two blank rows
                if (i == 0 || i >= 504)
                {
                    i++;
                    continue;
                }

                string[] tokenizedInfo = row.Split(',');

                //Clean data
                string rank = tokenizedInfo[0].Trim('"');
                string name = tokenizedInfo[3].Trim('"');
                string team = tokenizedInfo[4].Trim('"');
                string position = tokenizedInfo[5].Trim('"');
                int bye;
                if (team.Equals("FA")) //if the football player is a free agent
                {
                    bye = 0; //default bye week to week 0
                }
                else
                {
                    bye = int.Parse(tokenizedInfo[6].Trim('"'));
                }
                

                FootballPlayer newPlayer = new FootballPlayer() { Rank = rank, Name = name, Team = team, Position = position, Bye = bye };
                ListOfFootballPlayers.Add(newPlayer);

                i++;
            }
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
            //Add them to team's roster... stored in txt file?
            //Remove player from list of players
            //Add player to list of already selected players?
            FootballPlayer player = (FootballPlayer)selectedPlayer;

            FileStream fileStream = new FileStream(DraftResultFile, FileMode.Append, FileAccess.Write);
            StreamWriter fileWriter = new StreamWriter(fileStream);
            fileWriter.Write(player.Name);
            fileWriter.Flush();
            fileWriter.Close();
        }



        /// <summary>
        /// Insert new draft settings into FantasyDrafts text file
        /// </summary>
        private void InsertNewDraft()
        {
            FileStream fileStream = new FileStream(DataPath + "\\FantasyDrafts\\FantasyDrafts.txt", FileMode.Append, FileAccess.Write);
            StreamWriter fileWriter = new StreamWriter(fileStream);

            string newLine = FantasyDraftRecord.Name + "," + FantasyDraftRecord.StartDate + "," + FantasyDraftRecord.StartTime + "," +
                FantasyDraftRecord.NumOfQB + "," + FantasyDraftRecord.NumOfRB + "," + FantasyDraftRecord.NumOfWR + "," + FantasyDraftRecord.NumOfTE + "," + FantasyDraftRecord.NumOfFlex + "," +
                FantasyDraftRecord.NumOfK + "," + FantasyDraftRecord.NumOfDEF;

            fileWriter.WriteLine(newLine);
            fileWriter.Flush();
            fileWriter.Close();
        }


        /// <summary>
        /// Make sure all draft information is entered properly before saving
        /// </summary>
        private string ValidateNewDraft()
        {
            string message = string.Empty;

            if (string.IsNullOrEmpty(FantasyDraftRecord.Name))
            {
                message += "A Draft Name is required\n";
            }
            if (FantasyDraftRecord.StartDate == null)
            {
                //More validation
                message += "A Start Date is required\n";
            }
            if (FantasyDraftRecord.StartTime == null)
            {
                //More validation
                message += "A Start Time is required\n";
            }
            if (FantasyDraftRecord.NumOfQB == 0)
            {
                message += "The Number of QBs is required\n";
            }
            if (FantasyDraftRecord.NumOfRB == 0)
            {
                message += "The Number of RBs is required\n";
            }
            if (FantasyDraftRecord.NumOfWR == 0)
            {
                message += "The Number of WRs is required\n";
            }
            if (FantasyDraftRecord.NumOfTE == 0)
            {
                message += "The Number of TEs is required\n";
            }
            if (FantasyDraftRecord.NumOfFlex == 0)
            {
                message += "The Number of Flex is required\n";
            }
            if (FantasyDraftRecord.NumOfK == 0)
            {
                message += "The Number of Ks is required\n";
            }
            if (FantasyDraftRecord.NumOfDEF == 0)
            {
                message += "The Number of DEFs is required\n";
            }

            return message;
        }



        /// <summary>
        /// Create the file that holds the Draft Selection Results
        /// </summary>
        private void CreateDraftResultsTxtFile()
        {
            //Create the file name/path for the DRAFT RESULTS to be stored into
            string month, day, year;
            month = PutAZeroOnIt(DateTime.Now.Month);
            day = PutAZeroOnIt(DateTime.Now.Day);
            year = DateTime.Now.Year.ToString().Substring(2, 2); //ALL THIS STUFF SHOULD BE MOVED ELSEWHERE AND CALLED ONCE... otherwise, issues if draft goes past midnight into next day
            string fileName = month + day + year + "_" + FantasyDraftRecord.Name + ".txt";
            string filePath = DataPath + "\\DraftResults\\" + fileName;

            try
            {
                if (!File.Exists(filePath))
                {
                    File.Create(filePath);
                    DraftResultFile = filePath;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        private string PutAZeroOnIt(int value)
        {
            string returnVal = value.ToString();

            if (value < 10)
            {
                returnVal = "0" + value.ToString();
            }

            return returnVal;
        }

        private void BtnDraftPlayer_Click(object sender, RoutedEventArgs e)
        {
            // when a player clicks the draft button, timer count is reset
            SelectionTime = 91;

            SelectPlayer(GrdBigBoard.SelectedItem);
        }

        /// <summary>
        /// Clear settings textboxes, make a new FantasyDraftSettings instance and assign to FantasyDraftRecord
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCreateNewDraft_Click(object sender, RoutedEventArgs e)
        {
            TxtDraftName.Text = TxtStartDate.Text = TxtStartTime.Text = string.Empty;
            
        }

        /// <summary>
        /// Save the newly created Fantasy Draft
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSaveNewDraft_Click(object sender, RoutedEventArgs e)
        {
            string message = ValidateNewDraft();
            if (string.IsNullOrEmpty(message))
            {
                //Confirm new draft
                if (MessageBox.Show("Are you sure you want to create a new Fantasy Draft?","Create New Draft",MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    InsertNewDraft();
                }
            }
            else
            {
                //Errors found in validation
                MessageBox.Show(message, "Could Not Create Draft", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnLoadDraft_Click(object sender, RoutedEventArgs e)
        {
            string[] draftsRaw = System.IO.File.ReadAllLines(DataPath + "\\FantasyDrafts\\FantasyDrafts.txt");
            string[] drafts;

            foreach (var draft in draftsRaw)
            {
                drafts = draft.Split(',');
                if (drafts[0] == CboLoadedDrafts.SelectedItem.ToString())
                {
                    FantasyDraftRecord = new FantasyDraftSettings()
                    {
                        Name = drafts[0],
                        StartDate = DateTime.Parse(drafts[1]),
                        StartTime = DateTime.Parse(drafts[2]),
                        NumOfQB = int.Parse(drafts[3]),
                        NumOfRB = int.Parse(drafts[4]),
                        NumOfWR = int.Parse(drafts[5]),
                        NumOfTE = int.Parse(drafts[6]),
                        NumOfFlex = int.Parse(drafts[7]),
                        NumOfK = int.Parse(drafts[8]),
                        NumOfDEF = int.Parse(drafts[9])
                    };
                }
            }
        }

        private void BtnAddNewTeam_Click(object sender, RoutedEventArgs e)
        {
            NewDraftTeams.Add(new NewTeam());
        }

        private void BtnDeleteNewTeam_Click(object sender, RoutedEventArgs e)
        {
            NewDraftTeams.Remove(NewDraftTeams[GrdNewDraftTeams.SelectedIndex]);
        }
    }

    public class NewTeam
    {
        public string TeamName { get; set; }
        public string OwnerName { get; set; }
    }
}
