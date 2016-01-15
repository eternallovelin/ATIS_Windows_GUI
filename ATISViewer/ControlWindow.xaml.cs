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
using System.IO;

namespace ATISViewer
{
    /// <summary>
    /// Interaction logic for controls.xaml
    /// </summary>
    public partial class ControlWindow : Window
    {
        private static BiasesWindow _biasesWindow;
        public static FileStream propFsTD;// = new FileStream("video.val", FileMode.Create);
        public static BinaryWriter _writerTD;
        public static bool _logging = false;
        public static bool clearPixelData = false;
        public static int persistence_us = 20000;
        //  public static int firstEMSpikeThreshold = 50000;
        //        public static int secondEMSpikeThreshold = 200000;
        public ControlWindow()
        {
            InitializeComponent();
            boxEnableTD.IsChecked = ATISinterface.getWire(ATISinterface.ControlSignals.Enable_AER_TD);
            boxEnableAPS.IsChecked = ATISinterface.getWire(ATISinterface.ControlSignals.Enable_AER_APS);
            boxCouple.IsChecked = ATISinterface.getWire(ATISinterface.ControlSignals.couple);
            boxTDROIinv.IsChecked = ATISinterface.getWire(ATISinterface.ControlSignals.ROI_TD_inv);
            boxTDROI.IsChecked = ATISinterface.getWire(ATISinterface.ControlSignals.TD_ROI_en);
            boxAPSROI.IsChecked = ATISinterface.getWire(ATISinterface.ControlSignals.APS_ROI_en);
            boxAPSsequential.IsChecked = ATISinterface.getWire(ATISinterface.ControlSignals.sequential);

            slidePersistence.Value = 20;
        }



        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblPersistenceValue.Content = ((int)e.NewValue).ToString();
            persistence_us = (int)e.NewValue * 1000;
            ATISinterface.setTDfilter_value((uint)e.NewValue);
        }

        private void btnClearImage_Click(object sender, RoutedEventArgs e)
        {
            clearPixelData = true;
            //calibrate();

        }

        private void boxRecord_Checked(object sender, RoutedEventArgs e)
        {
            //propFsTD = new FileStream(imageSlideshow.databaseImage.saveArrayList[imageSlideshow.databaseImage.saveImageIndex].ToString(), FileMode.Create);
            //imageSlideshow.databaseImage.saveImageIndex = -1;
            //propLogging = true;
            //lock (propFsTD)
            //{
            int filenumber = 0;
            string curFile = filenumber.ToString("D4") + ".val";

            while (File.Exists(curFile))
            {
                filenumber++;
                curFile = filenumber.ToString("D4") + ".val";
            }
            propFsTD = new FileStream(curFile, FileMode.Create);
            _writerTD = new BinaryWriter(propFsTD);
            _logging = true;
            //}
        }

        public void boxRecord_UnChecked(object sender, RoutedEventArgs e)
        {
            lock (propFsTD)
            {
                _logging = false;
                propFsTD.Close();
                _writerTD.Close();
                propFsTD = null;
            }
        }

        private void boxEnableTD_Checked(object sender, RoutedEventArgs e)
        {
            ATISinterface.setWire(ATISinterface.ControlSignals.Enable_AER_TD);
        }

        private void boxEnableTD_UnChecked(object sender, RoutedEventArgs e)
        {
            ATISinterface.clearWire(ATISinterface.ControlSignals.Enable_AER_TD);
        }

        private void boxEnableAPS_UnChecked(object sender, RoutedEventArgs e)
        {
            ATISinterface.clearWire(ATISinterface.ControlSignals.Enable_AER_APS);
        }

        private void boxEnableAPS_Checked(object sender, RoutedEventArgs e)
        {
            ATISinterface.setWire(ATISinterface.ControlSignals.Enable_AER_APS);
        }

        private void boxEnableHighSpeed_UnChecked(object sender, RoutedEventArgs e)
        {

        }

        private void boxEnableHighSpeed_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void boxAPSROI_Checked(object sender, RoutedEventArgs e)
        {
            ATISinterface.setWire(ATISinterface.ControlSignals.APS_ROI_en);
        }

        private void boxAPSROI_UnChecked(object sender, RoutedEventArgs e)
        {
            ATISinterface.clearWire(ATISinterface.ControlSignals.APS_ROI_en);
        }

        private void boxTDROI_Checked(object sender, RoutedEventArgs e)
        {
            ATISinterface.setWire(ATISinterface.ControlSignals.TD_ROI_en);
        }

        private void boxTDROI_UnChecked(object sender, RoutedEventArgs e)
        {
            ATISinterface.clearWire(ATISinterface.ControlSignals.TD_ROI_en);
        }

        private void boxTDROIinv_Checked(object sender, RoutedEventArgs e)
        {
            ATISinterface.setWire(ATISinterface.ControlSignals.ROI_TD_inv);
        }

        private void boxTDROIinv_UnChecked(object sender, RoutedEventArgs e)
        {
            ATISinterface.clearWire(ATISinterface.ControlSignals.ROI_TD_inv);
        }

        private void boxCouple_Checked(object sender, RoutedEventArgs e)
        {
            ATISinterface.setWire(ATISinterface.ControlSignals.couple);
        }

        private void boxCouple_UnChecked(object sender, RoutedEventArgs e)
        {
            ATISinterface.clearWire(ATISinterface.ControlSignals.couple);
        }

        private void boxAPSsequential_Checked(object sender, RoutedEventArgs e)
        {
            ATISinterface.setWire(ATISinterface.ControlSignals.sequential);
        }
        private void boxAPSsequential_UnChecked(object sender, RoutedEventArgs e)
        {
            ATISinterface.clearWire(ATISinterface.ControlSignals.sequential);
        }
        private void btnProgROI_Click(object sender, RoutedEventArgs e)
        {
            ATISinterface.programROI();
        }

        private void btnShutter_Click(object sender, RoutedEventArgs e)
        {
            ATISinterface.setWire(ATISinterface.ControlSignals.shutter);
            ATISinterface.clearWire(ATISinterface.ControlSignals.shutter);
        }



        private void boxEnableCustomSpeed_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void boxEnableCustomSpeed_UnChecked(object sender, RoutedEventArgs e)
        {

        }

        private void TakeSnapshots_Checked(object sender, RoutedEventArgs e)
        {
            //    _TakingSnapshots = true;
        }

        private void TakeSnapshots_UnChecked(object sender, RoutedEventArgs e)
        {
            //    _TakingSnapshots = false;
        }

        private void btnEditBiases_Click(object sender, RoutedEventArgs e)
        {
            if (_biasesWindow == null)
            {
                _biasesWindow = new BiasesWindow();
                _biasesWindow.Show();
            }
        }

        public static void close_Biases()
        {
            _biasesWindow = null;
        }


        

        private void ControlWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (boxRecord.IsChecked == true)
                boxRecord.IsChecked = false;
            if (_biasesWindow != null)
                _biasesWindow.Close();
            MainWindow.CloseControlWindow();
        }

    }
}