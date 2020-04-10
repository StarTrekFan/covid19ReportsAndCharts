using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Binder;
using Microsoft.Extensions.Configuration.Json;
using Covid19Reports.Lib;
using Covid19Reports.Lib.Publisher;
using System.Collections.Generic;
using System.Linq;

namespace Covid19Reports.App
{
    class Program
    {

        private static IConfigurationRoot _configRoot;
        static void Main(string[] args)
        {
            var virusTrackerItems = GetVirusTrackerItems();


            var landingPagePublisher = new LandingPagePublisher();

            landingPagePublisher.DestinationFolder = ConfigRoot["DestinationFolder"];

            landingPagePublisher.VirusTrackerItems = virusTrackerItems;

            landingPagePublisher.PublishWebReports();

            var countries = virusTrackerItems.Select(item => item.Country).Distinct().ToList();
            
            Publishers.ForEach(publisher => {

                countries.ForEach(country => {
                     PublishReport(publisher,country,virusTrackerItems.Where(item => item.Country == country).ToList());
                  });
               
            });

        
            var virusTrackerItemsForOtherCountries = virusTrackerItems.Where(item => GetCountriesToCompare().Any(oCountry => oCountry == item.Country));

            countries.ForEach(country => {
              
                var comparisonReportPublisher = new ComparisonReportPublisher();

                comparisonReportPublisher.Country = country;

                comparisonReportPublisher.DestinationFolder = ConfigRoot["DestinationFolder"];

                comparisonReportPublisher.OtherCountries = GetCountriesToCompare();

                var virusTrackerItemsCombined = new List<VirusTrackerItem>();

                virusTrackerItemsCombined.AddRange(virusTrackerItemsForOtherCountries);

                virusTrackerItemsCombined.AddRange(virusTrackerItems.Where(item => item.Country == country));
               
                comparisonReportPublisher.VirusTrackerItems = virusTrackerItemsCombined;

                comparisonReportPublisher.PublishWebReports();

            });
        }


        private static void PublishReport(Covid19ReportPublisher publisher,string country, List<VirusTrackerItem> virusTrackerItems)
        {
             publisher.Country = country;

             publisher.DestinationFolder = ConfigRoot["DestinationFolder"];

             publisher.OtherCountries = GetCountriesToCompare();

             publisher.VirusTrackerItems = virusTrackerItems;

             publisher.PublishWebReports();

        }
        private static List<VirusTrackerItem> GetVirusTrackerItems()
        {
            var dataConsolidator = new VirusTrackerDataConsolidator();

            dataConsolidator.VirusDataFolder = ConfigRoot["TrackerDataLocation"];

            dataConsolidator.IgnoredDates = GetIgnoredDates();

            dataConsolidator.CountryMapping = GetCountryMappings();

            dataConsolidator.ProvinceOrStateMapping = GetProvinceOrStateMappings();

            dataConsolidator.ConsolidateData();

            return dataConsolidator.VirusTrackerItems;
        }
        private static IConfigurationRoot ConfigRoot
        {
            get
            {
                if (_configRoot == null)
                {
                    var configBuilder = new ConfigurationBuilder();
                
                    configBuilder.AddJsonFile("appSettings.json",true);

                    _configRoot = configBuilder.Build();
                }
               
                return _configRoot;
            }
        }

        private static List<DateTime> GetIgnoredDates()
        {
             /* var ignoredDates = new List<DateTime>();

              ConfigRoot.GetSection("IgnoredDates").Bind(ignoredDates);

              return ignoredDates;
            */
            return ConfigRoot.GetSection("IgnoredDates").Get<List<DateTime>>();
        }
        
        private static Dictionary<string,string> GetCountryMappings()
        {
           return ConfigRoot.GetSection("CountryMapping").Get<Dictionary<string,string>>();
        }

        private static Dictionary<string,string> GetProvinceOrStateMappings()
        {
            return ConfigRoot.GetSection("ProvinceOrStateMapping").Get<Dictionary<string,string>>();
        }
    
        private static List<string> GetCountriesToCompare()
        {
            return ConfigRoot.GetSection("CountriesToCompare").Get<List<string>>();
        }

        private static List<Covid19ReportPublisher> Publishers
        {
            get
            {
                return new List<Covid19ReportPublisher>() 
                {
                 { new CountryDataPublisher() },
                 { new CountryLandingPagePublisher() },
                 { new ProvinceOrStateDataPublisher() }
                };
            }
        }
    }

}
