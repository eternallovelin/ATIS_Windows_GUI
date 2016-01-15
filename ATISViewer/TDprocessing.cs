using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATISViewer
{
    public static class TDprocessing
    {
        private static long[,] _lastTDspike = new long[304, 240];
        private static long prev_timestamp = 0;
        public static double meanEvt = 0;
        private static double[,] _evtISI = new double[304, 240];

        public static bool filterTDEvent(long timestamp, int x, int y, int p, int type)
        {
            for (int sub_y = Math.Max(y - 1, 0); sub_y <= Math.Min(y + 1, 239); sub_y++)
                for (int sub_x = Math.Max(x - 1, 0); sub_x <= Math.Min(x + 1, 303); sub_x++)
                {
                    if (timestamp - _lastTDspike[sub_x, sub_y] < ControlWindow.persistence_us || ControlWindow.persistence_us == 0)
                        if (sub_x != x | sub_y != y)
                            return true;
                }
            return false;
        }

        public static void processTDevent(long timestamp, int x, int y, int p, int type, bool evt_valid)
        {
            //check mean event ISI
            meanEvt = meanEvt * 0.99 + (timestamp - prev_timestamp) * 0.01;
            _evtISI[x, y] = _evtISI[x, y] * 0.99 + (timestamp - prev_timestamp) * 0.01;
            _lastTDspike[x, y] = timestamp;
            prev_timestamp = timestamp;
            //if (UDP.sendUDP)
            //    UDP.sendspikeUDP(x, y, p, timestamp);
        }

        public static void updateTDdisplay(long timestamp, int x, int y, int p, int type, byte[] _pixelData)
        { 
                   _pixelData[y * 304 + x] = (byte)(2 - p);
        }
    }
}
