using System;
using System.Collections.Generic;
using System.Text;

namespace FantasyDraft
{
    public class FantasyDraftSettings
    {
        public string Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? StartTime { get; set; }
        public int NumOfTeams { get; set; }
        public List<string> TeamNames { get; set; } //Create FantasyTeam class?
        public int NumOfQB { get; set; }
        public int NumOfRB { get; set; }
        public int NumOfWR { get; set; }
        public int NumOfTE { get; set; }
        public int NumOfFlex { get; set; }
        public int NumOfK { get; set; }
        public int NumOfDEF { get; set; }

        public FantasyDraftSettings()
        {
            StartDate = null;
            StartTime = null;
        }
    }
}
