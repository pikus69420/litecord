using System;
using System.IO;
using System.Windows.Forms;
using System.Globalization;
using System.Diagnostics;
using CefSharp;
using CefSharp.WinForms;

namespace litecord
{
    public partial class Form1 : Form
    {
        private ChromiumWebBrowser browser;
        private ContextMenuStrip trayMenu;
        private SplashScreen _splash;
        private string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Litecord");

        public Form1(SplashScreen splash)
        {
            InitializeComponent();
            _splash = splash;

            if (!Directory.Exists(appDataPath)) Directory.CreateDirectory(appDataPath);

            InitCef();
            SetupTrayMenu();

            browser = new ChromiumWebBrowser("https://discord.com/app");
            browser.Dock = DockStyle.Fill;
            browser.LifeSpanHandler = new CustomLifeSpanHandler();

            browser.TitleChanged += (s, args) =>
            {
                this.BeginInvoke(new Action(() =>
                {
                    if (!string.IsNullOrEmpty(args.Title) && args.Title.Contains("Discord"))
                    {
                        this.Text = args.Title.Replace("Discord", "Litecord");
                    }
                }));
            };

            browser.LoadingStateChanged += (s, args) =>
            {
                if (args.IsLoading == false && _splash != null)
                {
                    this.BeginInvoke(new Action(() => {
                        _splash?.Close();
                        _splash = null;
                        this.Show();
                    }));
                }
            };

            this.Controls.Add(browser);
            this.ResumeLayout(true);
        }

        private void InitCef()
        {
            if (Cef.IsInitialized == false)
            {
                var settings = new CefSettings();
                settings.Locale = CultureInfo.CurrentCulture.Name;
                settings.RootCachePath = appDataPath;
                settings.CachePath = Path.Combine(appDataPath, "Cache");
                settings.PersistSessionCookies = true;
                settings.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

                settings.CefCommandLineArgs.Add("disable-infobars", "1");
                settings.CefCommandLineArgs.Add("no-sandbox", "1");

                Cef.Initialize(settings);
            }
        }

        private void SetupTrayMenu()
        {
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Show Litecord", null, (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; });
            trayMenu.Items.Add("Github Page", null, (s, e) => { Process.Start(new ProcessStartInfo("https://github.com/pikus69420/litecord") { UseShellExecute = true }); });
            trayMenu.Items.Add("-", null, null);
            trayMenu.Items.Add("Exit", null, (s, e) => {
                this.notifyIcon1.Visible = false;
                Cef.Shutdown();
                Application.Exit();
            });
            this.notifyIcon1.ContextMenuStrip = trayMenu;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            base.OnFormClosing(e);
        }
    }

    public class CustomLifeSpanHandler : ILifeSpanHandler
    {
        public bool OnBeforePopup(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
        {
            newBrowser = null;
            Process.Start(new ProcessStartInfo(targetUrl) { UseShellExecute = true });
            return true;
        }
        public void OnAfterCreated(IWebBrowser chromiumWebBrowser, IBrowser browser) { }
        public bool DoClose(IWebBrowser chromiumWebBrowser, IBrowser browser) { return false; }
        public void OnBeforeClose(IWebBrowser chromiumWebBrowser, IBrowser browser) { }
    }
}