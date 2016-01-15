using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ATISViewer
{
    public static class BiasesInterface  //This is biases interface for internal bias generator, ATIS V6
    {
        private static bool init = false;
        private static int bias_index = 0;
        private static byte[] bias_bytes;
        public static void initBiases()
        {
            if (init == false)
            {
                bias_bytes = new byte[114];
                populateLookupTables();
                programDACs();
                ATISinterface.setWire(ATISinterface.ControlSignals.BGenPower_Up);
                init = true;
            }
        }

        #region "Constants for biases"
        public enum BiasName
        {
            CtrlbiasLP,
            CtrlbiasLBBuff,
            CtrlbiasDelTD,
            CtrlbiasSeqDelAPS,
            CtrlbiasDelAPS,
            biasSendReqPdY,
            biasSendReqPdX,
            CtrlbiasGB,
            TDbiasReqPuY,
            TDbiasReqPuX,
            APSbiasReqPuY,
            APSbiasReqPuX,
            APSVrefL,
            APSVrefH,
            APSbiasOut,
            APSbiasHyst,
            APSbiasTail,
            TDbiasCas,
            TDbiasInv,
            TDbiasDiffOff,
            TDbiasDiffOn,
            TDbiasDiff,
            TDbiasFo,
            TDbiasRefr,
            TDbiasPR,
            TDbiasBulk,
            biasBuf,
            biasAPSreset,
        }
        
        public enum BiasPolarity
        {
            P,
            N,
        }

        public enum BiasType
        {
            Voltage,
            Current,
        }

        private static int[,] BiasLookup18;
        private static int[,] BiasLookup33;

        private static byte outputDriverValue;
        private static UInt32 num_1800_values;
        private static UInt32 num_3300_values;

        public static readonly Dictionary<BiasName, BiasType> BiasTypes = new Dictionary<BiasName, BiasType> { 
            {BiasName.CtrlbiasLP , BiasType.Voltage},
            {BiasName.CtrlbiasLBBuff , BiasType.Voltage},
            {BiasName.CtrlbiasDelTD , BiasType.Voltage},
            {BiasName.CtrlbiasSeqDelAPS , BiasType.Voltage},
            {BiasName.CtrlbiasDelAPS , BiasType.Voltage},
            {BiasName.biasSendReqPdY , BiasType.Voltage},
            {BiasName.biasSendReqPdX , BiasType.Voltage},
            {BiasName.CtrlbiasGB , BiasType.Voltage},
            {BiasName.TDbiasReqPuY , BiasType.Voltage},
            {BiasName.TDbiasReqPuX , BiasType.Voltage},
            {BiasName.APSbiasReqPuY , BiasType.Voltage},
            {BiasName.APSbiasReqPuX , BiasType.Voltage},
            {BiasName.APSVrefL , BiasType.Voltage},
            {BiasName.APSVrefH , BiasType.Voltage},
            {BiasName.APSbiasOut , BiasType.Voltage},
            {BiasName.APSbiasHyst , BiasType.Voltage},
            {BiasName.APSbiasTail , BiasType.Voltage},
            {BiasName.TDbiasCas , BiasType.Voltage},
            {BiasName.TDbiasInv , BiasType.Voltage},
            {BiasName.TDbiasDiffOff , BiasType.Voltage},
            {BiasName.TDbiasDiffOn , BiasType.Voltage},
            {BiasName.TDbiasDiff , BiasType.Voltage},
            {BiasName.TDbiasFo , BiasType.Voltage},
            {BiasName.TDbiasRefr , BiasType.Voltage},
            {BiasName.TDbiasPR , BiasType.Voltage},
            {BiasName.TDbiasBulk , BiasType.Voltage},
            {BiasName.biasBuf , BiasType.Voltage},
            {BiasName.biasAPSreset , BiasType.Voltage},
        };

        public static readonly Dictionary<BiasName, BiasPolarity> BiasPolarities = new Dictionary<BiasName, BiasPolarity> { 
            {BiasName.CtrlbiasLP , BiasPolarity.N},
            {BiasName.CtrlbiasLBBuff , BiasPolarity.N},
            {BiasName.CtrlbiasDelTD , BiasPolarity.N},
            {BiasName.CtrlbiasSeqDelAPS , BiasPolarity.N},
            {BiasName.CtrlbiasDelAPS , BiasPolarity.N},
            {BiasName.biasSendReqPdY , BiasPolarity.N},
            {BiasName.biasSendReqPdX , BiasPolarity.N},
            {BiasName.CtrlbiasGB , BiasPolarity.P},
            {BiasName.TDbiasReqPuY , BiasPolarity.P},
            {BiasName.TDbiasReqPuX , BiasPolarity.P},
            {BiasName.APSbiasReqPuY , BiasPolarity.P},
            {BiasName.APSbiasReqPuX , BiasPolarity.P},
            {BiasName.APSVrefL , BiasPolarity.N},
            {BiasName.APSVrefH , BiasPolarity.N},
            {BiasName.APSbiasOut , BiasPolarity.N},
            {BiasName.APSbiasHyst , BiasPolarity.N},
            {BiasName.APSbiasTail , BiasPolarity.N},
            {BiasName.TDbiasCas , BiasPolarity.N},
            {BiasName.TDbiasInv , BiasPolarity.N},
            {BiasName.TDbiasDiffOff , BiasPolarity.N},
            {BiasName.TDbiasDiffOn , BiasPolarity.N},
            {BiasName.TDbiasDiff , BiasPolarity.N},
            {BiasName.TDbiasFo , BiasPolarity.P},
            {BiasName.TDbiasRefr , BiasPolarity.P},
            {BiasName.TDbiasPR , BiasPolarity.P},
            {BiasName.TDbiasBulk , BiasPolarity.P},
            {BiasName.biasBuf , BiasPolarity.N},
            {BiasName.biasAPSreset , BiasPolarity.P},
        };

        public static readonly Dictionary<BiasName, string> stringNames = new Dictionary<BiasName, string> { 
            {BiasName.CtrlbiasLP , "CtrlbiasLP"},
            {BiasName.CtrlbiasLBBuff , "CtrlbiasLBBuff"},
            {BiasName.CtrlbiasDelTD , "CtrlbiasDelTD"},
            {BiasName.CtrlbiasSeqDelAPS , "CtrlbiasSeqDelAPS"},
            {BiasName.CtrlbiasDelAPS , "CtrlbiasDelAPS"},
            {BiasName.biasSendReqPdY , "biasSendReqPdY"},
            {BiasName.biasSendReqPdX , "biasSendReqPdX"},
            {BiasName.CtrlbiasGB , "CtrlbiasGB"},
            {BiasName.TDbiasReqPuY , "TDbiasReqPuY"},
            {BiasName.TDbiasReqPuX , "TDbiasReqPuX" },
            {BiasName.APSbiasReqPuY , "APSbiasReqPuY" },
            {BiasName.APSbiasReqPuX , "APSbiasReqPuX" },
            {BiasName.APSVrefL , "APSVrefL" },
            {BiasName.APSVrefH , "APSVrefH" },
            {BiasName.APSbiasOut , "APSbiasOut" },
            {BiasName.APSbiasHyst , "APSbiasHyst" },
            {BiasName.APSbiasTail , "APSbiasTail" },
            {BiasName.TDbiasCas , "TDbiasCas" },
            {BiasName.TDbiasInv , "TDbiasInv" },
            {BiasName.TDbiasDiffOff , "TDbiasDiffOff" },
            {BiasName.TDbiasDiffOn , "TDbiasDiffOn" },
            {BiasName.TDbiasDiff , "TDbiasDiff" },
            {BiasName.TDbiasFo , "TDbiasFo" },
            {BiasName.TDbiasRefr , "TDbiasRefr" },
            {BiasName.TDbiasPR , "TDbiasPR"},
            {BiasName.TDbiasBulk , "TDbiasBulk"},
            {BiasName.biasBuf , "biasBuf"},
            {BiasName.biasAPSreset , "biasAPSreset"},
        };

        public static readonly Dictionary<BiasName, bool> BiasCascode = new Dictionary<BiasName, bool> { 
            {BiasName.CtrlbiasLP , true},
            {BiasName.CtrlbiasLBBuff , true},
            {BiasName.CtrlbiasDelTD , true},
            {BiasName.CtrlbiasSeqDelAPS , true},
            {BiasName.CtrlbiasDelAPS , true},
            {BiasName.biasSendReqPdY , true},
            {BiasName.biasSendReqPdX , true},
            {BiasName.CtrlbiasGB , true},
            {BiasName.TDbiasReqPuY , true},
            {BiasName.TDbiasReqPuX , true},
            {BiasName.APSbiasReqPuY , true},
            {BiasName.APSbiasReqPuX , true},
            {BiasName.APSVrefL , true},
            {BiasName.APSVrefH , true},
            {BiasName.APSbiasOut , true},
            {BiasName.APSbiasHyst , true},
            {BiasName.APSbiasTail , true},
            {BiasName.TDbiasCas , true},
            {BiasName.TDbiasInv , true},
            {BiasName.TDbiasDiffOff , true},
            {BiasName.TDbiasDiffOn , true},
            {BiasName.TDbiasDiff , true},
            {BiasName.TDbiasFo , true},
            {BiasName.TDbiasRefr , true},
            {BiasName.TDbiasPR , true},
            {BiasName.TDbiasBulk , true},
            {BiasName.biasBuf , true},
            {BiasName.biasAPSreset , true},
        };

        public static readonly Dictionary<BiasName, bool> max_1800 = new Dictionary<BiasName, bool> { 
            {BiasName.CtrlbiasLP , true},
            {BiasName.CtrlbiasLBBuff , true},
            {BiasName.CtrlbiasDelTD , true},
            {BiasName.CtrlbiasSeqDelAPS , true},
            {BiasName.CtrlbiasDelAPS , true},
            {BiasName.biasSendReqPdY , true},
            {BiasName.biasSendReqPdX , true},
            {BiasName.CtrlbiasGB , true},
            {BiasName.TDbiasReqPuY , true},
            {BiasName.TDbiasReqPuX , true},
            {BiasName.APSbiasReqPuY , true},
            {BiasName.APSbiasReqPuX , true},
            {BiasName.APSVrefL , false},
            {BiasName.APSVrefH , false},
            {BiasName.APSbiasOut , false},
            {BiasName.APSbiasHyst , false},
            {BiasName.APSbiasTail , false},
            {BiasName.TDbiasCas , false},
            {BiasName.TDbiasInv , false},
            {BiasName.TDbiasDiffOff , false},
            {BiasName.TDbiasDiffOn , false},
            {BiasName.TDbiasDiff , false},
            {BiasName.TDbiasFo , false},
            {BiasName.TDbiasRefr , false},
            {BiasName.TDbiasPR , false},
            {BiasName.TDbiasBulk , false},
            {BiasName.biasBuf , false},
            {BiasName.biasAPSreset , false},
        };

       //public int[] BiasNormal = new int[] { 3050, 3150, 750, 620, 620, 700, 950, 2000, 400, 620, 320, 780, 350, 880, 850, 2950, 1150, 700, 970, 2680, 810, 2800, 1240, 3150, 1100, 820 };
        public static Dictionary<BiasName, int> BiasVoltage = new Dictionary<BiasName, int> { 
            {BiasName.CtrlbiasLP , 618},
            {BiasName.CtrlbiasLBBuff , 955},
            {BiasName.CtrlbiasDelTD , 400},
            {BiasName.CtrlbiasSeqDelAPS , 323},
            {BiasName.CtrlbiasDelAPS , 302},
            {BiasName.biasSendReqPdY , 849},
            {BiasName.biasSendReqPdX , 1152},
            {BiasName.CtrlbiasGB , 1159},
            {BiasName.TDbiasReqPuY , 849},
            {BiasName.TDbiasReqPuX , 1074},
            {BiasName.APSbiasReqPuY , 1102},
            {BiasName.APSbiasReqPuX , 849},
            {BiasName.APSVrefL , 3150},
            {BiasName.APSVrefH , 3240},
            {BiasName.APSbiasOut , 670},
            {BiasName.APSbiasHyst , 462},
            {BiasName.APSbiasTail , 526},
            {BiasName.TDbiasCas , 1204},
            {BiasName.TDbiasInv , 796},
            {BiasName.TDbiasDiffOff , 450},
            {BiasName.TDbiasDiffOn , 552},
            {BiasName.TDbiasDiff , 501},
            {BiasName.TDbiasFo , 2993},
            {BiasName.TDbiasRefr , 2863},
            {BiasName.TDbiasPR , 3197},
            {BiasName.TDbiasBulk , 2646},
            {BiasName.biasBuf , 27},
            {BiasName.biasAPSreset , 27},
        };


        public static readonly Dictionary<BiasName, int> BiasMaxVoltage = new Dictionary<BiasName, int> { 
            {BiasName.APSVrefL , 3300},
            {BiasName.APSVrefH , 3300},
            {BiasName.APSbiasOut , 3300},
            {BiasName.APSbiasHyst , 3300},
            {BiasName.CtrlbiasLP , 1800},
            {BiasName.APSbiasTail , 3300},
            {BiasName.CtrlbiasLBBuff , 1800},
            {BiasName.TDbiasCas , 3300},
            {BiasName.CtrlbiasDelTD , 1800},
            {BiasName.TDbiasDiffOff , 3300},
            {BiasName.CtrlbiasSeqDelAPS , 1800},
            {BiasName.TDbiasDiffOn , 3300},
            {BiasName.CtrlbiasDelAPS , 1800},
            {BiasName.TDbiasInv , 3300},
            {BiasName.biasSendReqPdY , 1800},
            {BiasName.TDbiasFo , 3300},
            {BiasName.biasSendReqPdX , 1800},
            {BiasName.TDbiasDiff , 3300},
            {BiasName.CtrlbiasGB , 1800},
            {BiasName.TDbiasBulk , 3300},
            {BiasName.TDbiasReqPuY , 1800},
            {BiasName.TDbiasRefr , 3300},
            {BiasName.TDbiasReqPuX , 1800},
            {BiasName.TDbiasPR , 3300},
            {BiasName.APSbiasReqPuY , 1800},
            {BiasName.APSbiasReqPuX , 1800},
            {BiasName.biasBuf, 3300},
            {BiasName.biasAPSreset, 3300},
        };
        
        //names                                       APSvrefL-26                               APSvrefH-25                                         APSbiasOut-24                                       APSbiasHyst-23                                      CtrlbiasLP-12                                       APSbiasTail-22                                      CtrlbiasLBBuff-11                                   TDbiasCas-21                                        CtrlbiasDelTD-10                                    TDbiasDiffOff-19                                    CtrlbiasSeqDelAPS-9                                 TDbiasDiffOn-18                                     CtrlbiasDelAPS-8                                    TDbiasInv-20                                        biasSendReqPdY 7                                    TDbiasFo-16                                         biasSendReqPdX-6                                    TDbiasDiff-17                                       CtrlbiasGB-5                                        TDbiasBulk-13                                       TDbiasReqPuY-4                                      TDbiasRefr-15                                       TDbiasReqPuX-3                                      TDbiasPR-14                                         APSbiasReqPuY-2                                     APSbiasReqPuX-1
        /*
        public Int64[] address = new Int64[] { (0 * (Int64)Math.Pow(2.0, 40) + 3 * (Int64)Math.Pow(2.0, 38)), (15 * (Int64)Math.Pow(2.0, 16) + 3 * (Int64)Math.Pow(2.0, 14)), (14 * (Int64)Math.Pow(2.0, 16) + 3 * (Int64)Math.Pow(2.0, 14)), (13 * (Int64)Math.Pow(2.0, 16) + 3 * (Int64)Math.Pow(2.0, 14)), (12 * (Int64)Math.Pow(2.0, 40) + 3 * (Int64)Math.Pow(2.0, 38)), (12 * (Int64)Math.Pow(2.0, 16) + 3 * (Int64)Math.Pow(2.0, 14)), (11 * (Int64)Math.Pow(2.0, 40) + 3 * (Int64)Math.Pow(2.0, 38)), (11 * (Int64)Math.Pow(2.0, 16) + 3 * (Int64)Math.Pow(2.0, 14)), (10 * (Int64)Math.Pow(2.0, 40) + 3 * (Int64)Math.Pow(2.0, 38)), (9 * (Int64)Math.Pow(2.0, 16) + 3 * (Int64)Math.Pow(2.0, 14)), (9 * (Int64)Math.Pow(2.0, 40) + 3 * (Int64)Math.Pow(2.0, 38)), (8 * (Int64)Math.Pow(2.0, 16) + 3 * (Int64)Math.Pow(2.0, 14)), (8 * (Int64)Math.Pow(2.0, 40) + 3 * (Int64)Math.Pow(2.0, 38)), (10 * (Int64)Math.Pow(2.0, 16) + 3 * (Int64)Math.Pow(2.0, 14)), (7 * (Int64)Math.Pow(2.0, 40) + 3 * (Int64)Math.Pow(2.0, 38)), (6 * (Int64)Math.Pow(2.0, 16) + 3 * (Int64)Math.Pow(2.0, 14)), (6 * (Int64)Math.Pow(2.0, 40) + 3 * (Int64)Math.Pow(2.0, 38)), (7 * (Int64)Math.Pow(2.0, 16) + 3 * (Int64)Math.Pow(2.0, 14)), (5 * (Int64)Math.Pow(2.0, 40) + 3 * (Int64)Math.Pow(2.0, 38)), (3 * (Int64)Math.Pow(2.0, 16) + 3 * (Int64)Math.Pow(2.0, 14)), (4 * (Int64)Math.Pow(2.0, 40) + 3 * (Int64)Math.Pow(2.0, 38)), (5 * (Int64)Math.Pow(2.0, 16) + 3 * (Int64)Math.Pow(2.0, 14)), (3 * (Int64)Math.Pow(2.0, 40) + 3 * (Int64)Math.Pow(2.0, 38)), (4 * (Int64)Math.Pow(2.0, 16) + 3 * (Int64)Math.Pow(2.0, 14)), (2 * (Int64)Math.Pow(2.0, 40) + 3 * (Int64)Math.Pow(2.0, 38)), (1 * (Int64)Math.Pow(2.0, 40) + 3 * (Int64)Math.Pow(2.0, 38)) };
        public Int64[] offset = new Int64[] { (Int64)Math.Pow(2.0, 26), (Int64)Math.Pow(2.0, 2), (Int64)Math.Pow(2.0, 2), (Int64)Math.Pow(2.0, 2), (Int64)Math.Pow(2.0, 26), (Int64)Math.Pow(2.0, 2), (Int64)Math.Pow(2.0, 26), (Int64)Math.Pow(2.0, 2), (Int64)Math.Pow(2.0, 26), (Int64)Math.Pow(2.0, 2), (Int64)Math.Pow(2.0, 26), (Int64)Math.Pow(2.0, 2), (Int64)Math.Pow(2.0, 26), (Int64)Math.Pow(2.0, 2), (Int64)Math.Pow(2.0, 26), (Int64)Math.Pow(2.0, 2), (Int64)Math.Pow(2.0, 26), (Int64)Math.Pow(2.0, 2), (Int64)Math.Pow(2.0, 26), (Int64)Math.Pow(2.0, 2), (Int64)Math.Pow(2.0, 26), (Int64)Math.Pow(2.0, 2), (Int64)Math.Pow(2.0, 26), (Int64)Math.Pow(2.0, 2), (Int64)Math.Pow(2.0, 26), (Int64)Math.Pow(2.0, 26) };
        public int[] BiasMax = new int[] { 3300, 3300, 3300, 3300, 1800, 3300, 1800, 3300, 1800, 3300, 1800, 3300, 1800, 3300, 1800, 3300, 1800, 3300, 1800, 3300, 1800, 3300, 1800, 3300, 1800, 1800 };
        
        
        public int[] BiasNormal = new int[] { 3050, 3150, 750, 620, 620, 700, 950, 2000, 400, 620, 320, 780, 350, 880, 850, 2950, 1150, 700, 970, 2680, 810, 2800, 1240, 3150, 1100, 820 };
        public int[] BiasPresent = new int[] { 3050, 3150, 750, 620, 620, 700, 950, 2000, 400, 620, 320, 780, 350, 880, 850, 2950, 1150, 700, 970, 2680, 810, 2800, 1240, 3150, 1100, 820 };
         * */
        //public int[] BiasCustom = new int[] { 2300, 3150, 750, 620, 620, 700, 950, 2000, 400, 550,/*570*/           320, 800/*800*/, 350, 880, 850, 2900, 1150,               /*590*/600, 970, 2680, 810, 2400, 1240, 2750, 1100, 820 };
        //private int[] BiasFast = new int[] { 3050, 3150, 750, 620, 620, 700, 950, 2000, 400, 520, 320, 720, 350, 790, 850, 2950, 1150, 600, 970, 2680, 810, 2900, 1240, 2950, 1100, 820 };
        //private int[] BiasFast = new int[] {      3050,           3150,           750,            620,                620,                700,                950,                2000,               400,                    520,                    320,                720,                350,                    790,                850,                2950,           1150,               600,            970,            2680,           810,                2900,           1240,              2950,            1100,               820 };
        //names                                 APSvrefL-26     APSvrefH-25     APSbiasOut-24   APSbiasHyst-23      CtrlbiasLP-12       APSbiasTail-22      CtrlbiasLBBuff-11   TDbiasCas-21        CtrlbiasDelTD-10        TDbiasDiffOff-19    CtrlbiasSeqDelAPS-9     TDbiasDiffOn-18     CtrlbiasDelAPS-8        TDbiasInv-20        biasSendReqPdY 7    TDbiasFo-16     biasSendReqPdX-6   TDbiasDiff-17   CtrlbiasGB-5    TDbiasBulk-13   TDbiasReqPuY-4      TDbiasRefr-15   TDbiasReqPuX-3      TDbiasPR-14     APSbiasReqPuY-2     APSbiasReqPuX-1                
        #endregion

        public static void ModifyBias(BiasName bname, int voltage)
        {
            BiasVoltage[bname] = voltage;
        }

        private static void ChangeBias(BiasName bname, int voltage)
        {
            BiasVoltage[bname] = voltage;

            UInt16 upper_16 = 0;
            UInt16 lower_16 = 0;
            
            UInt16 polarity = 0;
            UInt32 registerValue;

            //lookup the values
            int searchIndex = 0;
            if (BiasTypes[bname] == BiasType.Voltage)
                if (max_1800[bname] == true)
                {
                    while ((BiasLookup18[searchIndex, 0] < voltage) & (searchIndex < (num_1800_values - 1)))
                        searchIndex++;
                    registerValue = (UInt32)BiasLookup18[searchIndex, 1];
                    polarity = (UInt16)BiasLookup18[searchIndex, 2];
                }
                else
                {
                    while ((BiasLookup33[searchIndex, 0] < voltage) & (searchIndex < (num_3300_values - 1)))
                        searchIndex++;
                    registerValue = (UInt32)BiasLookup33[searchIndex, 1];
                    polarity = (UInt16)BiasLookup18[searchIndex, 2];
                }
            else
                registerValue = 0;


            //create the programming string
            
            //bit 31 (pad enable) set to 0 for internal bias generator

            //bit 30 (internal bias enable) set to 1 for internal bias generator
            upper_16 += 1 << 14;

            //bit 29 is polarity. Set to 1 for N
            upper_16 += (UInt16)(polarity << 13);

            //bit 28 is cascode. Set to 1 for cascode
            if (BiasCascode[bname] == true)
                upper_16 += 1 << 12;

            //bit 27 voltage/current type. Set to 1 for voltage
            if (BiasTypes[bname] == BiasType.Voltage)
                upper_16 += 1 << 11;

            //bit 26:21 = 8;
            upper_16 += 8 << 5;


            


            //bit 20:0 = voltage/current value
            upper_16 += (UInt16)(registerValue >> 16);

            lower_16 += (UInt16)(registerValue);

            bias_bytes[bias_index] = (byte)lower_16;
            bias_index++;
            bias_bytes[bias_index] = (byte)(lower_16>>8);
            bias_index++;
            bias_bytes[bias_index] = (byte)(upper_16);
            bias_index++;
            bias_bytes[bias_index] = (byte)(upper_16 >> 8);
            bias_index++;
        }

        public static void populateLookupTables()
        {
            BinaryReader reader = new BinaryReader(File.Open("Biases.dat", FileMode.Open));
            num_1800_values = reader.ReadUInt32();
            num_3300_values = reader.ReadUInt32();
            outputDriverValue = reader.ReadByte();
            
            UInt32 NumBiasValuesToRead = num_1800_values + num_3300_values;

            BiasLookup33 = new int[num_3300_values, 3];
            BiasLookup18 = new int[num_1800_values, 3];

            int biasIndex18 = 0;
            int biasIndex33 = 0;


            byte[] polarity = new byte[NumBiasValuesToRead];
            byte[] maxValue_1800 = new byte[NumBiasValuesToRead];
            int[] voltage = new int[NumBiasValuesToRead];
            int[] register_value = new int[NumBiasValuesToRead];

            for (int i = 0; i < NumBiasValuesToRead; i++)
            {
                polarity[i] = reader.ReadByte();
            }
            for (int i = 0; i < NumBiasValuesToRead; i++)
            {
                maxValue_1800[i] = reader.ReadByte();
            }
            for (int i = 0; i < NumBiasValuesToRead; i++)
            {
                voltage[i] = (int)reader.ReadUInt16();
            }
            for (int i = 0; i < NumBiasValuesToRead; i++)
            {
                register_value[i] = (int)reader.ReadUInt32();
            }

            for (int i = 0; i < NumBiasValuesToRead; i++)
            {
                if (maxValue_1800[i] == 1)
                {
                    BiasLookup18[biasIndex18, 0] = voltage[i];
                    BiasLookup18[biasIndex18, 1] = register_value[i];
                    BiasLookup18[biasIndex18, 2] = polarity[i];
                    biasIndex18++;
                }
                else
                {
                    BiasLookup33[biasIndex33, 0] = voltage[i];
                    BiasLookup33[biasIndex33, 1] = register_value[i];
                    BiasLookup33[biasIndex33, 2] = polarity[i];
                    biasIndex33++;
                }
            }
        }       


        public static void programDACs()
        {
            bias_index = 0;
            bias_bytes[bias_index] = 0;
            bias_index++;
            bias_bytes[bias_index] = (byte)(outputDriverValue << 4);
            bias_index++;

            BiasName b = 0;
            for (int i = 0; i < 28; i++) //there are 28 bias voltages for this version of the chip
            {
                ChangeBias(b, (int)BiasVoltage[b]);
                b++;
            }
            ATISinterface.SendBiasValues(bias_bytes);
            
            //temporary file writing for debugging
            string filename = "binaryBiases.bin";
            FileStream propFsBias = new FileStream(filename, FileMode.Create);
            BinaryWriter DebugWriter = new BinaryWriter(propFsBias);
            DebugWriter.Write(bias_bytes, 0, 114);
            DebugWriter.Close();
            //end debugging

            bias_index = 0;
        }
    }
}