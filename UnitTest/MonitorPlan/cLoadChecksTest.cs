using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using ECMPS.Checks.CheckEngine;
using ECMPS.Checks.MonitorPlan;
using ECMPS.Checks.LoadChecks;

using ECMPS.Definitions.Extensions;

using ECMPS.Checks;
using ECMPS.Checks.Data.Ecmps.Dbo.Table;
using ECMPS.Checks.Data.Ecmps.Dbo.View;
using ECMPS.Checks.Data.EcmpsAux.CrossCheck.Virtual;
using ECMPS.Checks.Data.Ecmps.CrossCheck.Table;
using ECMPS.Checks.Mp.Parameters;

using UnitTest.UtilityClasses;

namespace UnitTest.MonitorPlan
{
    /// <summary>
    /// Summary description for cLoadChecksTest
    /// </summary>
    [TestClass]
    public class cLoadChecksTest
    {
        public cLoadChecksTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            //populates Reporting Period Table for checks without making db call
            UnitTest.UtilityClasses.UnitTestExtensions.SetReportingPeriodTable();
        }

        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion


        /// <summary>
        /// 
        /// Now : Date and Hour of Current Time
        /// 
        /// DefBeg : Now.Hour - 730 days
        /// DefCco : Now.Date - 14600 days
        /// DefEnd : Now.Hour - 365 days
        /// 
        /// RuleDt : 1/1/1993
        /// MillDt : 1/1/2001
        /// 
        /// FirstCco : 
        /// LastCcl  : 
        /// 
        /// 
        /// Method | Code | BegHr |
        ///      0 | LME  | CcoDt |
        ///      2 | LTFF | CcoDt |
        ///      3 | MHHI | CcoDt |
        /// 
        ///              |          CurrentLoad          |        Method(1)        | OtherEndHr |
        /// | ## | Req   | AnlDt    | BegHr     | EndHr  | Code | BegHr  | EndHr   | DefBeg-1   | CcoDt  || Result || Note
        /// |  0 | false | null     | DefBeg    | null   | CEM  | DefBeg | null    | DefBeg-1   | DefCCo || null   || LoadLevelsRequired is false and LoadAnalysisDate is null.
        /// |  1 | false | Now      | DefBeg    | null   | CEM  | DefBeg | null    | DefBeg-1   | DefCCo || A      || LoadLevelsRequired is false and LoadAnalysisDate is not null.
        /// |  2 | false | DefBeg   | DefBeg    | null   | CEM  | DefBeg | null    | DefBeg-1   | DefCCo || A      || LoadLevelsRequired is false and LoadAnalysisDate is not null.
        /// |  3 | false | DefBeg   | DefBeg    | null   | AD   | DefBeg | null    | DefBeg-1   | DefCCo || A      || LoadLevelsRequired is false and LoadAnalysisDate is not null.
        /// |  4 | false | DefBeg   | DefBeg    | null   | AE   | DefBeg | null    | DefBeg-1   | DefCCo || A      || LoadLevelsRequired is false and LoadAnalysisDate is not null.
        /// |  5 | false | DefBeg   | DefBeg    | null   | CEM  | DefBeg | DefEnd  | DefBeg-1   | DefCCo || A      || LoadLevelsRequired is false and LoadAnalysisDate is not null.
        /// |  6 | false | DefBeg   | DefBeg    | null   | LME  | DefBeg | null    | DefBeg-1   | DefCCo || F      || LoadLevelsRequired is false, LoadAnalysisDate is not null, and LME location.
        /// |  7 | false | DefBeg   | DefBeg    | null   | LTFF | DefBeg | null    | DefBeg-1   | DefCCo || F      || LoadLevelsRequired is false, LoadAnalysisDate is not null, and LME location.
        /// |  8 | false | DefBeg   | DefBeg    | null   | MHHI | DefBeg | null    | DefBeg-1   | DefCCo || F      || LoadLevelsRequired is false, LoadAnalysisDate is not null, and LME location.
        /// |  9 | false | DefBeg   | DefBeg    | null   | LME  | DefBeg | DefEnd  | DefBeg-1   | DefCCo || F      || LoadLevelsRequired is false, LoadAnalysisDate is not null, and LME location.
        /// | 10 | false | DefBeg   | DefBeg    | null   | LME  | CcoDt  | DefBeg  | DefBeg-1   | DefCCo || F      || LoadLevelsRequired is false, LoadAnalysisDate is not null, and LME location.
        /// | 11 | false | DefBeg   | DefBeg    | DefEnd | LME  | DefEnd | null    | DefBeg-1   | DefCCo || F      || LoadLevelsRequired is false, LoadAnalysisDate is not null, and LME location.
        /// | 12 | true  | null     | DefBeg    | null   | CEM  | DefBeg | null    | DefBeg-1   | DefCCo || B      || LoadLevelsRequired is true and LoadAnalysisDate is null.
        /// | 13 | true  | RuleDt-1 | RuleDt-24 | null   | CEM  | DefBeg | null    | RuleDt-25  | DefCCo || C      || LoadLevelsRequired is true and LoadAnalysisDate is before 1/1/1993.
        /// | 14 | true  | RuleDt   | RuleDt-24 | null   | CEM  | DefBeg | null    | RuleDt-25  | DefCCo || null   || LoadLevelsRequired is true and LoadAnalysisDate is on 1/1/1993.
        /// | 15 | true  | MillDt+1 | MillDt    | null   | CEM  | DefBeg | null    | MillDt-1   | DefCCo || D      || LoadLevelsRequired is true, LoadAnalysisDate is after BeginDate, and BeginDate is 1/1/2001.
        /// | 16 | true  | MillDt+1 | MillDt-24 | null   | CEM  | DefBeg | null    | MillDt-25  | DefCCo || null   || LoadLevelsRequired is true, LoadAnalysisDate is after BeginDate, and BeginDate is 12/31/2000.
        /// </summary>
        [TestMethod()]
        public void Load1()
        {
            /* Initialize objects generally needed for testing checks. */
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            MpParameters.Init(category.Process);
            MpParameters.Category = category;


            /* Initialize General Variables */
            DateTime nowHr = DateTime.Now.Date.AddHours(DateTime.Now.Hour);
            DateTime defCcoDt = nowHr.Date.AddDays(-14600);
            DateTime defBegHr = nowHr.AddDays(-730);
            DateTime defEndHr = nowHr.AddDays(-365);
            DateTime millDt = new DateTime(2001, 1, 1);
            DateTime ruleDt = new DateTime(1993, 1, 1);

            /* Input Parameter Values */
            bool?[] requiredList = { false, false, false, false, false, false, false, false, false, false,
                                     false, false, true, true, true, true, true };
            DateTime?[] analysisDateList = { null, nowHr.Date, defBegHr.Date, defBegHr.Date, defBegHr.Date, defBegHr.Date, defBegHr.Date, defBegHr.Date, defBegHr.Date, defBegHr.Date,
                                             defBegHr.Date, defBegHr.Date, null, ruleDt.AddDays(-1), ruleDt, millDt.AddDays(1), millDt.AddDays(1) };
            DateTime?[] loadBeginList = { defBegHr, defBegHr, defBegHr, defBegHr, defBegHr, defBegHr, defBegHr, defBegHr, defBegHr, defBegHr,
                                          defBegHr, defBegHr, defBegHr, ruleDt.AddHours(-24), ruleDt.AddHours(-24), millDt, millDt.AddHours(-24) };
            DateTime?[] loadEndList = { null, null, null, null, null, null, null, null, null, null,
                                        null, defEndHr, null, null, null, null, null };
            string[] methodCodeList = { "CEM", "CEM", "CEM", "AD", "AE", "CEM", "LME", "LTFF", "MHHI", "LME",
                                        "LME", "LME", "CEM", "CEM", "CEM", "CEM", "CEM" };
            DateTime?[] methodBeginList = { defBegHr, defBegHr, defBegHr, defBegHr, defBegHr, defBegHr, defBegHr, defBegHr, defBegHr, defBegHr,
                                            defCcoDt, defEndHr, defBegHr, defBegHr, defBegHr, defBegHr, defBegHr };
            DateTime?[] methodEndList = { null, null, null, null, null, defEndHr, null, null, null, defEndHr,
                                          defBegHr, null, null, null, null, null, null };
            DateTime?[]otherEndList = { defBegHr.AddHours(-1), defBegHr.AddHours(-1), defBegHr.AddHours(-1), defBegHr.AddHours(-1), defBegHr.AddHours(-1),
                                        defBegHr.AddHours(-1), defBegHr.AddHours(-1), defBegHr.AddHours(-1), defBegHr.AddHours(-1), defBegHr.AddHours(-1),
                                        defBegHr.AddHours(-1), defBegHr.AddHours(-1), defBegHr.AddHours(-1), ruleDt.AddHours(-25), ruleDt.AddHours(-25),
                                        millDt.AddHours(-1), millDt.AddHours(-25) };
            DateTime?[] ccoDtList = { defCcoDt, defCcoDt, defCcoDt, defCcoDt, defCcoDt, defCcoDt, defCcoDt, defCcoDt, defCcoDt, defCcoDt,
                                      defCcoDt, defCcoDt, defCcoDt, defCcoDt, defCcoDt, defCcoDt, defCcoDt };

            /* Expected Values */
            string[] expResultList = { null, "A", "A", "A", "A", "A", "F", "F", "F", "F",
                                       "F", "F", "B", "C", null, "D", null };


            /* Test Case Count */
            int caseCount = 17;

            /* Check array lengths */
            Assert.AreEqual(caseCount, requiredList.Length, "requiredList length");
            Assert.AreEqual(caseCount, analysisDateList.Length, "analysisDateList length");
            Assert.AreEqual(caseCount, loadBeginList.Length, "loadBeginList length");
            Assert.AreEqual(caseCount, loadEndList.Length, "loadEndList length");
            Assert.AreEqual(caseCount, methodCodeList.Length, "methodCodeList length");
            Assert.AreEqual(caseCount, methodBeginList.Length, "methodBeginList length");
            Assert.AreEqual(caseCount, methodEndList.Length, "methodEndList length");
            Assert.AreEqual(caseCount, otherEndList.Length, "otherEndList length");
            Assert.AreEqual(caseCount, expResultList.Length, "expResultList length");


            /* Run Cases */
            for (int caseDex = 0; caseDex < caseCount; caseDex++)
            {
                /* Initialize Input Parameters */
                MpParameters.CurrentLoad = SetValues.DateHour(new VwMonitorLoadRow(loadAnalysisDate: analysisDateList[caseDex]), loadBeginList[caseDex], loadEndList[caseDex]);
                MpParameters.CurrentLocation = new VwMpLocationRow(comrOpDate: ccoDtList[caseDex]);
                MpParameters.LoadLevelsRequired = requiredList[caseDex];
                MpParameters.MethodRecords = new CheckDataView<VwMonitorMethodRow>
                    (
                        SetValues.DateHour(new VwMonitorMethodRow(methodCd: "LME"), defCcoDt, otherEndList[caseDex]),
                        SetValues.DateHour(new VwMonitorMethodRow(methodCd: methodCodeList[caseDex]), methodBeginList[caseDex], methodEndList[caseDex]),
                        SetValues.DateHour(new VwMonitorMethodRow(methodCd: "LTFF"), defCcoDt, otherEndList[caseDex]),
                        SetValues.DateHour(new VwMonitorMethodRow(methodCd: "MHHI"), defCcoDt, otherEndList[caseDex])
                    );


                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                /* Initialize variables needed to run the check. */
                bool log = false;
                string actual;

                /* Run Check */
                actual = cLoadChecks.LOAD1(category, ref log);


                /* Check Results */
                Assert.AreEqual(string.Empty, actual, string.Format("actual [{0}]", caseDex));
                Assert.AreEqual(false, log, string.Format("log [{0}]", caseDex));
                Assert.AreEqual(expResultList[caseDex], category.CheckCatalogResult, string.Format("category.CheckCatalogResult [{0}]", caseDex));
            }

        }


        #region LOAD-20

        /// <summary>
        ///A test for LOAD20_HG
        ///</summary>()
        [TestMethod()]
        public void LOAD20_HG()
        {
            //static check setup
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            MpParameters.Init(category.Process);
            MpParameters.Category = category;

            // Variables
            bool log = false;
            string actual;
            bool testTrue = false;
            string[] testSystemTypeList = { "SYSBAD", "SO2", "SO2R", "NOX", "NOXC", "CO2", "O2", "FLOW", "HG", "HCL", "HF" };

            // Init Input
            MpParameters.CurrentLoad = new VwMonitorLoadRow(monLocId: "LOC1");
            MpParameters.FacilityQualificationRecords = new CheckDataView<MonitorQualificationRow>();
            MpParameters.LoadEvaluationEndDate = DateTime.Today;
            MpParameters.LoadEvaluationEndHour = 0;
            MpParameters.LoadEvaluationStartDate = DateTime.Today.AddDays(-1);
            MpParameters.LoadEvaluationStartHour = 0;
            MpParameters.LocationType = "U";
            MpParameters.QaSupplementalDataRecords = new CheckDataView<VwQaSuppDataRow>();
            MpParameters.UnitStackConfigurationRecords = new CheckDataView<VwUnitStackConfigurationRow>();

            foreach (string testSystemTypeCode in testSystemTypeList)
            {
                // Init Input
                MpParameters.MonitorSystemRecords = new CheckDataView<VwMonitorSystemRow>
                    (new VwMonitorSystemRow(monLocId: "LOC1", sysTypeCd: testSystemTypeCode, beginDate: DateTime.Today.AddDays(-1), beginHour: 0, beginDatehour: DateTime.Today.AddDays(-1)));
                if (testSystemTypeCode.InList("SO2,SO2R,NOX,NOXC,CO2,O2,FLOW,HG,HCL,HF"))
                {
                    testTrue = true;
                }
                else
                {
                    testTrue = false;
                }

                // Init Output
                category.CheckCatalogResult = null;


                // Run Checks
                actual = cLoadChecks.LOAD20(category, ref log);

                // Check Results
                Assert.AreEqual(string.Empty, actual);
                Assert.AreEqual(false, log);
                if (testTrue)
                {
                    Assert.AreEqual(null, category.CheckCatalogResult, "Result");
                    Assert.AreEqual(true, MpParameters.RangeOfOperationRequired, "RangeOfOperationRequired");
                    Assert.AreEqual(true, MpParameters.LoadLevelsRequired, "LoadLevelsRequired");
                }
                else
                {
                    Assert.AreEqual(null, category.CheckCatalogResult, "Result");
                    Assert.AreEqual(false, MpParameters.RangeOfOperationRequired, "RangeOfOperationRequired");
                    Assert.AreEqual(false, MpParameters.LoadLevelsRequired, "LoadLevelsRequired");
                }
            }
        }
        #endregion

    }
}