using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
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

        public DateTime LastAccessTimeUtc { get; set; } = DateTime.MinValue;

        const string DEFAULT = "Default";

        public string Folder { get => ToSentence(folder); set => folder = string.IsNullOrWhiteSpace(value) ? DEFAULT : ToSentence(value); }
               

        
       
 
        private string folder = string.Empty;


        //write a function to turn a string into a cap[tical letter followed by lowercase letters
        public static string ToSentence(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return DEFAULT;
            }
            value = value.Trim();

            return value[0].ToString().ToUpperInvariant() + value.Substring(1).ToLowerInvariant();
        }







    }






}
