using System;
using System.Collections.Generic;
using System.Text;

namespace WebLinkBrowserInDesktop.Models
{
    public class WebLinkModel
    {
        private string _name;
        public int Id { get; set; }
        public string Name 
        { 
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _name = FormatNameFromUrl(Url);
                }
                else
                {
                    _name = value;
                }
            } 
        }
        public string Url { get; set; }
        public string BrowserType { get; set; }


        private string FormatNameFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return "Nieznany Link";

            string cleanName = url;

            if (cleanName.Contains("://"))
            {
                cleanName = cleanName.Split(new[] {"://"}, StringSplitOptions.None)[1];
            }

            if(cleanName.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
            {
                cleanName = cleanName.Substring(4);
            }

            if(cleanName.Contains("/"))
            {
                cleanName = cleanName.Split('/')[0];

            }

            if(cleanName.Length > 0)
            {
                cleanName = char.ToUpper(cleanName[0]) + cleanName.Substring(1);
            }

            return cleanName;
        }
    };
}
