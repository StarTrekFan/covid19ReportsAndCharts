using System;
using System.Linq;
using System.IO;
namespace Covid19Reports.Lib.Publisher
{
    public class CountryLandingPagePublisher : Covid19ReportPublisher
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

            var consolidatedTrackerItems =  distinctStatesOrProvinces.Select(state => new {
                    Country = Country,
                    ProvinceOrState = state,
                    Infections = VirusTrackerItems.Where(item => item.Country == Country && item.ProvinceOrState == state).Last().Infections,
                    Deaths = VirusTrackerItems.Where(item => item.Country == Country && item.ProvinceOrState == state).Last().Deaths,
                    Recovery = VirusTrackerItems.Where(item => item.Country == Country && item.ProvinceOrState == state).Last().Recovery
            }).ToList();

            var reportName = string.Format(@"{0}\{1}-States.html",DestinationFolder,Country);

            var template = File.ReadAllText(@"Templates\ProvinceOrStatesLandingPage.txt");

            var countryData = consolidatedTrackerItems
                                .Where(item => item.Infections > 0)
                                .OrderByDescending(item => item.Infections)
                                .Aggregate(string.Empty,(curr,next) => curr + string.Format("<tr><td><a href='{0}-{1}-Page.html'>{1}</a></td><td>{2}</td><td>{3}</td><td>{4}</td></tr>",Country,next.ProvinceOrState,next.Infections,next.Deaths,next.Recovery));


             template = template.Replace("ALLPROVINCEORSTATEDATAGOESHERE",countryData);

             template = template.Replace("COUNTRYNAMEGOESHERE",Country);

             template = template.Replace("LASTUPDATEDDATE",VirusTrackerItems.Last().StatusDate.ToShortDateString());
             

             File.WriteAllText(reportName,template);
        }
    }
}