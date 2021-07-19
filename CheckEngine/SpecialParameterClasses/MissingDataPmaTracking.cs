using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECMPS.Checks.CheckEngine.SpecialParameterClasses
{

    /// <summary>
    /// Tracks the last PMA and number of missing data hours for each MHV and DHV parameter.
    /// </summary>
    public class MissingDataPmaTracking
    {

        /// <summary>
        /// Initializes the Missing Data PMA Tracking Info object.
        /// </summary>
        /// <param name="currentLocationCount"></param>
        public MissingDataPmaTracking(int currentLocationCount)
        {
            HourlyParameterInfoArray = new MissingDataPmaTrackingInfo[currentLocationCount, Enum.GetValues(typeof(eHourlyParameter)).Length];

            for (int locationDex = 0; locationDex < currentLocationCount; locationDex++)
            {
                foreach (MissingDataPmaTracking.eHourlyParameter hourlyParameter in Enum.GetValues(typeof(MissingDataPmaTracking.eHourlyParameter)))
                {
                    HourlyParameterInfoArray[locationDex, (int)hourlyParameter] = new MissingDataPmaTrackingInfo(hourlyParameter);
                }
            }
        }


        #region Public Enumerations and Helper Static Methods

        /// <summary>
        /// MHV and DHV Parameters with PMA.
        /// Used to produce array index, so do not create enum index gaps.
        /// </summary>
        public enum eHourlyParameter
        {
            /// <summary>
            /// DHV CO2C
            /// </summary>
            DerivedCo2c,

            /// <summary>
            /// DHV H2O
            /// </summary>
            DerivedH2o,

            /// <summary>
            /// DHV NOXR
            /// </summary>
            DerivedNoxr,

            /// <summary>
            /// MHV CO2C
            /// </summary>
            MonitorCo2c,

            /// <summary>
            /// MHV FLOW
            /// </summary>
            MonitorFlow,

            /// <summary>
            /// MHV H2O
            /// </summary>
            MonitorH2o,

            /// <summary>
            /// MHV NOXC
            /// </summary>
            MonitorNoxc,

            /// <summary>
            /// MHV O2D
            /// </summary>
            MonitorO2d,

            /// <summary>
            /// MHV O2W
            /// </summary>
            MonitorO2w,

            /// <summary>
            /// MHV SO2C
            /// </summary>
            MonitorSo2c

        }


        /// <summary>
        /// Return the parameter code for the eHourlyParameter enumeration.
        /// </summary>
        /// <param name="hourlyParameter">The hourly parameter enum.</param>
        /// <returns>The parameter code for the hourly parameter enum.</returns>
        public static string GetHourlyParameterCd(eHourlyParameter hourlyParameter)
        {
            string result;

            switch (hourlyParameter)
            {
                case eHourlyParameter.DerivedCo2c: { result = "CO2C"; } break;
                case eHourlyParameter.DerivedH2o: { result = "H2O"; } break;
                case eHourlyParameter.DerivedNoxr: { result = "NOXR"; } break;
                case eHourlyParameter.MonitorCo2c: { result = "CO2C"; } break;
                case eHourlyParameter.MonitorFlow: { result = "FLOW"; } break;
                case eHourlyParameter.MonitorH2o: { result = "H2O"; } break;
                case eHourlyParameter.MonitorNoxc: { result = "NOXC"; } break;
                case eHourlyParameter.MonitorO2d: { result = "O2D"; } break;
                case eHourlyParameter.MonitorO2w: { result = "O2W"; } break;
                case eHourlyParameter.MonitorSo2c: { result = "SO2C"; } break;
                default: { result = null; } break;
            }

            return result;
        }

        /// <summary>
        /// Return the hourly type for the eHourlyParameter enumeration.
        /// </summary>
        /// <param name="hourlyParameter">The hourly parameter enum.</param>
        /// <returns>The parameter code for the hourly parameter enum.</returns>
        public static string GetHourlyType(eHourlyParameter hourlyParameter)
        {
            string result;

            switch (hourlyParameter)
            {
                case eHourlyParameter.DerivedCo2c: { result = "Derived"; } break;
                case eHourlyParameter.DerivedH2o: { result = "Derived"; } break;
                case eHourlyParameter.DerivedNoxr: { result = "Derived"; } break;
                case eHourlyParameter.MonitorCo2c: { result = "Monitor"; } break;
                case eHourlyParameter.MonitorFlow: { result = "Monitor"; } break;
                case eHourlyParameter.MonitorH2o: { result = "Monitor"; } break;
                case eHourlyParameter.MonitorNoxc: { result = "Monitor"; } break;
                case eHourlyParameter.MonitorO2d: { result = "Monitor"; } break;
                case eHourlyParameter.MonitorO2w: { result = "Monitor"; } break;
                case eHourlyParameter.MonitorSo2c: { result = "Monitor"; } break;
                default: { result = null; } break;
            }

            return result;
        }

        /// <summary>
        /// Return whether the eHourlyParameter enumeration is for derived data.
        /// </summary>
        /// <param name="hourlyParameter">The hourly parameter enum.</param>
        /// <returns>The parameter code for the hourly parameter enum.</returns>
        public static bool IsDerived(eHourlyParameter hourlyParameter)
        {
            bool result;

            switch (hourlyParameter)
            {
                case eHourlyParameter.DerivedCo2c: { result = true; } break;
                case eHourlyParameter.DerivedH2o: { result = true; } break;
                case eHourlyParameter.DerivedNoxr: { result = true; } break;
                case eHourlyParameter.MonitorCo2c: { result = false; } break;
                case eHourlyParameter.MonitorFlow: { result = false; } break;
                case eHourlyParameter.MonitorH2o: { result = false; } break;
                case eHourlyParameter.MonitorNoxc: { result = false; } break;
                case eHourlyParameter.MonitorO2d: { result = false; } break;
                case eHourlyParameter.MonitorO2w: { result = false; } break;
                case eHourlyParameter.MonitorSo2c: { result = false; } break;
                default: { result = false; } break;
            }

            return result;
        }

        /// <summary>
        /// Return whether the eHourlyParameter enumeration is for monitored data.
        /// </summary>
        /// <param name="hourlyParameter">The hourly parameter enum.</param>
        /// <returns>The parameter code for the hourly parameter enum.</returns>
        public static bool IsMonitored(eHourlyParameter hourlyParameter)
        {
            bool result;

            switch (hourlyParameter)
            {
                case eHourlyParameter.DerivedCo2c: { result = false; } break;
                case eHourlyParameter.DerivedH2o: { result = false; } break;
                case eHourlyParameter.DerivedNoxr: { result = false; } break;
                case eHourlyParameter.MonitorCo2c: { result = true; } break;
                case eHourlyParameter.MonitorFlow: { result = true; } break;
                case eHourlyParameter.MonitorH2o: { result = true; } break;
                case eHourlyParameter.MonitorNoxc: { result = true; } break;
                case eHourlyParameter.MonitorO2d: { result = true; } break;
                case eHourlyParameter.MonitorO2w: { result = true; } break;
                case eHourlyParameter.MonitorSo2c: { result = true; } break;
                default: { result = false; } break;
            }

            return result;
        }

        #endregion


        #region Private Properties and Supporting Fields

        /// <summary>
        /// Array of tracking information for each hourly parameter.
        /// </summary>
        private MissingDataPmaTrackingInfo[,] HourlyParameterInfoArray {get; set;}

        #endregion


        #region Public Methods

        /// <summary>
        /// Returns the missing data hour count and last percent available for the given location postion and hourly parameter.
        /// </summary>
        /// <param name="locationPosition">The position for the current location.</param> 
        /// <param name="hourlyParameter">The MHV or DHV parameter for which to return information.</param>
        /// <param name="missingDataHourCount">The missing data hour count return value.</param>
        /// <param name="lastPercentAvailable">The last percent available return value.</param>
        public void GetHourlyParamaterInfo(int locationPosition, eHourlyParameter hourlyParameter, out int missingDataHourCount, out decimal? lastPercentAvailable)
        {
            missingDataHourCount = HourlyParameterInfoArray[locationPosition, (int)hourlyParameter].MissingDataHourCount;
            lastPercentAvailable = HourlyParameterInfoArray[locationPosition, (int)hourlyParameter].LastPercentAvailable;
        }


        /// <summary>
        /// Returns the missing data hour count for the given location postion and hourly parameter.
        /// </summary>
        /// <param name="locationPosition">The position for the current location.</param> 
        /// <param name="hourlyParameter">The MHV or DHV parameter for which to return information.</param>
        /// <returns>The missing data hour count.</returns>
        public int GetMissingDataHourCount(int locationPosition, eHourlyParameter hourlyParameter)
        {
            return HourlyParameterInfoArray[locationPosition, (int)hourlyParameter].MissingDataHourCount;
        }


        /// <summary>
        /// Returns the last percent available for the given location postion and hourly parameter.
        /// </summary>
        /// <param name="locationPosition">The position for the current location.</param> 
        /// <param name="hourlyParameter">The MHV or DHV parameter for which to return information.</param>
        /// <returns>The last percent available.</returns>
        public decimal? GetLastPercentAvailable(int locationPosition, eHourlyParameter hourlyParameter)
        {
            return HourlyParameterInfoArray[locationPosition, (int)hourlyParameter].LastPercentAvailable;
        }


        /// <summary>
        /// Updates the missing data count and last percent available information for the given hourly parameter.
        /// </summary>
        /// <param name="locationPosition">The position for the current location.</param> 
        /// <param name="opHour">The current operating hour including date.</param>
        /// <param name="opTime">The current operating time.</param>
        /// <param name="hourlyParameter">The MHV or DHV parameter for which the update should occur.</param>
        /// <param name="modcCd">The MODC for the current MHV or DHV record.</param>
        /// <param name="percentAvailable">The Percent Available for the current MHV or DHV record.</param>
        public void Update(int locationPosition, DateTime opHour, decimal opTime, eHourlyParameter hourlyParameter, string modcCd, decimal? percentAvailable)
        {
            HourlyParameterInfoArray[locationPosition, (int)hourlyParameter].Update(opHour, opTime, modcCd, percentAvailable);
        }


        /// <summary>
        /// Initializes the hourly parameter of the specified location for test purposes.
        /// </summary>
        /// <param name="locationPosition">The position of the location to update.</param>
        /// <param name="hourlyParameter">The hourly parameter to update.</param>
        /// <param name="missingDataHourCount">The initial test MissingDataHourCount value.</param>
        /// <param name="missingDataHourCountLastOpHour">The initial test MissingDataHourCountLastOpHour value.</param>
        /// <param name="lastPercentAvailable">The initial test LastPercentAvailable value.</param>
        /// <param name="lastPercentAvailableOpHour">The initial test LastPercentAvailableOpHour value.</param>
        public void TestInit(int locationPosition, eHourlyParameter hourlyParameter, int missingDataHourCount, DateTime? missingDataHourCountLastOpHour, 
                                                                                     decimal? lastPercentAvailable, DateTime? lastPercentAvailableOpHour)
        {
            HourlyParameterInfoArray[locationPosition, (int)hourlyParameter].TestInit(missingDataHourCount, missingDataHourCountLastOpHour, lastPercentAvailable, lastPercentAvailableOpHour);
        }

        #endregion

    }

}
