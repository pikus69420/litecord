using Microsoft.Web.WebView2.Core;
using System.IO;

namespace litecord
{
    public partial class Form1 : Form
    {
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;

        public Form1()
        {
            InitializeComponent();

            var field = typeof(Form1).GetField("components", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field != null)
            {
                var container = (System.ComponentModel.IContainer)field.GetValue(this);
                if (container != null)
                {
                    foreach (var item in container.Components)
                    {
                        if (item is NotifyIcon oldIcon)
                        {
                            oldIcon.Visible = false;
                            oldIcon.Dispose();
                        }
                    }
                }
            }

            this.ContextMenuStrip = null;
            webView21.Source = null;
            this.Load += Form1_Load;
            SetupTray();
        }

        private void SetupTray()
        {
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Show Litecord", null, (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; });
            trayMenu.Items.Add("Github Page", null, (s, e) => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://github.com/pikus69420/litecord") { UseShellExecute = true }));
            trayMenu.Items.Add("-");
            trayMenu.Items.Add("Quit", null, (s, e) => Application.Exit());

            trayIcon = new NotifyIcon();
            trayIcon.Text = "Litecord";
            trayIcon.Icon = this.Icon;
            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.Visible = true;

            trayIcon.DoubleClick += (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; };

            this.Resize += (s, e) => { if (this.WindowState == FormWindowState.Minimized) this.Hide(); };
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Litecord");
                var env = await CoreWebView2Environment.CreateAsync(null, path);

                await webView21.EnsureCoreWebView2Async(env);

                webView21.CoreWebView2.Settings.IsStatusBarEnabled = false;
                webView21.CoreWebView2.Settings.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
                webView21.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;

                webView21.CoreWebView2.PermissionRequested += (s, args) =>
                {
                    if (args.PermissionKind == CoreWebView2PermissionKind.Notifications)
                    {
                        args.State = CoreWebView2PermissionState.Allow;
                    }
                };

                webView21.CoreWebView2.NewWindowRequested += (s, args) =>
                {
                    args.Handled = true;
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(args.Uri) { UseShellExecute = true });
                };

                webView21.CoreWebView2.DocumentTitleChanged += (s, args) =>
                {
                    this.Text = webView21.CoreWebView2.DocumentTitle;
                };

                webView21.Source = new Uri("https://discord.com/app");
            }
            catch
            {
                webView21.Source = new Uri("https://discord.com/app");
            }
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

        private void webView21_Click(object sender, EventArgs e)
        {
        }
    }
}