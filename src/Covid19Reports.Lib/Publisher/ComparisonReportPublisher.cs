using System;
using System.Linq;
using System.IO;

namespace Covid19Reports.Lib.Publisher
{
    public class ComparisonReportPublisher : Covid19ReportPublisher
    {
        public override void PublishWebReports()
        {
            ValidateInputs();

            OtherCountries.ForEach(oCountry => PublishCompareReport(oCountry));
        }

        private void PublishCompareReport (string otherCountry)
        {
            var reportName = string.Format(@"{0}\{1}-{2}-Page.html",DestinationFolder,Country,otherCountry);

            var distinctDates = VirusTrackerItems
                                    .Where(item => item.Country.Equals(Country))
                                    .Select(item => item.StatusDate.ToShortDateString())
                                    .Distinct()
                                    .OrderBy(item => DateTime.Parse(item));
            
            var consolidatedTrackerItems =  distinctDates.Select(statusDate => new {
                    Country = Country,
                    StatusDate = statusDate,
                    Infections = VirusTrackerItems.Where(item => item.StatusDate.ToShortDateString() == statusDate && item.Country == Country).Sum(item => item.Infections),
                    Deaths = VirusTrackerItems.Where(item => item.StatusDate.ToShortDateString() == statusDate && item.Country == Country).Sum(item => item.Deaths),
                    Recovery = VirusTrackerItems.Where(item => item.StatusDate.ToShortDateString() == statusDate && item.Country == Country).Sum(item => item.Recovery),
                    OtherCountryInfections = VirusTrackerItems.Where(item => item.StatusDate.ToShortDateString() == statusDate && item.Country == otherCountry).Sum(item => item.Infections),
                    OtherCountryDeaths = VirusTrackerItems.Where(item => item.StatusDate.ToShortDateString() == statusDate && item.Country == otherCountry).Sum(item => item.Deaths),
                    OtherCountryRecovery = VirusTrackerItems.Where(item => item.StatusDate.ToShortDateString() == statusDate && item.Country == otherCountry).Sum(item => item.Recovery)
            });

            //Create the Filesonly if the number of infections is more than zero
            if (consolidatedTrackerItems.Last().Deaths == 0)
                return;

            var template = File.ReadAllText(@"Templates\ComparePageTemplate.txt");

            var infectionsTitle = string.Format("COVID-19 Infections - {0} & {1}",Country,otherCountry);

            var deathsTitle = string.Format("COVID-19 Deaths - {0} & {1}",Country,otherCountry);

            var recoveryTitle = string.Format("COVID-19 Recovery - {0} & {1}",Country,otherCountry);


            var infectionsChartData = consolidatedTrackerItems.Aggregate(string.Format("['Date', '{0} Infections','{1} Infections']",Country,otherCountry),(curr,next) => curr + "," + "['" + DateTime.Parse(next.StatusDate).ToString("MM/dd") + "'," + next.Infections + "," + next.OtherCountryInfections + "]");

            var deathsChartData = consolidatedTrackerItems.Aggregate(string.Format("['Date', '{0} Deaths','{1} Deaths']",Country,otherCountry),(curr,next) => curr + "," + "['" + DateTime.Parse(next.StatusDate).ToString("MM/dd") + "'," + next.Deaths + "," + next.OtherCountryDeaths + "]");

            var recoveryChartData = consolidatedTrackerItems.Aggregate(string.Format("['Date', '{0} Recovery','{1} Recovery']",Country,otherCountry),(curr,next) => curr + "," + "['" + DateTime.Parse(next.StatusDate).ToString("MM/dd") + "',"  + next.Recovery  + "," + next.OtherCountryRecovery + "]");

            var infectionProgressBarWidth = Math.Round(( (double) consolidatedTrackerItems.Last().OtherCountryInfections / (double) consolidatedTrackerItems.Last().Infections) * 100);

            var deathsProgressBarWidth = Math.Round(( (double) consolidatedTrackerItems.Last().OtherCountryDeaths / (double) consolidatedTrackerItems.Last().Deaths) * 100);

            var recoveryProgressBarWidth = Math.Round(( (double) consolidatedTrackerItems.Last().OtherCountryRecovery / (double) consolidatedTrackerItems.Last().Recovery) * 100);

            template = template.Replace("INFECTIONTITLEGOESHERE",infectionsTitle);

            template = template.Replace("DEATHSTITLEGOESHERE",deathsTitle);

            template = template.Replace("RECOVERYTITLEGOESHERE",recoveryTitle);

            template = template.Replace("COUNTRYNAME",Country);

            template = template.Replace("COMPARETOCOUNTRY",otherCountry);

            template = template.Replace("COUNTRYPAGE",string.Format("{0}-Page.html",Country));

            template = template.Replace("INFECTIONDATAGOESHERE",infectionsChartData);
        
            template = template.Replace("DEATHSDATAGOESHERE",deathsChartData);

            template = template.Replace("RECOVERYDATAGOESHERE",recoveryChartData);


            template = template.Replace("TOTALINFECTIONS",string.Format("{0} - {1}",Country,consolidatedTrackerItems.Last().Infections.ToString()));
            template = template.Replace("OTHERCOUNTRYINFECTIONS",string.Format("{0} - {1}",otherCountry,consolidatedTrackerItems.Last().OtherCountryInfections.ToString()));

            template = template.Replace("TOTALDEATHS",string.Format("{0} - {1}",Country,consolidatedTrackerItems.Last().Deaths.ToString()));
            template = template.Replace("OTHERCOUNTRYDEATHS",string.Format("{0} - {1}",otherCountry,consolidatedTrackerItems.Last().OtherCountryDeaths.ToString()));

            template = template.Replace("TOTALRECOVERIES",string.Format("{0} - {1}",Country,consolidatedTrackerItems.Last().Recovery.ToString()));
            template = template.Replace("OTHERCOUNTRYRECOVERIES",string.Format("{0} - {1}",otherCountry,consolidatedTrackerItems.Last().OtherCountryRecovery.ToString()));

            template = template.Replace("INFECTIONSPROGRESSBARWIDTH",infectionProgressBarWidth.ToString());

            template = template.Replace("DEATHSPROGRESSBARWIDTH",deathsProgressBarWidth.ToString());

            template = template.Replace("RECOVERYPROGRESSBARWIDTH",recoveryProgressBarWidth.ToString());

             //Update or overwrite a file only if has changed
            if (!File.Exists(reportName))
                File.WriteAllText(reportName,template);
            else
            {
                var existingFileContent = File.ReadAllText(reportName);

                if (!existingFileContent.Equals(template))
                     File.WriteAllText(reportName,template);
            }
   
        }
    }
}