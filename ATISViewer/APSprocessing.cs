using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATISViewer
{
    public static class APSprocessing
    {
        private static long[,] _EMtimeDifference = new long[304, 240];
        private static long[,] _EMdata0 = new long[304, 240];
        private static bool[,] _EMreadingValid = new bool[304, 240];
        private static float _displayoffset = 1;
        private static float _displayscaling = 1;
        public static bool _calibrated = false;

        public static bool filterAPSEvent(long timestamp, int x, int y, int p, int type)
        {
            if (p == 1)
            {
                if (_EMreadingValid[x, y] == true)
                {
                    _EMreadingValid[x, y] = false;
                    return true;
                }
                return false;
            }
            else
            {
                _EMreadingValid[x, y] = true;
                return true;
            }
        }



        public static void processAPSevent(long timestamp, int x, int y, int p, int type, bool evt_valid)
        {

        }

        public static void updateAPSdisplay(long timestamp, int x, int y, int p, int type, byte[] _pixelDataAPS)
        {
            if (p == 1)
            {
                _EMtimeDifference[x, y] = timestamp - _EMdata0[x, y];
                var temp = _displayoffset + _displayscaling / _EMtimeDifference[x, y];
                _pixelDataAPS[y * 304 + x] = (byte)Math.Min(Math.Max(temp, 0), 255);
            }
            else
            {
                _EMdata0[x, y] = timestamp;
            }
        }

        public static void calibrate()
        {
            int blackpoint = (int)(304 * 240 * 0.05);
            int whitepoint = (int)(304 * 240 * 0.05);

            var TmaxTemp = new long[blackpoint];
            var TminTemp = new long[whitepoint];

            long temp = 0;
            long Tmax = 0;
            long Tmin = long.MaxValue;
            TminTemp = Enumerable.Repeat((long.MaxValue), 304 * 240).ToArray();
            var i = 0;

            for (int xcounter = 0; xcounter < 304; xcounter++)
            {
                for (int ycounter = 0; ycounter < 240; ycounter++)
                {
                    if (_EMtimeDifference[xcounter, ycounter] > TmaxTemp[0])
                    {
                        TmaxTemp[0] = _EMtimeDifference[xcounter, ycounter];
                        i = 1;
                        while (i < blackpoint-1 && TmaxTemp[i] < TmaxTemp[i - 1])
                        {
                            temp = TmaxTemp[i];
                            TmaxTemp[i] = TmaxTemp[i - 1];
                            TmaxTemp[i - 1] = temp;
                            i++;
                        }
                    }
                    if (_EMtimeDifference[xcounter, ycounter] < TminTemp[0])
                    {
                        if (_EMtimeDifference[xcounter, ycounter] > 0)
                        {
                            TminTemp[0] = _EMtimeDifference[xcounter, ycounter];
                            i = 1;
                            while (i < whitepoint && TminTemp[i] > TminTemp[i - 1])
                            {
                                temp = TminTemp[i];
                                TminTemp[i] = TminTemp[i - 1];
                                TminTemp[i - 1] = temp;
                                i++;
                            }
                        }

                    }
                }
            }
            Tmax = TmaxTemp[0];
            Tmin = TminTemp[0];

            _displayoffset = 255 / (1 - (float)(Tmax) / Tmin);
            _displayscaling = (float)(255) / ((float)(1) / Tmin - (float)(1) / Tmax);
            _calibrated = true;
        }

        public static void DrawBox(byte[] _pixelDataAPS, int _boxSizeX, int _boxSizeY)
        {
            if (_boxSizeX > 0)
                if (_boxSizeY > 0)
                {
                    int x_box = 152 - _boxSizeX / 2;
                    for (int y_box = 120 - _boxSizeY / 2; y_box < 120 + _boxSizeY / 2; y_box++)
                        _pixelDataAPS[y_box * 304 + x_box] = 128;
                    x_box = 152 + _boxSizeX / 2;
                    for (int y_box = 120 - _boxSizeY / 2; y_box < 120 + _boxSizeY / 2; y_box++)
                        _pixelDataAPS[y_box * 304 + x_box] = 128;

                    int y_box2 = 120 - _boxSizeY / 2;
                    for (x_box = 152 - _boxSizeX / 2; x_box < 152 + _boxSizeX / 2; x_box++)
                        _pixelDataAPS[y_box2 * 304 + x_box] = 128;
                    y_box2 = 120 + _boxSizeY / 2;
                    for (x_box = 152 - _boxSizeX / 2; x_box < 152 + _boxSizeX / 2; x_box++)
                        _pixelDataAPS[y_box2 * 304 + x_box] = 128;
                }
        }
    }
}
