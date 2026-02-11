using System;
using System.Collections.Generic;
using System.Text;

namespace WebLinkBrowserInDesktop.Models
{
    public class AppConfig
    {
        public string DatabasePath { get; set; }
        public string LastUser { get; set; }
        public string DefaultBrowser { get; set; } = "Chrome";
    }
}
