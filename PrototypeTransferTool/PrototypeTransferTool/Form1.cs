using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.Metrics;

namespace PrototypeTransferTool
{
    public partial class Form1 : Form
    {
        FileSystemWatcher watcher;
        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        private bool isWatching;
        public Form1()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;

            var services = new ServiceCollection();
            services.AddWindowsFormsBlazorWebView();
            blazorWebView1.HostPage = "wwwroot\\index.html";
            blazorWebView1.Services = services.BuildServiceProvider();
            blazorWebView1.RootComponents.Add<BlazorTest>("#app");
            startWatching();
        }

        private void blazorWebView1_Click(object sender, EventArgs e)
        {

        }

        private void startWatching()
        {
            isWatching = true;
            timer.Enabled = true;
            timer.Start();
            timer.Interval = 500;

            watcher = new FileSystemWatcher();
            watcher.Path = MyConfig.FilePath;
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName;

            watcher.Created += new FileSystemEventHandler(OnCreated);
            

            watcher.EnableRaisingEvents = true;
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {


        }
    }
}
