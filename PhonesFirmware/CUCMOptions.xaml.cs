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
using System.Windows.Shapes;

namespace PhonesFirmware
{
    /// <summary>
    /// Interaction logic for CUCMOptions.xaml
    /// </summary>
    /// 
    
    public partial class CUCMOptions : Window
    {
        public delegate void CustomEventDelegate(object sender, CustomEventArgs _args);
        public event CustomEventDelegate RaiseCustomEvent;
        

        public CUCMOptions()
        {
            InitializeComponent();

            txtIP.Text = Properties.Settings.Default.IP;
            txtUser.Text = Properties.Settings.Default.UserId;
            txtPassword.Text = Properties.Settings.Default.Password;
            txtPort.Text = Properties.Settings.Default.Port;
            txtVersion.Text = Properties.Settings.Default.Version;
            txtPhoneUser.Text = Properties.Settings.Default.WebAccessUser;
            txtPhonePassword.Text = Properties.Settings.Default.WebAccessPassword;
            txtDeviceInformation.Text = Properties.Settings.Default.DeviceInfo;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (txtIP.Text == "" ||
               txtUser.Text == "" ||
               txtPassword.Text == "" ||
               txtPort.Text == "" ||
               txtVersion.Text == "" ||
                txtPhoneUser.Text == "" ||
                txtPassword.Text == "" )
            {
                MessageBox.Show("Please, enter all the required information above to be able to access your CUCM.");
            }
            else
            {
                Properties.Settings.Default.IP = txtIP.Text;
                Properties.Settings.Default.UserId = txtUser.Text;
                Properties.Settings.Default.Password = txtPassword.Text;
                Properties.Settings.Default.Port = txtPort.Text;
                Properties.Settings.Default.Version = txtVersion.Text;
                Properties.Settings.Default.WebAccessUser = txtPhoneUser.Text;
                Properties.Settings.Default.WebAccessPassword = txtPhonePassword.Text;
                Properties.Settings.Default.DeviceInfo = txtDeviceInformation.Text;
                Properties.Settings.Default.Save();

                if (RaiseCustomEvent != null)
                    RaiseCustomEvent(this,new CustomEventArgs());

                MessageBox.Show("Your CUCM Information saved successfully.");
            }
        }
    }

    public class CustomEventArgs : EventArgs
    {
        public CustomEventArgs()
        {
        }
    }
}
