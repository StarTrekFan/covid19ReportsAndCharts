using System.Collections.Generic;
using System;
namespace Covid19Reports.Lib.Publisher
{
      public class Covid19ReportPublisher
    {
        public List<VirusTrackerItem> VirusTrackerItems {get;set;}
        public string DestinationFolder {get;set;}
        public string Country {get;set;}
        public List<string> OtherCountries {get;set;}
        
        public void ValidateInputs()
        {
            if (string.IsNullOrEmpty(DestinationFolder))
                throw new System.Exception("DestinationFolder property is not specified");

            if (string.IsNullOrEmpty(Country))
                throw new System.Exception("Country property is not specified");

            if (VirusTrackerItems == null)
                throw new System.Exception("VirusTrackerItems property is not specified");
        }

        public virtual void PublishWebReports()
        {
            throw new NotImplementedException();
        }
    }
}