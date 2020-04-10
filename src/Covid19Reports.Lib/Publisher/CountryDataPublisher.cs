using System.Linq;
using System;
using System.IO;
using System.Text;
namespace Covid19Reports.Lib.Publisher
{
    public class CountryDataPublisher : Covid19ReportPublisher
    {

        private string ReportName 
        {
            get
            {
                return string.Format(@"{0}\{1}-Page.html",DestinationFolder,Country);
            }
        }

        private string ChartsTitle
        {
            get
            {
                return string.Format("COVID-19 Infections, Deaths & Recovery - {0}",Country);
            }
        }
 
        private bool HasProviceOrStatesData
        {
            get
            {
                return VirusTrackerItems.Select(item => item.ProvinceOrState)
                                        .Any(state => !string.IsNullOrEmpty(state.Trim()));

            }
        }
       
        public override void PublishWebReports()
        {
            ValidateInputs();

            var distinctDates = VirusTrackerItems
                                            .Where(item => item.Country.Equals(Country))
                                            .Select(item => item.StatusDate.ToShortDateString())
                                            .Distinct()
                                            .OrderBy(item => DateTime.Parse(item));

            var consolidatedTrackerItems =  distinctDates.Select(statusDate => new {
                    Country = Country,
                    StatusDate = statusDate,
                    Infections = VirusTrackerItems.Where(item => item.StatusDate.ToShortDateString() == statusDate && item.Country.Equals(Country)).Sum(item => item.Infections),
                    Deaths = VirusTrackerItems.Where(item => item.StatusDate.ToShortDateString() == statusDate && item.Country.Equals(Country)).Sum(item => item.Deaths),
                    Recovery = VirusTrackerItems.Where(item => item.StatusDate.ToShortDateString() == statusDate && item.Country.Equals(Country)).Sum(item => item.Recovery)
            });

           //If there are no Deaths in a country don't produce the report
            if (consolidatedTrackerItems.Last().Deaths == 0)
                return;

            var template = File.ReadAllText(@"Templates\CountryPageTemplate.txt");

            var deathProgressBarWidth = Math.Round(( (double) consolidatedTrackerItems.Last().Deaths / (double) consolidatedTrackerItems.Last().Infections) * 100);

            var recoveryProgressBarWidth = Math.Round(( (double) consolidatedTrackerItems.Last().Recovery / (double) consolidatedTrackerItems.Last().Infections) * 100);

            var chartData =  consolidatedTrackerItems.Aggregate("['Date', 'Infections','Deaths','Recovery']",(curr,next) => curr + "," + "['" + DateTime.Parse(next.StatusDate).ToString("MM/dd") + "'," + next.Infections + "," + next.Deaths + "," + next.Recovery + "]");

            var otherCountriesList = OtherCountries.Aggregate(new StringBuilder(), (curr,next) => curr.AppendLine(string.Format("<a class=\"dropdown-item\" href=\"{0}-Page.html\">{0}</a>",next)));

            template = template.Replace("COUNTRYNAMEGOESHERE",Country);

            template = template.Replace("CHARTTITLEGOESHERE",ChartsTitle);

            template = template.Replace("CHARTDATAGOESHERE",chartData);

            template = template.Replace("TOTALINFECTIONS",consolidatedTrackerItems.Last().Infections.ToString());

            template = template.Replace("TOTALDEATHS",consolidatedTrackerItems.Last().Deaths.ToString());

            template = template.Replace("TOTALRECOVERIES",consolidatedTrackerItems.Last().Recovery.ToString());

            template = template.Replace("OTHERCOUNTRIESLIST",otherCountriesList.ToString());

            template = template.Replace("DEATHPROGRESSBARWIDTH",deathProgressBarWidth.ToString());

            template = template.Replace("RECOVERYPROGRESSBARWIDTH",recoveryProgressBarWidth.ToString());

            template = template.Replace("UNDERTREATMENTPROGRESSBARWIDTH",(100 -deathProgressBarWidth).ToString());

            template = template.Replace("NONRECOVEREDPROGRESSBARWIDTH",(100 - recoveryProgressBarWidth).ToString());

            
            if (!HasProviceOrStatesData)
            {
                template = template.Replace("PROVINCEORSTATEBUTTONSTATE","disabled");
                
                template = template.Replace("NOSTATESDATETOOLTIP","title=\"States or Province Data Not Avaialable\"");

                template = template.Replace("STATESLANDINGPAGELINK","onclick=\"alert('States or Province Data Not Avaialable')\"");
            }
            else
            {
                template = template.Replace("PROVINCEORSTATEBUTTONSTATE",string.Empty);
                
                template = template.Replace("NOSTATESDATETOOLTIP",string.Empty);

                 template = template.Replace("STATESLANDINGPAGELINK",string.Format("href='{0}-States.html'",Country))   ;


            }

            File.WriteAllText(ReportName,template);
        }

  
    }
}