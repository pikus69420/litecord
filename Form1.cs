using System;
using System.IO;
using System.Windows.Forms;
using System.Text.Json;
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
            browser.LifeSpanHandler = new CustomLifeSpanHandler();

            browser.TitleChanged += (s, args) =>
            {
                if (this.IsHandleCreated && !this.IsDisposed)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        string t = args.Title;
                        if (!string.IsNullOrEmpty(t) && t.Contains("Discord"))
                        {
                            string newTitle = t.Replace("Discord", "Litecord");
                            if (this.Text != newTitle) this.Text = newTitle;
                        }
                    }));
                }
            };

            browser.LoadingStateChanged += (s, args) =>
            {
                if (args.IsLoading == false)
                {
                    if (_splash != null)
                    {
                        this.BeginInvoke(new Action(() => {
                            if (_splash != null && !_splash.IsDisposed) _splash.Close();
                            _splash = null;
                        }));
                    }
                }
            };

            this.Controls.Add(browser);
            browser.Dock = DockStyle.Fill;
        }

        private void InitCef()
        {
            if (Cef.IsInitialized == false)
            {
                var settings = new CefSettings();
                string sysLang = CultureInfo.CurrentCulture.Name;
                settings.Locale = sysLang;
                settings.AcceptLanguageList = sysLang + "," + CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
                settings.RootCachePath = appDataPath;
                settings.CachePath = Path.Combine(appDataPath, "Cache");
                settings.PersistSessionCookies = true;
                settings.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
                Cef.Initialize(settings);
            }
        }

        private void SetupTrayMenu()
        {
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Show Litecord", null, (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; });
            trayMenu.Items.Add("Exit", null, (s, e) => {
                this.notifyIcon1.Visible = false;
                Cef.Shutdown();
                Application.Exit();
            });
            this.notifyIcon1.ContextMenuStrip = trayMenu;
        }

        
        private void Form1_Load(object sender, System.EventArgs e)
        {
           
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