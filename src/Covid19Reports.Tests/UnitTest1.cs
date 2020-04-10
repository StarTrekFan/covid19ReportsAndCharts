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

            var parser = new VirusTrackerDataParser(virusTrackerDataFile);

            var numRecords = parser.VirusTrackerItems.Count;

            var numInfectionsUS = parser.VirusTrackerItems.Where(item => item.Country == "US").Sum(item => item.Infections);

            var numDeathsUS = parser.VirusTrackerItems.Where(item => item.Country == "US").Sum(item => item.Deaths);

            var numRecoveryUS = parser.VirusTrackerItems.Where(item => item.Country == "US").Sum(item => item.Recovery);

            Assert.True(numRecords > 0);
            Assert.True(numInfectionsUS == 76);
            Assert.True(numDeathsUS == 1);
            Assert.True(numRecoveryUS == 7);
        }

        [Fact]
        public void TestVirusTrackerDataParsingOfAlternateFormat()
        {
            var virusTrackerDataFile = @"TestData\03-29-2020-Format2.csv";

            var parser = new VirusTrackerDataParser(virusTrackerDataFile);

             var numRecords = parser.VirusTrackerItems.Count;

            var numInfectionsUS = parser.VirusTrackerItems.Where(item => item.Country == "US").Sum(item => item.Infections);

            var numDeathsUS = parser.VirusTrackerItems.Where(item => item.Country == "US").Sum(item => item.Deaths);

            var numRecoveryUS = parser.VirusTrackerItems.Where(item => item.Country == "US").Sum(item => item.Recovery);

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

            trackerFiles.ForEach(trackerFile =>{
                    
                var parser = new VirusTrackerDataParser(trackerFile);

                virusTrackerItems.AddRange(parser.VirusTrackerItems);
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
    }
}
