using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeterDataLib
{
    public class Site
    {

        // init a guid with no dash or hyphens


        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Code { get; set; } = string.Empty; 
        public string Name { get; set; } = string.Empty;
        
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Postcode { get; set; } = string.Empty;
        
        public DateTime FirstDay { get; set; } = DateTime.MinValue;

        public DateTime LastDay { get; set; } = DateTime.MinValue;

        public DateTime DbTimeStampUtc { get; set; } = DateTime.MinValue;
        public DateTime LastUpdatedTimeStampUtc { get; set; } = DateTime.MinValue;

    }






}
