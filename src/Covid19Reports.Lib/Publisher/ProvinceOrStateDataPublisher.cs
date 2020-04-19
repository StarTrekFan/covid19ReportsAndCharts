using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
namespace Covid19Reports.Lib.Publisher
{
    public class ProvinceOrStateDataPublisher : Covid19ReportPublisher
    {
        public override void PublishWebReports()
        {
            ValidateInputs();

            var distinctStatesOrProvinces = VirusTrackerItems.Where(item => item.Country == Country)
                                                             .Select(item => item.ProvinceOrState)
                                                             .Distinct()
                                                             .Where(provinceOrState => provinceOrState.Trim() != string.Empty)
                                                             .ToList();

            if (!distinctStatesOrProvinces.Any())                                                             
                return;

            var hasStatesOrProvinceDate = VirusTrackerItems.Select(item => item.ProvinceOrState)
                                                           .Any(state => !string.IsNullOrEmpty(state.Trim()));

            Parallel.ForEach(distinctStatesOrProvinces,provinceOrState => 
            {
                   
                 var reportName = string.Format(@"{0}\{1}-{2}-Page.html",DestinationFolder,Country,provinceOrState);

                var distinctDates = VirusTrackerItems.Where(item => item.ProvinceOrState == provinceOrState)
                                                     .Select(item => item.StatusDate.ToShortDateString())
                                                     .Distinct()
                                                     .OrderBy(item => DateTime.Parse(item))
                                                     .ToList();
                        

                var consolidatedTrackerItems =  distinctDates.Select(statusDate => new {
                        Country = Country,
                        ProvinceOrState = provinceOrState,
                        StatusDate = statusDate,
                        Infections = VirusTrackerItems.Where(item => item.StatusDate.ToShortDateString() == statusDate && item.ProvinceOrState == provinceOrState).Sum(item => item.Infections),
                        Deaths = VirusTrackerItems.Where(item => item.StatusDate.ToShortDateString() == statusDate && item.ProvinceOrState == provinceOrState).Sum(item => item.Deaths),
                        Recovery = VirusTrackerItems.Where(item => item.StatusDate.ToShortDateString() == statusDate && item.ProvinceOrState == provinceOrState).Sum(item => item.Recovery)
                    }).ToList();

                //If there are no Infections in a province or state don't produce the report
                if (consolidatedTrackerItems.Last().Infections == 0)
                    return;

                var template = File.ReadAllText(@"Templates\ProvinceOrStatePage.txt");

                var chartTitle = string.Format("COVID-19 Infections, Deaths & Recovery - {0}-{1}",Country, provinceOrState);

                var chartData = consolidatedTrackerItems.Aggregate("['Date', 'Infections','Deaths','Recovery']",(curr,next) => curr + "," + "['" + DateTime.Parse(next.StatusDate).ToString("MM/dd") + "'," + next.Infections + "," + next.Deaths + "," + next.Recovery + "]");

                var deathProgressBarWidth = Math.Round(( (double) consolidatedTrackerItems.Last().Deaths / (double) consolidatedTrackerItems.Last().Infections) * 100);

                var recoveryProgressBarWidth = Math.Round(( (double) consolidatedTrackerItems.Last().Recovery / (double) consolidatedTrackerItems.Last().Infections) * 100);

                template = template.Replace("STATENAMEGOESHERE",provinceOrState);

                template = template.Replace("COUNTRYNAMEGOESHERE",Country);

                template = template.Replace("CHARTTITLEGOESHERE",chartTitle);

                template = template.Replace("CHARTDATAGOESHERE",chartData);

                template = template.Replace("TOTALINFECTIONS",consolidatedTrackerItems.Last().Infections.ToString());

                template = template.Replace("TOTALDEATHS",consolidatedTrackerItems.Last().Deaths.ToString());

                template = template.Replace("TOTALRECOVERIES",consolidatedTrackerItems.Last().Recovery.ToString());

                 template = template.Replace("DEATHPROGRESSBARWIDTH",deathProgressBarWidth.ToString());

                template = template.Replace("RECOVERYPROGRESSBARWIDTH",recoveryProgressBarWidth.ToString());

                template = template.Replace("UNDERTREATMENTPROGRESSBARWIDTH",(100 -deathProgressBarWidth).ToString());

                template = template.Replace("NONRECOVEREDPROGRESSBARWIDTH",(100 - recoveryProgressBarWidth).ToString());


               //Update or overwrite a file only if has changed
                if (!File.Exists(reportName))
                    File.WriteAllText(reportName,template);
                else
                {
                    var existingFileContent = File.ReadAllText(reportName);
                    if (!existingFileContent.Equals(template))
                        File.WriteAllText(reportName,template);
                }



            });
        }
     }
}