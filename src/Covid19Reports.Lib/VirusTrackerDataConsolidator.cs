using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace Covid19Reports.Lib
{
    /*
      The CSV files that contains the data that tracks the COVID19 infections is supposed to contain
      only one days consolidated data. For instance if the file named "03-13-2020.csv" is supposed
      to contain only the data for 3/13/2020. However this is not always the case. Sometimes  another
      day's consolidated data may contain in the same file. This class will make the decision
      which data should be used when it encounters multiple consolidations for the same days data

      Another issue this class solves is that some country names are not spelled correctly at all times. 
      For instance, China is refered to as both China & MainLand China. In some places United Kingdom is refered 
      to as UK and so on. This class will combine such cases using the property CountryMapping which is a
      dictionary object that contains the Country Name Mappings.

      Similar to Country Name mappings, we also need to do Provice or State levels mappings as well. 
      This data was tracked at the state level earlier on. At some point in mid march, John Hopkins University
      started to track data at the county and city levels for some states and provinces. We will map such
      places to their corresponding states or provices. For example "Tempe AZ" will be mapped to Arizona or
      "Cobb County, GA" will be mapped to "Georgia". This data will be maintained in the property ProvinceOrStateMapping.
      
     
      The VirusTrackerDataParser deals with one CSV file at a time. This class will deal with data in all the 
      CSV files at once.
    */
    public class VirusTrackerDataConsolidator
    {
        //This is the folder where all the CSV files are situated
        public string VirusDataFolder {get;set;}

        //The key Item will contain the name of the country that needs to be 
        //replaced with the value item in this dictionary object
        public Dictionary<string,string> CountryMapping {get; set;}

        //The key item will contain the name of the state that needs to be replaced
        //with the value item of the doctionary object
        public Dictionary<string,string> ProvinceOrStateMapping {get;set;}

        //There are certain dates where the tracking is just not right or imcomplete
        //We want to Ignore such data. The consolidator will remove infections and other
        //date when it encounters such dates
        public List<DateTime> IgnoredDates {get;set;}
      
        //This is the final consolidated list of VirusTrackerItems.
        public List<VirusTrackerItem> VirusTrackerItems {get;set;}

        public void ConsolidateData()
        {
            ValidateInputs();

            VirusTrackerItems = new List<VirusTrackerItem>();

            var virusTrackerFiles = Directory.GetFiles(VirusDataFolder,"*.csv").ToList();

            virusTrackerFiles.ForEach(trackerFile => {

                var virusTrackerItems = GetVirusTrackerItems(trackerFile);

                foreach(var virusTrackerItem in virusTrackerItems)
                {
                    //If this data is on the dates that we want to Ignore, then do not include this
                    //data in the final list
                    if (IgnoredDates.Any(dateToIgnore => dateToIgnore.ToShortDateString().Equals(virusTrackerItem.StatusDate.ToShortDateString())))
                        continue;

                    //Update the country Name if it is in the list of country names to be updated
                    if (CountryMapping.Keys.Contains(virusTrackerItem.Country))
                        virusTrackerItem.Country = CountryMapping[virusTrackerItem.Country] ?? virusTrackerItem.Country;

                    //Update the Province or State Name if it is in the list of state names to be updated
                    if (ProvinceOrStateMapping.Keys.Contains(virusTrackerItem.ProvinceOrState))
                        virusTrackerItem.ProvinceOrState = ProvinceOrStateMapping[virusTrackerItem.ProvinceOrState] ?? virusTrackerItem.ProvinceOrState;
                  
                   if  (!VirusTrackerItems.Any(aItem => aItem.Country == virusTrackerItem.Country && aItem.ProvinceOrState == virusTrackerItem.ProvinceOrState && aItem.StatusDate.Equals(virusTrackerItem.StatusDate)))
                   {
                        VirusTrackerItems.Add(virusTrackerItem);
                   }
                   else
                   {
                      var existingVirusTrackerItem =  VirusTrackerItems.First(aItem => aItem.Country == virusTrackerItem.Country && aItem.ProvinceOrState == virusTrackerItem.ProvinceOrState && aItem.StatusDate.Equals(virusTrackerItem.StatusDate));

                      UpdateTrackerItem(virusTrackerItem,existingVirusTrackerItem);
                     
                   }
                }

            });

        }

        //Ensures all properties that is needed by this class is assigned and initialized or else
        //throws an exception to prevent the program from moving forward
        private void ValidateInputs()
        {
            if (!Directory.Exists(VirusDataFolder))
                throw new Exception("VirusDataFolder property is not valid");

            if (CountryMapping == null)   
                 throw new Exception("CountryMapping property is not assigned");
            
             if (ProvinceOrStateMapping == null)   
                 throw new Exception("ProvinceOrStateMapping property is not assigned");

             if (IgnoredDates == null)
                throw new Exception("IgnoredDates property is not assigned");

        }
        private List<VirusTrackerItem> GetVirusTrackerItems(string trackerFile)
        {
            var parser = new VirusTrackerDataParser(trackerFile);

            return parser.VirusTrackerItems;
        }

        private void UpdateTrackerItem(VirusTrackerItem virusTrackerItem, VirusTrackerItem existingVirusTrackerItem )
        {
            //Use the most recent tracking data
            if (virusTrackerItem.StatusDate > existingVirusTrackerItem.StatusDate)
            {
                existingVirusTrackerItem.Infections = virusTrackerItem.Infections;
                existingVirusTrackerItem.Deaths = virusTrackerItem.Deaths;
                existingVirusTrackerItem.Recovery = virusTrackerItem.Recovery;
                existingVirusTrackerItem.StatusDate = virusTrackerItem.StatusDate;
            }

            //if the status date is the same, then use the data that has higher infection count
            if (virusTrackerItem.StatusDate == existingVirusTrackerItem.StatusDate && virusTrackerItem.Infections > existingVirusTrackerItem.Infections)
            {
                existingVirusTrackerItem.Infections = virusTrackerItem.Infections;
                existingVirusTrackerItem.Deaths = virusTrackerItem.Deaths;
                existingVirusTrackerItem.Recovery = virusTrackerItem.Recovery;
                existingVirusTrackerItem.StatusDate = virusTrackerItem.StatusDate;
            }
        }
    }
}