using System;
using System.Windows.Forms;

namespace litecord
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            SplashScreen splash = new SplashScreen();
            splash.Show();
            Application.DoEvents();

            Application.Run(new Form1(splash));
        }
    }
}