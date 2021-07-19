using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using ECMPS.Checks.TypeUtilities;
using ECMPS.Definitions.Extensions;


namespace ECMPS.Checks.CheckEngine
{
    /// <summary>
    /// MODC hour counts class
    /// </summary>
    public class cModcHourCounts
    {

        #region Constructors

        /// <summary>
        /// Creates QA MODC object that counts QA MODC hours, as well as MODC 17 (Like-Kind) hours and captures the first MODC 17 hour.
        /// </summary>
        /// <param name="hourlyTable">The primary table of hourly data for the category.</param>
        /// <param name="qaHourModcList">The list of QA MODC to use.</param>
        /// <param name="locationView">View containing the locations for the emission report's monitoring plan.</param>
        /// <param name="locationMonSysIdList">Array of MON_SYS_ID lists.  Null if systems are not being tracked, otheriwse contains an array element for each location in the same position as in locationView.  Each element contains a List containing MON_SYS_ID to track.</param>
        public cModcHourCounts(DataTable hourlyTable, int[] qaHourModcList, DataView locationView, List<string>[] locationMonSysIdList = null)
        {
            HourlyTable = hourlyTable;

            // Populate QaHourModcList
            {
                QaHourModcList = new bool[56];

                for (int Dex = 0; Dex <= 55; Dex++) { QaHourModcList[Dex] = false; }
                foreach (int ModcCd in qaHourModcList) { QaHourModcList[ModcCd] = true; }
            }

            // Initialize Tracking Arrays
            {
                if ((locationView != null) && (locationView.Count > 0))
                {
                    LocationCount = locationView.Count;
                    MonLocIds = new string[locationView.Count];
                    FirstLikeKindDateHours = new DateTime?[locationView.Count];
                    LikeKindHourCounts = new int[locationView.Count];
                    QaHourCounts = new int[locationView.Count];

                    QaHourSystemCounts = ((locationMonSysIdList != null) && (locationMonSysIdList.Length == locationView.Count))
                                       ? new Dictionary<string, int>[locationView.Count]
                                       : null;

                    for (int locationPos = 0; locationPos < locationView.Count; locationPos++)
                    {
                        MonLocIds[locationPos] = locationView[locationPos]["Mon_Loc_Id"].AsString();
                        FirstLikeKindDateHours[locationPos] = null;
                        LikeKindHourCounts[locationPos] = 0;
                        QaHourCounts[locationPos] = 0;

                        // Initialize system QA hour counts if object exists.
                        if (QaHourSystemCounts != null)
                        {
                            QaHourSystemCounts[locationPos] = new Dictionary<string, int>();

                            foreach (string monSysId in locationMonSysIdList[locationPos])
                                QaHourSystemCounts[locationPos].Add(monSysId, 0);
                        }
                    }
                }
                else
                {
                    LocationCount = 0;
                    MonLocIds = null;
                    LikeKindHourCounts = null;
                    QaHourCounts = null;
                }
            }
        }

        #endregion


        #region Public Properties

        /// <summary>
        /// The QA Hour MODC list
        /// </summary>
        public bool[] QaHourModcList { get; private set; }

        #endregion


        #region Public Methods: Add

        /// <summary>
        /// Handle a new hour
        /// </summary>
        /// <param name="currentLocationPos"></param>
        /// <param name="currentOpDate"></param>
        /// <param name="currentOpHour"></param>
        /// <param name="hourlyViewPosition"></param>
        public void HandleNewHour(int currentLocationPos, DateTime currentOpDate, int currentOpHour, int hourlyViewPosition)
        {
            DataView hourlyView = HourlyTable.DefaultView;

            if ((0 <= currentLocationPos) && (currentLocationPos < LocationCount) && (hourlyViewPosition < hourlyView.Count))
            {
                string currentMonLocId = MonLocIds[currentLocationPos];
                DateTime currentDateHour = currentOpDate.Date.AddHours(currentOpHour);

                DataRowView hourlyRow = hourlyView[hourlyViewPosition];
                string monLocId; DateTime opDateHour; int? modcCd;
                {
                    monLocId = hourlyRow["Mon_Loc_Id"].AsString();
                    opDateHour = hourlyRow["Begin_Date"].AsDateTime(DateTime.MinValue).Date.AddHours(hourlyRow["Begin_Hour"].AsInteger(0));
                    modcCd = hourlyRow["Modc_Cd"].AsInteger();
                }

                if ((monLocId == currentMonLocId) && (opDateHour <= currentDateHour) && modcCd.HasValue)
                {
                    // Handle QA Hour
                    if (QaHourModcList[modcCd.Value])
                    {
                        QaHourCounts[currentLocationPos] += 1;

                        // Handle system specific QA hour counts
                        if (QaHourSystemCounts != null)
                        {
                            string monSysId = hourlyRow["Mon_Sys_Id"].AsString();

                            if (QaHourSystemCounts[currentLocationPos].ContainsKey(monSysId))
                            {
                                QaHourSystemCounts[currentLocationPos][monSysId] += 1;
                            }
                        }
                    }

                    // Handle Like Kind Analyzer Hour
                    if (modcCd.Value == 17)
                    {
                        if (LikeKindHourCounts[currentLocationPos] == 0)
                            FirstLikeKindDateHours[currentLocationPos] = opDateHour;

                        LikeKindHourCounts[currentLocationPos] += 1;
                    }
                }
            }
        }

        #endregion


        #region Public Methods: Get

        /// <summary>
        /// Gets current First Like-Kind Analyzer row for a particular location.
        /// </summary>
        /// <param name="locationPos">The position of the location to get.</param>
        /// <returns>Returns the count for the location if it exists or null.</returns>
        public DateTime? FirstLikeKindDateHour(int locationPos)
        {
            DateTime? result;

            if (locationPos < LocationCount)
                result = FirstLikeKindDateHours[locationPos];
            else
                result = null;

            return result;
        }

        /// <summary>
        /// Gets current Like-Kind Analyzer Hour Count for a particular location.
        /// </summary>
        /// <param name="locationPos">The position of the location to get.</param>
        /// <returns>Returns the count for the location if it exists or MaxInt.</returns>
        public int LikeKindHourCount(int locationPos)
        {
            int result;

            if (locationPos < LocationCount)
                result = LikeKindHourCounts[locationPos];
            else
                result = int.MaxValue;

            return result;
        }


        /// <summary>
        /// Gets current QA Hour Count for a particular location.
        /// </summary>
        /// <param name="locationPos">The position of the location to get.</param>
        /// <param name="monSysId">The MON_SYS_ID to use when a system specific count is expected.</param>
        /// <returns>Returns the count for the location if it exists or MaxInt.</returns>
        public int QaHourCount(int locationPos, string monSysId = null)
        {
            int result;

            if (locationPos < LocationCount)
            {
                if (monSysId == null)
                {
                    result = QaHourCounts[locationPos];
                }
                else if ((QaHourSystemCounts != null) && QaHourSystemCounts[locationPos].ContainsKey(monSysId))
                {
                    result = QaHourSystemCounts[locationPos][monSysId];
                }
                else
                {
                    result = int.MaxValue;
                }
            }
            else
            {
                result = int.MaxValue;
            }

            return result;
        }

        #endregion


        #region Private Properties

        /// <summary>
        /// The first Like-Kind Analyzer row in the quarter.
        /// </summary>
        private DateTime?[] FirstLikeKindDateHours { get; set; }

        /// <summary>
        /// A count of the Like-Kind Analyzer Hours in the quarter up to the Last Date and Hour
        /// </summary>
        private int[] LikeKindHourCounts { get; set; }

        /// <summary>
        /// The count of locations in the current report.
        /// </summary>
        private int LocationCount { get; set; }

        /// <summary>
        /// The hourly table against which the counts occur.
        /// </summary>
        private DataTable HourlyTable { get; set; }

        /// <summary>
        /// The list of MON_LOC_ID for the current emission report.
        /// </summary>
        private string[] MonLocIds { get; set; }

        /// <summary>
        /// A count of the QA Hours in the quarter up to the Last Date and Hour
        /// </summary>
        private int[] QaHourCounts { get; set; }

        /// <summary>
        /// Contains the system counts for for each location.
        /// </summary>
        private Dictionary<string, int>[] QaHourSystemCounts;

        #endregion


        #region Programmer Testing Constructor and Public Methods

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="locationCount"></param>
        public cModcHourCounts(int locationCount)
        {
            LocationCount = locationCount;
            MonLocIds = new string[locationCount];
            FirstLikeKindDateHours = new DateTime?[locationCount];
            LikeKindHourCounts = new int[locationCount];
            QaHourCounts = new int[locationCount];

            for (int locationPos = 0; locationPos < locationCount; locationPos++)
            {
                MonLocIds[locationPos] = string.Format("MonLocId{0}", locationCount);
                FirstLikeKindDateHours[locationPos] = null;
                LikeKindHourCounts[locationPos] = 0;
                QaHourCounts[locationPos] = 0;
            }
        }

        /// <summary>
        /// Get test case results
        /// </summary>
        /// <param name="locationPos"></param>
        /// <param name="qaHourCount"></param>
        /// <returns></returns>
        public bool GetTestCase_QaHourCount(int locationPos, out int qaHourCount)
        {
            if ((QaHourCounts != null) && (QaHourCounts.Length > locationPos))
            {
                qaHourCount = QaHourCounts[locationPos];

                return true;
            }
            else
            {
                qaHourCount = int.MinValue;

                return false;
            }
        }

        #endregion
    }
}
