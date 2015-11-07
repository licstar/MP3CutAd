using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using MP3CutAd.App.Controls;
using CefSharp.WinForms;
using CefSharp;
using System.Threading;
using Newtonsoft.Json;

namespace MP3CutAd.App {
    public partial class MainForm : Form {

        public ChromiumWebBrowser Browser {
            get;
            private set;
        }

        public MainForm() {
            InitializeComponent();

            //WindowState = FormWindowState.Maximized;

            Browser = new ChromiumWebBrowser("about:blank") {
                Dock = DockStyle.Fill,
            };

            this.Controls.Add(Browser);

            Browser.TitleChanged += OnBrowserTitleChanged;

            this.FormClosed += OnMainFormClosed;

            var bitness = Environment.Is64BitProcess ? "x64" : "x86";
            var version = String.Format("Chromium: {0}, CEF: {1}, CefSharp: {2}, Environment: {3}", Cef.ChromiumVersion, Cef.CefVersion, Cef.CefSharpVersion, bitness);

            var bound = new BoundObject(this);
            bound.Versions = new VersionInfo{
                Chromium = Cef.ChromiumVersion,
                Cef = Cef.CefVersion,
                CefSharp = Cef.CefSharpVersion,
                Bitness = bitness,
            };
            Browser.RegisterJsObject("bound", bound);

            var cwd = System.Environment.CurrentDirectory;
            var webPath = Path.Combine(cwd, @"../WebApp/index.html").Replace(" ", "%20").Replace('\\', '/');
            var url = "file:///" + webPath;
            url = "http://localhost:8080/webpack-dev-server/";
            
            Browser.Load(url);
        }

        private void OnMainFormClosed(object sender, FormClosedEventArgs e) {
            //Browser.Dispose();
            //Cef.Shutdown();
        }

        private void OnBrowserTitleChanged(object sender, TitleChangedEventArgs args) {
            //this.InvokeOnUiThreadIfRequired(() => {
            //    this.Text = args.Title;
            //});
        }
    }
}
