using System;

using ECMPS.Definitions.Extensions;


namespace ECMPS.Checks.CheckEngine.SpecialParameterClasses
{

    /// <summary>
    /// Keeps track of the missing data hour count and last PMA.
    /// </summary>
    public class MissingDataPmaTrackingInfo
    {

        /// <summary>
        /// Initializes a MissingDataPmaTrackingInfo class.
        /// </summary>
        public MissingDataPmaTrackingInfo(MissingDataPmaTracking.eHourlyParameter hourlyParameter)
        {
            HourlyParameter = hourlyParameter;

            LastPercentAvailable = null;
            LastPercentAvailableOpHour = null;
            MissingDataHourCount = 0;
            MissingDataHourCountLastOpHour = null;
        }


        #region Static Properties and Methods

        /// <summary>
        /// The list of MODC for quality assured hours.
        /// </summary>
        public static string MissingDataModcList { get { return "06,08,09,10,11,12,13,15,18,23,24,25,26,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,48,55"; } }

        #endregion


        #region Public Properties and Supporting Fields

        /// <summary>
        /// The hourly parameter type.
        /// </summary>
        public MissingDataPmaTracking.eHourlyParameter HourlyParameter { get; private set; }

        /// <summary>
        /// The last PMA for an operating hour.
        /// </summary>
        public decimal? LastPercentAvailable { get; private set; }

        /// <summary>
        /// The operating hour of the Last Pervent Available.  Prevents double counting.
        /// </summary>
        public DateTime? LastPercentAvailableOpHour { get; private set; }

        /// <summary>
        /// The current number of missing data hours in the quarter.
        /// </summary>
        public int MissingDataHourCount { get; private set; }

        /// <summary>
        /// The last operating hour counted for missing data.  Prevents double counting.
        /// </summary>
        public DateTime? MissingDataHourCountLastOpHour { get; private set; }

        #endregion


        #region Public Methods

        /// <summary>
        /// Updates the missing data count and last percent available information.
        /// </summary>
        /// <param name="opHour">The current operating hour including date.</param>
        /// <param name="opTime">The current operating time.</param>
        /// <param name="modcCd">The MODC for the current MHV or DHV record.</param>
        /// <param name="percentAvailable">The Percent Available for the current MHV or DHV record.</param>
        public void Update(DateTime opHour, decimal opTime, string modcCd, decimal? percentAvailable)
        {
            if (!MissingDataHourCountLastOpHour.HasValue || (opHour > MissingDataHourCountLastOpHour))
            {
                if ((opTime > 0.00m) && modcCd.InList(MissingDataModcList))
                {
                    MissingDataHourCount += 1;
                    MissingDataHourCountLastOpHour = opHour;
                }
            }

            if (!LastPercentAvailableOpHour.HasValue || (opHour > LastPercentAvailableOpHour))
            {
                if ((opTime > 0.00m) && percentAvailable.HasValue)
                {
                    LastPercentAvailable = percentAvailable;
                    LastPercentAvailableOpHour = opHour;
                }
            }
        }


        /// <summary>
        /// Initializes the object for test purposes.
        /// </summary>
        /// <param name="missingDataHourCount">The initial test MissingDataHourCount value.</param>
        /// <param name="missingDataHourCountLastOpHour">The initial test MissingDataHourCountLastOpHour value.</param>
        /// <param name="lastPercentAvailable">The initial test LastPercentAvailable value.</param>
        /// <param name="lastPercentAvailableOpHour">The initial test LastPercentAvailableOpHour value.</param>
        public void TestInit(int missingDataHourCount, DateTime? missingDataHourCountLastOpHour, decimal? lastPercentAvailable, DateTime? lastPercentAvailableOpHour)
        {
            MissingDataHourCount = missingDataHourCount;
            MissingDataHourCountLastOpHour = missingDataHourCountLastOpHour;
            LastPercentAvailable = lastPercentAvailable;
            LastPercentAvailableOpHour = lastPercentAvailableOpHour;
        }

        #endregion

    }

}
