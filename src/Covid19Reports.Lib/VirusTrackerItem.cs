using System;

namespace Covid19Reports.Lib
{
    /*
        VirusTrackerItem is the object that denotes the fundemental piece of info used to build
        all the reports and charts in this app. This object will tell you what the total number of
        Infections,Deaths and Recoveries in a Country & State on a given day.
    */
    public class VirusTrackerItem
    {
        public string Country {get; set;}
        public string ProvinceOrState {get; set;}
        public DateTime StatusDate {get;set;}
        public int Infections {get; set;}
        public int Deaths {get;set;}
        public int Recovery {get;set;}
    }
}
