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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NationalInstruments.DAQmx;
using NationalInstruments.NI4882;

namespace GPIB_Lab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NationalInstruments.NI4882.Device device;


        public MainWindow()
        {
            InitializeComponent();
            cboAdd.Items.Add("None");
            for (int i = 0; i <= 32; i++)
            {
                cboAdd.Items.Add(i);
            }
            for (int i = 0; i <= 10; i++)
            {
                cboID.Items.Add("GPIB"+i);
            }
            rbn196.IsChecked = true;
            rbn2000.IsChecked = false;

            cboAdd.SelectedIndex = 17;
            cboID.SelectedIndex = 0;
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Convert the Secondary Address Combo Text into a number.
                int address = 0;
                if (cboAdd.SelectedIndex != 0)
                {
                    address = (int)cboAdd.SelectedItem;
                }
                // Intialize the device
                int ID;
                bool idparse = int.TryParse(cboID.Text, out ID);
                byte addy;
                bool addparse = byte.TryParse(cboAdd.Text, out addy);
                device = new NationalInstruments.NI4882.Device(ID, addy);

                btnOpen.IsEnabled = false;
                btnClose.IsEnabled = true;
                cboAdd.IsEnabled = false;
                cboID.IsEnabled = false;

                if (rbn196.IsChecked == true)
                {
                    device.Write("L0X");
                    device.Write("F0X"); //DC volts                 page 61 of user manual
                    device.Write("R0X"); //Auto range
                    device.Write("S3X"); //precision setup
                    device.Write("G2X"); //data format
                    device.Write("Q50X"); //read into buffer every 50 ms
                    device.Write("T4X"); //trigger setup - check this later
                }
                else        //page 4-9 of the manual for gpib operation, common commands p.4-39, SCPI commands p.5-5 to 5-18
                {
                    device.Write(":SYST:PRES"); //restore defaults before changing stuff around
                    device.Write(":VOLT:DC:DIG 7"); //set to DC volts w/ 7 dig res
                    device.Write(":TRIG:DEL 0.05"); //set delay to 0.05 seconds
                    device.Write("TRAC:POIN 100");
                    device.Write("TRIG:COUN 1");

                }

                rbn196.IsEnabled = false;
                rbn2000.IsEnabled= false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                device.Dispose();
                btnOpen.IsEnabled = true;
                btnClose.IsEnabled = false;
                cboAdd.IsEnabled = true;
                cboID.IsEnabled = true;
                rbn2000.IsEnabled= true;
                rbn196.IsEnabled= true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnColl_Click(object sender, RoutedEventArgs e)
        {
            btnClose.IsEnabled = false;
            btnColl.IsEnabled = false;
            btnOpen.IsEnabled = false;
            btnClr.IsEnabled = false;
            rbn196.IsEnabled = false;
            rbn2000.IsEnabled = false;

            if(rbn196.IsChecked == true)
            {
                try
                {
                    string[] data = new string[200];
                    device.Write("I0X"); //clear the buffer
                    device.Write("I100X"); //set buffer to 100
                    device.Write("B1X"); //begin transmission
                    string dat = device.ReadString();
                    dat = dat + device.ReadString();
                    dat = dat + device.ReadString();
                    data = dat.Split(',');

                    for (int i = 0; i < 200; i += 2)
                    {
                        lbxData.Items.Add(data[i]);
                        lbxData.SelectedIndex = lbxData.Items.Count - 1;
                        lbxData.ScrollIntoView(lbxData.SelectedItem);
                    }
                }
                catch(Exception ex) { MessageBox.Show(ex.Message); }

            }
            else
            {
                try
                {
                    device.Write("TRAC:CLE");

                    device.Write(":TRIG:SOUR IMM"); //trigger source to immediate
                    device.Write(":STAT:PRES;*CLS");
                    device.Write(":STAT:MEAS:ENAB 512");
                    device.Write(":*SRE 1");
                    device.Write(":TRAC:FEED:CONT NEXT");

                    device.Write(":TRAC:DATA?"); //send to computer

                    Byte[] dat = device.ReadByteArray(1600); //figure out number of bytes in the 100 data points and enter into the method

                    string data = System.Text.Encoding.UTF8.GetString(dat, 0, dat.Length);
                    String[] findat = data.Split(',');


                    for (int i = 0; i < 100; i += 2)
                    {
                        lbxData.Items.Add(findat[i]);
                        lbxData.SelectedIndex = lbxData.Items.Count - 1;
                        lbxData.ScrollIntoView(lbxData.SelectedItem);
                    }
                    
                }
                catch(Exception ex) { MessageBox.Show(ex.Message); }

            }




            btnClose.IsEnabled = true;
            btnColl.IsEnabled = true;
            btnClr.IsEnabled = true;
            rbn196.IsEnabled = true;
            rbn2000.IsEnabled = true;

        }

        private void btnClr_Click(object sender, RoutedEventArgs e)
        {
            lbxData.Items.Clear();
        }

        private void rbn196_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void rbn2000_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                device.Dispose();
                btnOpen.IsEnabled = true;
                btnClose.IsEnabled = false;
                cboAdd.IsEnabled = true;
                cboID.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
