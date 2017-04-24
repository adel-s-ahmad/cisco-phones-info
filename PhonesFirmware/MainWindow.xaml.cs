using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;


namespace PhonesFirmware
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        BackgroundWorker worker = new BackgroundWorker();

        public MainWindow()
        {
            log.Info("application started.");

            InitializeComponent();

            initializeHeader();

            worker.WorkerReportsProgress = true ;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork+=worker_DoWork;
            worker.RunWorkerCompleted+=worker_RunWorkerCompleted;
            worker.ProgressChanged+=worker_ProgressChanged;


            if( Properties.Settings.Default.IP == "" ||
                Properties.Settings.Default.UserId == "" ||
                Properties.Settings.Default.Password == "" ||
                Properties.Settings.Default.Port == "" ||
                Properties.Settings.Default.Version == "" ||
                Properties.Settings.Default.WebAccessUser == "" ||
                Properties.Settings.Default.WebAccessPassword == "" )
            {
                log.Info("CUCM Options not initialized.");
                btnStart.IsEnabled = false;
                lblStatus.Content = "Please, Initialize your CUCM Options.";
            }
        }

        private void initializeHeader()
        {
            var deviceInfo = Properties.Settings.Default.DeviceInfo.Split(',');
            txtResult.Text = "Name  -  Description  -  IP Address  -  Status";
            foreach (var info in deviceInfo)
            {
                if (!string.IsNullOrEmpty(info))
                    txtResult.Text += "  -  " + info;
            }
            txtResult.Text += "\r\n";

            txtResult.Text += "____________________________________________________\r\n\r\n";   
        }

        private void CUCMOptions_Click(object sender, RoutedEventArgs e)
        {
            CUCMOptions options = new CUCMOptions();
            options.RaiseCustomEvent += CUCMOptions_RaiseSavedEvent;
            options.Show();
        }

        public void CUCMOptions_RaiseSavedEvent(object sender, CustomEventArgs e)
        {
            if (Properties.Settings.Default.IP == "" ||
              Properties.Settings.Default.UserId == "" ||
              Properties.Settings.Default.Password == "" ||
              Properties.Settings.Default.Port == "" ||
              Properties.Settings.Default.Version == "" ||
              Properties.Settings.Default.WebAccessUser == "" ||
              Properties.Settings.Default.WebAccessPassword == "")
            {
                log.Info("CUCM Options not initialized after updating the options.");
                btnStart.IsEnabled = false;
                lblStatus.Content = "Please, Initialize your CUCM Options.";
            }
            else
            {
                log.Info("CUCM Options initialized successfully.");
                btnStart.IsEnabled = true;

                initializeHeader();

                lblStatus.Content = "CUCM Configured, you can now click the start button to start getting phones information";
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            log.Info("phones information retrieval in progress...");
            btnStart.IsEnabled = false;
            btnStart.Content = "In Progress...";
            lblStatus.Content = "Getting Phones Information...";

            initializeHeader();
            lblStatus.Content = "";

            if (worker.IsBusy != true)
            {
                log.Info("worker started.");
                // Start the asynchronous operation.
                worker.RunWorkerAsync();
            }
            else
            {
                log.Info("worker is busy with another task.");
                lblStatus.Content = "It seems that some work in progress, restart the app if you sure nothing is running!";
            }

        }

        public void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                BackgroundWorker senderWorker = sender as BackgroundWorker;
                senderWorker.ReportProgress(0, null);

                RisServiceConsumer consumer = new RisServiceConsumer();

                log.Info("getting phones...");
                List<Device> axlPhones = consumer.getPhonesAXL();
                int axlCount = axlPhones.Count();

                List<GetDevicesByName.CmDevice> rispPhones = consumer.getPhonesDetails(axlPhones);
                int count = rispPhones.Count();
                senderWorker.ReportProgress( (int) ((1 / (float)count) * 100), "\r\nFound (" + axlCount + ") devices, retreiving details of registered phones...\r\n");
                
                log.Info("phones count: " + count);
                senderWorker.ReportProgress(1, "Found (" + count + ") registered phones, getting their details...\r\n");
                //var phonesInfo = consumer.getPhonesInfo(rispPhones, Properties.Settings.Default.WebAccessUser, Properties.Settings.Default.WebAccessPassword);
                int counter = 2;
                foreach (var device in rispPhones)
                {
                    PhoneInfo deviceInfo = consumer.getPhoneInfo(device);
                    int progress = (int) (((float)counter / (float) count) * 100);
                    senderWorker.ReportProgress(progress, deviceInfo);
                    counter++;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                MessageBox.Show(ex.Message + "\r\nsee log file for more info\r\n", "Error");
            }
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                if(e.UserState == null)
                {
                    this.Title = e.ProgressPercentage + "%";
                }
                else if (Type.GetTypeCode(e.UserState.GetType()) == TypeCode.String)
                {
                    lblStatus.Content += e.UserState.ToString();
                }
                else
                {
                    var deviceInfo = Properties.Settings.Default.DeviceInfo.Split(',');
                    PhoneInfo device = (PhoneInfo)e.UserState;
                    txtResult.Text += device.device.Name + "  |  " + device.device.Description + "  |  " + device.device.IpAddress + "  |  " + device.device.Status;
                    foreach (var info in deviceInfo)
                    {
                        if (!string.IsNullOrEmpty(info))
                            txtResult.Text += "  |  " + device.info.Where(d=> d.Name == info).Select(i => i.Value).FirstOrDefault();
                    }
                    txtResult.Text += "\r\n";
                    txtResult.Text += "---------------------------------------------------------\r\n";
                    this.Title = e.ProgressPercentage + "%";
                }
            }catch(Exception ex)
            {
                log.Error(ex.Message, ex);
                MessageBox.Show(ex.Message + "\r\nsee log file for more info\r\n", "Error");
            }
        }

        public void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            log.Info("task completed.");
            btnStart.Content = "Start!";
            this.Title = "100%";
            lblStatus.Content += "Task Completed Successfully";
            btnStart.IsEnabled = true;
        }

        private void ExitItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.Show();
        }

    }
}
