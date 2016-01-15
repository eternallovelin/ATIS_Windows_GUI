// GET THIS WORKING, THEN SPLIT HMAX ETC INTO NEW CLASSES (NOT SUBCLASSES)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpalKelly.FrontPanel;

namespace ATISViewer
{
    public static class ATISinterface
    {
        private static okCFrontPanel dev;

        public static void initATIS(string filename)
        {
            // detect Opal Kelly and Program
            dev = new okCFrontPanel();
            int num_boards = 0;
            bool device_opened = false;
            lock (dev)
            {
                while (device_opened == false)
                { 
                    num_boards = dev.GetDeviceCount();
                
                    for(int i=0; i < num_boards; i++)
                    {
                        if (dev.GetDeviceListModel(i) == okCFrontPanel.BoardModel.brdXEM6010LX150)
                        {
                            dev.OpenBySerial(dev.GetDeviceListSerial(i));
                            device_opened = true;
                        }
                    }
                }
                /*
                while (dev.OpenBySerial("") != okCFrontPanel.ErrorCode.NoError)
                {

                }//wait for Opal Kelly to be plugged in
                */
                dev.LoadDefaultPLLConfiguration();
                dev.ConfigureFPGA(filename);
            }

            // Initialize the Biases
            BiasesInterface.initBiases();
        }

        #region "enum definitions"
        public enum PipeOutAddress : int
        {
            EventsIn = 0xA0,
        }

        public enum WireOutAddress : int
        {
            RAM_pagesLSb = 0x20,
            RAM_pagesMSb = 0x21,
            GPIO1       = 0x21,
            GPIO2       = 0x22,
        }

        //------------------- define the triggerin bits ----------------------//
        public enum TriggerInAddress : int
        {
            ControlTriggers = 0x40,
        }
        
        public enum TriggerWires : int
        {
            DAC_reset          = 0, 
            //DAC_latch          = 1, 
            Reset_module_fifo  = 2,
            reset_event_chain  = 7,
            WireInEventValid   = 10,
        }
        //--------------------------------------------------------------------//


        //--------------------- define the wirein addresses ------------------------//
        public enum WireInAddress : int
        {
            ControlSignalWire = 0x00,
            TDfilter          = 0x04,
            HMAX              = 0x06, //send all on the same wire, use different triggers???
            //HMAX uses 0x06 0x07 0x08
            WireInEvt           = 0x09,
            GPIO_control        = 0x10,
            GPIO_value          = 0x11,
        }

        //--------------------- define the wirein bits ------------------------//
        public static uint controlWireIn = 0;
        public enum ControlSignals : int
        {
            couple = 0x0001,
            sequential = 0x0002,
            LIFUdownB = 0x0004,
            APS_ROI_en = 0x0008,
            TD_ROI_en = 0x0010,
            
            //public const uint set_ctrl_bias  = 0x0020;
            shutter = 0x0040,
            output_driver_default = 0x0080,
            BGenPower_Up  = 0x0100,
            ROI_TD_inv = 0x0200,
            Enable_AER_TD = 0x0400,
            Enable_AER_APS = 0x0800,
        }

        //--------------------- define the pipeIn addresses ------------------------//
        public enum PipeInAddress : int
        {
            HMAX        = 0x80,
            //HMAX uses 0x80, 0x81, 0x82
            Motor       = 0x83,
            Biases      = 0x90,
            ROI         = 0x91,
        }

        #endregion

        //------------------ HMAX parameters-----------------------------------------------------------
        public class HMAX
        {
            public static byte[] thresholds = new byte[3] {0,0,0};
            public static byte[] decays = new byte[3] { 0, 0, 0 };
        }

        public enum HMAXlayer : int
        {
            S1 = 0,
            C1 = 1,
            S2 = 2,

        }

        public static void setHMAXThreshold(HMAXlayer layer, byte value)
        {
            HMAX.thresholds[(int)layer] = value;
            SetWireInValue(WireInAddress.HMAX, (int)layer, (uint)(HMAX.thresholds[(int)layer]), 0x00FF);
            UpdateWireIns();
        }

        public static void setHMAXDecay(HMAXlayer layer, byte value)
        {
            HMAX.decays[(int)layer] = value;
            SetWireInValue(WireInAddress.HMAX, (int)layer, (uint)(HMAX.decays[(int)layer] << 8), 0xFF00);
            UpdateWireIns();
            
        }

        public static void programHMAXkernels(HMAXlayer layer, string filename)
        {
            var fs = new FileStream(filename, FileMode.Open);
            var len = (int)fs.Length;
            var KernelBytes = new byte[len];
            fs.Read(KernelBytes, 0, len);
            WriteToPipeIn(PipeInAddress.HMAX, (int)layer, len, KernelBytes);
        }
        //-----------------------------------------------------------------------------------------


        public static void setWire(ControlSignals wire)
        {
            controlWireIn = controlWireIn | (uint)wire;
            SetWireInValue(WireInAddress.ControlSignalWire, controlWireIn);
            UpdateWireIns();
        }

        public static void clearWire(ControlSignals wire)
        {
            
            controlWireIn = controlWireIn & ~(uint)wire;
            SetWireInValue((int)WireInAddress.ControlSignalWire, controlWireIn);
            UpdateWireIns();
            
        }

        public static bool getWire(ControlSignals wire)
        {
            return ((controlWireIn & (uint)wire) > 0);
        }

        public static void setWireInEvt(uint wire)
        {
            
            while (SetWireInValue(WireInAddress.WireInEvt, wire) != 0);
            UpdateWireIns();
        }
        //--------------------------------------------------------------------//


        public static void setTDfilter_value(uint persistence_ms)
        {
            SetWireInValue(WireInAddress.TDfilter, persistence_ms);
            UpdateWireIns();
        }

        public static int getEvents(ref byte[] DataInTD)  //CLEANUP !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        {
            int numbytesTD = 0;

            UpdateWireOuts();
            uint readlength = GetWireOutValue(WireOutAddress.RAM_pagesLSb);
            readlength += GetWireOutValue(WireOutAddress.RAM_pagesMSb) << 16;
            if (readlength > 0)
            {

                if (readlength > Int32.MaxValue / 1024)
                {
                    //_readlength = Int32.MaxValue / 128;
                    numbytesTD = 0;
                }
                else
                {
                    
                    DataInTD = new byte[1024 * readlength];
                    //numbytesTD = dev.ReadFromPipeOut(0xA0, 128 * (int)_readlength, DataInTD);
                    numbytesTD = ReadFromBlockPipeOut(PipeOutAddress.EventsIn, 1024, 1024 * (int)(readlength), DataInTD);
                    //n_events_ = dev_->ReadFromBlockPipeOut(0xA0, 1024, read_length * 1024, ev_buffer_);
                }
            }
            return numbytesTD;
        }

        #region "private OpalKelly wrappers"

        private static uint GetWireOutValue(WireOutAddress wout)
        {
            lock(dev)
            {
                return dev.GetWireOutValue((int)wout);
            }
        }

        private static int ReadFromPipeOut(PipeOutAddress pout, int len, byte[] PipeOutData)
        {
            lock(dev)
            { 
                return dev.ReadFromPipeOut((int)pout, len, PipeOutData);
            }
        }


        private static int ReadFromBlockPipeOut(PipeOutAddress pout, int blocksize, int len, byte[] PipeOutData)
        {
            lock(dev)
            { 
                //return dev.ReadFromPipeOut((int)pout, blocksize, len, PipeOutData);
                return dev.ReadFromBlockPipeOut((int)pout, blocksize, len, PipeOutData);
            }
        }



        private static int WriteToPipeIn(PipeInAddress pin, int len, byte[] PipeInData)
        {
            lock (dev)
            {
                return (int)dev.WriteToPipeIn((int)pin, len, PipeInData);
            }
        }


        private static int WriteToPipeIn(PipeInAddress pin, int offset, int len, byte[] PipeInData)
        {
            lock(dev)
            {
                return (int)dev.WriteToPipeIn((int)pin + offset, len, PipeInData);
            }
        }

        

        public static void SetWireInValue(WireInAddress win, int win_offset, uint value)
        {
            lock (dev)
            {
                dev.SetWireInValue((int)win + win_offset, value);
            }
        }

        public static void SetWireInValue(WireInAddress win, int win_offset, uint value, uint mask)
        {
            lock (dev)
            {
                dev.SetWireInValue((int)win + win_offset, value, mask);
            }
        }

        public static int ActivateTriggerIn(TriggerInAddress tin, TriggerWires wireNum)
        {
            lock (dev)
            {
                return (int)dev.ActivateTriggerIn((int)tin, (int)wireNum);
            }
        }

        public static void UpdateWireIns() 
        {
            lock (dev)
            {
                dev.UpdateWireIns();
            }
        }

        public static void UpdateWireOuts()
        {
            lock (dev)
            {
                dev.UpdateWireOuts();
            }
        }

        #endregion



        public static void reset()
        {
            ActivateTriggerIn(TriggerInAddress.ControlTriggers, TriggerWires.Reset_module_fifo);
            ActivateTriggerIn(TriggerInAddress.ControlTriggers, TriggerWires.reset_event_chain);
            ActivateTriggerIn(TriggerInAddress.ControlTriggers, TriggerWires.DAC_reset);
        }

        public static void writeToMotors(int len, byte[] MotorBytes)
        {

            var _OK_Response = WriteToPipeIn(PipeInAddress.Motor, len, MotorBytes); // Transmit Data to FPGA
            while (_OK_Response != len)
                _OK_Response = WriteToPipeIn(PipeInAddress.Motor, len, MotorBytes);// Transmit Data to FPGA
            while (ActivateTriggerIn(TriggerInAddress.ControlTriggers, TriggerWires.WireInEventValid) != 0) ; // FPGA Transmit Data to Motor

            System.Threading.Thread.Sleep(10); //will wait for 3ms

        }

        
        public static void programROI()
        {
            byte[] ROI_bytes = new byte[68];

            //row 0 enabled
            ROI_bytes[0] = 0x01;

            //the rest of the rows
            for (int i = 1; i < 30; i++)
            {
                ROI_bytes[i] = 0x00;
            }

            //the columns
            for (int i = 30; i < 68; i++)
            {
                ROI_bytes[i] = 0xFF;
            }
            /*The first register cell in the chain (byte 0 bit 0) controls line 239, register cell 240 controls line 0, register cell 241 controls column 303 and the 544th and last register cell column 0.*/


            //WriteToPipeIn(PipeInAddress.ROI, 70, ROI_bytes);
            WriteToPipeIn(PipeInAddress.ROI, 68, ROI_bytes);
        }

        public static void TriggerEvtMarker()
        {
            while (ActivateTriggerIn(TriggerInAddress.ControlTriggers, TriggerWires.WireInEventValid) != 0) ; // trigger event writing/motor command(if one has been sent)
        }



        public static int SetWireInValue(WireInAddress win, uint value)
        {
            lock (dev)
            {
                return (int)dev.SetWireInValue((int)win, value);
            }
        }

        public static void resetFifos()
        {
            ActivateTriggerIn(TriggerInAddress.ControlTriggers, TriggerWires.reset_event_chain);
            //System.Threading.Thread.Sleep(10);
        }

        public static void SendBiasValues(byte[] bias_bytes)
        {
            WriteToPipeIn(PipeInAddress.Biases, 114, bias_bytes);
        }
    }
}