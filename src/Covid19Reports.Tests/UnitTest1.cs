using System;
using Xunit;
using Covid19Reports.Lib;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace Covid19Reports.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void TestVirusTrackerDataParsing()
        {
            var virusTrackerDataFile = @"TestData\03-01-2020-Format1.csv";

             var csvHeaderMappings = GetCsvHeaderMappings();

            var parser = new VirusTrackerDataParser(virusTrackerDataFile,csvHeaderMappings);

            var virusTrackerItems = parser.GetVirusTrackerItems();

            var numRecords = virusTrackerItems.Count;

            var numInfectionsUS = virusTrackerItems.Where(item => item.Country == "US").Sum(item => item.Infections);

            var numDeathsUS = virusTrackerItems.Where(item => item.Country == "US").Sum(item => item.Deaths);

            var numRecoveryUS = virusTrackerItems.Where(item => item.Country == "US").Sum(item => item.Recovery);

            Assert.True(numRecords > 0);
            Assert.True(numInfectionsUS == 76);
            Assert.True(numDeathsUS == 1);
            Assert.True(numRecoveryUS == 7);
        }

        [Fact]
        public void TestVirusTrackerDataParsingOfAlternateFormat()
        {
            var virusTrackerDataFile = @"TestData\03-29-2020-Format2.csv";

            var csvHeaderMappings = GetCsvHeaderMappings();

            var parser = new VirusTrackerDataParser(virusTrackerDataFile,csvHeaderMappings);

            var virusTrackerItems = parser.GetVirusTrackerItems();
            
            var numRecords = virusTrackerItems.Count;

            var numInfectionsUS = virusTrackerItems.Where(item => item.Country == "US").Sum(item => item.Infections);

            var numDeathsUS = virusTrackerItems.Where(item => item.Country == "US").Sum(item => item.Deaths);

            var numRecoveryUS = virusTrackerItems.Where(item => item.Country == "US").Sum(item => item.Recovery);

            Assert.True(numRecords > 0);
            Assert.True(numInfectionsUS == 140909);
            Assert.True(numDeathsUS == 2467);
            Assert.True(numRecoveryUS == 2665);
        }

        [Fact]
        public void TestParsingAllFilesTogether()
        {
            var trackerFiles = Directory.GetFiles(@"TestData").ToList();

            var virusTrackerItems = new List<VirusTrackerItem>();

            var csvHeaderMappings = GetCsvHeaderMappings();

            trackerFiles.ForEach(trackerFile =>{
                    
                var parser = new VirusTrackerDataParser(trackerFile,csvHeaderMappings);

                virusTrackerItems.AddRange(parser.GetVirusTrackerItems());
            });

            using (var writer = new StreamWriter("ConsolidatedReport.csv"))
            {
                using (var csv = new CsvHelper.CsvWriter(writer,CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(virusTrackerItems);
                }
            }

        }

        [Fact]
        public void TestVirusTrackerDataConsolidator()
        {
            var consolidator = new VirusTrackerDataConsolidator();

            consolidator.VirusDataFolder = "TestData";

            consolidator.ConsolidateData();

            var virusTrackerItems = consolidator.VirusTrackerItems;

            Assert.True(virusTrackerItems.Count > 0);
        }
        
        private  Dictionary<string,List<string>> GetCsvHeaderMappings()
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
