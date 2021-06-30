using System;
using System.Collections.Generic;
using System.Data;

using ECMPS.Checks.CheckEngine;
using ECMPS.Checks.CheckEngine.SpecialParameterClasses;
using ECMPS.Checks.Data.Ecmps.CheckEm.Function;
using ECMPS.Checks.Data.Ecmps.Dbo.View;
using ECMPS.Checks.Em.Parameters;
using ECMPS.Checks.TypeUtilities;
using ECMPS.Checks.EmissionsReport;

using ECMPS.Definitions.Extensions;


namespace ECMPS.Checks.EmissionsChecks
{

    public class cFlowToLoadStatusChecks : cEmissionsChecks
    {

        #region Constructors

        /// <summary>
        /// Creates and instance of the Flow-to-Load checks object and populates its CheckProcedures array.
        /// </summary>
        /// <param name="emissionReportProcess">The owning emission report process object.</param>
        public cFlowToLoadStatusChecks(cEmissionsReportProcess emissionReportProcess)
            : base(emissionReportProcess)
        {
            CheckProcedures = new dCheckProcedure[7];

            CheckProcedures[1] = new dCheckProcedure(F2LSTAT1);
            CheckProcedures[2] = new dCheckProcedure(F2LSTAT2);
            CheckProcedures[3] = new dCheckProcedure(F2LSTAT3);
            CheckProcedures[4] = new dCheckProcedure(F2LSTAT4);
            CheckProcedures[5] = new dCheckProcedure(F2LSTAT5);
            CheckProcedures[6] = new dCheckProcedure(F2LSTAT6);
        }

        #endregion


        #region Checks

        #region Checks (1 - 10)

        /// <summary>
        /// F2LSTAT1: Determine Most Recent Flow-to-Load QA Operating Quarter
        /// </summary>
        /// <param name="category">Category Object</param>
        /// <param name="log">Indicates whether to log results.</param>
        /// <returns>Returns error message if check fails to run correctly.</returns>
        public string F2LSTAT1(cCategory category, ref bool log)
        {
            string returnVal = "";

            try
            {
                F2lStatusPriorTestRequiredQuarter.SetValue(null, category);
                EmParameters.F2lStatusPriorTestRequiredQuarterSetFromSystem = null;
                F2lStatusPriorTestRequiredQuarterMissingOpData.SetValue(null, category);

                Dictionary<string, string> f2lStatusSystemResultDictionary = (Dictionary<string, string>)F2lStatusSystemResultDictionary.Value;
                string monSysId = CurrentMhvRecord.Value["MON_SYS_ID"].AsString();

                if (!f2lStatusSystemResultDictionary.ContainsKey(monSysId))
                {
                    cReportingPeriod latestReportingPeriod = new cReportingPeriod(CurrentReportingPeriod.Value.Default(0));

                    DataRowView rataTestRecord = cRowFilter.FindMostRecentRow(
                                                                               RataTestRecordsByLocationForQaStatus.Value,
                                                                               latestReportingPeriod.BeganDate.AddHours(-1).Date,
                                                                               latestReportingPeriod.BeganDate.AddHours(-1).Hour,
                                                                               new cFilterCondition[]
                                                                           {
                                                                             new cFilterCondition("MON_SYS_ID", CurrentMhvRecord.Value["MON_SYS_ID"].AsString()),
                                                                             new cFilterCondition("TEST_RESULT_CD", "INVALID", true)
                                                                           }
                                                                             );

                    if (rataTestRecord != null)
                    {
                        //Bug 11318
                        DateTime maxRptDt;
                        DateTime earliestLocationRptDt = EarliestLocationReportDate.Value.Default(DateTime.MinValue);
                        DateTime rataTstDt = rataTestRecord["END_DATE"].AsDateTime(DateTime.MinValue);

                        //Bug 11318
                        if (earliestLocationRptDt > rataTstDt)
                            maxRptDt = earliestLocationRptDt;
                        else
                            maxRptDt = rataTstDt;

                        cReportingPeriod earliestF2LReportingPeriod = new cReportingPeriod(maxRptDt);

                        for (cReportingPeriod reportingPeriod = latestReportingPeriod.AddQuarter(-1);
                             reportingPeriod.CompareTo(earliestF2LReportingPeriod) >= 0;
                             reportingPeriod = reportingPeriod.AddQuarter(-1))
                        {
                            if (AnnualReportingRequirement.Value.Default(false) || reportingPeriod.Quarter.AsInteger().AsString().InList("2,3"))
                            {
                                int? opHourCount;
                                bool? opHourCountSetFromSystem;

                                SystemOpSuppData systemOpSuppDataRecord = EmParameters.SystemOperatingSuppDataRecordsByLocation.FindRow
                                                                          (
                                                                              new cFilterCondition("MON_SYS_ID", EmParameters.QaStatusSystemId),
                                                                              new cFilterCondition("RPT_PERIOD_ID", reportingPeriod.RptPeriodId.AsString()),
                                                                              new cFilterCondition("OP_SUPP_DATA_TYPE_CD", "OP")
                                                                          );

                                if (systemOpSuppDataRecord != null)
                                {
                                    opHourCount = systemOpSuppDataRecord.Hours;
                                    opHourCountSetFromSystem = true;
                                }
                                else
                                {
                                    VwMpOpSuppDataRow locationOpSuppDataRecord = EmParameters.OperatingSuppDataRecordsByLocation.FindRow
                                                                                (
                                                                                    new cFilterCondition("RPT_PERIOD_ID", reportingPeriod.RptPeriodId.AsString()),
                                                                                    new cFilterCondition("OP_TYPE_CD", "OPHOURS"),
                                                                                    new cFilterCondition("FUEL_CD", DBNull.Value, eFilterDataType.String)
                                                                                );

                                    if (locationOpSuppDataRecord != null)
                                    {
                                        opHourCount = locationOpSuppDataRecord.OpValue.HasValue ? Decimal.ToInt32(locationOpSuppDataRecord.OpValue.Value) : (int?)null;
                                        opHourCountSetFromSystem = false;
                                    }
                                    else
                                    {
                                        opHourCount = null;
                                        opHourCountSetFromSystem = null;
                                    }
                                }


                                if (opHourCount != null)
                                {
                                    if (F2lStatusPriorTestRequiredQuarter.Value != -1)
                                    {
                                        if (opHourCount.Value >= 168)
                                        {
                                            DataRowView f2lCheckRecord = cRowFilter.FindRow(F2lCheckRecordsForQaStatus.Value,
                                                                                            new cFilterCondition[] 
                                                                { 
                                                                  new cFilterCondition("MON_SYS_ID", monSysId),
                                                                  new cFilterCondition("RPT_PERIOD_ID", reportingPeriod.RptPeriodId.AsString()),
                                                                  new cFilterCondition("TEST_RESULT_CD", "EXC168H,FEW168H", eFilterConditionStringCompare.InList)
                                                                });

                                            if (f2lCheckRecord == null)
                                            {
                                                F2lStatusPriorTestRequiredQuarter.SetValue((4 * reportingPeriod.Year) + (reportingPeriod.Quarter.AsInteger() - 1), category);
                                                EmParameters.F2lStatusPriorTestRequiredQuarterSetFromSystem = opHourCountSetFromSystem;
                                                return returnVal;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    F2lStatusPriorTestRequiredQuarter.SetValue(-1, category);
                                    F2lStatusPriorTestRequiredQuarterMissingOpData.AggregateValue(string.Format("{0}Q{1}", reportingPeriod.Year, reportingPeriod.Quarter.AsInteger()));
                                }
                            }
                        }
                    }
                    else
                    {
                        F2lStatusPriorTestRequiredQuarter.SetValue(-1, category);
                        F2lStatusPriorTestRequiredQuarterMissingOpData.AggregateValue("No Prior RATA");
                    }
                }
            }
            catch (Exception ex)
            {
                returnVal = category.CheckEngine.FormatError(ex);
            }

            return returnVal;
        }


        /// <summary>
        /// F2LSTAT2: Locate Most Recent Flow-to-Load Check Prior to the Current Hour 
        /// </summary>
        /// <param name="category">Category Object</param>
        /// <param name="log">Indicates whether to log results.</param>
        /// <returns>Returns error message if check fails to run correctly.</returns>
        public string F2LSTAT2(cCategory category, ref bool log)
        {
            string returnVal = "";

            try
            {
                Dictionary<string, string> f2lStatusSystemResultDictionary = (Dictionary<string, string>)F2lStatusSystemResultDictionary.Value;
                Dictionary<string, DataRowView> f2lStatusSystemCheckDictionary = (Dictionary<string, DataRowView>)F2lStatusSystemCheckDictionary.Value;
                Dictionary<string, string> f2lStatusSystemMissingOpDictionary = (Dictionary<string, string>)F2lStatusSystemMissingOpDictionary.Value;

                string monSysId = CurrentMhvRecord.Value["MON_SYS_ID"].AsString();

                if (f2lStatusSystemResultDictionary.ContainsKey(monSysId))
                {
                    F2lStatusResult.SetValue(f2lStatusSystemResultDictionary[monSysId], category);
                    CurrentFlowToLoadStatusCheck.SetValue(f2lStatusSystemCheckDictionary[monSysId], category);
                    F2lStatusMissingOpDataInfo.SetValue(f2lStatusSystemMissingOpDictionary[monSysId], category);
                }
                else
                {
                    F2lStatusResult.SetValue(null, category);
                    CurrentFlowToLoadStatusCheck.SetValue(null, category);
                    F2lStatusMissingOpDataInfo.SetValue(null, category);

                    int previousPeriodId = ((new cReportingPeriod(CurrentReportingPeriod.Value.Value)).AddQuarter(-1)).RptPeriodId;

                    DataRowView f2lCheckRecord = cRowFilter.FindMostRecentRow(
                                                                               F2lCheckRecordsForQaStatus.Value,
                                                                               CurrentReportingPeriodBeginHour.Value.Value.AddHours(-1).Date,
                                                                               CurrentReportingPeriodBeginHour.Value.Value.AddHours(-1).Hour,
                                                                               new cFilterCondition[]
                                                                     {
                                                                       new cFilterCondition("MON_SYS_ID", monSysId),
                                                                       new cFilterCondition("TEST_RESULT_CD", "PASSED,FAILED", eFilterConditionStringCompare.InList)
                                                                     }
                                                                              );

                    if (f2lCheckRecord == null)
                    {
                        DataRowView facilityLocationNonLoadBasedRecord = cRowFilter.FindRow(
                                                                                             MpLocationNonLoadBasedRecords.Value,
                                                                                             new cFilterCondition[]
                                                                                 {
                                                                                   new cFilterCondition("MON_LOC_ID", CurrentMhvRecord.Value["MON_LOC_ID"].AsString())
                                                                                 }
                                                                                           );

                        if ((facilityLocationNonLoadBasedRecord != null) && (facilityLocationNonLoadBasedRecord["NON_LOAD_BASED_IND"].AsInteger() == 1))
                        {
                            F2lStatusResult.SetValue("IC-Exempt", category);
                        }
                        else
                        {
                            DataRowView testExtensionExemptionRecord = cRowFilter.FindRow(
                                                                                           TestExtensionExemptionRecords.Value,
                                                                                           new cFilterCondition[]
                                                                             {
                                                                               new cFilterCondition("MON_SYS_ID", monSysId),
                                                                               new cFilterCondition("EXTENS_EXEMPT_CD", "F2LEXP"),
                                                                               new cFilterCondition("RPT_PERIOD_ID", previousPeriodId, eFilterDataType.Integer)
                                                                             }
                                                                                         );

                            if (testExtensionExemptionRecord != null)
                            {
                                F2lStatusResult.SetValue("IC-Exempt", category);
                            }
                            else
                            {
                                DataRowView rataTestRecord = cRowFilter.FindMostRecentRow(
                                                                                           RataTestRecordsByLocationForQaStatus.Value,
                                                                                           CurrentReportingPeriodBeginHour.Value.Value.AddHours(-1).Date,
                                                                                           CurrentReportingPeriodBeginHour.Value.Value.AddHours(-1).Hour,
                                                                                           new cFilterCondition[]
                                                                           {
                                                                             new cFilterCondition("MON_SYS_ID", monSysId),
                                                                             new cFilterCondition("TEST_RESULT_CD", "INVALID", true)
                                                                           }
                                                                                         );

                                if (rataTestRecord == null)
                                {
                                    F2lStatusResult.SetValue("IC-No Prior RATA", category);
                                }
                                else if (F2lStatusPriorTestRequiredQuarter.Value == -1)
                                {
                                    F2lStatusResult.SetValue("Missing Op Data", category);
                                    F2lStatusMissingOpDataInfo.SetValue(F2lStatusPriorTestRequiredQuarterMissingOpData.Value, category);
                                }
                                else if (!F2lStatusPriorTestRequiredQuarter.Value.HasValue ||
                                         (F2lStatusPriorTestRequiredQuarter.Value.IntToYear() < rataTestRecord["CALENDAR_YEAR"].AsInteger()) ||
                                         ((F2lStatusPriorTestRequiredQuarter.Value.IntToYear() == rataTestRecord["CALENDAR_YEAR"].AsInteger()) &&
                                          (F2lStatusPriorTestRequiredQuarter.Value.IntToQuarter() < rataTestRecord["QUARTER"].AsInteger())))
                                {
                                    F2lStatusResult.SetValue("IC", category);
                                }
                                else if ((new cReportingPeriod(rataTestRecord["CALENDAR_YEAR"].AsInteger(0), rataTestRecord["QUARTER"].AsInteger(0)))
                                             .Equals((new cReportingPeriod(CurrentReportingPeriod.Value.Default(0))).AddQuarter(-1)) &&
                                         rataTestRecord["TEST_REASON_CD"].AsString().InList("INITIAL,RECERT") && rataTestRecord["TEST_RESULT_CD"].AsString() == "PASSED")
                                {
                                    F2lStatusResult.SetValue("IC", category);
                                }
                                else if ((CurrentMhvRecord.Value["SYS_DESIGNATION_CD"].AsString() == "RB") && (EmParameters.F2lStatusPriorTestRequiredQuarterSetFromSystem != true))
                                {
                                    F2lStatusResult.SetValue("Undetermined-No Prior Check reported for Redundant Backup Monitor", category);
                                }
                                else
                                {
                                    F2lStatusResult.SetValue("OOC-Prior Check Missing", category);
                                }
                            }
                        }
                    }
                    else
                    {
                        CurrentFlowToLoadStatusCheck.SetValue(f2lCheckRecord, category);

                        if (CurrentFlowToLoadStatusCheck.Value["RPT_PERIOD_ID"].AsInteger() != previousPeriodId)
                        {
                            if (F2lStatusPriorTestRequiredQuarter.Value == -1)
                            {
                                F2lStatusResult.SetValue("Missing Op Data", category);
                                F2lStatusMissingOpDataInfo.SetValue(F2lStatusPriorTestRequiredQuarterMissingOpData.Value, category);
                            }
                            else if (!(!F2lStatusPriorTestRequiredQuarter.Value.HasValue ||
                                       (F2lStatusPriorTestRequiredQuarter.Value.IntToYear() < CurrentFlowToLoadStatusCheck.Value["CALENDAR_YEAR"].AsInteger()) ||
                                       ((F2lStatusPriorTestRequiredQuarter.Value.IntToYear() == CurrentFlowToLoadStatusCheck.Value["CALENDAR_YEAR"].AsInteger()) &&
                                        (F2lStatusPriorTestRequiredQuarter.Value.IntToQuarter() <= CurrentFlowToLoadStatusCheck.Value["QUARTER"].AsInteger()))))
                            {
                                if ((CurrentMhvRecord.Value["SYS_DESIGNATION_CD"].AsString() == "RB") && (EmParameters.F2lStatusPriorTestRequiredQuarterSetFromSystem != true))
                                {
                                    F2lStatusResult.SetValue("Undetermined-No Prior Check reported for Redundant Backup Monitor", category);
                                }
                                else
                                {
                                    F2lStatusResult.SetValue("OOC-Prior Check Missing", category);
                                }
                            }
                            else if (CurrentFlowToLoadStatusCheck.Value["TEST_RESULT_CD"].AsString() == "PASSED")
                            {
                                F2lStatusResult.SetValue("IC", category);
                            }
                        }
                        else
                        {
                            if (CurrentFlowToLoadStatusCheck.Value["TEST_RESULT_CD"].AsString() == "PASSED")
                            {
                                F2lStatusResult.SetValue("IC", category);
                            }
                        }
                    }

                    f2lStatusSystemResultDictionary[monSysId] = F2lStatusResult.Value;
                    f2lStatusSystemCheckDictionary[monSysId] = CurrentFlowToLoadStatusCheck.Value;
                    f2lStatusSystemMissingOpDictionary[monSysId] = F2lStatusMissingOpDataInfo.Value;
                }
            }
            catch (Exception ex)
            {
                returnVal = category.CheckEngine.FormatError(ex);
            }

            return returnVal;
        }


        /// <summary>
        /// F2LSTAT3: Locate Intervening RATA
        /// </summary>
        /// <param name="category">Category Object</param>
        /// <param name="log">Indicates whether to log results.</param>
        /// <returns>Returns error message if check fails to run correctly.</returns>
        public string F2LSTAT3(cCategory category, ref bool log)
        {
            string returnVal = "";

            try
            {
                F2lStatusInterveningRata.SetValue(null, category);

                if (F2lStatusResult.Value == null)
                {
                    DateTime currentHour = CurrentMhvRecord.Value["BEGIN_DATE"].AsDateTime(DateTime.MinValue).AddHours(CurrentMhvRecord.Value["BEGIN_HOUR"].AsInteger(0));

                    DataRowView rataTestRecord = cRowFilter.FindMostRecentRow(
                                                                               RataTestRecordsByLocationForQaStatus.Value,
                                                                               currentHour.AddHours(-1).Date,
                                                                               currentHour.AddHours(-1).Hour,
                                                                               new cFilterCondition[]
                                                                           {
                                                                             new cFilterCondition("MON_SYS_ID", CurrentMhvRecord.Value["MON_SYS_ID"].AsString()),
                                                                             new cFilterCondition("END_DATEHOUR", CurrentFlowToLoadStatusCheck.Value["END_DATEHOUR"].AsDateTime(), eFilterDataType.DateBegan, eFilterConditionRelativeCompare.GreaterThan),
                                                                             new cFilterCondition("TEST_RESULT_CD", "INVALID", true)
                                                                           }
                                                                             );

                    if (rataTestRecord != null)
                    {
                        F2lStatusResult.SetValue("IC-Subsequent RATA Performed", category);
                        F2lStatusInterveningRata.SetValue(rataTestRecord, category);
                    }
                }
            }
            catch (Exception ex)
            {
                returnVal = category.CheckEngine.FormatError(ex);
            }

            return returnVal;
        }


        /// <summary>
        /// F2LSTAT4: Locate Most Recent QA Cert Event
        /// </summary>
        /// <param name="category">Category Object</param>
        /// <param name="log">Indicates whether to log results.</param>
        /// <returns>Returns error message if check fails to run correctly.</returns>
        public string F2LSTAT4(cCategory category, ref bool log)
        {
            string returnVal = "";

            try
            {
                F2lStatusQaCertEvent.SetValue(null, category);
                F2lStatusEventRequiresRata.SetValue(false, category);
                F2lStatusEventRequiresAbbreviatedCheck.SetValue(false, category);

                if (F2lStatusResult.Value == null)
                {
                    DateTime currentHour = CurrentMhvRecord.Value["BEGIN_DATE"].AsDateTime(DateTime.MinValue).AddHours(CurrentMhvRecord.Value["BEGIN_HOUR"].AsInteger(0));
                    DateTime currentFlowToLoadStatusHour = CurrentFlowToLoadStatusCheck.Value["END_DATEHOUR"].AsDateTime(DateTime.MaxValue);

                    DataRowView qaCertificationEventRecord = cRowFilter.FindMostRecentRow(
                                                                                           EmParameters.F2lQaCertificationEventRecords.SourceView,
                                                                                           currentHour.Date,
                                                                                           currentHour.Hour,
                                                                                           "QA_CERT_EVENT_DATE", "QA_CERT_EVENT_HOUR",
                                                                                           new cFilterCondition[]
                                                                                       {
                                                                                         new cFilterCondition("MON_SYS_ID", CurrentMhvRecord.Value["MON_SYS_ID"].AsString()),
                                                                                         new cFilterCondition("QA_CERT_EVENT_CD", "312"),
                                                                                         new cFilterCondition("QA_CERT_EVENT_DATEHOUR", currentFlowToLoadStatusHour, eFilterDataType.DateBegan, eFilterConditionRelativeCompare.GreaterThanOrEqual)
                                                                                       }
                                                                                         );

                    if (qaCertificationEventRecord != null)
                    {
                        F2lStatusQaCertEvent.SetValue(qaCertificationEventRecord, category);

                        DataRowView rataToRequiredRow = cRowFilter.FindRow(
                                                                            CrossCheckTestTypeToRequiredTestCode.Value,
                                                                            new cFilterCondition[]
                                                                    {
                                                                      new cFilterCondition("TestTypeCode", "RATA", eFilterConditionStringCompare.BeginsWith),
                                                                      new cFilterCondition("RequiredTestCode", qaCertificationEventRecord["REQUIRED_TEST_CD"].AsString())
                                                                    }
                                                                          );

                        if (rataToRequiredRow != null)
                            F2lStatusEventRequiresRata.SetValue(true, category);

                        DataRowView abbreviatedF2lToRequiredRow = cRowFilter.FindRow(
                                                                                      CrossCheckTestTypeToRequiredTestCode.Value,
                                                                                      new cFilterCondition[]
                                                                              {
                                                                                new cFilterCondition("TestTypeCode", "AF2LCHK"),
                                                                                new cFilterCondition("RequiredTestCode", qaCertificationEventRecord["REQUIRED_TEST_CD"].AsString())
                                                                              }
                                                                                    );

                        if (abbreviatedF2lToRequiredRow != null)
                            F2lStatusEventRequiresAbbreviatedCheck.SetValue(true, category);

                        if (F2lStatusQaCertEvent.Value["LAST_TEST_COMPLETED_DATEHOUR"].AsDateTime(DateTime.MaxValue) <= currentHour)
                        {
                            if (F2lStatusEventRequiresAbbreviatedCheck.Value.Default(false))
                                F2lStatusResult.SetValue("IC-Subsequent Abbreviated Flow-to-Load Check Passed", category);
                        }
                    }

                    if (F2lStatusResult.Value == null)
                    {
                        if ((F2lStatusQaCertEvent.Value == null) ||
                            (F2lStatusQaCertEvent.Value["CONDITIONAL_DATA_BEGIN_DATEHOUR"] == DBNull.Value) ||
                            (F2lStatusQaCertEvent.Value["CONDITIONAL_DATA_BEGIN_DATEHOUR"].AsDateTime().Value > currentHour))
                        {
                            DataView rataTestRecords = cRowFilter.FindRows(
                                                                            RataTestRecordsByLocationForQaStatus.Value,
                                                                            new cFilterCondition[]
                                                                {
                                                                  new cFilterCondition("MON_SYS_ID", CurrentMhvRecord.Value["MON_SYS_ID"].AsString()),
                                                                  new cFilterCondition("END_DATEHOUR", 
                                                                                        currentFlowToLoadStatusHour, 
                                                                                        eFilterDataType.DateBegan, 
                                                                                        eFilterConditionRelativeCompare.GreaterThan),
                                                                  new cFilterCondition("END_DATEHOUR", 
                                                                                        currentHour, 
                                                                                        eFilterDataType.DateBegan, 
                                                                                        eFilterConditionRelativeCompare.LessThan),
                                                                  new cFilterCondition("TEST_RESULT_CD", "INVALID")
                                                                }
                                                                          );

                            if (rataTestRecords.Count > 0)
                            {
                                F2lStatusResult.SetValue("OOC-Check Failed - Invalid RATA Ignored", category);
                            }
                            else
                            {
                                F2lStatusResult.SetValue("OOC-Check Failed", category);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                returnVal = category.CheckEngine.FormatError(ex);
            }

            return returnVal;
        }


        /// <summary>
        /// F2LSTAT5: Locate Earliest Valid Required Test
        /// </summary>
        /// <param name="category">Category Object</param>
        /// <param name="log">Indicates whether to log results.</param>
        /// <returns>Returns error message if check fails to run correctly.</returns>
        public string F2LSTAT5(cCategory category, ref bool log)
        {
            string returnVal = "";

            try
            {
                F2lStatusEarliestValidRequiredTest.SetValue(null, category);

                if (F2lStatusResult.Value == null)
                {
                    DateTime currentHour = CurrentMhvRecord.Value["BEGIN_DATE"].AsDateTime(DateTime.MinValue).AddHours(CurrentMhvRecord.Value["BEGIN_HOUR"].AsInteger(0));

                    if (F2lStatusEventRequiresRata.Value.Default(false))
                    {
                        DataRowView validRataTestRecord = cRowFilter.FindEarliestRow(
                                                                                      RataTestRecordsByLocationForQaStatus.Value,
                                                                                      new cFilterCondition[]
                                                                              {
                                                                                new cFilterCondition("MON_SYS_ID", CurrentMhvRecord.Value["MON_SYS_ID"].AsString()),
                                                                                new cFilterCondition("END_DATEHOUR", currentHour, eFilterDataType.DateEnded, eFilterConditionRelativeCompare.GreaterThan),
                                                                                new cFilterCondition("TEST_RESULT_CD", "INVALID", true)
                                                                              }
                                                                                    );

                        if (validRataTestRecord != null)
                        {
                            F2lStatusEarliestValidRequiredTest.SetValue(validRataTestRecord, category);

                            if (F2lStatusEarliestValidRequiredTest.Value["TEST_RESULT_CD"].AsString() == "FAILED")
                            {
                                DataView invalidRataTestRecords = cRowFilter.FindRows(
                                                                                       RataTestRecordsByLocationForQaStatus.Value,
                                                                                       new cFilterCondition[]
                                                                       {
                                                                         new cFilterCondition("MON_SYS_ID", CurrentMhvRecord.Value["MON_SYS_ID"].AsString()),
                                                                         new cFilterCondition("END_DATEHOUR", F2lStatusQaCertEvent.Value["QA_CERT_EVENT_DATEHOUR"].AsEndDateTime(), eFilterDataType.DateBegan, eFilterConditionRelativeCompare.GreaterThan),
                                                                         new cFilterCondition("END_DATEHOUR", F2lStatusEarliestValidRequiredTest.Value["END_DATEHOUR"].AsBeginDateTime(), eFilterDataType.DateEnded, eFilterConditionRelativeCompare.LessThan),
                                                                         new cFilterCondition("TEST_RESULT_CD", "INVALID")
                                                                       }
                                                                                     );

                                if (invalidRataTestRecords.Count > 0)
                                {
                                    F2lStatusResult.SetValue("OOC-Recertification RATA Failed - Invalid RATA Ignored", category);
                                }
                                else
                                {
                                    F2lStatusResult.SetValue("OOC-Recertification RATA Failed", category);
                                }
                            }
                        }
                    }
                    else if (!F2lStatusEventRequiresAbbreviatedCheck.Value.Default(false))
                    {
                        F2lStatusResult.SetValue("OOC-Invalid Cert Event", category);
                    }
                }
            }
            catch (Exception ex)
            {
                returnVal = category.CheckEngine.FormatError(ex);
            }

            return returnVal;
        }


        /// <summary>
        /// F2LSTAT6: Determine Event Conditional Status and Final Status
        /// </summary>
        /// <param name="category">Category Object</param>
        /// <param name="log">Indicates whether to log results.</param>
        /// <returns>Returns error message if check fails to run correctly.</returns>
        public string F2LSTAT6(cCategory category, ref bool log)
        {
            string returnVal = "";

            try
            {
                if (F2lStatusResult.Value == null)
                {
                    F2lStatusMissingOpDataInfo.SetValue(null, category);

                    DateTime currentHour = CurrentMhvRecord.Value["BEGIN_DATE"].AsDateTime(DateTime.MinValue).AddHours(CurrentMhvRecord.Value["BEGIN_HOUR"].AsInteger(0));
                    DateTime conditionalDataBeginDateHour = F2lStatusQaCertEvent.Value["CONDITIONAL_DATA_BEGIN_DATEHOUR"].AsDateTime(DateTime.MaxValue);

                    int locationPosition = CurrentMonitorPlanLocationPostion.Value.AsInteger(int.MinValue);

                    int operatingHourLimit = (F2lStatusEventRequiresRata.Value.Default(false) ? 720 : 168);

                    if (F2lStatusQaCertEvent.Value["CONDITIONAL_DATA_BEGIN_DATEHOUR"].AsDateTime().SameQuarter(currentHour))
                    {
                        int? conditionalDataHours;
                        {
                            string dictionaryKey = QaCertificationSupplementalData.FormatKey(EmParameters.F2lStatusQaCertEvent.QaCertEventId, eQaCertificationSupplementalDataTargetDateHour.ConditionalDataBeginHour);

                            if (EmParameters.QaCertEventSuppDataDictionaryArray[locationPosition].ContainsKey(dictionaryKey))
                            {
                                if (!AnnualReportingRequirement.Value.Default(false) && (currentHour.Quarter() == 2))
                                    conditionalDataHours = EmParameters.QaCertEventSuppDataDictionaryArray[locationPosition][dictionaryKey].MayAndJuneSystemOperatingCounts.Count;
                                else
                                    conditionalDataHours = EmParameters.QaCertEventSuppDataDictionaryArray[locationPosition][dictionaryKey].QuarterlySystemOperatingCounts.Count;
                            }
                            else
                            {
                                conditionalDataHours = null;
                            }
                        }

                        if (conditionalDataHours > operatingHourLimit)
                        {
                            F2lStatusResult.SetValue("OOC-Conditional Period Expired", category);
                        }
                        else
                        {
                            F2lStatusResult.SetValue("IC-Conditional", category);
                        }
                    }
                    else
                    {
                        if (F2lStatusQaCertEvent.Value["MIN_OP_HOURS_PRIOR_QTR"].AsInteger() == null)
                        {
                            F2lStatusQaCertEvent.Value["MIN_OP_HOURS_PRIOR_QTR"] = 0;
                            F2lStatusQaCertEvent.Value["MAX_OP_HOURS_PRIOR_QTR"] = 0;

                            int yearQtrFirst = (4 * conditionalDataBeginDateHour.Year) + (conditionalDataBeginDateHour.Quarter() - 1);
                            int yearQtrLast = ((4 * currentHour.Year) + (currentHour.Quarter() - 1)) - 1;

                            for (int yearQtr = yearQtrFirst; yearQtr <= yearQtrLast; yearQtr++)
                            {
                                int year = yearQtr.IntToYear();
                                int quarter = yearQtr.IntToQuarter();
                                DateTime quarterEndDate = (new DateTime(year, 3 * (quarter - 1) + 1, 1)).AddMonths(3).AddDays(-1);

                                if (EarliestLocationReportDate.Value.Default(DateTime.MaxValue) <= quarterEndDate)
                                {

                                    int? opHourCount = GetOpHourCount(year, quarter);


                                    if (opHourCount == null)
                                    {
                                        F2lStatusQaCertEvent.Value["MIN_OP_HOURS_PRIOR_QTR"] = -1;
                                        F2lStatusMissingOpDataInfo.AggregateValue(string.Format("{0}Q{1}", year, quarter));
                                        break;
                                    }
                                    else
                                    {
                                        int opHours = opHourCount.Value;

                                        if ((year == conditionalDataBeginDateHour.Year) && (quarter == conditionalDataBeginDateHour.Quarter()))
                                        {
                                            // Determine whether supplemental count exists, try system specific for Primary-Bypass situation first.
                                            int? supplementalCount = null;
                                            {
                                                if (F2lStatusQaCertEvent.Value["CONDITIONAL_BEGIN_HOUR_SYSTEM_SUPP_DATA_EXISTS_IND"].AsInteger() == 1)
                                                {
                                                    supplementalCount = F2lStatusQaCertEvent.Value["CONDITIONAL_BEGIN_SYSTEM_OP_HOUR_COUNT"].AsShort(0);
                                                }

                                                if (!supplementalCount.HasValue && (F2lStatusQaCertEvent.Value["CONDITIONAL_BEGIN_HOUR_SUPP_DATA_EXISTS_IND"].AsInteger() == 1))
                                                {
                                                    supplementalCount = F2lStatusQaCertEvent.Value["CONDITIONAL_BEGIN_OP_HOUR_COUNT"].AsShort(0);
                                                }
                                            }


                                            if (supplementalCount.HasValue)
                                            {
                                                F2lStatusQaCertEvent.Value["MIN_OP_HOURS_PRIOR_QTR"] = F2lStatusQaCertEvent.Value["MIN_OP_HOURS_PRIOR_QTR"].AsInteger(0) + supplementalCount.Value;
                                                F2lStatusQaCertEvent.Value["MAX_OP_HOURS_PRIOR_QTR"] = F2lStatusQaCertEvent.Value["MAX_OP_HOURS_PRIOR_QTR"].AsInteger(0) + supplementalCount.Value;
                                            }
                                            else
                                            {
                                                int quarterConditionalOpHours = opHours - (24 * cDateFunctions.DateDifference(cDateFunctions.StartDateThisQuarter(conditionalDataBeginDateHour.Date), conditionalDataBeginDateHour.Date) + conditionalDataBeginDateHour.Hour);

                                                if (quarterConditionalOpHours > 0)
                                                    F2lStatusQaCertEvent.Value["MIN_OP_HOURS_PRIOR_QTR"] = quarterConditionalOpHours;

                                                int quarterConditionalTotalHours = 24 * cDateFunctions.DateDifference(conditionalDataBeginDateHour.Date, cDateFunctions.LastDateThisQuarter(conditionalDataBeginDateHour.Date)) + (24 - conditionalDataBeginDateHour.Hour);

                                                if (opHours < quarterConditionalTotalHours)
                                                    F2lStatusQaCertEvent.Value["MAX_OP_HOURS_PRIOR_QTR"] = opHours;
                                                else
                                                    F2lStatusQaCertEvent.Value["MAX_OP_HOURS_PRIOR_QTR"] = quarterConditionalTotalHours;
                                            }
                                        }
                                        else
                                        {
                                            F2lStatusQaCertEvent.Value["MIN_OP_HOURS_PRIOR_QTR"] = F2lStatusQaCertEvent.Value["MIN_OP_HOURS_PRIOR_QTR"].AsInteger(0) + opHours;
                                            F2lStatusQaCertEvent.Value["MAX_OP_HOURS_PRIOR_QTR"] = F2lStatusQaCertEvent.Value["MAX_OP_HOURS_PRIOR_QTR"].AsInteger(0) + opHours;
                                        }
                                    }

                                }
                            }
                        }


                        int currentOpHours = EmParameters.SystemOperatingSuppDataDictionaryArray[category.CurrentMonLocPos][EmParameters.QaStatusSystemId].QuarterlyOperatingCounts.Hours;


                        if (F2lStatusQaCertEvent.Value["MIN_OP_HOURS_PRIOR_QTR"].AsInteger() == -1)
                        {
                            F2lStatusResult.SetValue("Missing Op Data", category);
                        }

                        else if (F2lStatusQaCertEvent.Value["MIN_OP_HOURS_PRIOR_QTR"].AsInteger() > operatingHourLimit)
                        {
                            if (F2lStatusEventRequiresRata.Value.Default(false))
                            {
                                DataView invalidRataTestRecords = cRowFilter.FindRows(
                                                                                       RataTestRecordsByLocationForQaStatus.Value,
                                                                                       new cFilterCondition[]
                                                                           {
                                                                             new cFilterCondition("MON_SYS_ID", CurrentMhvRecord.Value["MON_SYS_ID"].AsString()),
                                                                             new cFilterCondition("TEST_RESULT_CD", "INVALID"),
                                                                             new cFilterCondition("END_DATEHOUR", conditionalDataBeginDateHour, eFilterDataType.DateBegan, eFilterConditionRelativeCompare.GreaterThanOrEqual),
                                                                             new cFilterCondition("END_DATEHOUR", currentHour, eFilterDataType.DateEnded, eFilterConditionRelativeCompare.LessThanOrEqual)
                                                                           }
                                                                                     );

                                if (invalidRataTestRecords.Count > 0)
                                {
                                    F2lStatusResult.SetValue("OOC-Conditional Period Expired-Invalid RATA Ignored", category);
                                }
                                else
                                {
                                    F2lStatusResult.SetValue("OOC-Conditional Period Expired", category);
                                }
                            }
                            else
                            {
                                F2lStatusResult.SetValue("OOC-Conditional Period Expired", category);
                            }
                        }

                        else if ((F2lStatusQaCertEvent.Value["MIN_OP_HOURS_PRIOR_QTR"].AsInteger() + currentOpHours) > operatingHourLimit)
                        {
                            if (F2lStatusEventRequiresRata.Value.Default(false))
                            {
                                DataView invalidRataTestRecords = cRowFilter.FindRows(
                                                                                       RataTestRecordsByLocationForQaStatus.Value,
                                                                                       new cFilterCondition[]
                                                                           {
                                                                             new cFilterCondition("MON_SYS_ID", CurrentMhvRecord.Value["MON_SYS_ID"].AsString()),
                                                                             new cFilterCondition("TEST_RESULT_CD", "INVALID"),
                                                                             new cFilterCondition("END_DATEHOUR", conditionalDataBeginDateHour, eFilterDataType.DateBegan, eFilterConditionRelativeCompare.GreaterThanOrEqual),
                                                                             new cFilterCondition("END_DATEHOUR", currentHour, eFilterDataType.DateEnded, eFilterConditionRelativeCompare.LessThanOrEqual)
                                                                           }
                                                                                     );

                                if (invalidRataTestRecords.Count > 0)
                                {
                                    F2lStatusResult.SetValue("OOC-Conditional Period Expired-Invalid RATA Ignored", category);
                                }
                                else
                                {
                                    F2lStatusResult.SetValue("OOC-Conditional Period Expired", category);
                                }
                            }
                            else
                            {
                                F2lStatusResult.SetValue("OOC-Conditional Period Expired", category);
                            }
                        }

                        else if ((F2lStatusQaCertEvent.Value["MAX_OP_HOURS_PRIOR_QTR"].AsInteger() + currentOpHours) > operatingHourLimit)
                        {
                            F2lStatusResult.SetValue("Undetermined-Conditional Data", category);
                        }

                        else
                        {
                            F2lStatusResult.SetValue("IC-Conditional", category);
                        }
                    }
                }

                if (!F2lStatusResult.Value.StartsWith("IC"))
                    category.CheckCatalogResult = F2lStatusResult.Value;
            }
            catch (Exception ex)
            {
                returnVal = category.CheckEngine.FormatError(ex);
            }

            return returnVal;
        }

        #endregion


        #region Helper Methods

        /// <summary>
        /// Retrieves the op hour count for the QA Status System and quarter, first from the System Operating Supp Data if it exists,
        /// otherwise from the location level Operating Supp Data.
        /// </summary>
        /// <param name="year">The year of the op supp data to retrieve.</param>
        /// <param name="quarter">The quarter of the op supp data to retrieve.</param>
        /// <returns>The quarter specific operating count for the QA Status System, null if it does not exist.</returns>
        public int? GetOpHourCount(int year, int quarter)
        {
            int? opHourCount;

            string locationOpSuppType, systemOpSuppType;
            {
                if (!AnnualReportingRequirement.Value.Default(false) && (quarter == 2))
                {
                    locationOpSuppType = "OSHOURS";
                    systemOpSuppType = "OPMJ";
                }
                else
                {
                    locationOpSuppType = "OPHOURS";
                    systemOpSuppType = "OP";
                }
            }

            SystemOpSuppData systemOpSuppDataRecord = EmParameters.SystemOperatingSuppDataRecordsByLocation.FindRow
                                                      (
                                                          new cFilterCondition("MON_SYS_ID", EmParameters.QaStatusSystemId),
                                                          new cFilterCondition("CALENDAR_YEAR", year, eFilterDataType.Integer),
                                                          new cFilterCondition("QUARTER", quarter, eFilterDataType.Integer),
                                                          new cFilterCondition("OP_SUPP_DATA_TYPE_CD", systemOpSuppType)
                                                      );

            if (systemOpSuppDataRecord != null)
            {
                opHourCount = systemOpSuppDataRecord.Hours;
            }
            else
            {
                VwMpOpSuppDataRow locationOpSuppDataRecord = EmParameters.OperatingSuppDataRecordsByLocation.FindRow
                                                            (
                                                                new cFilterCondition("CALENDAR_YEAR", year, eFilterDataType.Integer),
                                                                new cFilterCondition("QUARTER", quarter, eFilterDataType.Integer),
                                                                new cFilterCondition("OP_TYPE_CD", locationOpSuppType),
                                                                new cFilterCondition("FUEL_CD", DBNull.Value, eFilterDataType.String)
                                                            );

                if (locationOpSuppDataRecord != null)
                {
                    opHourCount = locationOpSuppDataRecord.OpValue.HasValue ? Decimal.ToInt32(locationOpSuppDataRecord.OpValue.Value) : (int?)null;
                }
                else
                {
                    opHourCount = null;
                }
            }

            return opHourCount;
        }

        #endregion

        #endregion

    }

}
