using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitTest.UtilityClasses
{

    /// <summary>
    /// Provides standardized list of codes and other information, particular useful for test valid values.
    /// 
    /// Add to but do not remove from the lists.
    /// </summary>
    public static class UnitTestStandardLists
    {

        /// <summary>
        /// List of boolean types.
        /// </summary>
        /// <returns></returns>
        public static bool[] BooleanList
        {
            get
            {
                bool[] result;

                result = new bool[] { false, true };

                return result;
            }
        }

        /// <summary>
        /// List of program class codes for testing.
        /// </summary>
        public static string[] ClassCodeList
        {
            get
            {
                string[] result;

                result = new string[] { "A", "B", "N", "NA", "NB", "P1", "P2" };

                return result;
            }
        }

        /// <summary>
        /// Large list of actual component type codes for testing.
        /// </summary>
        /// <returns></returns>
        public static string[] ComponentTypeCodeList
        {
            get
            {
                string[] result;

                result = new string[] { "BGFF", "BOFF", "CALR", "CO2", "DAHS", "DL", "DP", "FLC", "FLOW", "GCH", "GFFM", "H2O", "HCL", "HF", "HG", "MS", "NOX", "O2", "OFFM", "OP", "PLC", "PM", "PRB", "PRES", "SO2", "STRAIN", "TANK", "TEMP" };

                return result;
            }
        }

        /// <summary>
        /// List of equation codes for testing.
        /// </summary>
        public static string[] EquationCodeList
        {
            get
            {
                string[] result;

                result = new string[] { "19-1", "19-14", "19-2", "19-3", "19-3D", "19-4", "19-5", "19-5D", "19-6", "19-7", "19-8", "19-9", "A-2", "A-3", "A-4", "D-12", "D-15", "D-15A", "D-1H",
                                        "D-2", "D-3", "D-4", "D-5", "D-6", "D-8", "E-2", "F-1", "F-11", "F-14A", "F-14B", "F-15", "F-16", "F-17", "F-18", "F-19", "F-19V", "F-2", "F-20", "F-21A",
                                        "F-21B", "F-21C", "F-21D", "F-23", "F-24A", "F-25", "F-26A", "F-26B", "F-28", "F-29", "F-31", "F-5", "F-6", "F-7A", "F-7B", "F-8", "G-1", "G-2", "G-3", "G-4",
                                        "G-4A", "G-5", "G-6", "G-8", "HC-2", "HC-3", "HC-4", "HF-2", "HF-3", "HF-4", "HG-1", "K-5", "M-1K", "MS-1", "MS-2", "N-GAS", "N-OIL", "NS-1", "NS-2", "S-2",
                                        "S-3", "S-4", "SS-1A", "SS-1B", "SS-2A", "SS-2B", "SS-2C", "SS-3A", "SS-3B", "T-FL", "X-FL" };

                return result;
            }
        }

        /// <summary>
        /// List of possible indicator types (0, 1, null).
        /// </summary>
        /// <returns></returns>
        public static int?[] IndicatorList
        {
            get
            {
                int?[] result;

                result = new int?[] { null, 0, 1 };

                return result;
            }
        }

        /// <summary>
        /// List of location codes for testing.
        /// </summary>
        public static string[] LocationCodeList
        {
            get
            {
                string[] result;

                result = new string[] { "UN", "CS", "MS", "CP", "MP" };

                return result;
            }
        }

        /// <summary>
        /// List of method parameter codes for testing.
        /// </summary>
        public static string[] MethodParameterCodeList
        {
            get
            {
                string[] result;

                result = new string[] { "CO2", "CO2M", "H2O", "HCLRE", "HCLRH", "HFRE", "HFRH", "HGRE", "HGRH", "HI", "HIT", "NOX", "NOXM", "NOXR", "OP", "SO2", "SO2M", "SO2RE", "SO2RH" };

                return result;
            }
        }

        /// <summary>
        /// List of MODC codes for testing.
        /// </summary>
        public static string[] ModcCodeList
        {
            get
            {
                string[] result;

                result = new string[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10",
                                        "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
                                        "21", "22", "23", "24", "25", "26",
                                        "32", "33", "34", "35", "36", "37", "38", "39",
                                        "40", "41", "42", "45", "46", "47", "48", "53", "54", "55" };

                return result;
            }
        }

        /// <summary>
        /// List of moisture basis codes for testing.
        /// </summary>
        public static string[] MoistureBasisCodeList
        {
            get
            {
                string[] result;

                result = new string[] { null, "D", "W" };

                return result;
            }
        }


        /// <summary>
        /// List of the Op Supp Data Type Codes used for System and Component Op Supp Data.
        /// </summary>
        public static string[] OpSuppDataTypeCodeList
        {
            get
            {
                string[] result;

                result = new string[] { "MA", "MAMJ", "OP", "OPMJ", "QA", "QAMJ" };

                return result;
            }
        }


        /// <summary>
        /// List of the Op Type Codes used for Op Supp Data.
        /// </summary>
        public static string[] OpTypeCodeList
        {
            get
            {
                string[] result;

                result = new string[] { "CO2M", "HIT", "HITOS", "NOXM", "NOXMOS", "NOXR", "NOXRHRS", "NOXRSUM", "NOXRYTD", "OPDAYS", "OPHOURS", "OPTIME", "OSHOURS", "OSTIME", "SO2M" };

                return result;
            }
        }


        public static string[] OperatingConditionCodeList
        {
            get
            {
                string[] result;

                result = new string[] { "A", "B", "C", "E", "M", "N", "P", "U", "W", "X", "Y", "Z" };

                return result;
            }
        }

        /// <summary>
        /// List of program codes for testing.
        /// </summary>
        public static string[] ProgramCodeList
        {
            get
            {
                string[] result;

                result = new string[] { "ARP", "MATS", "NBP", "NHNOX", "NSPS4T", "OTC", "RGGI", "SIPNOX",
                                        "CAIRNOX", "CAIROS", "CAIRSO2",
                                        "CSNOX", "CSNOXOS", "CSOSG1", "CSOSG2", "CSOSG3", "CSSO2G1", "CSSO2G2",
                                        "TRNOX", "TRNOXOS", "TRSO2G1", "TRSO2G2", "TXSO2" };

                return result;
            }
        }

        /// <summary>
        /// List of program parameter codes for testing.
        /// </summary>
        public static ProgramParameter[] ProgramParameterRequiredList
        {
            get
            {
                ProgramParameter[] result;

                result = new ProgramParameter[] 
                {
                    new ProgramParameter ("ARP", "CO2"),
                    new ProgramParameter ("ARP", "HI"),
                    new ProgramParameter ("ARP", "NOXR"),
                    new ProgramParameter ("ARP", "OP"),
                    new ProgramParameter ("ARP", "SO2"),
                    new ProgramParameter ("CAIRNOX", "HI"),
                    new ProgramParameter ("CAIRNOX", "NOX"),
                    new ProgramParameter ("CAIROS", "HI"),
                    new ProgramParameter ("CAIROS", "NOX"),
                    new ProgramParameter ("CAIRSO2", "HI"),
                    new ProgramParameter ("CAIRSO2", "SO2"),
                    new ProgramParameter ("CSNOX", "HI"),
                    new ProgramParameter ("CSNOX", "NOX"),
                    new ProgramParameter ("CSNOXOS", "HI"),
                    new ProgramParameter ("CSNOXOS", "NOX"),
                    new ProgramParameter ("CSOSG1", "HI"),
                    new ProgramParameter ("CSOSG1", "NOX"),
                    new ProgramParameter ("CSOSG2", "HI"),
                    new ProgramParameter ("CSOSG2", "NOX"),
                    new ProgramParameter ("CSOSG3", "HI"),
                    new ProgramParameter ("CSOSG3", "NOX"),
                    new ProgramParameter ("CSSO2G1", "HI"),
                    new ProgramParameter ("CSSO2G1", "SO2"),
                    new ProgramParameter ("CSSO2G2", "HI"),
                    new ProgramParameter ("CSSO2G2", "SO2"),
                    new ProgramParameter ("MATS", "HCL"),
                    new ProgramParameter ("MATS", "HG"),
                    new ProgramParameter ("NBP", "HI"),
                    new ProgramParameter ("NBP", "NOX"),
                    new ProgramParameter ("NHNOX", "HI"),
                    new ProgramParameter ("NHNOX", "NOX"),
                    new ProgramParameter ("NSPS4T", "CO2"),
                    new ProgramParameter ("OTC", "HI"),
                    new ProgramParameter ("OTC", "NOX"),
                    new ProgramParameter ("RGGI", "CO2"),
                    new ProgramParameter ("RGGI", "HI"),
                    new ProgramParameter ("SIPNOX", "HI"),
                    new ProgramParameter ("SIPNOX", "NOX"),
                    new ProgramParameter ("TXSO2", "HI"),
                    new ProgramParameter ("TXSO2", "SO2")

                };

                return result;
            }
        }

        /// <summary>
        /// List of severity codes for testing.
        /// </summary>
        public static string[] SeverityCodeList
        {
            get
            {
                string[] result;

                result = new string[] { "ADMNOVR", "CRIT1", "CRIT2", "CRIT3", "FATAL", "FORGIVE", "INFORM", "NONCRIT", "NONE" };

                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static string[] SpanScaleCodeList
        {
            get
            {
                string[] result;

                result = new string[] { "H", "L", null };

                return result;
            }
        }

        /// <summary>
        /// Large list of actual system type codes for testing.
        /// </summary>
        /// <returns></returns>
        public static string[] SystemTypeCodeList
        {
            get
            {
                string[] result;

                result = new string[] { "CO2", "FLOW", "GAS", "H2O", "H2OM", "H2OT", "HCL", "HF", "HG", "LTGS", "LTOL", "NOX", "NOXC", "NOXE", "NOXP", "O2", "OILM", "OILV", "OP", "PM", "SO2", "ST" };

                return result;
            }
        }

        /// <summary>
        /// List of possible valid types (false, true, null).
        /// </summary>
        /// <returns></returns>
        public static bool?[] ValidList
        {
            get
            {
                bool?[] result;

                result = new bool?[] { null, false, true };

                return result;
            }
        }

    }

}
