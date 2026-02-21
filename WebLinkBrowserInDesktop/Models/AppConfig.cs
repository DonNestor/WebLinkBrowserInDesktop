using System;
using System.Collections.Generic;
using System.Text;

namespace WebLinkBrowserInDesktop.Models
{
    public class AppConfig
    {
        public string DatabasePath { get; set; }
        public string LastUser { get; set; } = "Default user";
        //public string DefaultBrowser { get; set; } = "Chrome";

        public Dictionary<string, string> BrowserPaths { get; set; } = new Dictionary<string, string>
        {
            {"Chrome", "" },
            {"Opera", "" },
            {"Firefox", "" },
            {"Tor", "" }
        };
    }
}
