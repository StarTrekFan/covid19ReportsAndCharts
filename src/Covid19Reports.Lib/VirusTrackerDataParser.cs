using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using System.Globalization;
using System.Linq;

namespace Covid19Reports.Lib
{
    public class VirusTrackerDataParser
    {
        public List<VirusTrackerItem> VirusTrackerItems {get;set;}
        public VirusTrackerDataParser(string trackerFile)
        {
            if (trackerFile == null)
                throw new Exception("Tracker File is not valid");

            if (!File.Exists(trackerFile))
                throw new Exception("Tracker File is invalid");

            VirusTrackerItems = new List<VirusTrackerItem>();

            using (var reader = new StreamReader(trackerFile))
            {
                using (var csv = new CsvReader(reader,CultureInfo.InvariantCulture))
                {
                    
                    csv.Configuration.HasHeaderRecord = true;
                                          
                    csv.GetRecords<dynamic>()
                       .ToList()
                       .ForEach(record =>
                        {
                            var dynamicRecord = (IDictionary<string, object>) record;

                            var virusTrackerItem = GetVirusTrackerItem(dynamicRecord);
    
                            AddVirusTrackerItem(virusTrackerItem);
                        });

                }
            }
        }

        private VirusTrackerItem GetVirusTrackerItem(dynamic record)
        {
             var dataItem = (IDictionary<string, object>) record;

            var virusTrackerItem = new VirusTrackerItem() 
            {
                Country = GetData("Country",dataItem),
                ProvinceOrState = GetData("ProvinceOrState",dataItem),
                StatusDate = DateTime.Parse(GetData("StatusDate",dataItem)),
                Infections = GetData("Infections",dataItem).Trim() == string.Empty ? 0 : Int32.Parse(GetData("Infections",dataItem)),
                Deaths = GetData("Deaths",dataItem).Trim() == string.Empty ? 0 : Int32.Parse(GetData("Deaths",dataItem)),
                Recovery = GetData("Recovery",dataItem).Trim() == string.Empty ? 0 : Int32.Parse(GetData("Recovery",dataItem))
            };

            return virusTrackerItem;
        }
        private void AddVirusTrackerItem(VirusTrackerItem virusTrackerItem)
        {
            if (!VirusTrackerItems.Any(item => item.Country.Equals(virusTrackerItem.Country) && item.ProvinceOrState.Equals(virusTrackerItem.ProvinceOrState) && item.StatusDate.Equals(virusTrackerItem.StatusDate)))
                VirusTrackerItems.Add(virusTrackerItem);
            else
            {
                var existingVirusTrackerItem = VirusTrackerItems.First(item => item.Country.Equals(virusTrackerItem.Country) && item.ProvinceOrState.Equals(virusTrackerItem.ProvinceOrState) && item.StatusDate.Equals(virusTrackerItem.StatusDate));

                existingVirusTrackerItem.Infections += virusTrackerItem.Infections;

                existingVirusTrackerItem.Deaths += virusTrackerItem.Deaths;

                existingVirusTrackerItem.Recovery += virusTrackerItem.Recovery;
            }
        }

        private static string GetData(string columnName,IDictionary<string, object> dynamicRecord)
        {
            string returnValue = null;

            if (GetColumnHeaders()[columnName].Count() == 1)
                returnValue = dynamicRecord[GetColumnHeaders()[columnName].First()].ToString();

            if (string.IsNullOrEmpty(returnValue))
            {
                var columnHeaders = GetColumnHeaders()[columnName];

                foreach(var columnHeader in columnHeaders)
                {
                    if (dynamicRecord.Keys.Contains(columnHeader))
                        returnValue = dynamicRecord[columnHeader].ToString();
                }
            }

            return  returnValue;
        }
   
        private static Dictionary<string,List<string>> GetColumnHeaders()
        {
            var columnHeaders = new Dictionary<string,List<string>>()
            {
                {"Country", new List<string>() {"Country/Region","Country_Region"}},
                {"ProvinceOrState", new List<string>() {"Province/State","Province_State"}},
                {"StatusDate", new List<string>() {"Last Update","Last_Update"}},
                {"Infections", new List<string>() {"Confirmed"}},
                {"Deaths", new List<string>() {"Deaths",""}},
                {"Recovery", new List<string>() {"Recovered"}}
            };

            return columnHeaders;
        }
    }
}