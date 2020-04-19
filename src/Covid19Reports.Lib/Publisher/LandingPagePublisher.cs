using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
namespace Covid19Reports.Lib.Publisher
{
    //This class will generate the starting page on the static site. It will be named Index.html
    //This is going to be a tabular report list all the countries and the total number of Infections
    //Deaths and Recoveries. Unlike other publishers this class will not operate on a single country.

    public class LandingPagePublisher : Covid19ReportPublisher
    {
        public override void PublishWebReports()
        {
                       
            var reportName = string.Format(@"{0}\Index.html",DestinationFolder);


            var lastStatusDate = VirusTrackerItems.Select(item => item.StatusDate.ToShortDateString())
                                            .Distinct()
                                            .OrderBy(item => DateTime.Parse(item))
                                            .Last();

            var allCountries = VirusTrackerItems.Select(item => item.Country).Distinct().ToList();
            
            var consolidatedTrackerItems =  allCountries.Select(country => new {
                    Country = country,
                    StatusDate = lastStatusDate,
                    Infections = VirusTrackerItems.Where(item => item.StatusDate.ToShortDateString() == lastStatusDate && item.Country == country).Sum(item => item.Infections),
                    Deaths = VirusTrackerItems.Where(item => item.StatusDate.ToShortDateString() == lastStatusDate && item.Country == country).Sum(item => item.Deaths),
                    Recovery = VirusTrackerItems.Where(item => item.StatusDate.ToShortDateString() == lastStatusDate && item.Country == country).Sum(item => item.Recovery)
            });

            var template = File.ReadAllText(@"Templates\LandingPage.txt");

            var countryData = consolidatedTrackerItems
                                .Where(item => item.Infections > 0)
                                .OrderByDescending(item => item.Infections)
                                .Aggregate(string.Empty,(curr,next) => curr + string.Format("<tr><td><a href='{0}-Page.html'>{0}</a></td><td>{1}</td><td>{2}</td><td>{3}</td></tr>",next.Country,next.Infections,next.Deaths,next.Recovery));

            template = template.Replace("ALLCOUNTRIESDATAGOESHERE",countryData);

            template = template.Replace("LASTUPDATEDDATE",lastStatusDate);
            
            File.WriteAllText(reportName,template);
        }
    }
}