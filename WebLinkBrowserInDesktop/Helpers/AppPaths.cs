using System;
using System.IO;

namespace WebLinkBrowserInDesktop.Helpers
{
    public static class AppPaths
    {
        // Base directory of the application (where the executable is located).
        public static readonly string BaseDir = AppDomain.CurrentDomain.BaseDirectory;

        // These are the main directories for the application.
        public static readonly string DatabaseDir = Path.Combine(BaseDir, "Db");
        public static readonly string ProfileDir = Path.Combine(BaseDir, "Profiles");
        public static readonly string TemplateDir = Path.Combine(BaseDir, "Templates");

        // Paths for specific files.
        public static readonly string ActiveProfileFile = Path.Combine(BaseDir, "active_profile.json");
        public static readonly string TemplateDbFile = Path.Combine(TemplateDir, "empty_template.db");
        public static string TemplateConfigFile = Path.Combine(TemplateDir, "default_template.json");

        public static void EnsureDirectoriesExist()
        {
            if (!Directory.Exists(DatabaseDir)) Directory.CreateDirectory(DatabaseDir);
            if (!Directory.Exists(ProfileDir)) Directory.CreateDirectory(ProfileDir);
            if (!Directory.Exists(TemplateDir)) Directory.CreateDirectory(TemplateDir);
        }
    }
}
