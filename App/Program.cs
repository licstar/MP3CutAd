using CefSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MP3CutAd.App
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var settings = new CefSettings();
            settings.CefCommandLineArgs["javascript-harmony"] = "1";
            settings.CefCommandLineArgs["allow-file-access-from-files"] = "1";
            settings.CefCommandLineArgs["disable-web-security"] = "1";

            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            settings.CachePath = Path.Combine(appDataPath, "MP3CutAd", "cef-cache");

            //Perform dependency check to make sure all relevant resources are in our output directory.
            Cef.Initialize(settings, shutdownOnProcessExit: true, performDependencyCheck: true);
            Application.Run(new MainForm());

            Application.ApplicationExit += OnApplicationExit;
        }

        private static void OnApplicationExit(object sender, EventArgs e) {
            Cef.Shutdown();
        }
    }
}
