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
 
        private string _trackerFile;
        private Dictionary<string,List<string>> CsvHeaderMappings {get;set;}

        private List<VirusTrackerItem> _virusTrackerItems;

        public VirusTrackerDataParser(string trackerFile,Dictionary<string,List<string>> csvHeaderMappings)
        {
            if (trackerFile == null)
                throw new Exception("Tracker File is not valid");

            if (!File.Exists(trackerFile))
                throw new Exception("Tracker File is invalid");
            
            _trackerFile = trackerFile;

            CsvHeaderMappings = csvHeaderMappings;

            _virusTrackerItems = new List<VirusTrackerItem>();
        
        }

        public List<VirusTrackerItem> GetVirusTrackerItems()
        {
            var trackerFileInfo =new FileInfo(_trackerFile);

            //Starting with 4/23, we are unable to trust the LastUpdateDate on the CSV files. We will replace
            //the LastUpdate date with the tracker file date of they differ
            var trackerFileDate = DateTime.Parse(trackerFileInfo.Name.Replace(trackerFileInfo.Extension,string.Empty));

            using (var reader = new StreamReader(_trackerFile))
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

                            if (!virusTrackerItem.StatusDate.ToShortDateString().Equals(trackerFileDate.ToShortDateString()))
                                virusTrackerItem.StatusDate = trackerFileDate;
    
                            AddVirusTrackerItem(virusTrackerItem);
                        });

                }
            }
            return _virusTrackerItems;
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
            if (!_virusTrackerItems.Any(item => item.Country.Equals(virusTrackerItem.Country) && item.ProvinceOrState.Equals(virusTrackerItem.ProvinceOrState) && item.StatusDate.Equals(virusTrackerItem.StatusDate)))
                _virusTrackerItems.Add(virusTrackerItem);
            else
            {
                var existingVirusTrackerItem = _virusTrackerItems.First(item => item.Country.Equals(virusTrackerItem.Country) && item.ProvinceOrState.Equals(virusTrackerItem.ProvinceOrState) && item.StatusDate.Equals(virusTrackerItem.StatusDate));

                existingVirusTrackerItem.Infections += virusTrackerItem.Infections;

                existingVirusTrackerItem.Deaths += virusTrackerItem.Deaths;

                existingVirusTrackerItem.Recovery += virusTrackerItem.Recovery;
            }
        }

        private  string GetData(string columnName,IDictionary<string, object> dynamicRecord)
        {
            string returnValue = null;

            if (CsvHeaderMappings[columnName].Count() == 1)
                returnValue = dynamicRecord[CsvHeaderMappings[columnName].First()].ToString();

            if (string.IsNullOrEmpty(returnValue))
            {
                var columnHeaders = CsvHeaderMappings[columnName];

                foreach(var columnHeader in columnHeaders)
                {
                    if (dynamicRecord.Keys.Contains(columnHeader))
                        returnValue = dynamicRecord[columnHeader].ToString();
                }
            }

            return  returnValue;
        }
   
      
    }
}