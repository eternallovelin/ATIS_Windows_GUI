using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;//added
//using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using OpalKelly.FrontPanel;
using System.Timers;
//using Ookii.Dialogs.Wpf;//for browsing the folders
using System.Diagnostics;//for outputting debug messages when we get errors

namespace ATISViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private static ControlWindow _controlWindow = null;
        private MemoryStream stream;
        private BinaryReader reader;
        private static int y_error_Counter = 0;
        private static byte[] readData = new byte[4];
        private static long timestamp_overflowCounter = (1 << 13);
        [DllImport("Winmm.dll", EntryPoint = "timeBeginPeriod")]
        private static extern int TimeBeginPeriod(uint period);

        private System.Timers.Timer screenUpdateTimer;
        private DateTime _lastFrameDrawn = DateTime.MinValue;
        private DateTime _predNextFrame = DateTime.MinValue;

        private bool _TakingSnapshots = false;
        private bool _runningAcquisition = false;//changed included volatile

        private static int _boxSizeX, _boxSizeY = 0;
        private static byte[] _pixelDataAPS = new byte[304 * 240];

        private FileStream _fsTD = null;//only used in data logging routines

        private DateTime _SnapShotTimer;
        public bool auto = false;//changed, defined auto mode to know when the system is in auto

        private WriteableBitmap _TDbackBuffer;
        private WriteableBitmap _APSbackBuffer;
        //private BinaryWriter _writerTD;//only used in data logging routines and in the acquisition thread loop
        private bool _pauseAcquisitionThread = false;

        private static BackgroundWorker _bw = new BackgroundWorker();

        //public double _motorPos1_1, _motorPos2_1, _motorPos1_2, _motorPos2_2, _motorPos1_3, _motorPos2_3;
        //public double _motorSpeed1_1, _motorSpeed2_1, _motorSpeed1_2, _motorSpeed2_2, _motorSpeed1_3, _motorSpeed2_3;

        private byte[] _pixelData = new byte[304 * 240];
        private byte[] _S1Data = new byte[128 * 128];
        private byte[] _C1Data = new byte[32 * 32];
        private byte[] _S2Data = new byte[32 * 32];


        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region "color definitions"
            //The 12 S1 colours
            List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>();
            colors.Add(Color.FromRgb(0, 0, 0));
            colors.Add(Color.FromRgb(255, 0, 0));
            colors.Add(Color.FromRgb(255, 128, 0));
            colors.Add(Color.FromRgb(255, 255, 0));
            colors.Add(Color.FromRgb(128, 255, 0));
            colors.Add(Color.FromRgb(0, 255, 0));
            colors.Add(Color.FromRgb(0, 255, 128));
            colors.Add(Color.FromRgb(0, 255, 255));
            colors.Add(Color.FromRgb(0, 128, 255));
            colors.Add(Color.FromRgb(0, 0, 255));
            colors.Add(Color.FromRgb(128, 0, 255));
            colors.Add(Color.FromRgb(255, 0, 255));
            colors.Add(Color.FromRgb(128, 0, 128));
            BitmapPalette myPalette = new BitmapPalette(colors);

            //The 36 S2 colours
            List<System.Windows.Media.Color> fullcolors = new List<System.Windows.Media.Color>();
            fullcolors.Add(Color.FromRgb(0, 0, 0));
            fullcolors.Add(Color.FromRgb(255, 42, 0));
            fullcolors.Add(Color.FromRgb(255, 85, 0));
            fullcolors.Add(Color.FromRgb(255, 128, 0));
            fullcolors.Add(Color.FromRgb(255, 170, 0));
            fullcolors.Add(Color.FromRgb(255, 213, 0));
            fullcolors.Add(Color.FromRgb(255, 255, 0));
            fullcolors.Add(Color.FromRgb(212, 255, 0));
            fullcolors.Add(Color.FromRgb(170, 255, 0));
            fullcolors.Add(Color.FromRgb(128, 255, 0));
            fullcolors.Add(Color.FromRgb(85, 255, 0));
            fullcolors.Add(Color.FromRgb(42, 255, 0));
            fullcolors.Add(Color.FromRgb(0, 255, 0));
            fullcolors.Add(Color.FromRgb(0, 255, 42));
            fullcolors.Add(Color.FromRgb(0, 255, 85));
            fullcolors.Add(Color.FromRgb(0, 255, 128));
            fullcolors.Add(Color.FromRgb(0, 255, 170));
            fullcolors.Add(Color.FromRgb(0, 255, 212));
            fullcolors.Add(Color.FromRgb(0, 255, 255));
            fullcolors.Add(Color.FromRgb(0, 212, 255));
            fullcolors.Add(Color.FromRgb(0, 170, 255));
            fullcolors.Add(Color.FromRgb(0, 128, 255));
            fullcolors.Add(Color.FromRgb(0, 85, 255));
            fullcolors.Add(Color.FromRgb(0, 43, 255));
            fullcolors.Add(Color.FromRgb(0, 0, 255));
            fullcolors.Add(Color.FromRgb(42, 0, 255));
            fullcolors.Add(Color.FromRgb(85, 0, 255));
            fullcolors.Add(Color.FromRgb(128, 0, 255));
            fullcolors.Add(Color.FromRgb(170, 0, 255));
            fullcolors.Add(Color.FromRgb(212, 0, 255));
            fullcolors.Add(Color.FromRgb(255, 0, 255));
            fullcolors.Add(Color.FromRgb(255, 0, 212));
            fullcolors.Add(Color.FromRgb(255, 0, 170));
            fullcolors.Add(Color.FromRgb(128, 0, 128));
            fullcolors.Add(Color.FromRgb(128, 0, 85));
            fullcolors.Add(Color.FromRgb(128, 0, 43));
            BitmapPalette myfullPalette = new BitmapPalette(fullcolors);

            //The 3 TD "colours"
            List<System.Windows.Media.Color> colorsBW = new List<System.Windows.Media.Color>();
            colorsBW.Add(Color.FromRgb(128, 128, 128));
            colorsBW.Add(Color.FromRgb(0, 0, 0));
            colorsBW.Add(Color.FromRgb(255, 255, 255));
            BitmapPalette myBWPalette = new BitmapPalette(colorsBW);
            #endregion

            _TDbackBuffer = new WriteableBitmap(304, 240, 96, 96, PixelFormats.Indexed8, myBWPalette);
            _APSbackBuffer = new WriteableBitmap(304, 240, 96, 96, PixelFormats.Gray8, null);
            
            ATISinterface.initATIS("ramtest.bit");
            BiasesInterface.initBiases();

            ATISinterface.setWire(ATISinterface.ControlSignals.Enable_AER_TD);
            ATISinterface.setWire(ATISinterface.ControlSignals.Enable_AER_APS);
            ATISinterface.resetFifos();
            /*
            //init HMAX
            ATIS.setHMAXThreshold(ATISinterface.HMAXlayer.S1, 150);
            ATIS.setHMAXDecay(ATISinterface.HMAXlayer.S1, 1);
            ATIS.setHMAXThreshold(ATISinterface.HMAXlayer.C1, 5); //actually a refractory period...
            ATIS.setHMAXThreshold(ATISinterface.HMAXlayer.S2, 150);
            ATIS.setHMAXDecay(ATISinterface.HMAXlayer.S2, 1);

            ATIS.programHMAXkernels(ATISinterface.HMAXlayer.S2, "S2Kernels.val");
            //ATIS.programHMAXkernels(ATISinterface.HMAXlayer.S1, "S1Kernels.val");
         */

            ATISinterface.setWire(ATISinterface.ControlSignals.couple);

            AcquisitionThread();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Kill the background thread
            _runningAcquisition = false;
            if (_controlWindow != null)
                _controlWindow.Close();
        }

        private void screenUpdate(Object source, ElapsedEventArgs e)
        {
            /*
             * _bw1.DoWork += getmorebytes;  //bytes1 
             * _bw1.RunWorkerCompleted += bw1_RunWorkerCompleted;
             * _bw2.DoWork += getmorebytes;  //bytes2 
             * _bw2.RunWorkerCompleted += bw2_RunWorkerCompleted;
             * 
            void init ()
             *  {
             *      _bw1.RunWorkerAsync(ref bytes1)
             *  }
             * 
             * 
             * bw1_RunWorkerCompleted (object sender, RunWorkerCompletedEventArgs e)
             * {
             *      _bw2.RunWorkerAsync(ref bytes2)
             *      processEvents(bytes1);
             * }
             * 
             * * bw2_RunWorkerCompleted (object sender, RunWorkerCompletedEventArgs e)
             * {
             *      _bw1.RunWorkerAsync(ref bytes1)
             *      processEvents(bytes2);
             * }
* 
             *  void getmorebytes(ref byte[] bytes)
             *  {
             *      lock(bytes)
             *      {
             *  
             *      }
             *  }
             *  
             * void processEvents(bytes, APS, TD)
             * {
             *      lock(bytes)
             *      lock(APS)
             *      {
             *      
             *      }
             * }
             * 
             * void updateDisplay(APS, TD)
             * lock(APS)
             */

            if (_runningAcquisition == false)
            {
                _runningAcquisition = true;
                _pixelData = Enumerable.Repeat((byte)0, 304 * 240).ToArray();

                APSprocessing.DrawBox(_pixelDataAPS, _boxSizeX, _boxSizeY);

                if (ATISinterface.getWire(ATISinterface.ControlSignals.Enable_AER_TD) || ATISinterface.getWire(ATISinterface.ControlSignals.Enable_AER_APS))
                {
                    byte[] DataInTD = new byte[1];
                    int numbytesTD = 0;

                    if (!APSprocessing._calibrated)
                        System.Threading.Thread.Sleep(50); //allow time to gather enough data for calibration

                    numbytesTD = ATISinterface.getEvents(ref DataInTD);

                    if (numbytesTD > 0)
                    {
                        var numbytesTD_written = 0;
                        stream = new MemoryStream(DataInTD);
                        reader = new BinaryReader(stream);

                        for (uint i = 0; i < numbytesTD; i += 4)
                        {
                            long timestamp = ((long)(DataInTD[i + 1] & 0x1F) << 8) + DataInTD[i] + timestamp_overflowCounter;
                            var x = ((DataInTD[i + 1] & 0x20) << 3) + DataInTD[i + 2];
                            var y = DataInTD[i + 3];
                            var p = (DataInTD[i + 1] & 0x80) >> 7;
                            var type = (DataInTD[i + 1] & 0x40) >> 6;

                            if ((y == 240) && (x == 305))
                            {
                                timestamp_overflowCounter += 1 << 13;
                            }
                            else
                                if (y > 239)
                                {
                                    y_error_Counter++;
                                    // these errors occur only at large Y and small X values
                                }
                                else
                                    if (type == 1) //if it is an exposure event EM
                                    {
                                        var evt_valid = APSprocessing.filterAPSEvent(timestamp, x, y, p, type);
                                        if (evt_valid)
                                            APSprocessing.updateAPSdisplay(timestamp, x, y, p, type, _pixelDataAPS);
                                        APSprocessing.processAPSevent(timestamp, x, y, p, type, evt_valid);
                                    }
                                    else if (type == 0) //if it is a TD event
                                    {
                                        var evt_valid = TDprocessing.filterTDEvent(timestamp, x, y, p, type);
                                        if (evt_valid)
                                            TDprocessing.updateTDdisplay(timestamp, x, y, p, type, _pixelData);
                                        TDprocessing.processTDevent(timestamp, x, y, p, type, evt_valid);
                                    }

                            /* THIS PART IS IMPORTANT, HANDLES THE SACCADES
                            if (x == 2)
                                if (_logging == true)
                                {
                                    _writerTD.Write(DataInTD, numbytesTD_written, (int)(i) - numbytesTD_written);
                                    numbytesTD_written = (int)(i);
                                    CloseLogging();
                                    imageSlideshow.databaseImage.saveImageIndex++;
                                    if (imageSlideshow.databaseImage.saveArrayList.Count > imageSlideshow.databaseImage.saveImageIndex)
                                    { 
                                        propFsTD = new FileStream(imageSlideshow.databaseImage.saveArrayList[imageSlideshow.databaseImage.saveImageIndex].ToString(), FileMode.Create);
                                        _writerTD = new BinaryWriter(propFsTD);
                                    }else
                                    {
                                        //_logging = false;
                                        motorTimer.DisableTimer();
                                        //boxRecord.IsChecked = false;
                                    }
                                }
                             */
                            //  }
                        }

                        if (ControlWindow._logging == true)
                        {
                            lock (ControlWindow.propFsTD)
                            {
                                if (ControlWindow._logging == true)
                                    ControlWindow._writerTD.Write(DataInTD, numbytesTD_written, numbytesTD - numbytesTD_written);
                            }
                        }

                        reader.Dispose();
                        stream.Dispose();
                    }

                    if (ControlWindow.clearPixelData)
                    {
                        _pixelDataAPS = Enumerable.Repeat((byte)0, 304 * 240).ToArray();
                        ControlWindow.clearPixelData = false;
                    }

                    Dispatcher.Invoke(DispatcherPriority.Send, new Action<byte[], byte[], byte[], byte[], byte[]>(HandleFrameData), _pixelData, _S1Data, _C1Data, _S2Data, _pixelDataAPS);
                    if (!APSprocessing._calibrated)
                        APSprocessing.calibrate();
                }
                _runningAcquisition = false;
            }
        }

        public void AcquisitionThread()
        {
            screenUpdateTimer = new System.Timers.Timer(10);
            screenUpdateTimer.Elapsed += screenUpdate;
            screenUpdateTimer.Enabled = true;//initially disabled
        }

        private void HandleFrameData(byte[] pixelData, byte[] S1Data, byte[] C1Data, byte[] S2Data, byte[] apsData)
        {
            _TDbackBuffer.WritePixels(new Int32Rect(0, 0, 304, 240), pixelData, (304), 0);
            _APSbackBuffer.WritePixels(new Int32Rect(0, 0, 304, 240), apsData, (304), 0);
            TDCanvas.Source = _TDbackBuffer;
            APSCanvas.Source = _APSbackBuffer;
            statusBar.Items[0] = TDprocessing.meanEvt.ToString();
            //statusBar.Items[0] = y_error_Counter.ToString();
        }

        public static void CloseControlWindow()
        {
            _controlWindow = null;
        }

        private void btnControl_Click(object sender, RoutedEventArgs e)
        {
            if (_controlWindow == null)
            {
                _controlWindow = new ControlWindow();
                _controlWindow.Show();
            }
        }

        private void APSCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            APSprocessing.calibrate();
        }
    }
}