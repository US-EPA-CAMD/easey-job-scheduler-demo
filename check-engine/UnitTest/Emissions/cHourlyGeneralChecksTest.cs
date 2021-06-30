using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;

using ECMPS.Checks.CheckEngine;
using ECMPS.Checks.CheckEngine.SpecialParameterClasses;
using ECMPS.Checks.Data.Ecmps.CheckEm.Function;
using ECMPS.Checks.Data.Ecmps.Dbo.View;
using ECMPS.Checks.Data.Ecmps.Dbo.Table;
using ECMPS.Checks.Data.Ecmps.Lookup.Table;
using ECMPS.Checks.Data.EcmpsAux.CrossCheck.Virtual;
using ECMPS.Checks.Em.Parameters;
using ECMPS.Checks.EmissionsChecks;
using ECMPS.Checks.EmissionsReport;

using UnitTest.UtilityClasses;


namespace UnitTest.Emissions
{
    [TestClass]
    public class cHourlyGeneralChecksTest
    {

        /// <summary>
        /// HOURGEN-1
        /// 
        /// Currently only ensures that the following are set correctly:
        /// 
        ///     InvalidCylinderIdList
        ///     
        /// </summary>
        /// <returns></returns>
        [TestMethod()]
        public void HourGen1_Incomplete()
        {
            /* Initialize objects generally needed for testing checks. */
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            EmParameters.Init(category.Process);
            EmParameters.Category = category;

            cHourlyGeneralChecks target = new cHourlyGeneralChecks(new cEmissionsCheckParameters());


            /* Initialize Input Parameters */
            EmParameters.CurrentLocationCount = 3;
            EmParameters.HourlyFuelFlowRecords = new CheckDataView<VwMpHrlyFuelFlowRow>();


            /* Init Cateogry Result */
            category.CheckCatalogResult = null;

            /* Initialize variables needed to run the check. */
            bool log = false;
            string actual;

            /* Run Check */
            actual = target.HOURGEN1(category, ref log);


            /* Check Results */
            Assert.AreEqual(string.Empty, actual, string.Format("actual"));
            Assert.AreEqual(false, log, string.Format("log"));
            Assert.AreEqual(null, category.CheckCatalogResult, string.Format("category.CheckCatalogResult"));

            Assert.AreEqual(0, EmParameters.InvalidCylinderIdList.Count, string.Format("InvalidCylinderIdList"));
        }


        /// <summary>
        /// 
        /// CurrentReportingPeriodYear: 2021
        /// 
        /// 
        /// | ## | Severity | NeedsEval | MustSubmit | LastEvalDate || Result | Success | Timframe              || Notes
        /// |  0 | null     | null      | N          | 2021-01-01   || null   | true    | empty                 || Passing severity values.
        /// |  1 | ADMNOVR  | null      | N          | 2021-01-01   || null   | true    | empty                 || Passing severity values.
        /// |  2 | CRIT2    | null      | N          | 2021-01-01   || null   | true    | empty                 || Passing severity values.
        /// |  3 | CRIT3    | null      | N          | 2021-01-01   || null   | true    | empty                 || Passing severity values.
        /// |  4 | FORGIVE  | null      | N          | 2021-01-01   || null   | true    | empty                 || Passing severity values.
        /// |  5 | INFORM   | null      | N          | 2021-01-01   || null   | true    | empty                 || Passing severity values.
        /// |  6 | NONCRIT  | null      | N          | 2021-01-01   || null   | true    | empty                 || Passing severity values.
        /// |  7 | NONE     | null      | N          | 2021-01-01   || null   | true    | empty                 || Passing severity values.
        /// |  8 | CRIT1    | null      | N          | 2021-01-01   || A      | false   | empty                 || Result A.
        /// |  9 | FATAL    | null      | N          | 2021-01-01   || A      | false   | empty                 || Result A.
        /// | 10 | NONE     | null      | Y          | 2021-01-01   || null   | true    | empty                 || Both NeedsEval is null and MustSubmit is Y.
        /// | 11 | NONE     | N         | N          | 2021-01-01   || null   | true    | empty                 || Both NeedsEval and MustSubmit are N.
        /// | 12 | NONE     | N         | Y          | 2021-01-01   || null   | true    | empty                 || Both NeedsEval is N and MustSubmit is Y.
        /// | 13 | NONE     | Y         | N          | 2021-01-01   || null   | true    | empty                 || Both NeedsEval is Y and MustSubmit is N.
        /// | 14 | NONE     | Y         | Y          | 2021-01-01   || B      | false   | empty                 || Both NeedsEval and MustSubmit are Y.
        /// | 15 | NONE     | null      | N          | 2020-12-31   || C      | false   | " since 12/31/2020"   || Last eval in previous year.
        /// | 16 | NONE     | null      | N          | null         || C      | false   | " this calendar year" || Last eval is null.
        /// </summary>
        [TestMethod]
        public void HourGen8()
        {
            /* Initialize objects generally needed for testing checks. */
            cEmissionsCheckParameters emCheckParameters = UnitTestCheckParameters.InstantiateEmParameters(); // Old Instantiated Parameters Used
            cHourlyGeneralChecks target = new cHourlyGeneralChecks(emCheckParameters);
            cCategory category = EmParameters.Category;


            /* Value Variables */
            DateTime dPrev = new DateTime(2020, 12, 31);
            DateTime dCurr = new DateTime(2021, 01, 01);


            /* Input Parameter Values */
            string[] severityList = { null, "ADMNOVR", "CRIT2", "CRIT3", "FORGIVE", "INFORM", "NONCRIT", "NONE", "CRIT1", "FATAL", "NONE", "NONE", "NONE", "NONE", "NONE", "NONE", "NONE" };
            string[] needsEvalList = { null, null, null, null, null, null, null, null, null, null, null, "N", "N", "Y", "Y", null, null };
            string[] mustSubmitList = { "N", "N", "N", "N", "N", "N", "N", "N", "N", "N", "Y", "N", "Y", "N", "Y", "N", "N" };
            DateTime?[] lastEvalDateList = { dCurr, dCurr, dCurr, dCurr, dCurr, dCurr, dCurr, dCurr, dCurr, dCurr, dCurr, dCurr, dCurr, dCurr, dCurr, dPrev, null };

            /* Expected Values */
            string[] expResultList = { null, null, null, null, null, null, null, null, "A", "A", null, null, null, null, "B", "C", "C" };
            bool?[] expSuccessList = { true, true, true, true, true, true, true, true, false, false, true, true, true, true, false, false, false };
            string[] expTimeframeList = { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", " since 12/31/2020", " this calendar year" };

            /* Test Case Count */
            int caseCount = 17;

            /* Check array lengths */
            Assert.AreEqual(caseCount, severityList.Length, "severityList length");
            Assert.AreEqual(caseCount, needsEvalList.Length, "needsEvalList length");
            Assert.AreEqual(caseCount, mustSubmitList.Length, "mustSubmitList length");
            Assert.AreEqual(caseCount, lastEvalDateList.Length, "lastEvalDateList length");
            Assert.AreEqual(caseCount, expResultList.Length, "expResultList length");
            Assert.AreEqual(caseCount, expSuccessList.Length, "expSuccessList length");
            Assert.AreEqual(caseCount, expTimeframeList.Length, "expTimeframeList length");


            /* Run Cases */
            for (int caseDex = 0; caseDex < caseCount; caseDex++)
            {
                /*  Initialize Input Parameters*/
                EmParameters.CurrentMonitoringPlanRecord = new VwMpMonitorPlanRow(severityCd: severityList[caseDex], needsEvalFlg: needsEvalList[caseDex], mustSubmit: mustSubmitList[caseDex], 
                                                                                  lastEvaluatedDate: lastEvalDateList[caseDex]);
                EmParameters.CurrentReportingPeriodYear = 2021;


                /*  Initialize Output Parameters*/
                EmParameters.MpSuccessfullyEvaluated = null;
                EmParameters.MpLastEvaluatedTimeframe = "Base Timeframe";


                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                /* Initialize variables needed to run the check. */
                bool log = false;
                string actual;


                /* Run Check */
                actual = target.HOURGEN8(category, ref log);


                /* Check Results */
                Assert.AreEqual(string.Empty, actual, string.Format("actual {0}", caseDex));
                Assert.AreEqual(false, log, string.Format("log {0}", caseDex));

                Assert.AreEqual(expResultList[caseDex], category.CheckCatalogResult, $"CheckCatalogResult {caseDex}");
                Assert.AreEqual(expSuccessList[caseDex], EmParameters.MpSuccessfullyEvaluated, $"MpSuccessfullyEvaluated {caseDex}");
                Assert.AreEqual(expTimeframeList[caseDex], EmParameters.MpLastEvaluatedTimeframe, $"MpLastEvaluatedTimeframe {caseDex}");
            }
        }


        /// <summary>
        /// 
        /// 
        /// Current Reporting Period
        /// 
        ///     Begin Hour (prdB) : 2017-04-01 00
        ///     End Hour (prdE)   : 2017-06-30 23
        /// 
        ///               |            Qual 1            |  Qual 2  |  Qual 3  |    Id for 2017 % Row     ||
        /// | ## | MpEval | QualType | BegDt   | EndDt   | QualType | QualType | % 1 Id | % 2 Id | % 3 Id || Result | List                                                                           || Note
        /// |  0 | null   | GF       | prdB    | null    |          |          |        |        |        || null   | ""                                                                             || MpSuccessfullyEvaluated is null, so check body is not executed.
        /// |  1 | false  | GF       | prdB    | null    |          |          |        |        |        || null   | ""                                                                             || MpSuccessfullyEvaluated is false, so check body is not executed.
        /// |  2 | true   | COMPLEX  | prdB    | null    |          |          |        |        |        || null   | ""                                                                             || Existing qualification is not GF, PK or SK.
        /// |  3 | true   | GF       | prdB    | null    |          |          |        |        |        || A      | "Gas-Fired Unit 1"                                                             || GF exists without a percent row for current year.
        /// |  4 | true   | HGAVG    | prdB    | null    |          |          |        |        |        || null   | ""                                                                             || Existing qualification is not GF, PK or SK.
        /// |  5 | true   | LEE      | prdB    | null    |          |          |        |        |        || null   | ""                                                                             || Existing qualification is not GF, PK or SK.
        /// |  6 | true   | LMEA     | prdB    | null    |          |          |        |        |        || null   | ""                                                                             || Existing qualification is not GF, PK or SK.
        /// |  7 | true   | LMES     | prdB    | null    |          |          |        |        |        || null   | ""                                                                             || Existing qualification is not GF, PK or SK.
        /// |  8 | true   | LOWSULF  | prdB    | null    |          |          |        |        |        || null   | ""                                                                             || Existing qualification is not GF, PK or SK.
        /// |  9 | true   | PK       | prdB    | null    |          |          |        |        |        || A      | "Year-Round Peaking Unit 1"                                                    || PK exists without a percent row for current year.
        /// | 10 | true   | PRATA1   | prdB    | null    |          |          |        |        |        || null   | ""                                                                             || Existing qualification is not GF, PK or SK.
        /// | 11 | true   | PRATA2   | prdB    | null    |          |          |        |        |        || null   | ""                                                                             || Existing qualification is not GF, PK or SK.
        /// | 12 | true   | SK       | prdB    | null    |          |          |        |        |        || A      | "Ozone-Season Peaking Unit 1"                                                  || SK exists without a percent row for current year.
        /// | 13 | true   | GF       | prdE    | null    |          |          |        |        |        || A      | "Gas-Fired Unit 1"                                                             || GF begins on ending period date without a percent row for current year.
        /// | 14 | true   | GF       | prdB-2d | prdB    |          |          |        |        |        || A      | "Gas-Fired Unit 1"                                                             || GF ends on the beginning period date without a percent row for current year.
        /// | 15 | true   | GF       | prdE+1d | null    |          |          |        |        |        || null   | ""                                                                             || GF begins after the period without a percent row for current year.
        /// | 16 | true   | GF       | prdB-2d | prdB-1d |          |          |        |        |        || null   | ""                                                                             || GF ends before the period without a percent row for current year.
        /// | 17 | true   | GF       | prdB    | null    |          |          | QUAL2  |        |        || A      | "Gas-Fired Unit 1"                                                             || GF exists, but percent row for current year is for another qualification.
        /// | 18 | true   | GF       | prdB    | null    |          |          | QUAL1  |        |        || null   | ""                                                                             || GF exists with a percent row for current year.
        /// | 19 | true   | PK       | prdB    | null    |          |          | QUAL1  |        |        || null   | ""                                                                             || PK exists with a percent row for current year.
        /// | 20 | true   | SK       | prdB    | null    |          |          | QUAL1  |        |        || null   | ""                                                                             || SK exists with a percent row for current year.
        /// | 21 | true   | GF       | prdB    | null    | PK       | SK       | QUAL1  | QUAL2  | QUAL3  || null   | ""                                                                             || GF exists with a percent row for current year.
        /// | 22 | true   | GF       | prdB    | null    | PK       | SK       |        |        |        || A      | "Gas-Fired Unit 1, Year-Round Peaking Unit 2, and Ozone-Season Peaking Unit 3" || GF exists with a percent row for current year.
        /// </summary>
        [TestMethod]
        public void HourGen17()
        {
            /* Initialize objects generally needed for testing checks. */
            cEmissionsCheckParameters emCheckParameters = UnitTestCheckParameters.InstantiateEmParameters(); // Old Instantiated Parameters Used
            cHourlyGeneralChecks target = new cHourlyGeneralChecks(emCheckParameters);
            cCategory category = EmParameters.Category;


            /* Input Parameter Values */
            DateTime prdBegHr = new DateTime(2017, 4, 1, 0, 0, 0);
            DateTime prdBegDt = prdBegHr.Date;
            DateTime prdEndHr = new DateTime(2017, 6, 30, 23, 0, 0);
            DateTime prdEndDt = prdEndHr.Date;
            int prdYear = prdBegHr.Year;

            bool?[] mpEvalList = { null, false, true, true, true, true, true, true, true, true,
                                   true, true, true, true, true, true, true, true, true, true,
                                   true, true, true };
            string[] qual1TypeList = { "GF", "GF", "COMPLEX", "GF", "HGAVG", "LEE", "LMEA", "LMES", "LOWSULF", "PK",
                                       "PRATA1", "PRATA2", "SK", "GF", "GF", "GF", "GF", "GF", "GF", "PK",
                                       "SK", "GF", "GF" };
            DateTime?[] qual1BegDateList = { prdBegDt, prdBegDt, prdBegDt, prdBegDt, prdBegDt, prdBegDt, prdBegDt, prdBegDt, prdBegDt, prdBegDt,
                                             prdBegDt, prdBegDt, prdBegDt, prdBegDt, prdBegDt.AddDays(-2), prdEndDt.AddDays(1), prdBegDt.AddDays(-2), prdBegDt, prdBegDt, prdBegDt,
                                             prdBegDt, prdBegDt, prdBegDt };
            DateTime?[] qual1EndDateList = { null, null, null, null, null, null, null, null, null, null,
                                             null, null, null, null, prdBegDt, null, prdBegDt.AddDays(-1), null, null, null,
                                             null, null, null };
            string[] qual2TypeList = { null, null, null, null, null, null, null, null, null, null,
                                       null, null, null, null, null, null, null, null, null, null,
                                       null, "PK", "PK" };
            string[] qual3TypeList = { null, null, null, null, null, null, null, null, null, null,
                                       null, null, null, null, null, null, null, null, null, null,
                                       null, "SK", "SK" };
            string[] pct1IdList = { null, null, null, null, null, null, null, null, null, null,
                                    null, null, null, null, null, null, null, "QUAL2", "QUAL1", "QUAL1",
                                    "QUAL1", "QUAL1", null };
            string[] pct2IdList = { null, null, null, null, null, null, null, null, null, null,
                                    null, null, null, null, null, null, null, null, null, null,
                                    null, "QUAL2", null };
            string[] pct3IdList = { null, null, null, null, null, null, null, null, null, null,
                                    null, null, null, null, null, null, null, null, null, null,
                                    null, "QUAL3", null };

            /* Expected Values */
            string[] expResultList = { null, null, null, "A", null, null, null, null, null, "A",
                                       null, null, "A", "A", "A", null, null, "A", null, null,
                                       null, null, "A" };
            string[] expMissingList = { "", "", "", "Gas-Fired Unit 1", "", "", "", "", "", "Year-Round Peaking Unit 1",
                                        "", "", "Ozone-Season Peaking Unit 1", "Gas-Fired Unit 1", "Gas-Fired Unit 1", "", "", "Gas-Fired Unit 1", "", "",
                                        "", "", "Gas-Fired Unit 1, Year-Round Peaking Unit 2, and Ozone-Season Peaking Unit 3" };


            /* Test Case Count */
            int caseCount = 23;

            /* Check array lengths */
            Assert.AreEqual(caseCount, mpEvalList.Length, "mpEvalList length");
            Assert.AreEqual(caseCount, qual1TypeList.Length, "qual1TypeList length");
            Assert.AreEqual(caseCount, qual1BegDateList.Length, "qual1BegDateList length");
            Assert.AreEqual(caseCount, qual1EndDateList.Length, "qual1EndDateList length");
            Assert.AreEqual(caseCount, qual2TypeList.Length, "qual2TypeList length");
            Assert.AreEqual(caseCount, qual3TypeList.Length, "qual3TypeList length");
            Assert.AreEqual(caseCount, pct1IdList.Length, "pct1IdList length");
            Assert.AreEqual(caseCount, pct2IdList.Length, "pct2IdList length");
            Assert.AreEqual(caseCount, pct3IdList.Length, "pct3IdList length");
            Assert.AreEqual(caseCount, expResultList.Length, "expResultList length");
            Assert.AreEqual(caseCount, expMissingList.Length, "expMissingList length");


            /* Run Cases */
            for (int caseDex = 0; caseDex < caseCount; caseDex++)
            {
                /*  Initialize Input Parameters*/
                EmParameters.CurrentReportingPeriodBeginHour = prdBegHr;
                EmParameters.CurrentReportingPeriodEndHour = prdEndHr;
                EmParameters.CurrentReportingPeriodYear = prdYear;
                EmParameters.MpQualificationPercentRecords = new CheckDataView<MonitorQualificationPercentData>();
                {
                    EmParameters.MpQualificationPercentRecords.Add(new MonitorQualificationPercentData(monQualId: pct1IdList[caseDex], qualYear: prdYear - 1));

                    if (pct1IdList[caseDex] != null)
                        EmParameters.MpQualificationPercentRecords.Add(new MonitorQualificationPercentData(monQualId: pct1IdList[caseDex], qualYear: prdYear));

                    if (pct2IdList[caseDex] != null)
                        EmParameters.MpQualificationPercentRecords.Add(new MonitorQualificationPercentData(monQualId: pct2IdList[caseDex], qualYear: prdYear));

                    if (pct3IdList[caseDex] != null)
                        EmParameters.MpQualificationPercentRecords.Add(new MonitorQualificationPercentData(monQualId: pct3IdList[caseDex], qualYear: prdYear));

                    EmParameters.MpQualificationPercentRecords.Add(new MonitorQualificationPercentData(monQualId: pct1IdList[caseDex], qualYear: prdYear + 1));
                }
                EmParameters.MpQualificationRecords = new CheckDataView<VwMpMonitorQualificationRow>();
                {
                    EmParameters.MpQualificationRecords.Add(new VwMpMonitorQualificationRow(monQualId: "Bad1", locationId: "Bad1", qualTypeCd: "BAD1", beginDate: prdBegDt));

                    if (qual1TypeList[caseDex] != null)
                        EmParameters.MpQualificationRecords.Add(new VwMpMonitorQualificationRow(monQualId: "QUAL1", locationId: "1", qualTypeCd: qual1TypeList[caseDex], beginDate: qual1BegDateList[caseDex], endDate: qual1EndDateList[caseDex]));

                    if (qual2TypeList[caseDex] != null)
                        EmParameters.MpQualificationRecords.Add(new VwMpMonitorQualificationRow(monQualId: "QUAL2", locationId: "2", qualTypeCd: qual2TypeList[caseDex], beginDate: prdBegDt));

                    if (qual3TypeList[caseDex] != null)
                        EmParameters.MpQualificationRecords.Add(new VwMpMonitorQualificationRow(monQualId: "QUAL3", locationId: "3", qualTypeCd: qual3TypeList[caseDex], beginDate: prdBegDt));

                    EmParameters.MpQualificationRecords.Add(new VwMpMonitorQualificationRow(monQualId: "Bad2", locationId: "Bad2", qualTypeCd: "BAD2", beginDate: prdBegDt));
                }
                EmParameters.MpSuccessfullyEvaluated = mpEvalList[caseDex];


                /*  Initialize Output Parameters*/
                EmParameters.QualificationPercentMissingList = "Bad List";


                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                /* Initialize variables needed to run the check. */
                bool log = false;
                string actual;

                /* Run Check */
                actual = target.HOURGEN17(category, ref log);

                /* Check Results */
                Assert.AreEqual(string.Empty, actual, string.Format("actual {0}", caseDex));
                Assert.AreEqual(false, log, string.Format("log {0}", caseDex));

                Assert.AreEqual(expResultList[caseDex], category.CheckCatalogResult, string.Format("category.CheckCatalogResult {0}", caseDex));
                Assert.AreEqual(expMissingList[caseDex], EmParameters.QualificationPercentMissingList, string.Format("QualificationPercentMissingList {0}", caseDex));
            }

        }


        [TestMethod]
        public void HourGen19()
        {
            /* Initialize objects generally needed for testing checks. */
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            EmParameters.Init(category.Process);
            EmParameters.Category = category;

            cHourlyGeneralChecks target = new cHourlyGeneralChecks(new cEmissionsCheckParameters());


            /* Run cases */
            foreach (bool trap1IsSupplementalData in UnitTestStandardLists.BooleanList)
                foreach (bool trap2IsSupplementalData in UnitTestStandardLists.BooleanList)
                {
                    /* Initialize input parameters */
                    EmParameters.CurrentLocationCount = 3;
                    EmParameters.MatsSorbentTrapRecords = new CheckDataView<MatsSorbentTrapRecord>();
                    {
                        EmParameters.MatsSorbentTrapRecords.Add(new MatsSorbentTrapRecord(trapId: "trap1", suppDataInd: (trap1IsSupplementalData ? 1 : 0)));
                        EmParameters.MatsSorbentTrapRecords.Add(new MatsSorbentTrapRecord(trapId: "trap2", suppDataInd: (trap2IsSupplementalData ? 1 : 0)));
                    }

                    /* Initialize output parameters */
                    EmParameters.MatsSamplingTrainDictionary = null;
                    EmParameters.MatsSorbentTrapListByLocationArray = null;
                    EmParameters.MatsSorbentTrapDictionary = null;
                    EmParameters.MatsSorbentTrapEvaluationNeeded = null;


                    /* Init Cateogry Result */
                    category.CheckCatalogResult = null;

                    /* Initialize variables needed to run the check. */
                    bool log = false;
                    string actual;

                    /* Run Check */
                    actual = target.HOURGEN19(category, ref log);


                    /* Check results */
                    Assert.AreEqual(string.Empty, actual);
                    Assert.AreEqual(false, log);
                    Assert.AreEqual(null, category.CheckCatalogResult, "Result");

                    Assert.AreNotEqual(null, EmParameters.MatsSamplingTrainDictionary, string.Format("MatsSamplingTrainDictionary [{0}, {1}]", trap1IsSupplementalData, trap2IsSupplementalData));
                    Assert.AreEqual(0, EmParameters.MatsSamplingTrainDictionary.Count, string.Format("MatsSamplingTrainDictionary.Count [{0}, {1}]", trap1IsSupplementalData, trap2IsSupplementalData));
                    Assert.AreNotEqual(null, EmParameters.MatsSorbentTrapDictionary, string.Format("MatsSorbentTrapDictionary [{0}, {1}]", trap1IsSupplementalData, trap2IsSupplementalData));
                    Assert.AreEqual(0, EmParameters.MatsSorbentTrapDictionary.Count, string.Format("MatsSorbentTrapDictionary.Count [{0}, {1}]", trap1IsSupplementalData, trap2IsSupplementalData));
                    Assert.AreNotEqual(null, EmParameters.MatsSorbentTrapListByLocationArray, string.Format("MatsSorbentTrapListByLocationArray [{0}, {1}]", trap1IsSupplementalData, trap2IsSupplementalData));
                    Assert.AreEqual(3, EmParameters.MatsSorbentTrapListByLocationArray.Length, string.Format("MatsSorbentTrapListByLocationArray.Length [{0}, {1}]", trap1IsSupplementalData, trap2IsSupplementalData));
                    Assert.AreNotEqual(null, EmParameters.MatsSorbentTrapListByLocationArray[0], string.Format("MatsSorbentTrapListByLocationArray[0] [{0}, {1}]", trap1IsSupplementalData, trap2IsSupplementalData));
                    Assert.AreEqual(0, EmParameters.MatsSorbentTrapListByLocationArray[0].Count, string.Format("MatsSorbentTrapListByLocationArray[0].Count [{0}, {1}]", trap1IsSupplementalData, trap2IsSupplementalData));
                    Assert.AreNotEqual(null, EmParameters.MatsSorbentTrapListByLocationArray[1], string.Format("MatsSorbentTrapListByLocationArray[1] [{0}, {1}]", trap1IsSupplementalData, trap2IsSupplementalData));
                    Assert.AreEqual(0, EmParameters.MatsSorbentTrapListByLocationArray[1].Count, string.Format("MatsSorbentTrapListByLocationArray[1].Count [{0}, {1}]", trap1IsSupplementalData, trap2IsSupplementalData));
                    Assert.AreNotEqual(null, EmParameters.MatsSorbentTrapListByLocationArray[2], string.Format("MatsSorbentTrapListByLocationArray[2] [{0}, {1}]", trap1IsSupplementalData, trap2IsSupplementalData));
                    Assert.AreEqual(0, EmParameters.MatsSorbentTrapListByLocationArray[2].Count, string.Format("MatsSorbentTrapListByLocationArray[2].Count [{0}, {1}]", trap1IsSupplementalData, trap2IsSupplementalData));
                    Assert.AreEqual(!trap1IsSupplementalData || !trap2IsSupplementalData, EmParameters.MatsSorbentTrapEvaluationNeeded, string.Format("MatsSorbentTrapEvaluationNeeded [{0}, {1}]", trap1IsSupplementalData, trap2IsSupplementalData));
                }
        }

        [TestMethod]
        public void HourGen20()
        {
            /* Initialize objects generally needed for testing checks. */
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            EmParameters.Init(category.Process);
            EmParameters.Category = category;

            cHourlyGeneralChecks target = new cHourlyGeneralChecks(new cEmissionsCheckParameters());


            /* Initialize variables needed to run the check. */
            bool log = false;
            string actual;

            /* Initialize output parameters */
            EmParameters.WsiTestDictionary = null;

            /* Call check */
            actual = target.HOURGEN20(category, ref log);

            /* Check results */
            Assert.AreEqual(string.Empty, actual);
            Assert.AreEqual(false, log);
            Assert.AreEqual(null, category.CheckCatalogResult, "Result");

            Assert.AreNotEqual(null, EmParameters.WsiTestDictionary, "WsiTestDictionary");
            Assert.AreEqual(0, EmParameters.WsiTestDictionary.Count, "WsiTestDictionary.Count");
        }

        [TestMethod]
        public void HourGen21()
        {
            /* Initialize objects generally needed for testing checks. */
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            EmParameters.Init(category.Process);
            EmParameters.Category = category;

            cHourlyGeneralChecks target = new cHourlyGeneralChecks(new cEmissionsCheckParameters());


            /* Initialize variables needed to run the check. */
            bool log = false;
            string actual;

            /* Initialize output parameters */
            EmParameters.CurrentLocationCount = 3;
            EmParameters.TestResultCodeLookupTable = new CheckDataView<TestResultCodeRow>
            (
              new TestResultCodeRow(testResultCd: "THREE"),
              new TestResultCodeRow(testResultCd: "TWO"),
              new TestResultCodeRow(testResultCd: "ONE")
            );

            /* Initialize output parameters */
            EmParameters.TestResultCodeList = null;

            /* Call check */
            actual = target.HOURGEN21(category, ref log);

            /* Check results */
            Assert.AreEqual(string.Empty, actual);
            Assert.AreEqual(false, log);
            Assert.AreEqual(null, category.CheckCatalogResult, "Result");

            Assert.AreEqual("THREE,TWO,ONE", EmParameters.TestResultCodeList, "TestResultCodeList.Count");

            int missingDataHourCount;
            decimal? lastPercentAvailable;

            for (int locationPostion = 0; locationPostion < EmParameters.CurrentLocationCount; locationPostion++)
                foreach (MissingDataPmaTracking.eHourlyParameter hourlyParameter in Enum.GetValues(typeof(MissingDataPmaTracking.eHourlyParameter)))
                {
                    EmParameters.MissingDataPmaTracking.GetHourlyParamaterInfo(locationPostion, hourlyParameter, out missingDataHourCount, out lastPercentAvailable);
                    Assert.AreEqual(0, missingDataHourCount, $"MissingDataHourCount( {missingDataHourCount}, {MissingDataPmaTracking.GetHourlyType(hourlyParameter)}, {MissingDataPmaTracking.GetHourlyParameterCd(hourlyParameter)} )");
                    Assert.AreEqual(null, lastPercentAvailable, $"MissingDataHourCount( {missingDataHourCount}, {MissingDataPmaTracking.GetHourlyType(hourlyParameter)}, {MissingDataPmaTracking.GetHourlyParameterCd(hourlyParameter)} )");
                }

        }

        /// <summary>
        /// HourGen-23
        /// 
        /// </summary>
        [TestMethod]
        public void HourGen23()
        {
            /* Initialize objects generally needed for testing checks. */
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            EmParameters.Init(category.Process);
            EmParameters.Category = category;

            cHourlyGeneralChecks target = new cHourlyGeneralChecks(new cEmissionsCheckParameters());


            /* Initialize Required Parameters */
            EmParameters.ProgramCodeTable = new CheckDataView<ProgramCodeRow>
                (
                    new ProgramCodeRow(prgCd: "ARP", prgName: "Acid Rain Program", osInd: 0, rueInd: 1, so2CertInd: 1, noxCertInd: 1, noxcCertInd: 0),
                    new ProgramCodeRow(prgCd: "CAIRNOX", prgName: "CAIR NOx Annual Program", osInd: 0, rueInd: 1, so2CertInd: 0, noxCertInd: 1, noxcCertInd: 1),
                    new ProgramCodeRow(prgCd: "CAIROS", prgName: "CAIR NOx Ozone Season Program", osInd: 1, rueInd: 1, so2CertInd: 0, noxCertInd: 1, noxcCertInd: 1),
                    new ProgramCodeRow(prgCd: "CAIRSO2", prgName: "CAIR SO2 Annual Program", osInd: 0, rueInd: 1, so2CertInd: 1, noxCertInd: 0, noxcCertInd: 0),
                    new ProgramCodeRow(prgCd: "CSNOX", prgName: "CAIR NOx Annual Program", osInd: 0, rueInd: 1, so2CertInd: 0, noxCertInd: 1, noxcCertInd: 1),
                    new ProgramCodeRow(prgCd: "CSNOXOS", prgName: "Cross - State Air Pollution Rule NOx Ozone Season Program", osInd: 1, rueInd: 1, so2CertInd: 0, noxCertInd: 1, noxcCertInd: 1),
                    new ProgramCodeRow(prgCd: "CSOSG1", prgName: "Cross - State Air Pollution Rule NOx Ozone Season Group 1 Program", osInd: 1, rueInd: 1, so2CertInd: 0, noxCertInd: 1, noxcCertInd: 1),
                    new ProgramCodeRow(prgCd: "CSOSG2", prgName: "Cross - State Air Pollution Rule NOx Ozone Season Group 2 Program", osInd: 1, rueInd: 1, so2CertInd: 0, noxCertInd: 1, noxcCertInd: 1),
                    new ProgramCodeRow(prgCd: "CSSO2G1", prgName: "Cross - State Air Pollution Rule SO2 Annual Group 1 Program", osInd: 0, rueInd: 1, so2CertInd: 1, noxCertInd: 0, noxcCertInd: 0),
                    new ProgramCodeRow(prgCd: "CSSO2G2", prgName: "Cross - State Air Pollution Rule SO2 Annual Group 2 Program", osInd: 0, rueInd: 1, so2CertInd: 1, noxCertInd: 0, noxcCertInd: 0),
                    new ProgramCodeRow(prgCd: "MATS", prgName: "Mercury and Air Toxics Standards", osInd: 0, rueInd: 0, so2CertInd: 0, noxCertInd: 0, noxcCertInd: 0),
                    new ProgramCodeRow(prgCd: "NBP", prgName: "NOx Budget Trading Program", osInd: 1, rueInd: 0, so2CertInd: 0, noxCertInd: 1, noxcCertInd: 1),
                    new ProgramCodeRow(prgCd: "NHNOX", prgName: "NH NOx Program", osInd: 1, rueInd: 0, so2CertInd: 0, noxCertInd: 1, noxcCertInd: 1),
                    new ProgramCodeRow(prgCd: "OTC", prgName: "OTC NOx Budget Program", osInd: 0, rueInd: 0, so2CertInd: 0, noxCertInd: 0, noxcCertInd: 0, notes: "OTC is not treated as a OS program in ECMPS."),
                    new ProgramCodeRow(prgCd: "RGGI", prgName: "Regional Greenhouse Gas Initiative", osInd: 0, rueInd: 0, so2CertInd: 0, noxCertInd: 0, noxcCertInd: 0),
                    new ProgramCodeRow(prgCd: "SIPNOX", prgName: "SIP NOx Program", osInd: 1, rueInd: 0, so2CertInd: 0, noxCertInd: 1, noxcCertInd: 1, notes: " SIPNOX is treated as a OS program in ECMPS")
                );

            /* Initialize Output Parameters */
            EmParameters.ProgramIsOzoneSeasonList = "Bad List";
            EmParameters.ProgramRequiresNoxSystemCertificationList = "Bad List";
            EmParameters.ProgramRequiresNoxcSystemCertificationList = "Bad List";
            EmParameters.ProgramRequiresSo2SystemCertificationList = "Bad List";
            EmParameters.ProgramUsesRueList = "Bad List";


            /* Init Cateogry Result */
            category.CheckCatalogResult = null;

            /* Initialize variables needed to run the check. */
            bool log = false;
            string actual;

            /* Run Check */
            actual = target.HOURGEN23(category, ref log);

            /* Check Results */
            Assert.AreEqual(string.Empty, actual, string.Format("actual"));
            Assert.AreEqual(false, log, string.Format("log"));
            Assert.AreEqual(null, category.CheckCatalogResult, string.Format("category.CheckCatalogResult"));

            Assert.AreEqual("CAIROS,CSNOXOS,CSOSG1,CSOSG2,NBP,NHNOX,SIPNOX", EmParameters.ProgramIsOzoneSeasonList, string.Format("ProgramIsOzoneSeasonList"));
            Assert.AreEqual("ARP,CAIRNOX,CAIROS,CSNOX,CSNOXOS,CSOSG1,CSOSG2,NBP,NHNOX,SIPNOX", EmParameters.ProgramRequiresNoxSystemCertificationList, string.Format("ProgramRequiresNoxSystemCertificationList"));
            Assert.AreEqual("CAIRNOX,CAIROS,CSNOX,CSNOXOS,CSOSG1,CSOSG2,NBP,NHNOX,SIPNOX", EmParameters.ProgramRequiresNoxcSystemCertificationList, string.Format("ProgramRequiresNoxcSystemCertificationList"));
            Assert.AreEqual("ARP,CAIRSO2,CSSO2G1,CSSO2G2", EmParameters.ProgramRequiresSo2SystemCertificationList, string.Format("ProgramRequiresSo2SystemCertificationList"));
            Assert.AreEqual("ARP,CAIRNOX,CAIROS,CAIRSO2,CSNOX,CSNOXOS,CSOSG1,CSOSG2,CSSO2G1,CSSO2G2", EmParameters.ProgramUsesRueList, string.Format("ProgramUsesRueList"));

        }


        [TestMethod]
        public void HourGen24()
        {
            /* Initialize objects generally needed for testing checks. */
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            EmParameters.Init(category.Process);
            EmParameters.Category = category;

            cHourlyGeneralChecks target = new cHourlyGeneralChecks(new cEmissionsCheckParameters());

            category.Process.ProcessParameters.RegisterParameter(3618, "Location_Name_Array"); // Currently cannot access arrays using the new check parameter access.


            /* Initialize Required Parameters */
            EmParameters.CurrentReportingPeriodBeginDate = new DateTime(2020, 4, 1);
            EmParameters.CurrentReportingPeriodEndDate = new DateTime(2020, 6, 30);
            EmParameters.EmUnitStackConfigurationRecords = new CheckDataView<VwMpUnitStackConfigurationRow>();
            EmParameters.HourlyEmissionsTolerancesCrossCheckTable = new CheckDataView<HourlyEmissionsTolerancesRow>();
            {
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "CARBON", uOM: "MW", tolerance: "0.1"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "CO2", uOM: "MW", tolerance: "0.1"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "CO2C", uOM: "MW", tolerance: "0.1"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "CO2M", uOM: "MW", tolerance: "0.1"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "CO2M DAILY", uOM: "MW", tolerance: "0.1"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "FLOW", uOM: "MW", tolerance: "1000"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "FOIL", uOM: "MW", tolerance: "0.1"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "H2O", uOM: "MW", tolerance: "0.1"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "HI", uOM: "MW", tolerance: "1"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "HI HPFF", uOM: "MW", tolerance: "0.1"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "HIT", uOM: "MW", tolerance: "1"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "LOAD", uOM: null, tolerance: "0.1"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "LOAD", uOM: "LB", tolerance: "0.1"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "LOAD", uOM: "LBHR", tolerance: "0.1"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "LOAD", uOM: "LBMMBTU", tolerance: "0.005"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "LOAD", uOM: "MMBTU", tolerance: "1"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "LOAD", uOM: "MMBTUHR", tolerance: "0.1"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "LOAD", uOM: "MW", tolerance: "13"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "LOAD", uOM: "PCT", tolerance: "0.1"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "LOAD", uOM: "PPM", tolerance: "0.1"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "LOAD", uOM: "SCFH", tolerance: "1000"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "LOAD", uOM: "TNHR", tolerance: "0.1"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "LOAD", uOM: "TON", tolerance: "0.1"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "NOX", uOM: "MW", tolerance: "0.1"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "NOXC", uOM: "MW", tolerance: "0.1"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "NOXM", uOM: "MW", tolerance: "0.1"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "NOXR", uOM: "MW", tolerance: "0.005"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "OILM", uOM: "MW", tolerance: "0.5"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "SO2", uOM: "MW", tolerance: "0.1"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "SO2 Gas HPFF", uOM: "MW", tolerance: "0.0001"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "SO2 Oil HPFF", uOM: "MW", tolerance: "0.1"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "SO2C", uOM: "MW", tolerance: "0.1"));
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable.Add(new HourlyEmissionsTolerancesRow(parameter: "SO2M", uOM: "MW", tolerance: "0.1"));
            }
            EmParameters.MonitoringPlanLocationRecords = new CheckDataView<VwMpMonitorLocationRow>();
            {
                EmParameters.MonitoringPlanLocationRecords.Add(new VwMpMonitorLocationRow(locationName: "CS12"));
                EmParameters.MonitoringPlanLocationRecords.Add(new VwMpMonitorLocationRow(locationName: "1"));
                EmParameters.MonitoringPlanLocationRecords.Add(new VwMpMonitorLocationRow(locationName: "2"));
                EmParameters.MonitoringPlanLocationRecords.Add(new VwMpMonitorLocationRow(locationName: "MS2"));
            }

            /* Initialize Output Parameters */
            category.SetCheckParameter("Location_Name_Array", new string[0]);
            EmParameters.MwLoadHourlyTolerance = null;


            /* Init Cateogry Result */
            category.CheckCatalogResult = null;

            /* Initialize variables needed to run the check. */
            bool log = false;
            string actual;

            /* Run Check */
            actual = target.HOURGEN24(category, ref log);


            /* Check Results */
            Assert.AreEqual(string.Empty, actual, string.Format("actual"));
            Assert.AreEqual(false, log, string.Format("log"));
            Assert.AreEqual(null, category.CheckCatalogResult, string.Format("category.CheckCatalogResult"));

            Assert.AreEqual(13, EmParameters.MwLoadHourlyTolerance, string.Format("MwLoadHourlyTolerance"));

            string[] lacationNameArray = (string[])category.GetCheckParameter("Location_Name_Array").ParameterValue;
            {
                Assert.AreNotEqual(null, lacationNameArray, string.Format("lacationNameArray"));
                Assert.AreEqual(4, lacationNameArray.Length, string.Format(" lacationNameArray.Length"));
                Assert.AreEqual("CS12", lacationNameArray[0], string.Format("lacationNameArray[0]"));
                Assert.AreEqual("1", lacationNameArray[1], string.Format("lacationNameArray[1]"));
                Assert.AreEqual("2", lacationNameArray[2], string.Format("lacationNameArray[2]"));
                Assert.AreEqual("MS2", lacationNameArray[3], string.Format("lacationNameArray[3]"));
            }
        }


        /// <summary>
        /// 
        /// CurrentReportingPeriodBeginDate : 2020-04-01
        /// CurrentReportingPeriodEndDate   : 2020-06-30
        /// 
        ///      |     Configuration 1     |     Configuration 2     ||
        /// | ## | BeginDate  | EndDate    | BeginDate  | EndDate    || ConfigChange || Notes
        /// |  0 | 2020-03-31 | 2020-07-01 | 2020-04-01 | 2020-06-30 || false        || First contains quarter, and second spans the quarter.
        /// |  1 | 2020-04-01 | 2020-06-30 | 2020-04-01 | 2020-06-30 || false        || Both span quarter.
        /// |  2 | 2020-04-01 | 2020-06-30 | 2020-04-01 | 2020-06-30 || false        || Both span quarter.
        /// |  3 | 2020-01-01 | 2020-03-31 | 2020-04-01 | 2020-06-30 || false        || First ends before quarter, and second spans the quarter.
        /// |  4 | 2020-01-01 | 2020-04-01 | 2020-04-01 | 2020-06-30 || true         || First begins before and ends on first day of the quarter, and second spans the quarter.
        /// |  5 | 2020-01-01 | 2020-06-29 | 2020-04-01 | 2020-06-30 || true         || First begins before and ends on day before end of the quarter, and second spans the quarter.
        /// |  6 | 2020-01-01 | 2020-06-30 | 2020-04-01 | 2020-06-30 || false        || First begins before and ends on last day of the quarter, and second spans the quarter.
        /// |  7 | 2020-01-01 | 2020-09-30 | 2020-04-01 | 2020-06-30 || false        || First begins before and ends after the quarter, and second spans the quarter.
        /// |  8 | 2020-04-01 | 2020-09-30 | 2020-04-01 | 2020-06-30 || false        || First begins on the first day and ends after the quarter, and second spans the quarter.
        /// |  9 | 2020-04-02 | 2020-09-30 | 2020-04-01 | 2020-06-30 || true         || First begins on the day after then first day and ends after the quarter, and second spans the quarter.
        /// | 10 | 2020-04-02 | 2020-09-30 | 2020-04-01 | 2020-06-30 || true         || First begins after the quarter, and second spans the quarter.
        /// | 11 | 2020-04-01 | 2020-06-30 | 2020-04-02 | 2020-06-30 || true         || First spans quarter, and second begins day after begin of quarter.
        /// | 12 | 2020-04-01 | 2020-06-30 | 2020-04-01 | 2020-06-29 || true         || First spans quarter, and second ends day before end of quarter.
        /// | 13 | 2020-04-01 | 2020-06-30 | 2020-04-01 | null       || false        || Both span quarter.
        /// </summary>
        [TestMethod]
        public void HourGen24_ConfigChange()
        {
            /* Initialize objects generally needed for testing checks. */
            cEmissionsCheckParameters emCheckParameters = UnitTestCheckParameters.InstantiateEmParameters(); // Old Instantiated Parameters Used
            cHourlyGeneralChecks target = new cHourlyGeneralChecks(emCheckParameters);
            cCategory category = EmParameters.Category;


            /* Shared Values */
            DateTime d0101 = new DateTime(2020, 01, 01);
            DateTime d0331 = new DateTime(2020, 03, 31);
            DateTime d0401 = new DateTime(2020, 04, 01);
            DateTime d0402 = new DateTime(2020, 04, 02);
            DateTime d0629 = new DateTime(2020, 06, 29);
            DateTime d0630 = new DateTime(2020, 06, 30);
            DateTime d0701 = new DateTime(2020, 07, 01);
            DateTime d0930 = new DateTime(2020, 09, 30);

            /* Parameter Value Lists */
            DateTime?[] config1BegList = new DateTime?[] { d0331, d0401, d0401, d0101, d0101, d0101, d0101, d0101, d0401, d0402, d0402, d0401, d0401, d0401 };
            DateTime?[] config1EndList = new DateTime?[] { d0701, d0630, d0630, d0331, d0401, d0629, d0630, d0930, d0930, d0930, d0930, d0630, d0630, d0630 };
            DateTime?[] config2BegList = new DateTime?[] { d0401, d0401, d0401, d0401, d0401, d0401, d0401, d0401, d0401, d0401, d0401, d0402, d0401, d0401 };
            DateTime?[] config2EndList = new DateTime?[] { d0630, d0630, d0630, d0630, d0630, d0630, d0630, d0630, d0630, d0630, d0630, d0630, d0629, null  };

            bool?[] expConfigChangedList = new bool?[] { false, false, false, false, true, true, false, false, false, true, true, true, true, false };


            /* Test Case Count */
            int caseCount = 14;

            /* Check array lengths */
            Assert.AreEqual(caseCount, config1BegList.Length, "config1BegList length");
            Assert.AreEqual(caseCount, config1EndList.Length, "config1EndList length");
            Assert.AreEqual(caseCount, config2BegList.Length, "config2BegList length");
            Assert.AreEqual(caseCount, config2EndList.Length, "config2EndList length");
            Assert.AreEqual(caseCount, expConfigChangedList.Length, "expConfigChangedList length");


            /* Currently cannot access arrays using the new check parameter access. */
            category.Process.ProcessParameters.RegisterParameter(3618, "Location_Name_Array");


            /* Run Cases */
            for (int caseDex = 0; caseDex < caseCount; caseDex++)
            {
                /*  Initialize Input Parameters*/
                EmParameters.CurrentReportingPeriodBeginDate = d0401;
                EmParameters.CurrentReportingPeriodEndDate = d0630;
                EmParameters.EmUnitStackConfigurationRecords = new CheckDataView<VwMpUnitStackConfigurationRow>
                    (
                        new VwMpUnitStackConfigurationRow(beginDate: config1BegList[caseDex], endDate: config1EndList[caseDex]),
                        new VwMpUnitStackConfigurationRow(beginDate: config2BegList[caseDex], endDate: config2EndList[caseDex])
                    );
                EmParameters.HourlyEmissionsTolerancesCrossCheckTable = new CheckDataView<HourlyEmissionsTolerancesRow>
                    (
                        new HourlyEmissionsTolerancesRow(parameter: "LOAD", uOM: "MW", tolerance: "13")
                    );
                EmParameters.MonitoringPlanLocationRecords = new CheckDataView<VwMpMonitorLocationRow>();


                /*  Initialize Output Parameters*/
                EmParameters.ConfigurationChangeOccuredDurringQuarter = null;
                category.SetCheckParameter("Location_Name_Array", new string[0]);
                EmParameters.MwLoadHourlyTolerance = null;


                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                /* Initialize variables needed to run the check. */
                bool log = false;
                string actual;


                /* Run Check */
                actual = target.HOURGEN24(category, ref log);


                /* Check Results */
                Assert.AreEqual(string.Empty, actual, string.Format("actual {0}", caseDex));
                Assert.AreEqual(false, log, string.Format("log {0}", caseDex));
                
                Assert.AreEqual(expConfigChangedList[caseDex], EmParameters.ConfigurationChangeOccuredDurringQuarter, $"ConfigurationChangeOccuredDurringQuarter {caseDex}");
                Assert.AreEqual(0, ((string[])category.GetCheckParameter("Location_Name_Array").ParameterValue).Length, $"Location_Name_Array {caseDex}");
                Assert.AreEqual(13, EmParameters.MwLoadHourlyTolerance, $"MwLoadHourlyTolerance {caseDex}");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        [TestMethod()]
        public void HourGen25()
        {
            /* Initialize objects generally needed for testing checks. */
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            EmParameters.Init(category.Process);
            EmParameters.Category = category;

            cHourlyGeneralChecks target = new cHourlyGeneralChecks(new cEmissionsCheckParameters());


            /* InvalidCylinderIdList is null */
            {
                /* Initialize Input Parameters */
                EmParameters.InvalidCylinderIdList = null;


                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                /* Initialize variables needed to run the check. */
                bool log = false;
                string actual;

                /* Run Check */
                actual = target.HOURGEN25(category, ref log);


                /* Check Results */
                Assert.AreEqual(string.Empty, actual, string.Format("actual [{0}]", "NullList"));
                Assert.AreEqual(false, log, string.Format("log [{0}]", "EmptyList"));
                Assert.AreEqual(null, category.CheckCatalogResult, string.Format("category.CheckCatalogResult [{0}]", "NullList"));

                Assert.AreEqual("", EmParameters.FormattedCylinderIdList, string.Format("InvalidCylinderIdList [{0}]", "NullList"));
            }


            /* InvalidCylinderIdList contains no items */
            {
                /* Initialize Input Parameters */
                EmParameters.InvalidCylinderIdList = new List<string>();


                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                /* Initialize variables needed to run the check. */
                bool log = false;
                string actual;

                /* Run Check */
                actual = target.HOURGEN25(category, ref log);


                /* Check Results */
                Assert.AreEqual(string.Empty, actual, string.Format("actual [{0}]", "EmptyList"));
                Assert.AreEqual(false, log, string.Format("log [{0}]", "EmptyList"));
                Assert.AreEqual(null, category.CheckCatalogResult, string.Format("category.CheckCatalogResult [{0}]", "EmptyList"));

                Assert.AreEqual("", EmParameters.FormattedCylinderIdList, string.Format("InvalidCylinderIdList [{0}]", "EmptyList"));
            }


            /* InvalidCylinderIdList contains items not entered in alphabetical order */
            {
                /* Initialize Input Parameters */
                EmParameters.InvalidCylinderIdList = new List<string> { "OldId", "NewId", "AbcId" };


                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                /* Initialize variables needed to run the check. */
                bool log = false;
                string actual;

                /* Run Check */
                actual = target.HOURGEN25(category, ref log);


                /* Check Results */
                Assert.AreEqual(string.Empty, actual, string.Format("actual [{0}]", "UnsortedList"));
                Assert.AreEqual(false, log, string.Format("log [{0}]", "UnsortedList"));
                Assert.AreEqual("A", category.CheckCatalogResult, string.Format("category.CheckCatalogResult [{0}]", "UnsortedList"));

                Assert.AreEqual("AbcId, NewId, and OldId", EmParameters.FormattedCylinderIdList, string.Format("InvalidCylinderIdList [{0}]", "UnsortedList"));
            }
        }


        /// <summary>
        /// 
        ///      |     Location 0     |     Location 1     |     Location 2     ||
        /// | ## | MonLocId | SdCount | MonLocId | SdCount | MonLocId | SdCount || ExistArray          || Note
        /// |  0 | LocKey0  |       0 | LocKey1  |       0 | LocKey2  |       0 || [false,false,false] || No Linearities
        /// |  1 | LocKey0  |       1 | LocKey1  |       0 | LocKey2  |       0 || [true,false,false]  || Location 0 has a linearity.
        /// |  2 | LocKey0  |       0 | LocKey1  |       1 | LocKey2  |       0 || [false,true,false]  || Location 1 has a linearity.
        /// |  3 | LocKey0  |       0 | LocKey1  |       2 | LocKey2  |       0 || [false,true,false]  || Location 1 has linearities.
        /// |  4 | LocKey0  |       0 | LocKey1  |       0 | LocKey2  |       1 || [false,false,true]  || Location 2 has a linearity.
        /// |  5 | LocKey0  |       1 | LocKey1  |       1 | LocKey2  |       1 || [true,true,true]    || All Locations have a linearity.
        /// |  6 | LocKey0  |       1 | LocKey1  |       0 | (null)   |       0 || [true,false]        || Two locations, Location 0 has a linearity.
        /// |  7 | LocKey0  |       0 | (null)   |       0 | (null)   |       0 || [false]             || One location without Linearities.
        /// </summary>
        [TestMethod]
        public void HourGen31()
        {
            /* Initialize objects generally needed for testing checks. */
            cEmissionsCheckParameters emCheckParameters = UnitTestCheckParameters.InstantiateEmParameters(); // Old Instantiated Parameters Used
            cHourlyGeneralChecks target = new cHourlyGeneralChecks(emCheckParameters);
            cCategory category = EmParameters.Category;


            /* Input Parameter Values */
            string[][] locIdList = new string[][] {
                                                    new string[] { "LocKey0", "LocKey0", "LocKey0", "LocKey0", "LocKey0", "LocKey0", "LocKey0", "LocKey0" },
                                                    new string[] { "LocKey1", "LocKey1", "LocKey1", "LocKey1", "LocKey1", "LocKey1", "LocKey1", null },
                                                    new string[] { "LocKey2", "LocKey2", "LocKey2", "LocKey2", "LocKey2", "LocKey2", null, null }
                                                  };

            int[][] sdCntList = new int[][] {
                                                new int[] { 0, 1, 0, 0, 0, 1, 1, 0 },
                                                new int[] { 0, 0, 1, 2, 0, 1, 0, 0 },
                                                new int[] { 0, 0, 0, 0, 1, 1, 0, 0 }
                                            };

            /* Expected Values */
            bool[][] expExistArrayList = { new bool[] { false, false, false },
                                           new bool[] { true, false, false },
                                           new bool[] { false, true, false },
                                           new bool[] { false, true, false },
                                           new bool[] { false, false, true },
                                           new bool[] { true, true, true },
                                           new bool[] { true, false },
                                           new bool[] { false } };


            /* Test Case Count */
            int caseCount = 8;

            /* Check array lengths */
            Assert.AreEqual(3, locIdList.Length, "locIdList length");
            Assert.AreEqual(3, sdCntList.Length, "locSdCntList length");
            Assert.AreEqual(caseCount, locIdList[0].Length, "locIdList[0] length");
            Assert.AreEqual(caseCount, sdCntList[0].Length, "ocSdCntList[0] length");
            Assert.AreEqual(caseCount, locIdList[1].Length, "locIdList[1] length");
            Assert.AreEqual(caseCount, sdCntList[1].Length, "ocSdCntList[1] length");
            Assert.AreEqual(caseCount, locIdList[2].Length, "locIdList[2] length");
            Assert.AreEqual(caseCount, sdCntList[2].Length, "ocSdCntList[2] length");
            Assert.AreEqual(caseCount, expExistArrayList.Length, "expExistArrayList length");


            /* Run Cases */
            for (int caseDex = 0; caseDex < caseCount; caseDex++)
            {
                /*  Initialize Input Parameters*/
                EmParameters.LinearityTestRecordsByLocationForQaStatus = new CheckDataView<VwQaSuppDataHourlyStatusRow>();
                EmParameters.MonitoringPlanLocationRecords = new CheckDataView<VwMpMonitorLocationRow>();

                for (int locationDex = 0; locationDex < 3; locationDex++)
                {
                    if (locIdList[locationDex][caseDex] != null)
                    {
                        EmParameters.MonitoringPlanLocationRecords.Add(new VwMpMonitorLocationRow(monLocId: locIdList[locationDex][caseDex]));

                        for (int dex = 0; dex < sdCntList[locationDex][caseDex]; dex++)
                        {
                            EmParameters.LinearityTestRecordsByLocationForQaStatus.Add(new VwQaSuppDataHourlyStatusRow(monLocId: locIdList[locationDex][caseDex]));
                        }
                    }
                }


                /*  Initialize Output Parameters*/
                EmParameters.LinearityExistsLocationArray = null;


                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                /* Initialize variables needed to run the check. */
                bool log = false;
                string actual;

                /* Run Check */
                actual = target.HOURGEN31(category, ref log);

                /* Check Results */
                Assert.AreEqual(string.Empty, actual, string.Format("actual {0}", caseDex));
                Assert.AreEqual(false, log, string.Format("log {0}", caseDex));

                Assert.AreEqual(expExistArrayList[caseDex].Length, EmParameters.LinearityExistsLocationArray.Length, string.Format("LinearityExistsLocationArray.Length {0}", caseDex));

                for (int dex = 0; dex < expExistArrayList[caseDex].Length; dex++)
                {
                    Assert.AreEqual(expExistArrayList[caseDex][dex], EmParameters.LinearityExistsLocationArray[dex], string.Format("LinearityExistsLocationArray[{1}] {0}", caseDex, dex));
                }
            }

        }


        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void HourGen32()
        {
            /* Initialize objects generally needed for testing checks. */
            cEmissionsCheckParameters emCheckParameters = UnitTestCheckParameters.InstantiateEmParameters(); // Old Instantiated Parameters Used
            cHourlyGeneralChecks target = new cHourlyGeneralChecks(emCheckParameters);
            cCategory category = EmParameters.Category;


            /* Static Values */
            DateTime opHour = new DateTime(2020, 6, 17, 22, 0, 0);
            int annualLimit = 1752;
            int osLimit = 734;
            decimal? percentAvailable = 80m;
            int locationPostion;
            string delim, expResult, problemDerivedList, problemMonitorList;

            /* Case Loop */
            foreach (bool? annualReportingRequirement in UnitTestStandardLists.ValidList)
                foreach (bool? osReportingRequirement in UnitTestStandardLists.ValidList)
                {
                    EmParameters.AnnualReportingRequirement = annualReportingRequirement;
                    EmParameters.OsReportingRequirement = osReportingRequirement;

                    int countLimit = ((annualReportingRequirement != true) && (osReportingRequirement == true) ? osLimit : annualLimit) + 5;
                    string reporterType = (annualReportingRequirement != true) && (osReportingRequirement == true) ? "ozone season" : "year";

                    foreach (MissingDataPmaTracking.eHourlyParameter hourlyParameter in Enum.GetValues(typeof(MissingDataPmaTracking.eHourlyParameter)))
                    {
                        EmParameters.MissingDataPmaTracking = new MissingDataPmaTracking(4);

                        // Set all to pass
                        EmParameters.MissingDataPmaTracking.TestInit(1, MissingDataPmaTracking.eHourlyParameter.DerivedCo2c, countLimit, opHour, percentAvailable, opHour);
                        EmParameters.MissingDataPmaTracking.TestInit(1, MissingDataPmaTracking.eHourlyParameter.DerivedH2o, countLimit, opHour, percentAvailable, opHour);
                        EmParameters.MissingDataPmaTracking.TestInit(1, MissingDataPmaTracking.eHourlyParameter.DerivedNoxr, countLimit, opHour, percentAvailable, opHour);
                        EmParameters.MissingDataPmaTracking.TestInit(1, MissingDataPmaTracking.eHourlyParameter.MonitorCo2c, countLimit, opHour, percentAvailable, opHour);
                        EmParameters.MissingDataPmaTracking.TestInit(1, MissingDataPmaTracking.eHourlyParameter.MonitorFlow, countLimit, opHour, percentAvailable, opHour);
                        EmParameters.MissingDataPmaTracking.TestInit(1, MissingDataPmaTracking.eHourlyParameter.MonitorH2o, countLimit, opHour, percentAvailable, opHour);
                        EmParameters.MissingDataPmaTracking.TestInit(1, MissingDataPmaTracking.eHourlyParameter.MonitorNoxc, countLimit, opHour, percentAvailable, opHour);
                        EmParameters.MissingDataPmaTracking.TestInit(1, MissingDataPmaTracking.eHourlyParameter.MonitorO2d, countLimit, opHour, percentAvailable, opHour);
                        EmParameters.MissingDataPmaTracking.TestInit(1, MissingDataPmaTracking.eHourlyParameter.MonitorO2w, countLimit, opHour, percentAvailable, opHour);
                        EmParameters.MissingDataPmaTracking.TestInit(1, MissingDataPmaTracking.eHourlyParameter.MonitorSo2c, countLimit, opHour, percentAvailable, opHour);

                        // Set all to fail
                        EmParameters.MissingDataPmaTracking.TestInit(2, MissingDataPmaTracking.eHourlyParameter.DerivedCo2c, countLimit + 1, opHour, percentAvailable, opHour);
                        EmParameters.MissingDataPmaTracking.TestInit(2, MissingDataPmaTracking.eHourlyParameter.DerivedH2o, countLimit + 1, opHour, percentAvailable, opHour);
                        EmParameters.MissingDataPmaTracking.TestInit(2, MissingDataPmaTracking.eHourlyParameter.DerivedNoxr, countLimit + 1, opHour, percentAvailable, opHour);
                        EmParameters.MissingDataPmaTracking.TestInit(2, MissingDataPmaTracking.eHourlyParameter.MonitorCo2c, countLimit + 1, opHour, percentAvailable, opHour);
                        EmParameters.MissingDataPmaTracking.TestInit(2, MissingDataPmaTracking.eHourlyParameter.MonitorFlow, countLimit + 1, opHour, percentAvailable, opHour);
                        EmParameters.MissingDataPmaTracking.TestInit(2, MissingDataPmaTracking.eHourlyParameter.MonitorH2o, countLimit + 1, opHour, percentAvailable, opHour);
                        EmParameters.MissingDataPmaTracking.TestInit(2, MissingDataPmaTracking.eHourlyParameter.MonitorNoxc, countLimit + 1, opHour, percentAvailable, opHour);
                        EmParameters.MissingDataPmaTracking.TestInit(2, MissingDataPmaTracking.eHourlyParameter.MonitorO2d, countLimit + 1, opHour, percentAvailable, opHour);
                        EmParameters.MissingDataPmaTracking.TestInit(2, MissingDataPmaTracking.eHourlyParameter.MonitorO2w, countLimit + 1, opHour, percentAvailable, opHour);
                        EmParameters.MissingDataPmaTracking.TestInit(2, MissingDataPmaTracking.eHourlyParameter.MonitorSo2c, countLimit + 1, opHour, percentAvailable, opHour);

                        // Set Individual Hourly Parameter
                        EmParameters.MissingDataPmaTracking.TestInit(0, hourlyParameter, 0, opHour, percentAvailable, opHour);              // Nerver fails because count is zero.
                        EmParameters.MissingDataPmaTracking.TestInit(1, hourlyParameter, countLimit + 1, opHour, percentAvailable, opHour); // Fails because count is one over limit (max+ 5).
                        EmParameters.MissingDataPmaTracking.TestInit(2, hourlyParameter, countLimit, opHour, percentAvailable, opHour);     // Passes because count is at limit (max+ 5).
                        EmParameters.MissingDataPmaTracking.TestInit(3, hourlyParameter, annualLimit + 5 + 1, opHour, null, opHour);        // Passes becaise PMA is null


                        /* Check Location 0 */
                        {
                            locationPostion = 0;

                            // Expected Values
                            expResult = null;
                            problemDerivedList = null;
                            problemMonitorList = null;

                            // Initialize Input Parameters
                            EmParameters.CurrentMonitorPlanLocationPostion = locationPostion;

                            // Initialize Output Parameters
                            EmParameters.MissingDataPmaProblemDerivedList = "Bad List";
                            EmParameters.MissingDataPmaProblemMonitorList = "Bad List";
                            EmParameters.MissingDataPmaReporterType = "Bad";

                            // Init Cateogry Result
                            category.CheckCatalogResult = null;

                            // Initialize variables needed to run the check.
                            bool log = false;
                            string actual;

                            // Run Check
                            actual = target.HOURGEN32(category, ref log);

                            // Check Results
                            Assert.AreEqual(string.Empty, actual, $"actual {locationPostion}  Param: {hourlyParameter}, Annual: {annualReportingRequirement} OS: {osReportingRequirement}");
                            Assert.AreEqual(false, log, $"log {locationPostion}  Param: {hourlyParameter}, Annual: {annualReportingRequirement} OS: {osReportingRequirement}");
                            Assert.AreEqual(expResult, category.CheckCatalogResult, $"Result {locationPostion}  Param: {hourlyParameter}, Annual: {annualReportingRequirement} OS: {osReportingRequirement}");
                            Assert.AreEqual(problemDerivedList, EmParameters.MissingDataPmaProblemDerivedList, $"MissingDataPmaProblemDerivedList {locationPostion}  Param: {hourlyParameter}, Annual: {annualReportingRequirement} OS: {osReportingRequirement}");
                            Assert.AreEqual(problemMonitorList, EmParameters.MissingDataPmaProblemMonitorList, $"MissingDataPmaProblemMonitorList {locationPostion}  Param: {hourlyParameter}, Annual: {annualReportingRequirement} OS: {osReportingRequirement}");
                            Assert.AreEqual(reporterType, EmParameters.MissingDataPmaReporterType, $"MissingDataPmaReporterType {locationPostion}  Param: {hourlyParameter}, Annual: {annualReportingRequirement} OS: {osReportingRequirement}");
                        }


                        /* Check Location 1 */
                        {
                            locationPostion = 1;

                            // Expected Values
                            problemDerivedList = hourlyParameter.ToString().StartsWith("Derived") ? MissingDataPmaTracking.GetHourlyParameterCd(hourlyParameter) : null;
                            problemMonitorList = hourlyParameter.ToString().StartsWith("Monitor") ? MissingDataPmaTracking.GetHourlyParameterCd(hourlyParameter) : null;

                            if ((problemDerivedList != null) && (problemMonitorList != null))
                                expResult = "A";
                            else if (problemDerivedList != null)
                                expResult = "B";
                            else if (problemMonitorList != null)
                                expResult = "C";
                            else
                                expResult = null;


                            // Initialize Input Parameters
                            EmParameters.CurrentMonitorPlanLocationPostion = locationPostion;

                            // Initialize Output Parameters
                            EmParameters.MissingDataPmaProblemDerivedList = "Bad List";
                            EmParameters.MissingDataPmaProblemMonitorList = "Bad List";
                            EmParameters.MissingDataPmaReporterType = "Bad";

                            // Init Cateogry Result
                            category.CheckCatalogResult = null;

                            // Initialize variables needed to run the check.
                            bool log = false;
                            string actual;

                            // Run Check
                            actual = target.HOURGEN32(category, ref log);

                            // Check Results
                            Assert.AreEqual(string.Empty, actual, $"actual {locationPostion}  Param: {hourlyParameter}, Annual: {annualReportingRequirement} OS: {osReportingRequirement}");
                            Assert.AreEqual(false, log, $"log {locationPostion}  Param: {hourlyParameter}, Annual: {annualReportingRequirement} OS: {osReportingRequirement}");
                            Assert.AreEqual(expResult, category.CheckCatalogResult, $"Result {locationPostion}  Param: {hourlyParameter}, Annual: {annualReportingRequirement} OS: {osReportingRequirement}");
                            Assert.AreEqual(problemDerivedList, EmParameters.MissingDataPmaProblemDerivedList, $"MissingDataPmaProblemDerivedList {locationPostion}  Param: {hourlyParameter}, Annual: {annualReportingRequirement} OS: {osReportingRequirement}");
                            Assert.AreEqual(problemMonitorList, EmParameters.MissingDataPmaProblemMonitorList, $"MissingDataPmaProblemMonitorList {locationPostion}  Param: {hourlyParameter}, Annual: {annualReportingRequirement} OS: {osReportingRequirement}");
                            Assert.AreEqual(reporterType, EmParameters.MissingDataPmaReporterType, $"MissingDataPmaReporterType {locationPostion}  Param: {hourlyParameter}, Annual: {annualReportingRequirement} OS: {osReportingRequirement}");
                        }


                        /* Check Location 2 */
                        {
                            locationPostion = 2;

                            // Expected Values
                            problemDerivedList = null;
                            problemMonitorList = null;

                            delim = ",";

                            foreach (MissingDataPmaTracking.eHourlyParameter tempParameter in Enum.GetValues(typeof(MissingDataPmaTracking.eHourlyParameter)))
                                if (tempParameter != hourlyParameter)
                                {
                                    if (MissingDataPmaTracking.IsDerived(tempParameter))
                                    {
                                        problemDerivedList = string.IsNullOrWhiteSpace(problemDerivedList) ? "" : problemDerivedList + delim;
                                        problemDerivedList += MissingDataPmaTracking.GetHourlyParameterCd(tempParameter);
                                    }

                                    if (MissingDataPmaTracking.IsMonitored(tempParameter))
                                    {
                                        problemMonitorList = string.IsNullOrWhiteSpace(problemMonitorList) ? "" : problemMonitorList + delim;
                                        problemMonitorList += MissingDataPmaTracking.GetHourlyParameterCd(tempParameter);
                                    }
                                }

                            if ((problemDerivedList != null) && (problemMonitorList != null))
                                expResult = "A";
                            else if (problemDerivedList != null)
                                expResult = "B";
                            else if (problemMonitorList != null)
                                expResult = "C";
                            else
                                expResult = null;

                            problemDerivedList = ECMPS.Definitions.Extensions.cExtensions.FormatList(problemDerivedList);
                            problemMonitorList = ECMPS.Definitions.Extensions.cExtensions.FormatList(problemMonitorList);


                            // Initialize Input Parameters
                            EmParameters.CurrentMonitorPlanLocationPostion = locationPostion;

                            // Initialize Output Parameters
                            EmParameters.MissingDataPmaProblemDerivedList = "Bad List";
                            EmParameters.MissingDataPmaProblemMonitorList = "Bad List";
                            EmParameters.MissingDataPmaReporterType = "Bad";

                            // Init Cateogry Result
                            category.CheckCatalogResult = null;

                            // Initialize variables needed to run the check.
                            bool log = false;
                            string actual;

                            // Run Check
                            actual = target.HOURGEN32(category, ref log);

                            // Check Results
                            Assert.AreEqual(string.Empty, actual, $"actual {locationPostion}  Param: {hourlyParameter}, Annual: {annualReportingRequirement} OS: {osReportingRequirement}");
                            Assert.AreEqual(false, log, $"log {locationPostion}  Param: {hourlyParameter}, Annual: {annualReportingRequirement} OS: {osReportingRequirement}");
                            Assert.AreEqual(expResult, category.CheckCatalogResult, $"Result {locationPostion}  Param: {hourlyParameter}, Annual: {annualReportingRequirement} OS: {osReportingRequirement}");
                            Assert.AreEqual(problemDerivedList, EmParameters.MissingDataPmaProblemDerivedList, $"MissingDataPmaProblemDerivedList {locationPostion}  Param: {hourlyParameter}, Annual: {annualReportingRequirement} OS: {osReportingRequirement}");
                            Assert.AreEqual(problemMonitorList, EmParameters.MissingDataPmaProblemMonitorList, $"MissingDataPmaProblemMonitorList {locationPostion}  Param: {hourlyParameter}, Annual: {annualReportingRequirement} OS: {osReportingRequirement}");
                            Assert.AreEqual(reporterType, EmParameters.MissingDataPmaReporterType, $"MissingDataPmaReporterType {locationPostion}  Param: {hourlyParameter}, Annual: {annualReportingRequirement} OS: {osReportingRequirement}");
                        }


                        /* Check Location 3 */
                        {
                            locationPostion = 3;

                            // Expected Values
                            expResult = null;
                            problemDerivedList = null;
                            problemMonitorList = null;

                            // Initialize Input Parameters
                            EmParameters.CurrentMonitorPlanLocationPostion = locationPostion;

                            // Initialize Output Parameters
                            EmParameters.MissingDataPmaProblemDerivedList = "Bad List";
                            EmParameters.MissingDataPmaProblemMonitorList = "Bad List";
                            EmParameters.MissingDataPmaReporterType = "Bad";

                            // Init Cateogry Result
                            category.CheckCatalogResult = null;

                            // Initialize variables needed to run the check.
                            bool log = false;
                            string actual;

                            // Run Check
                            actual = target.HOURGEN32(category, ref log);

                            // Check Results
                            Assert.AreEqual(string.Empty, actual, $"actual {locationPostion}  Param: {hourlyParameter}, Annual: {annualReportingRequirement} OS: {osReportingRequirement}");
                            Assert.AreEqual(false, log, $"log {locationPostion}  Param: {hourlyParameter}, Annual: {annualReportingRequirement} OS: {osReportingRequirement}");
                            Assert.AreEqual(expResult, category.CheckCatalogResult, $"Result {locationPostion}  Param: {hourlyParameter}, Annual: {annualReportingRequirement} OS: {osReportingRequirement}");
                            Assert.AreEqual(problemDerivedList, EmParameters.MissingDataPmaProblemDerivedList, $"MissingDataPmaProblemDerivedList {locationPostion}  Param: {hourlyParameter}, Annual: {annualReportingRequirement} OS: {osReportingRequirement}");
                            Assert.AreEqual(problemMonitorList, EmParameters.MissingDataPmaProblemMonitorList, $"MissingDataPmaProblemMonitorList {locationPostion}  Param: {hourlyParameter}, Annual: {annualReportingRequirement} OS: {osReportingRequirement}");
                            Assert.AreEqual(reporterType, EmParameters.MissingDataPmaReporterType, $"MissingDataPmaReporterType {locationPostion}  Param: {hourlyParameter}, Annual: {annualReportingRequirement} OS: {osReportingRequirement}");
                        }
                    }
                }
        }
    }
}
