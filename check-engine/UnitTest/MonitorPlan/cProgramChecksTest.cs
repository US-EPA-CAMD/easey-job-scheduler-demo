using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

using ECMPS.Checks.CheckEngine;
using ECMPS.Checks.MonitorPlan;
using ECMPS.Checks.Data.Ecmps.CheckMp.Function;
using ECMPS.Checks.ProgramChecks;
using ECMPS.Definitions.Enumerations;
using ECMPS.Definitions.Extensions;

using ECMPS.Checks.Data.Ecmps.Dbo.Table;
using ECMPS.Checks.Data.Ecmps.Dbo.View;
using ECMPS.Checks.Data.EcmpsAux.CrossCheck.Virtual;
using ECMPS.Checks.Data.Ecmps.CrossCheck.Table;
using ECMPS.Checks.Mp.Parameters;

using ECMPS.Checks.TypeUtilities;

using UnitTest.UtilityClasses;

namespace UnitTest.MonitorPlan
{
    /// <summary>
    ///This is a test class for cSystemChecksTest and is intended
    ///to contain all cSystemChecksTest Unit Tests
    /// </summary>
    [TestClass()]
    public class cProgramChecksTest
    {
        //public cProgramChecksTest()
        //{
        //    //
        //    // TODO: Add constructor logic here
        //    //
        //}

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
        /// Current Date (Curr)           : 2017-06-16
        /// Evaluation Begin Date (EvalB) : 2017-06-16 - 210
        /// Evaluation End Date (EvalE)   : 2017-06-16 + 190
        /// 
        /// Current Reporting Period (Prd)        : 2016 Q2
        /// Current Reporting Period Begin (PrdB) : 2016-04-01
        /// Current Reporting Period End (PrdE)   : 2016-06-30
        /// 
        /// 
        /// |    |            Current Program              |       MATS        |        |        Op Status         ||                              ||
        /// | ## | PrgCd   | UMCB      | ERBD    | PrgE    | Reg   | EvalB     | MpEnd  | Code | BegDt   | EndDt   || Active | EvalB     | EvalE   || Note
        /// |  0 | MISC    | null      | null    | null    | false | null      | null   | OPR  | EvalB   | null    || false  | MinDate   | MaxDate || UMCB is null
        /// |  1 | MISC    | Curr+1    | null    | null    | false | null      | null   | OPR  | EvalB   | null    || false  | MinDate   | MaxDate || UMCB is after the current date.
        /// |  2 | MISC    | EvalB-2   | null    | EvalB-1 | false | null      | null   | OPR  | EvalB   | null    || false  | MinDate   | MaxDate || Program ends before evaluation period.
        /// |  3 | MISC    | PrdB      | null    | null    | false | null      | Prd-1  | OPR  | EvalB   | null    || false  | MinDate   | MaxDate || UMCB is after the MP end reporting period.
        /// |  4 | MATS    | PrdB      | null    | EvalE-1 | false | EvalE-1   | null   | OPR  | EvalB   | null    || false  | MinDate   | MaxDate || MATS and program ends on the MATS Evaluation Begin Date.
        /// |  5 | MATS    | PrdB      | null    | EvalE-2 | false | EvalE-1   | null   | OPR  | EvalB   | null    || false  | MinDate   | MaxDate || MATS and program ends before the MATS Evaluation Begin Date.
        /// |  6 | MATS    | PrdB      | null    | null    | false | EvalE     | null   | OPR  | EvalB   | null    || false  | MinDate   | MaxDate || MATS and evaluation period ends on the MATS Evaluation Begin Date.
        /// |  7 | MATS    | PrdB      | null    | null    | false | EvalE+1   | null   | OPR  | EvalB   | null    || false  | MinDate   | MaxDate || MATS and evaluation period ends before the MATS Evaluation Begin Date.
        /// |  8 | MISC    | Curr      | null    | EvalB   | false | null      | null   | OPR  | EvalB   | null    || true   | Curr+180  | EvalB   || UMCB is on the current date, and program end is on evaluation end.
        /// |  9 | MISC    | PrdB-1    | null    | null    | false | null      | Prd-1  | OPR  | EvalB   | null    || true   | PrdB+179  | EvalE   || UMCB ended on last day of the MP end reporting period.
        /// | 10 | MISC    | PrdB      | null    | null    | false | null      | null   | OPR  | EvalB   | null    || true   | PrdB+180  | EvalE   || MP does not have an ending reporting period.
        /// | 11 | MATS    | PrdB      | null    | null    | false | EvalE-1   | null   | OPR  | EvalB   | null    || true   | PrdB+180  | EvalE   || Is MATS without and end date.
        /// | 12 | MATS    | PrdB      | null    | EvalE   | false | EvalE-1   | null   | OPR  | EvalB   | null    || true   | PrdB+180  | EvalE   || Is MATS and program ends after the MATS Evaluation Begin Date.
        /// | 13 | MATS    | PrdB      | null    | null    | false | EvalE-1   | null   | OPR  | EvalB   | null    || true   | PrdB+180  | EvalE   || Is MATS and evaluation period ends after the MATS Evaluation Begin Date.
        /// | 14 | MISC    | EvalB-2   | EvalB-1 | EvalE-1 | false | null      | null   | OPR  | EvalB   | null    || true   | EvalB+178 | EvalE-1 || ERBD exists but is before the EvalB.  Prg End is before EvalE.
        /// | 15 | MISC    | EvalB-2   | EvalB   | EvalE   | false | null      | null   | OPR  | EvalB   | null    || true   | EvalB+178 | EvalE   || ERBD exists but is on the EvalB.  Prg End is on EvalE.
        /// | 16 | MISC    | EvalB-2   | EvalB+1 | EvalE+1 | false | null      | null   | OPR  | EvalB   | null    || true   | EvalB+1   | EvalE   || ERBD exists but is after the EvalB.  Prg End is after EvalE.
        /// | 17 | MISC    | EvalB-181 | null    | null    | false | null      | null   | OPR  | EvalB   | null    || true   | EvalB     | EvalE   || No ERBD, but UMCB began 181 days before the EvalB.
        /// | 18 | MISC    | EvalB-180 | null    | null    | false | null      | null   | OPR  | EvalB   | null    || true   | EvalB     | EvalE   || No ERBD, but UMCB began 180 days before the EvalB.
        /// | 19 | MISC    | EvalB-179 | null    | null    | false | null      | null   | OPR  | EvalB   | null    || true   | EvalB+1   | EvalE   || No ERBD, but UMCB began 179 days before the EvalB.
        /// | 20 | MATS    | EvalB     | null    | null    | true  | EvalB+181 | null   | OPR  | EvalB   | null    || true   | EvalB+181 | EvalE   || MATS, MATS Required, and MATS evaluation begin date after EvalB + 180.
        /// | 21 | MISC    | EvalB     | null    | null    | true  | EvalB+181 | null   | OPR  | EvalB   | null    || true   | EvalB+180 | EvalE   || Not MATS, MATS Required, and MATS evaluation begin date after EvalB + 180.
        /// | 22 | MATS    | EvalB     | null    | null    | false | EvalB+181 | null   | OPR  | EvalB   | null    || true   | EvalB+180 | EvalE   || MATS, MATS not Required, and MATS evaluation begin date after EvalB + 180.
        /// | 23 | MATS    | EvalB     | null    | null    | true  | EvalB+180 | null   | OPR  | EvalB   | null    || true   | EvalB+180 | EvalE   || MATS, MATS Required, and MATS evaluation begin date on EvalB + 180.
        /// | 24 | MATS    | EvalB     | null    | null    | true  | EvalB+179 | null   | OPR  | EvalB   | null    || true   | EvalB+180 | EvalE   || MATS, MATS Required, and MATS evaluation begin date before EvalB + 180.
        /// | 25 | MISC    | EvalB     | null    | null    | false | null      | null   | RET  | EvalE   | null    || true   | EvalB+180 | EvalE-1 || Retired on EvalE.
        /// | 26 | MISC    | EvalB     | null    | null    | false | null      | null   | RET  | EvalE-1 | null    || true   | EvalB+180 | EvalE-2 || Retired on the day before EvalE.
        /// | 27 | MISC    | EvalB     | null    | null    | false | null      | null   | RET  | EvalE   | EvalE+1 || true   | EvalB+180 | EvalE   || Retirement ended.
        /// | 28 | MISC    | EvalB     | null    | null    | false | null      | null   | RET  | EvalE+1 | null    || true   | EvalB+180 | EvalE   || Retired day after EvalE.
        /// 
        /// </summary>
        [TestMethod()]
        public void Program1()
        {
            /* Initialize objects generally needed for testing checks. */
            cMpCheckParameters mpCheckParameters = new cMpCheckParameters();
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory(mpCheckParameters);
            cProgramChecks target = new cProgramChecks(mpCheckParameters);

            MpParameters.Init(category.Process);
            MpParameters.Category = category;


            /* Input Parameter Values */
            DateTime curr = new DateTime(2016, 6, 17);
            {
                ((UnitTestCheckEngine)category.CheckEngine).SetNowDateTime(curr);
            }
            DateTime evalB = curr.AddDays(-210);
            DateTime evalE = curr.AddDays(190);
            cReportingPeriod prdPrev = cReportingPeriod.GetReportingPeriod(2016, 1);
            DateTime prdB = new DateTime(2016, 4, 1);

            string[] prgCodeList = { "MISC", "MISC", "MISC", "MISC", "MATS", "MATS", "MATS", "MATS", "MISC", "MISC",
                                     "MISC", "MATS", "MATS", "MATS", "MISC", "MISC", "MISC", "MISC", "MISC", "MISC",
                                     "MATS", "MISC", "MATS", "MATS", "MATS", "MISC", "MISC", "MISC", "MISC" };
            DateTime?[] prgUmcbList = { null, curr.AddDays(1), evalB.AddDays(-2), prdB, prdB, prdB, prdB, prdB, curr, prdB.AddDays(-1),
                                        prdB, prdB, prdB, prdB, evalB.AddDays(-2), evalB.AddDays(-2), evalB.AddDays(-2), evalB.AddDays(-181), evalB.AddDays(-180), evalB.AddDays(-179),
                                        evalB, evalB, evalB, evalB, evalB, evalB, evalB, evalB, evalB };
            DateTime?[] prgErbdList = { null, null, null, null, null, null, null, null, null, null,
                                        null, null, null, null, evalB.AddDays(-1), evalB, evalB.AddDays(1), null, null, null,
                                        null, null, null, null, null, null, null, null, null };
            DateTime?[] prgEndList = { null, null, evalB.AddDays(-1), null, evalE.AddDays(-1), evalE.AddDays(-2), null, null, evalB, null,
                                       null, null, null, null, evalE.AddDays(-1), evalE, evalE.AddDays(1), null, null, null,
                                       null, null, null, null, null, null, null, null, null };
            bool?[] matsReqList = { false, false, false, false, false, false, false, false, false, false,
                                    false, false, false, false, false, false, false, false, false, false,
                                    true, true, false, true, true, false, false, false, false };
            DateTime?[] matsBeginList = { null, null, null, null, evalE.AddDays(-1), evalE.AddDays(-1), evalE, evalE.AddDays(1), null, null,
                                          null, null, null, null, evalE.AddDays(-1), evalE.AddDays(-1), evalE.AddDays(-1), null, null, null,
                                          evalB.AddDays(181), evalB.AddDays(181), evalB.AddDays(181), evalB.AddDays(180), evalB.AddDays(179), null, null, null, null };
            cReportingPeriod[] mpEndList = { null, null, null, prdPrev, null, null, null, null, null, prdPrev,
                                             null, null, null, null, null, null, null, null, null, null,
                                             null, null, null, null, null, null, null, null, null };
            string[] opCodeList = { "OPR", "OPR", "OPR", "OPR", "OPR", "OPR", "OPR", "OPR", "OPR", "OPR",
                                    "OPR", "OPR", "OPR", "OPR", "OPR", "OPR", "OPR", "OPR", "OPR", "OPR",
                                    "OPR", "OPR", "OPR", "OPR", "OPR", "RET", "RET", "RET", "RET" };
            DateTime?[] opBeginList = { evalB, evalB, evalB, evalB, evalB, evalB, evalB, evalB, evalB, evalB,
                                        evalB, evalB, evalB, evalB, evalB, evalB, evalB, evalB, evalB, evalB,
                                        evalB, evalB, evalB, evalB, evalB, evalE, evalE.AddDays(-1), evalE, evalE.AddDays(1) };
            DateTime?[] opEndList = { null, null, null, null, null, null, null, null, null, null,
                                      null, null, null, null, null, null, null, null, null, null,
                                      null, null, null, null, null, null, null, evalE.AddDays(1), null };


            /* Expected Values */
            bool?[] expActiveList = { false, false, false, false, false, false, false, false, true, true,
                                      true, true, true, true, true, true, true, true, true, true,
                                      true, true, true, true, true, true, true, true, true };
            DateTime?[] expEvalB = { DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, curr.AddDays(180), prdB.AddDays(179),
                                     prdB.AddDays(180), prdB.AddDays(180), prdB.AddDays(180), prdB.AddDays(180), evalB.AddDays(178), evalB.AddDays(178), evalB.AddDays(1), evalB, evalB, evalB.AddDays(1),
                                     evalB.AddDays(181), evalB.AddDays(180), evalB.AddDays(180), evalB.AddDays(180), evalB.AddDays(180), evalB.AddDays(180), evalB.AddDays(180), evalB.AddDays(180), evalB.AddDays(180) };
            DateTime?[] expEvalE = { DateTime.MaxValue, DateTime.MaxValue, DateTime.MaxValue, DateTime.MaxValue, DateTime.MaxValue, DateTime.MaxValue, DateTime.MaxValue, DateTime.MaxValue, evalB, evalE,
                                     evalE, evalE, evalE, evalE, evalE.AddDays(-1), evalE, evalE, evalE, evalE, evalE,
                                     evalE, evalE, evalE, evalE, evalE, evalE.AddDays(-1), evalE.AddDays(-2), evalE, evalE };

            /* Test Case Count */
            int caseCount = 29;

            /* Check array lengths */
            Assert.AreEqual(caseCount, prgCodeList.Length, "prgCodeList length");
            Assert.AreEqual(caseCount, prgUmcbList.Length, "prgUmcbList length");
            Assert.AreEqual(caseCount, prgErbdList.Length, "prgErbdList length");
            Assert.AreEqual(caseCount, prgEndList.Length, "prgEndList length");
            Assert.AreEqual(caseCount, matsReqList.Length, "matsReqList length");
            Assert.AreEqual(caseCount, matsBeginList.Length, "matsBeginList length");
            Assert.AreEqual(caseCount, mpEndList.Length, "mpEndList length");
            Assert.AreEqual(caseCount, opCodeList.Length, "opCodeList length");
            Assert.AreEqual(caseCount, opBeginList.Length, "opBeginList length");
            Assert.AreEqual(caseCount, opEndList.Length, "opEndList length");
            Assert.AreEqual(caseCount, expActiveList.Length, "expActiveList length");
            Assert.AreEqual(caseCount, expEvalB.Length, "expEvalB length");
            Assert.AreEqual(caseCount, expEvalE.Length, "expEvalE length");


            /* Run Cases */
            for (int caseDex = 0; caseDex < caseCount; caseDex++)
            {
                /* Initialize Input Parameters */
                MpParameters.CurrentMonitoringPlanConfiguration = new VwMpMonitorPlanRow(endRptPeriodId: (mpEndList[caseDex] != null) ? mpEndList[caseDex].RptPeriodId : (int?)null);
                MpParameters.CurrentProgram = new VwLocationProgramRow(prgCd: prgCodeList[caseDex], unitMonitorCertBeginDate: prgUmcbList[caseDex], emissionsRecordingBeginDate: prgErbdList[caseDex], endDate: prgEndList[caseDex]);
                MpParameters.EvaluationBeginDate = evalB;
                MpParameters.EvaluationEndDate = evalE;
                MpParameters.MatsEvaluationBeginDate = matsBeginList[caseDex];
                MpParameters.MatsRequiredCheck = matsReqList[caseDex];
                MpParameters.UnitOperatingStatusRecords = new CheckDataView<VwUnitOpStatusRow>();
                {
                    MpParameters.UnitOperatingStatusRecords.Add(new VwUnitOpStatusRow(opStatusCd: "FUT", beginDate: evalB.AddDays(-365), endDate: evalB.AddDays(-183)));
                    MpParameters.UnitOperatingStatusRecords.Add(new VwUnitOpStatusRow(opStatusCd: "OPR", beginDate: evalB.AddDays(-182), endDate: opBeginList[caseDex].Value.AddDays(-1)));
                    MpParameters.UnitOperatingStatusRecords.Add(new VwUnitOpStatusRow(opStatusCd: opCodeList[caseDex], beginDate: opBeginList[caseDex], endDate: opEndList[caseDex]));
                    MpParameters.UnitOperatingStatusRecords.Add(new VwUnitOpStatusRow(opStatusCd: "RET", beginDate: evalE.AddDays(365), endDate:null));
                }

                /* Initialize Output Parameters */
                MpParameters.CurrentProgramActive = null;
                MpParameters.ProgramEvaluationBeginDate = DateTime.MinValue;
                MpParameters.ProgramEvaluationEndDate = DateTime.MaxValue;


                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                /* Initialize variables needed to run the check. */
                bool log = false;
                string actual;

                /* Run Check */
                actual = target.PROGRAM1(MpParameters.Category, ref log);


                /* Check Results */
                Assert.AreEqual(string.Empty, actual, string.Format("actual [{0}]", caseDex));
                Assert.AreEqual(false, log, string.Format("log [{0}]", caseDex));
                Assert.AreEqual(null, category.CheckCatalogResult, string.Format("category.CheckCatalogResult [{0}]", caseDex));

                Assert.AreEqual(expActiveList[caseDex], MpParameters.CurrentProgramActive, string.Format("CurrentProgramActive [{0}]", caseDex));
                Assert.AreEqual(expEvalB[caseDex], MpParameters.ProgramEvaluationBeginDate, string.Format("ProgramEvaluationBeginDate [{0}]", caseDex));
                Assert.AreEqual(expEvalE[caseDex], MpParameters.ProgramEvaluationEndDate, string.Format("ProgramEvaluationEndDate [{0}]", caseDex));
            }
        }

        #region Program-10

        [TestMethod()]
        public void Program10()
        {
            /* Setup Checks, Category and Parameters */
            cProgramChecks target = new cProgramChecks(new cMpCheckParameters()); // Old instantiated parameters are not used
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            MpParameters.Init(category.Process);
            MpParameters.Category = category;

            /* Setup Test Cases */
            List<program10Case> testCases = new List<program10Case>();

            testCases.Add(new program10Case(programEvaluationBeginOffset: 1, programEvaluationEndOffset: 5,
                                            currentProgramParamBeginOffset: 1, currentProgramParamEndOffset: 5,
                                            matsEvaluationBeginDate: 0,
                                            currentProgramParameterActive: true, programParameterEvalBeginOffset: 1, programParameterEvalEndOffset: 5));

            testCases.Add(new program10Case(programEvaluationBeginOffset: 1, programEvaluationEndOffset: 4,
                                            currentProgramParamBeginOffset: 2, currentProgramParamEndOffset: 5,
                                            matsEvaluationBeginDate: 0,
                                            currentProgramParameterActive: true, programParameterEvalBeginOffset: 2, programParameterEvalEndOffset: 4));

            testCases.Add(new program10Case(programEvaluationBeginOffset: 2, programEvaluationEndOffset: 5,
                                            currentProgramParamBeginOffset: 1, currentProgramParamEndOffset: 4,
                                            matsEvaluationBeginDate: 0,
                                            currentProgramParameterActive: true, programParameterEvalBeginOffset: 2, programParameterEvalEndOffset: 4));

            testCases.Add(new program10Case(programEvaluationBeginOffset: 1, programEvaluationEndOffset: 5,
                                            currentProgramParamBeginOffset: 3, currentProgramParamEndOffset: 4,
                                            matsEvaluationBeginDate: 2,
                                            currentProgramParameterActive: true, programParameterEvalBeginOffset: 3, programParameterEvalEndOffset: 4));

            testCases.Add(new program10Case(programEvaluationBeginOffset: 3, programEvaluationEndOffset: 5,
                                            currentProgramParamBeginOffset: 1, currentProgramParamEndOffset: 4,
                                            matsEvaluationBeginDate: 2,
                                            currentProgramParameterActive: true, programParameterEvalBeginOffset: 3, programParameterEvalEndOffset: 4));

            testCases.Add(new program10Case(programEvaluationBeginOffset: 2, programEvaluationEndOffset: 5,
                                            currentProgramParamBeginOffset: 2, currentProgramParamEndOffset: 4,
                                            matsEvaluationBeginDate: 2,
                                            currentProgramParameterActive: true, programParameterEvalBeginOffset: 2, programParameterEvalEndOffset: 4));

            testCases.Add(new program10Case(programEvaluationBeginOffset: 1, programEvaluationEndOffset: 5,
                                            currentProgramParamBeginOffset: 2, currentProgramParamEndOffset: 4,
                                            matsEvaluationBeginDate: 3,
                                            currentProgramParameterActive: true, programParameterEvalBeginOffset: 2, programParameterEvalEndOffset: 4));

            testCases.Add(new program10Case(programEvaluationBeginOffset: 2, programEvaluationEndOffset: 5,
                                            currentProgramParamBeginOffset: 1, currentProgramParamEndOffset: 4,
                                            matsEvaluationBeginDate: 3,
                                            currentProgramParameterActive: true, programParameterEvalBeginOffset: 2, programParameterEvalEndOffset: 4));

            testCases.Add(new program10Case(programEvaluationBeginOffset: 1, programEvaluationEndOffset: 5,
                                            currentProgramParamBeginOffset: 2, currentProgramParamEndOffset: 4,
                                            matsEvaluationBeginDate: 3, currentProgramCode: "MATS",
                                            currentProgramParameterActive: true, programParameterEvalBeginOffset: 3, programParameterEvalEndOffset: 4));

            testCases.Add(new program10Case(programEvaluationBeginOffset: 2, programEvaluationEndOffset: 5,
                                            currentProgramParamBeginOffset: 1, currentProgramParamEndOffset: 4,
                                            matsEvaluationBeginDate: 3, currentProgramCode: "MATS",
                                            currentProgramParameterActive: true, programParameterEvalBeginOffset: 3, programParameterEvalEndOffset: 4));

            testCases.Add(new program10Case(programEvaluationBeginOffset: 1, programEvaluationEndOffset: 5,
                                            currentProgramParamBeginOffset: 2, currentProgramParamEndOffset: 4,
                                            matsEvaluationBeginDate: 3, currentProgramCode: "MATS",
                                            currentProgramParameterActive: true, programParameterEvalBeginOffset: 3, programParameterEvalEndOffset: 4));

            testCases.Add(new program10Case(programEvaluationBeginOffset: 1, programEvaluationEndOffset: 5,
                                            currentProgramParamBeginOffset: 6, currentProgramParamEndOffset: 9,
                                            matsEvaluationBeginDate: 0,
                                            currentProgramParameterActive: false, programParameterEvalBeginOffset: null, programParameterEvalEndOffset: null));

            testCases.Add(new program10Case(programEvaluationBeginOffset: 6, programEvaluationEndOffset: 9,
                                            currentProgramParamBeginOffset: 1, currentProgramParamEndOffset: 5,
                                            matsEvaluationBeginDate: 0,
                                            currentProgramParameterActive: false, programParameterEvalBeginOffset: null, programParameterEvalEndOffset: null));

            testCases.Add(new program10Case(programEvaluationBeginOffset: 1, programEvaluationEndOffset: 5,
                                            currentProgramParamBeginOffset: 2, currentProgramParamEndOffset: 4,
                                            matsEvaluationBeginDate: 5, currentProgramCode: "MATS",
                                            currentProgramParameterActive: false, programParameterEvalBeginOffset: null, programParameterEvalEndOffset: null));

            testCases.Add(new program10Case(programEvaluationBeginOffset: 2, programEvaluationEndOffset: 4,
                                            currentProgramParamBeginOffset: 1, currentProgramParamEndOffset: 5,
                                            matsEvaluationBeginDate: 5, currentProgramCode: "MATS",
                                            currentProgramParameterActive: false, programParameterEvalBeginOffset: null, programParameterEvalEndOffset: null));

            /* Run Test Cases */
            foreach (program10Case testCase in testCases)
            {
                /* Initialize Input Parameters */
                MpParameters.CurrentProgramActive = testCase.CurrentProgramActive;
                MpParameters.CurrentProgramParameter = new UnitProgramParameter(prgCd: testCase.CurrentProgramCode, paramBeginDate: testCase.CurrentProgramParamBeginDate, paramEndDate: testCase.CurrentProgramParamEndDate);
                MpParameters.MatsEvaluationBeginDate = testCase.MatsEvaluationBeginDate;
                MpParameters.ProgramEvaluationBeginDate = testCase.ProgramEvaluationBeginDate;
                MpParameters.ProgramEvaluationEndDate = testCase.ProgramEvaluationEndDate;

                /* Initialize Output Parameter */
                MpParameters.CurrentProgramParameterActive = null;
                MpParameters.ProgramParameterEvaluationBeginDate = null;
                MpParameters.ProgramParameterEvaluationEndDate = null;

                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                /* Declare Check Call Variables */
                bool log = false;
                string actual;

                /* Run Check */
                actual = target.PROGRAM10(category, ref log);

                /* Check Results */
                Assert.AreEqual(string.Empty, actual, testCase.ResultPrefix + ".Actual");
                Assert.AreEqual(false, log, testCase.ResultPrefix + ".Log");

                Assert.AreEqual(null, category.CheckCatalogResult, testCase.ResultPrefix + ".Result");
                Assert.AreEqual(testCase.CurrentProgramParameterActive, (MpParameters.CurrentProgramParameterActive.HasValue ? MpParameters.CurrentProgramParameterActive.Value : (bool?)null), testCase.ResultPrefix + ".CurrentProgramParameterActive");
                Assert.AreEqual(testCase.ProgramParameterEvaluationBeginDate, (MpParameters.ProgramParameterEvaluationBeginDate.HasValue ? MpParameters.ProgramParameterEvaluationBeginDate.Value : (DateTime?)null), testCase.ResultPrefix + ".ProgramParameterEvaluationBeginDate");
                Assert.AreEqual(testCase.ProgramParameterEvaluationEndDate, (MpParameters.ProgramParameterEvaluationEndDate.HasValue ? MpParameters.ProgramParameterEvaluationEndDate.Value : (DateTime?)null), testCase.ResultPrefix + ".ProgramParameterEvaluationEndDate");
            }
        }

        private class program10Case
        {
            public program10Case
            (
              DateTime? seedDate = null,
              bool? currentProgramActive = true,
              string currentProgramCode = "OTHER",
              int? currentProgramParamBeginOffset = null,
              int? currentProgramParamEndOffset = null,
              int? programEvaluationBeginOffset = null,
              int? programEvaluationEndOffset = null,
              int? matsEvaluationBeginDate = null,
              bool? currentProgramParameterActive = null,
              int? programParameterEvalBeginOffset = null,
              int? programParameterEvalEndOffset = null
            )
            {
                if (seedDate == null) seedDate = DateTime.Now.Date;

                CurrentProgramActive = currentProgramActive;
                CurrentProgramCode = currentProgramCode;
                CurrentProgramParamBeginDate = currentProgramParamBeginOffset.HasValue ? seedDate.Value.AddDays(currentProgramParamBeginOffset.Value) : (DateTime?)null;
                CurrentProgramParamEndDate = currentProgramParamEndOffset.HasValue ? seedDate.Value.AddDays(currentProgramParamEndOffset.Value) : (DateTime?)null;
                ProgramEvaluationBeginDate = programEvaluationBeginOffset.HasValue ? seedDate.Value.AddDays(programEvaluationBeginOffset.Value) : (DateTime?)null;
                ProgramEvaluationEndDate = programEvaluationEndOffset.HasValue ? seedDate.Value.AddDays(programEvaluationEndOffset.Value) : (DateTime?)null;
                MatsEvaluationBeginDate = matsEvaluationBeginDate.HasValue ? seedDate.Value.AddDays(matsEvaluationBeginDate.Value) : (DateTime?)null;
                CurrentProgramParameterActive = currentProgramParameterActive;
                ProgramParameterEvaluationBeginDate = programParameterEvalBeginOffset.HasValue ? seedDate.Value.AddDays(programParameterEvalBeginOffset.Value) : (DateTime?)null;
                ProgramParameterEvaluationEndDate = programParameterEvalEndOffset.HasValue ? seedDate.Value.AddDays(programParameterEvalEndOffset.Value) : (DateTime?)null;

                ResultPrefix = string.Format("[PrgAct: {0}, Par [Cd: {1}, Beg: {2}, End: {3}], Eval [Beg: {4}, End: {5}], Mats: {6}]",
                                             currentProgramActive,
                                             currentProgramCode,
                                             currentProgramParamBeginOffset.HasValue ? currentProgramParamBeginOffset.Value : (int?)null,
                                             currentProgramParamEndOffset.HasValue ? currentProgramParamEndOffset.Value : (int?)null,
                                             programEvaluationBeginOffset.HasValue ? programEvaluationBeginOffset.Value : (int?)null,
                                             programEvaluationEndOffset.HasValue ? programEvaluationEndOffset.Value : (int?)null,
                                             matsEvaluationBeginDate.HasValue ? matsEvaluationBeginDate.Value : (int?)null);
            }

            /* Input Parameter Values */
            public bool? CurrentProgramActive { get; private set; }
            public string CurrentProgramCode { get; private set; }
            public DateTime? CurrentProgramParamBeginDate { get; private set; }
            public DateTime? CurrentProgramParamEndDate { get; private set; }
            public DateTime? ProgramEvaluationBeginDate { get; private set; }
            public DateTime? ProgramEvaluationEndDate { get; private set; }
            public DateTime? MatsEvaluationBeginDate { get; private set; }
            /* Expected Parameter Values */
            public bool? CurrentProgramParameterActive { get; private set; }
            public DateTime? ProgramParameterEvaluationBeginDate { get; private set; }
            public DateTime? ProgramParameterEvaluationEndDate { get; private set; }
            /* Result Prefix */
            public string ResultPrefix { get; private set; }
        }

        #endregion


        #region PROGRAM-11

        /// <summary>
        /// Program11: Evaluate Body Conditions
        /// 
        /// This set of tests checks the conditions that determine whether the body of Program-11 will run.
        /// Any time the condition to run the body is met, the result will be A.
        /// 
        /// MP-Desc: "Good Parameter"
        /// 
        /// | ## | PrgCode | Class | Req | Active || Result | Description || Note
        /// |  0 | ARP     | P1    |   1 | true   || A      | MP-Desc     || ARP Phase 1 Affected
        /// |  1 | ARP     | P2    |   1 | true   || A      | MP-Desc     || ARP Phase 2 Affected
        /// |  2 | ARP     | NA    |   1 | true   || null   | ""          || ARP Non Affected
        /// |  3 | ARP     | P1    |   0 | true   || null   | ""          || ARP parameter not required
        /// |  4 | ARP     | P1    |   1 | false  || null   | ""          || ARP program parameter not active
        /// |  5 | OTC     | B     |   1 | true   || A      | MP-Desc     || OTC Affected
        /// |  6 | OTC     | NB    |   1 | true   || null   | ""          || OTC Non Affected
        /// |  7 | TRSO2G1 | A     |   1 | true   || A      | MP-Desc     || TR SO2 Group 1 Affected
        /// |  8 | TRSO2G1 | N     |   1 | true   || null   | ""          || TR SO2 Group 1 Non Affected
        /// |  9 | TRSO2G2 | A     |   1 | true   || A      | MP-Desc     || TR SO2 Group 2 Affected
        /// | 10 | TRSO2G2 | N     |   1 | true   || null   | ""          || TR SO2 Group 2 Non Affected
        /// | 11 | TRNOX   | A     |   1 | true   || A      | MP-Desc     || TR NOX Affected
        /// | 12 | TRNOX   | N     |   1 | true   || null   | ""          || TR NOX Non Affected
        /// | 13 | TRNOXOS | A     |   1 | true   || A      | MP-Desc     || TR NOX OS Affected
        /// | 14 | TRNOXOS | N     |   1 | true   || null   | ""          || TR NOX OS Non Affected
        /// | 15 | MATS    | A     |   1 | true   || null   | ""          || MATS Affected
        /// | 16 | NSPS4T  | A     |   1 | true   || null   | ""          || NSPS4T Affected
        /// 
        /// </summary>
        [TestMethod()]
        public void Program11_RunCheckConditions()
        {
            /* Initialize objects generally needed for testing checks. */
            cMpCheckParameters mpCheckParameters = new cMpCheckParameters();
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory(mpCheckParameters);
            cProgramChecks target = new cProgramChecks(mpCheckParameters);

            MpParameters.Init(category.Process);
            MpParameters.Category = category;


            /* Helper Values */
            DateTime evalB = new DateTime(2015, 7, 1);
            DateTime evalE = new DateTime(2016, 5, 31);
            string desc = "Good Parameter";

            /* Input Parameter Values */
            string[] prgCodeList = { "ARP", "ARP", "ARP", "ARP", "ARP", "OTC", "OTC", "TRSO2G1", "TRSO2G1", "TRSO2G2", "TRSO2G2", "TRNOX", "TRNOX", "TRNOXOS", "TRNOXOS", "MATS", "NSPS4T" };
            string[] prgClassList = { "P1", "P2", "NA", "P1", "P1", "B", "NB", "A", "N", "A", "N", "A", "N", "A", "N", "A", "A" };
            int[] prgParamReqList = { 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            bool[] activeList = { true, true, true, true, false, true, true, true, true, true, true, true, true, true, true, true, true };

            /* Expected Values */
            string[] resultList = { "A", "A", null, null, null, "A", null, "A", null, "A", null, "A", null, "A", null, null, null };
            string[] descriptionList = { desc, desc, "", "", "", desc, "", desc, "", desc, "", desc, "", desc, "", "", "" };

            /* Test Case Count */
            int caseCount = 17;

            /* Check array lengths */
            Assert.AreEqual(caseCount, prgCodeList.Length, "prgCodeList length");
            Assert.AreEqual(caseCount, prgClassList.Length, "prgClassList length");
            Assert.AreEqual(caseCount, prgParamReqList.Length, "prgParamReqList length");
            Assert.AreEqual(caseCount, activeList.Length, "activeList length");
            Assert.AreEqual(caseCount, resultList.Length, "resultList length");
            Assert.AreEqual(caseCount, descriptionList.Length, "descriptionList length");

            /* Run Cases */
            for (int caseDex = 0; caseDex < caseCount; caseDex++)
            {
                /* Initialize Input Parameters */
                MpParameters.FacilityMethodRecords = new CheckDataView<VwMonitorMethodRow>();
                MpParameters.CrosscheckProgramparametertolocationtype = new CheckDataView<ProgramParameterToLocationTypeRow>(new ProgramParameterToLocationTypeRow(programParameterCd: "GOODP", commonLocationTypeList: "CS", multipleLocationTypeList: "MS"));
                MpParameters.CrosscheckProgramparametertomethodparameter = new CheckDataView<ProgramParameterToMethodParameterRow>(new ProgramParameterToMethodParameterRow(programParameterCd: "GOODP", methodParameterList: "GOODM", methodParameterDescription: desc));
                MpParameters.CrosscheckProgramparametertomethodcode = new CheckDataView<ProgramParameterToMethodCodeRow>();
                MpParameters.CrosscheckProgramparametertoseverity = new CheckDataView<ProgramParameterToSeverityRow>();
                MpParameters.CurrentProgramParameter = new UnitProgramParameter(prgCd: prgCodeList[caseDex], classCd: prgClassList[caseDex], parameterCd: "GOODP", requiredInd: prgParamReqList[caseDex], monLocId: "ULOC");
                MpParameters.CurrentProgramParameterActive = activeList[caseDex];
                MpParameters.ProgramParameterEvaluationBeginDate = evalB.Date;
                MpParameters.ProgramParameterEvaluationEndDate = evalE.Date;
                MpParameters.UnitStackConfigurationRecords = new CheckDataView<VwUnitStackConfigurationRow>();

                /* Initialize Input Parameters */
                MpParameters.ProgramMethodParameterDescription = null;


                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                /* Initialize variables needed to run the check. */
                bool log = false;
                string actual;

                if (caseDex == 33)
                { }

                /* Run Check */
                actual = target.PROGRAM11(category, ref log);

                /* Check Results */
                Assert.AreEqual(string.Empty, actual, string.Format("actual {0}", caseDex));
                Assert.AreEqual(false, log, string.Format("log {0}", caseDex));
                Assert.AreEqual(resultList[caseDex], category.CheckCatalogResult, string.Format("category.CheckCatalogResult {0}", caseDex));
                Assert.AreEqual(descriptionList[caseDex], MpParameters.ProgramMethodParameterDescription, string.Format("ProgramMethodParameterDescription {0}", caseDex));
            }
        }

        /// <summary>
        /// Performs Span Testing where two common stacks and the unit cover portions of the evaluation period.
        /// 
        /// 
        ///                    |             CS1          |             CS2         ||
        /// | ## | Unit Method | USC          | Method    | USC          | Method   || Result || Note
        /// |  0 | Covers      | Active       | Covers    | Active       | Covers   || null   || Methods span.
        /// |  1 | GapBefore   | Active       | Covers    | Active       | Covers   || B      || Gap at beginning of unit range. 
        /// |  2 | GapAfter    | Active       | Covers    | Active       | Covers   || B      || Gap at end of unit range.
        /// |  3 | Missing     | Active       | Covers    | Active       | Covers   || B      || Gap at end of unit range.
        /// |  4 | Covers      | EndOverlap   | Covers    | Active       | Covers   || null   || Common 1 USC ended before.
        /// |  5 | Covers      | BeginOverlap | Covers    | Active       | Covers   || null   || Common 1 USC ended after.
        /// |  6 | Covers      | EndedBefore  | Covers    | Active       | Covers   || B      || Common 1 USC ended before.
        /// |  7 | Covers      | EndedAfter   | Covers    | Active       | Covers   || B      || Common 1 USC ended after.
        /// |  8 | Covers      | Missing      | Covers    | Active       | Covers   || B      || Common 1 USC missing.
        /// |  9 | Covers      | Active       | GapBefore | Active       | Covers   || B      || Common 1 has Method gap at beginning of range.
        /// | 10 | Covers      | Active       | GapAfter  | Active       | Covers   || B      || Common 1 has Method gap at end of range.
        /// | 11 | Covers      | Active       | Missing   | Active       | Covers   || B      || Common 1 Method is missing.
        /// | 12 | Missing     | Missing      | Covers    | Missing      | Covers   || A      || No connected common.
        /// | 13 | Missing     | Active       | Missing   | Active       | Missing  || A      || No common methods.
        /// 
        /// </summary>
        [TestMethod]
        public void Program11_UnitAndCommon()
        {
            /* Initialize objects generally needed for testing checks. */
            cMpCheckParameters mpCheckParameters = new cMpCheckParameters();
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory(mpCheckParameters);
            cProgramChecks target = new cProgramChecks(mpCheckParameters);

            MpParameters.Init(category.Process);
            MpParameters.Category = category;


            /* Helper Values */
            DateTime evalB = new DateTime(2020, 3, 17);
            DateTime evalE = new DateTime(2020, 6, 18);

            string prgCd = "ARP";
            string prgClassCd = "P1";
            string prgParameterCd = "SO2";
            string prgParameterDescription = "SO2 (or SO2M for an LME Unit)";

            string untMonLocId = "UntLocId";
            string untMethodParameterCd = "SO2";
            DateTime untRangeBegin = new DateTime(2020, 3, 17, 23, 0, 0);
            DateTime untRangeEnd = new DateTime(2020, 4, 17, 22, 0, 0);

            string cs1Name = "CS1";
            string cs1MonLocId = "Cs1LocId";
            string cs1MethodParameterCd = "SO2";
            DateTime cs1RangeBegin = new DateTime(2020, 4, 17, 23, 0, 0);
            DateTime cs1RangeEnd = new DateTime(2020, 5, 17, 22, 0, 0);

            string cs2Name = "CS2";
            string cs2MonLocId = "Cs2LocId";
            string cs2MethodParameterCd = "SO2M";
            DateTime cs2RangeBegin = new DateTime(2020, 5, 17, 23, 0, 0);
            DateTime cs2RangeEnd = new DateTime(2020, 6, 18, 0, 0, 0);


            /* Input Parameter Values */
            string[] untMthList = { "Covers", "GapBefore", "GapAfter", "Missing", "Covers", "Covers", "Covers", "Covers", "Covers", "Covers", "Covers", "Covers", "Missing", "Missing" };
            string[] cs1UscList = { "Active", "Active", "Active", "Active", "EndOverlap", "BeginOverlap", "EndedBefore", "EndedAfter", "Missing", "Active", "Active", "Active", "Missing", "Active" };
            string[] cs1MthList = { "Covers", "Covers", "Covers", "Covers", "Covers", "Covers", "Covers", "Covers", "Covers", "GapBefore", "GapAfter", "Missing", "Covers", "Missing" };
            string[] cs2UscList = { "Active", "Active", "Active", "Active", "Active", "Active", "Active", "Active", "Active", "Active", "Active", "Active", "Missing", "Active" };
            string[] cs2MthList = { "Covers", "Covers", "Covers", "Covers", "Covers", "Covers", "Covers", "Covers", "Covers", "Covers", "Covers", "Covers", "Covers", "Missing" };

            /* Expected Values */
            string[] resultList = { null, "B", "B", "B", null, null, "B", "B", "B", "B", "B", "B", "A", "A" };

            /* Test Case Count */
            int caseCount = 14;

            /* Check array lengths */
            Assert.AreEqual(caseCount, untMthList.Length, "untMthList length");
            Assert.AreEqual(caseCount, cs1UscList.Length, "cs1UscList length");
            Assert.AreEqual(caseCount, cs1MthList.Length, "cs1MthList length");
            Assert.AreEqual(caseCount, cs2UscList.Length, "cs2UscList length");
            Assert.AreEqual(caseCount, cs2MthList.Length, "cs2MthList length");
            Assert.AreEqual(caseCount, resultList.Length, "resultList length");


            /* Setup Unchanging Input Parameters */
            PopulateProgramCrosschecksInit();

            MpParameters.CurrentProgramParameter = new UnitProgramParameter(prgCd: prgCd, classCd: prgClassCd, parameterCd: prgParameterCd, requiredInd: 1, monLocId: untMonLocId);
            MpParameters.CurrentProgramParameterActive = true;
            MpParameters.ProgramParameterEvaluationBeginDate = evalB.Date;
            MpParameters.ProgramParameterEvaluationEndDate = evalE.Date;


            /* Set Value Action */
            Func<string, DateTime, DateTime, DateTime> setMethodBeginHour = (type, rangeHourBegin, rangeHourEnd) => type == "GapBefore" ? rangeHourBegin.AddHours(1) :
                                                                                                                    type == "Missing" ? rangeHourEnd :
                                                                                                                    rangeHourBegin;
            Func<string, DateTime, DateTime, DateTime> setMethodEndHour = (type, rangeHourBegin, rangeHourEnd) => type == "GapAfter" ? rangeHourEnd.AddHours(-1) : 
                                                                                                                  type == "Missing" ? rangeHourBegin : 
                                                                                                                  rangeHourEnd;

            Func<string, DateTime, DateTime, DateTime> setUscBeginHour = (type, rangeHourBegin, rangeHourEnd) => type == "BeginOverlap" ? rangeHourEnd.Date :
                                                                                                                 type == "EndOverlap" ? rangeHourBegin.Date.AddDays(-1) :
                                                                                                                 type == "EndedAfter" ? rangeHourEnd.Date.AddDays(1) :
                                                                                                                 type == "EndedBefore" ? rangeHourBegin.Date.AddDays(-2) :
                                                                                                                 type == "Missing" ? rangeHourEnd.Date :
                                                                                                                 evalB.Date;
            Func<string, DateTime, DateTime, DateTime?> setUscEndHour = (type, rangeHourBegin, rangeHourEnd) => type == "BeginOverlap" ? rangeHourEnd.Date.AddDays(1) :
                                                                                                                type == "EndOverlap" ? rangeHourBegin.Date :
                                                                                                                type == "EndedAfter" ? rangeHourEnd.Date.AddDays(2) :
                                                                                                                type == "EndedBefore" ? rangeHourBegin.Date.AddDays(-1) :
                                                                                                                type == "Missing" ? rangeHourBegin.Date :
                                                                                                                (DateTime?)null;



            /* Run Cases */
            for (int caseDex = 0; caseDex < caseCount; caseDex++)
            {
                /* Initialize Input Parameters */
                MpParameters.FacilityMethodRecords = new CheckDataView<VwMonitorMethodRow>();
                {
                    DateTime beginHour, endHour;

                    if ((beginHour = setMethodBeginHour(untMthList[caseDex], untRangeBegin, untRangeEnd)) <= (endHour = setMethodEndHour(untMthList[caseDex], untRangeBegin, untRangeEnd)))
                        MpParameters.FacilityMethodRecords.Add(SetValues.DateHour(new VwMonitorMethodRow(monLocId: untMonLocId, parameterCd: untMethodParameterCd), beginHour, endHour));

                    if ((beginHour = setMethodBeginHour(cs1MthList[caseDex], cs1RangeBegin, cs1RangeEnd)) <= (endHour = setMethodEndHour(cs1MthList[caseDex], cs1RangeBegin, cs1RangeEnd)))
                        MpParameters.FacilityMethodRecords.Add(SetValues.DateHour(new VwMonitorMethodRow(monLocId: cs1MonLocId, parameterCd: cs1MethodParameterCd), beginHour, endHour));

                    if ((beginHour = setMethodBeginHour(cs2MthList[caseDex], cs2RangeBegin, cs2RangeEnd)) <= (endHour = setMethodEndHour(cs2MthList[caseDex], cs2RangeBegin, cs2RangeEnd)))
                        MpParameters.FacilityMethodRecords.Add(SetValues.DateHour(new VwMonitorMethodRow(monLocId: cs2MonLocId, parameterCd: cs2MethodParameterCd), beginHour, endHour));
                }
                MpParameters.UnitStackConfigurationRecords = new CheckDataView<VwUnitStackConfigurationRow>();
                {
                    DateTime? beginDate, endDate;

                    if ((beginDate = setUscBeginHour(cs1UscList[caseDex], evalB, evalE)) <= (endDate = setUscEndHour(cs1UscList[caseDex], evalB, evalE).Default(DateTime.MaxValue)))
                        MpParameters.UnitStackConfigurationRecords.Add(new VwUnitStackConfigurationRow(monLocId: untMonLocId, stackName: cs1Name, stackPipeMonLocId: cs1MonLocId, beginDate: beginDate, endDate: endDate));

                    if ((beginDate = setUscBeginHour(cs2UscList[caseDex], evalB, evalE)) <= (endDate = setUscEndHour(cs2UscList[caseDex], evalB, evalE).Default(DateTime.MaxValue)))
                        MpParameters.UnitStackConfigurationRecords.Add(new VwUnitStackConfigurationRow(monLocId: untMonLocId, stackName: cs2Name, stackPipeMonLocId: cs2MonLocId, beginDate: beginDate, endDate: endDate));
                }

                /* Initialize Input Parameters */
                MpParameters.ProgramMethodParameterDescription = null;


                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                /* Initialize variables needed to run the check. */
                bool log = false;
                string actual;

                /* Run Check */
                actual = target.PROGRAM11(category, ref log);

                /* Check Results */
                Assert.AreEqual(string.Empty, actual, $"actual {caseDex}");
                Assert.AreEqual(false, log, $"log {caseDex}");
                Assert.AreEqual(resultList[caseDex], category.CheckCatalogResult, $"category.CheckCatalogResult {caseDex}");
                Assert.AreEqual(prgParameterDescription, MpParameters.ProgramMethodParameterDescription, $"ProgramMethodParameterDescription {caseDex}");
            }
        }

        /// <summary>
        /// Performs Span Testing where only multiple stacks having covering methods.
        /// 
        /// 
        /// 
        ///      |             MS1          |             MS2         ||
        /// | ## | USC          | Method    | USC          | Method   || Result || Note
        /// |  0 | Active       | Covers    | Active       | Covers   || null   || Methods span.
        /// |  1 | EndOverlap   | Covers    | Active       | Covers   || null   || Common 1 USC ended before.
        /// |  2 | BeginOverlap | Covers    | Active       | Covers   || null   || Common 1 USC ended after.
        /// |  3 | EndedBefore  | Covers    | Active       | Covers   || null   || Common 1 USC ended before.
        /// |  4 | EndedAfter   | Covers    | Active       | Covers   || null   || Common 1 USC ended after.
        /// |  5 | Missing      | Covers    | Active       | Covers   || null   || Common 1 USC missing.
        /// |  6 | Active       | GapBefore | Active       | Covers   || B      || Common 1 has Method gap at beginning of range.
        /// |  7 | Active       | GapAfter  | Active       | Covers   || B      || Common 1 has Method gap at end of range.
        /// |  8 | Active       | Missing   | Active       | Covers   || B      || Common 1 Method is missing.
        /// |  9 | Missing      | Covers    | Missing      | Covers   || A      || No connected common.
        /// | 10 | Active       | Missing   | Active       | Missing  || A      || No common methods.
        /// 
        /// </summary>
        [TestMethod]
        public void Program11_MultipleOnly()
        {
            /* Initialize objects generally needed for testing checks. */
            cMpCheckParameters mpCheckParameters = new cMpCheckParameters();
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory(mpCheckParameters);
            cProgramChecks target = new cProgramChecks(mpCheckParameters);

            MpParameters.Init(category.Process);
            MpParameters.Category = category;


            /* Helper Values */
            DateTime evalB = new DateTime(2020, 3, 17);
            DateTime evalE = new DateTime(2020, 6, 18);

            string prgCd = "ARP";
            string prgClassCd = "P1";
            string prgParameterCd = "SO2";
            string prgParameterDescription = "SO2 (or SO2M for an LME Unit)";

            string untMonLocId = "UntLocId";

            string cs1Name = "MS1";
            string cs1MonLocId = "Ms1LocId";
            string cs1MethodParameterCd = "SO2";
            DateTime cs1RangeBegin = new DateTime(2020, 3, 17, 23, 0, 0);
            DateTime cs1RangeEnd = new DateTime(2020, 6, 18, 0, 0, 0);

            string cs2Name = "MS2";
            string cs2MonLocId = "Ms2LocId";
            string cs2MethodParameterCd = "SO2M";
            DateTime cs2RangeBegin = new DateTime(2020, 3, 17, 23, 0, 0);
            DateTime cs2RangeEnd = new DateTime(2020, 6, 18, 0, 0, 0);


            /* Input Parameter Values */
            string[] cs1UscList = { "Active", "EndOverlap", "BeginOverlap", "EndedBefore", "EndedAfter", "Missing", "Active", "Active", "Active", "Missing", "Active" };
            string[] cs1MthList = { "Covers", "Covers", "Covers", "Covers", "Covers", "Covers", "GapBefore", "GapAfter", "Missing", "Covers", "Missing" };
            string[] cs2UscList = { "Active", "Active", "Active", "Active", "Active", "Active", "Active", "Active", "Active", "Missing", "Active" };
            string[] cs2MthList = { "Covers", "Covers", "Covers", "Covers", "Covers", "Covers", "Covers", "Covers", "Covers", "Covers", "Missing" };

            /* Expected Values */
            string[] resultList = { null, null, null, null, null, null, "B", "B", "B", "A", "A" };

            /* Test Case Count */
            int caseCount = 11;

            /* Check array lengths */
            Assert.AreEqual(caseCount, cs1UscList.Length, "cs1UscList length");
            Assert.AreEqual(caseCount, cs1MthList.Length, "cs1MthList length");
            Assert.AreEqual(caseCount, cs2UscList.Length, "cs2UscList length");
            Assert.AreEqual(caseCount, cs2MthList.Length, "cs2MthList length");
            Assert.AreEqual(caseCount, resultList.Length, "resultList length");


            /* Setup Unchanging Input Parameters */
            PopulateProgramCrosschecksInit();

            MpParameters.CurrentProgramParameter = new UnitProgramParameter(prgCd: prgCd, classCd: prgClassCd, parameterCd: prgParameterCd, requiredInd: 1, monLocId: untMonLocId);
            MpParameters.CurrentProgramParameterActive = true;
            MpParameters.ProgramParameterEvaluationBeginDate = evalB.Date;
            MpParameters.ProgramParameterEvaluationEndDate = evalE.Date;


            /* Set Value Action */
            Func<string, DateTime, DateTime, DateTime> setMethodBeginHour = (type, rangeHourBegin, rangeHourEnd) => type == "GapBefore" ? rangeHourBegin.AddHours(1) :
                                                                                                                    type == "Missing" ? rangeHourEnd :
                                                                                                                    rangeHourBegin;
            Func<string, DateTime, DateTime, DateTime> setMethodEndHour = (type, rangeHourBegin, rangeHourEnd) => type == "GapAfter" ? rangeHourEnd.AddHours(-1) :
                                                                                                                  type == "Missing" ? rangeHourBegin :
                                                                                                                  rangeHourEnd;

            Func<string, DateTime, DateTime, DateTime> setUscBeginHour = (type, rangeHourBegin, rangeHourEnd) => type == "BeginOverlap" ? rangeHourEnd.Date :
                                                                                                                 type == "EndOverlap" ? rangeHourBegin.Date.AddDays(-1) :
                                                                                                                 type == "EndedAfter" ? rangeHourEnd.Date.AddDays(1) :
                                                                                                                 type == "EndedBefore" ? rangeHourBegin.Date.AddDays(-2) :
                                                                                                                 type == "Missing" ? rangeHourEnd.Date :
                                                                                                                 evalB.Date;
            Func<string, DateTime, DateTime, DateTime?> setUscEndHour = (type, rangeHourBegin, rangeHourEnd) => type == "BeginOverlap" ? rangeHourEnd.Date.AddDays(1) :
                                                                                                                type == "EndOverlap" ? rangeHourBegin.Date :
                                                                                                                type == "EndedAfter" ? rangeHourEnd.Date.AddDays(2) :
                                                                                                                type == "EndedBefore" ? rangeHourBegin.Date.AddDays(-1) :
                                                                                                                type == "Missing" ? rangeHourBegin.Date :
                                                                                                                (DateTime?)null;



            /* Run Cases */
            for (int caseDex = 0; caseDex < caseCount; caseDex++)
            {
                /* Initialize Input Parameters */
                MpParameters.FacilityMethodRecords = new CheckDataView<VwMonitorMethodRow>();
                {
                    DateTime beginHour, endHour;

                    if ((beginHour = setMethodBeginHour(cs1MthList[caseDex], cs1RangeBegin, cs1RangeEnd)) <= (endHour = setMethodEndHour(cs1MthList[caseDex], cs1RangeBegin, cs1RangeEnd)))
                        MpParameters.FacilityMethodRecords.Add(SetValues.DateHour(new VwMonitorMethodRow(monLocId: cs1MonLocId, parameterCd: cs1MethodParameterCd), beginHour, endHour));

                    if ((beginHour = setMethodBeginHour(cs2MthList[caseDex], cs2RangeBegin, cs2RangeEnd)) <= (endHour = setMethodEndHour(cs2MthList[caseDex], cs2RangeBegin, cs2RangeEnd)))
                        MpParameters.FacilityMethodRecords.Add(SetValues.DateHour(new VwMonitorMethodRow(monLocId: cs2MonLocId, parameterCd: cs2MethodParameterCd), beginHour, endHour));
                }
                MpParameters.UnitStackConfigurationRecords = new CheckDataView<VwUnitStackConfigurationRow>();
                {
                    DateTime? beginDate, endDate;

                    if ((beginDate = setUscBeginHour(cs1UscList[caseDex], cs1RangeBegin, cs1RangeEnd)) <= (endDate = setUscEndHour(cs1UscList[caseDex], cs1RangeBegin, cs1RangeEnd).Default(DateTime.MaxValue)))
                        MpParameters.UnitStackConfigurationRecords.Add(new VwUnitStackConfigurationRow(monLocId: untMonLocId, stackName: cs1Name, stackPipeMonLocId: cs1MonLocId, beginDate: beginDate, endDate: endDate));

                    if ((beginDate = setUscBeginHour(cs2UscList[caseDex], cs2RangeBegin, cs2RangeEnd)) <= (endDate = setUscEndHour(cs2UscList[caseDex], cs2RangeBegin, cs2RangeEnd).Default(DateTime.MaxValue)))
                        MpParameters.UnitStackConfigurationRecords.Add(new VwUnitStackConfigurationRow(monLocId: untMonLocId, stackName: cs2Name, stackPipeMonLocId: cs2MonLocId, beginDate: beginDate, endDate: endDate));
                }

                /* Initialize Output Parameters */
                MpParameters.ProgramMethodParameterDescription = null;


                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                /* Initialize variables needed to run the check. */
                bool log = false;
                string actual;

                /* Run Check */
                actual = target.PROGRAM11(category, ref log);

                /* Check Results */
                Assert.AreEqual(string.Empty, actual, $"actual {caseDex}");
                Assert.AreEqual(false, log, $"log {caseDex}");
                Assert.AreEqual(resultList[caseDex], category.CheckCatalogResult, $"category.CheckCatalogResult {caseDex}");
                Assert.AreEqual(prgParameterDescription, MpParameters.ProgramMethodParameterDescription, $"ProgramMethodParameterDescription {caseDex}");
            }
        }

        /// <summary>
        /// Performs range checks for MS including handling MS USC beginning or ending on day that MS should start or end covering.  The USC begin and end test that at least
        /// one MS completely covers the begin or the end, while the others just have to begin or end on the day.
        /// 
        /// 
        /// Date Ranges:
        /// 
        ///     Evaluation Begin    : 2020-03-17
        ///     Evaluation End      : 2020-06-18
        /// 
        ///     UN1 Method Begin    : 2020-03-17 23
        ///     CS4 Method End      : 2020-06-18 00
        ///     CS4 USC Begin       : 2020-01-01
        ///     CS4 USC End         : null
        ///     
        ///     Loc     Method Begin    Method End      USC Begin       USC End     
        ///     UN1     2020-03-17 23   2020-04-17 22
        ///     CS4     2020-05-17 23   2020-06-18 00   2020-01-01      null
        /// 
        /// 
        /// 
        ///      |  UN1 Method   |          MS2 Method           |          MS3 Method           |  CS4 Method   |         MS2 USC         |         MS3 USC         ||
        /// | ## |    EndDate    | BeginDate     | EndDate       | BeginDate     | EndDate       |   BeginDate   | BeginDate  | EndDate    | BeginDate  | EndDate    || Result || Notes
        /// | -- | ------------- | ------------- | ------------- | ------------- | ------------- | ------------- | ---------- | ---------- | ---------- | ---------- || ------ || ------------------------------------------------------------------------------------------------------------
        /// |  0 | 2020-04-17 22 | 2020-04-17 23 | 2020-05-17 22 | 2020-04-17 23 | 2020-05-17 22 | 2020-05-17 23 | 2020-01-01 | null       | 2020-01-01 | null       || null   || Both MS cover the MS period, and USC are always active.
        /// |  1 | 2020-04-17 22 | null          | null          | 2020-04-17 23 | 2020-05-17 22 | 2020-05-17 23 | 2020-01-01 | null       | 2020-01-01 | null       || B      || Only MS3 covers the MS period, and USC are always active.
        /// |  2 | 2020-04-17 22 | 2020-04-17 23 | 2020-05-17 22 | null          | null          | 2020-05-17 23 | 2020-01-01 | null       | 2020-01-01 | null       || B      || Only MS2 covers the MS period, and USC are always active.
        /// |  3 | 2020-04-17 22 | null          | null          | null          | null          | 2020-05-17 23 | 2020-01-01 | null       | 2020-01-01 | null       || B      || Neither MS cover the MS period, and USC are always active.
        /// |  4 | 2020-04-17 22 | 2020-04-18 00 | 2020-05-17 22 | 2020-04-17 23 | 2020-05-17 22 | 2020-05-17 23 | 2020-01-01 | null       | 2020-01-01 | null       || B      || MS2 has a gap a the beginning of the MS period, MS3 covers period, and USC are always active.
        /// |  5 | 2020-04-17 22 | 2020-04-17 23 | 2020-05-17 21 | 2020-04-17 23 | 2020-05-17 22 | 2020-05-17 23 | 2020-01-01 | null       | 2020-01-01 | null       || B      || MS2 has a gap a the end of the MS period, MS3 covers period, and USC are always active.
        /// |  6 | 2020-04-17 22 | 2020-04-17 23 | 2020-05-17 22 | 2020-04-18 00 | 2020-05-17 22 | 2020-05-17 23 | 2020-01-01 | null       | 2020-01-01 | null       || B      || MS3 has a gap a the beginning of the MS period, MS2 covers period, and USC are always active.
        /// |  7 | 2020-04-17 22 | 2020-04-17 23 | 2020-05-17 22 | 2020-04-17 23 | 2020-05-17 21 | 2020-05-17 23 | 2020-01-01 | null       | 2020-01-01 | null       || B      || MS3 has a gap a the end of the MS period, MS2 covers period, and USC are always active.
        /// |  8 | 2020-04-17 22 | 2020-04-18 00 | 2020-05-17 22 | null          | null          | 2020-05-17 23 | 2020-01-01 | null       | 2020-01-01 | null       || B      || MS2 has a gap a the beginning of the MS period, MS3 does not cover period, and USC are always active.
        /// |  9 | 2020-04-17 22 | 2020-04-17 23 | 2020-05-17 21 | null          | null          | 2020-05-17 23 | 2020-01-01 | null       | 2020-01-01 | null       || B      || MS2 has a gap a the end of the MS period, MS3 does not cover period, and USC are always active.
        /// | 10 | 2020-04-17 00 | 2020-04-17 23 | 2020-05-17 22 | 2020-04-17 01 | 2020-05-17 22 | 2020-05-17 23 | 2020-04-17 | null       | 2020-04-17 | null       || null   || USC begins on Method Begin Date for both MS, MS3 covers MS period, and MS2 method begins on hour 23.
        /// | 11 | 2020-04-17 00 | 2020-04-17 23 | 2020-05-17 22 | 2020-04-17 02 | 2020-05-17 22 | 2020-05-17 23 | 2020-04-17 | null       | 2020-04-17 | null       || B      || USC begins on Method Begin Date for both MS, MS3 does not cover MS period, and MS2 method begins on hour 23.
        /// | 12 | 2020-04-17 00 | 2020-04-18 00 | 2020-05-17 22 | 2020-04-17 01 | 2020-05-17 22 | 2020-05-17 23 | 2020-04-17 | null       | 2020-04-17 | null       || B      || USC begins on Method Begin Date for both MS, MS3 covers MS period, and MS2 method begins on hour 0 of the next day.
        /// | 13 | 2020-04-17 00 | 2020-04-17 01 | 2020-05-17 00 | 2020-04-17 01 | 2020-05-17 22 | 2020-05-17 23 | 2020-01-01 | 2020-05-17 | 2020-01-01 | 2020-05-17 || null   || USC ends on Method End Date for both MS, MS3 covers MS period, and MS2 method ends on hour 0.
        /// | 14 | 2020-04-17 00 | 2020-04-17 01 | 2020-05-17 00 | 2020-04-17 01 | 2020-05-17 21 | 2020-05-17 23 | 2020-01-01 | 2020-05-17 | 2020-01-01 | 2020-05-17 || B      || USC ends on Method End Date for both MS, MS3 does not cover MS period, and MS2 method ends on hour 0.
        /// | 15 | 2020-04-17 00 | 2020-04-17 01 | 2020-05-16 23 | 2020-04-17 01 | 2020-05-17 22 | 2020-05-17 23 | 2020-01-01 | 2020-05-17 | 2020-01-01 | 2020-05-17 || B      || USC ends on Method End Date for both MS, MS3 covers MS period, and MS2 method ends on hour 23 of the previous day.
        /// </summary>
        [TestMethod]
        public void Program11_MultipleWithUnitAndCommon()
        {
            /* Initialize objects generally needed for testing checks. */
            cMpCheckParameters mpCheckParameters = new cMpCheckParameters();
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory(mpCheckParameters);
            cProgramChecks target = new cProgramChecks(mpCheckParameters);

            MpParameters.Init(category.Process);
            MpParameters.Category = category;


            /* Helper Values */
            DateTime evalB = new DateTime(2020, 3, 17);
            DateTime evalE = new DateTime(2020, 6, 18);

            DateTime? apr17_00 = new DateTime(2020, 4, 17, 0, 0, 0);
            DateTime? apr17_01 = new DateTime(2020, 4, 17, 1, 0, 0);
            DateTime? apr17_02 = new DateTime(2020, 4, 17, 2, 0, 0);
            DateTime? apr17_22 = new DateTime(2020, 4, 17, 22, 0, 0);
            DateTime? apr17_23 = new DateTime(2020, 4, 17, 23, 0, 0);
            DateTime? apr18_00 = new DateTime(2020, 4, 18, 0, 0, 0);

            DateTime? may16_23 = new DateTime(2020, 5, 16, 23, 0, 0);
            DateTime? may17_00 = new DateTime(2020, 5, 17, 0, 0, 0);
            DateTime? may17_01 = new DateTime(2020, 5, 17, 1, 0, 0);
            DateTime? may17_21 = new DateTime(2020, 5, 17, 21, 0, 0);
            DateTime? may17_22 = new DateTime(2020, 5, 17, 22, 0, 0);
            DateTime? may17_23 = new DateTime(2020, 5, 17, 23, 0, 0);

            DateTime? jan01 = new DateTime(2020, 1, 1);
            DateTime? apr17 = new DateTime(2020, 4, 17);
            DateTime? may17 = new DateTime(2020, 5, 17);


            string prgCd = "ARP";
            string prgClassCd = "P1";
            string prgParameterCd = "SO2";
            string prgParameterDescription = "SO2 (or SO2M for an LME Unit)";

            string un1MonLocId = "UntLocId";
            string un1MethodParameterCd = "SO2";
            DateTime? un1MethodBegHour = new DateTime(2020, 3, 17, 23, 0, 0);

            string ms2Name = "MS2";
            string ms2MonLocId = "Ms2LocId";
            string ms2MethodParameterCd = "SO2";

            string ms3Name = "MS3";
            string ms3MonLocId = "Ms3LocId";
            string ms3MethodParameterCd = "SO2M";

            string cs4Name = "CS4";
            string cs4MonLocId = "Cs4LocId";
            string cs4MethodParameterCd = "SO2M";
            DateTime? cs4MethodEndHour = new DateTime(2020, 6, 18, 00, 0, 0);
            DateTime? cs4UscBegHour = new DateTime(2020, 1, 1);
            DateTime? cs4UscEndHour = null;


            /* Input Parameter Values */
            DateTime?[] un1MethodEndHours = { apr17_22, apr17_22, apr17_22, apr17_22, apr17_22, apr17_22, apr17_22, apr17_22, apr17_22, apr17_22,
                                              apr17_00, apr17_00, apr17_00, apr17_00, apr17_00, apr17_00 };
            DateTime?[] ms2MethodBegHours = { apr17_23, null, apr17_23, null, apr18_00, apr17_23, apr17_23, apr17_23, apr18_00, apr17_23,
                                              apr17_23, apr17_23, apr18_00, apr17_01, apr17_01, apr17_01 };
            DateTime?[] ms2MethodEndHours = { may17_22, null, may17_22, null, may17_22, may17_21, may17_22, may17_22, may17_22, may17_21,
                                              may17_22, may17_22, may17_22, may17_00, may17_00, may16_23 };
            DateTime?[] ms3MethodBegHours = { apr17_23, apr17_23, null, null, apr17_23, apr17_23, apr18_00, apr17_23, null, null,
                                              apr17_01, apr17_02, apr17_01, apr17_01, apr17_01, apr17_01 };
            DateTime?[] ms3MethodEndHours = { may17_22, may17_22, null, null, may17_22, may17_22, may17_22, may17_21, null, null,
                                              may17_22, may17_22, may17_22, may17_22, may17_21, may17_22 };
            DateTime?[] cs4MethodBegHours = { may17_23, may17_23, may17_23, may17_23, may17_23, may17_23, may17_23, may17_23, may17_23, may17_23,
                                              may17_23, may17_23, may17_23, may17_23, may17_23, may17_23 };

            DateTime?[] ms2UscBegDates = { jan01, jan01, jan01, jan01, jan01, jan01, jan01, jan01, jan01, jan01,
                                           apr17, apr17, apr17, jan01, jan01, jan01 };
            DateTime?[] ms2UscEndDates = { null, null, null, null, null, null, null, null, null, null,
                                           null, null, null, may17, may17, may17 };
            DateTime?[] ms3UscBegDates = { jan01, jan01, jan01, jan01, jan01, jan01, jan01, jan01, jan01, jan01,
                                           apr17, apr17, apr17, jan01, jan01, jan01 };
            DateTime?[] ms3UscEndDates = { null, null, null, null, null, null, null, null, null, null,
                                           null, null, null, may17, may17, may17 };

            /* Expected Values */
            string[] resultList = { null, "B", "B", "B", "B", "B", "B", "B", "B", "B",
                                    null, "B", "B", null, "B", "B" };

            /* Test Case Count */
            int caseCount = 16;

            /* Check array lengths */
            Assert.AreEqual(caseCount, un1MethodEndHours.Length, "un1MethodEndHours length");
            Assert.AreEqual(caseCount, ms2MethodBegHours.Length, "ms2MethodBegHours length");
            Assert.AreEqual(caseCount, ms2MethodEndHours.Length, "ms2MethodEndHours length");
            Assert.AreEqual(caseCount, ms3MethodBegHours.Length, "ms3MethodBegHours length");
            Assert.AreEqual(caseCount, ms3MethodEndHours.Length, "ms3MethodEndHours length");
            Assert.AreEqual(caseCount, cs4MethodBegHours.Length, "cs4MethodBegHours length");
            Assert.AreEqual(caseCount, ms2UscBegDates.Length, "ms2UscBegDates length");
            Assert.AreEqual(caseCount, ms2UscEndDates.Length, "ms2UscEndDates length");
            Assert.AreEqual(caseCount, ms3UscBegDates.Length, "ms3UscBegDates length");
            Assert.AreEqual(caseCount, ms3UscEndDates.Length, "ms3UscEndDates length");
            Assert.AreEqual(caseCount, resultList.Length, "resultList length");


            /* Setup Unchanging Input Parameters */
            PopulateProgramCrosschecksInit();

            MpParameters.CurrentProgramParameter = new UnitProgramParameter(prgCd: prgCd, classCd: prgClassCd, parameterCd: prgParameterCd, requiredInd: 1, monLocId: un1MonLocId);
            MpParameters.CurrentProgramParameterActive = true;
            MpParameters.ProgramParameterEvaluationBeginDate = evalB.Date;
            MpParameters.ProgramParameterEvaluationEndDate = evalE.Date;


            /* Run Cases */
            for (int caseDex = 0; caseDex < caseCount; caseDex++)
            {
                /* Initialize Input Parameters */
                MpParameters.FacilityMethodRecords = new CheckDataView<VwMonitorMethodRow>();
                {
                    MpParameters.FacilityMethodRecords.Add(SetValues.DateHour(new VwMonitorMethodRow(monLocId: un1MonLocId, parameterCd: un1MethodParameterCd), un1MethodBegHour, un1MethodEndHours[caseDex]));

                    if ((ms2MethodBegHours[caseDex] != null) || (ms2MethodEndHours[caseDex] != null))
                        MpParameters.FacilityMethodRecords.Add(SetValues.DateHour(new VwMonitorMethodRow(monLocId: ms2MonLocId, parameterCd: ms2MethodParameterCd), ms2MethodBegHours[caseDex], ms2MethodEndHours[caseDex]));

                    if ((ms3MethodBegHours[caseDex] != null) || (ms3MethodEndHours[caseDex] != null))
                        MpParameters.FacilityMethodRecords.Add(SetValues.DateHour(new VwMonitorMethodRow(monLocId: ms3MonLocId, parameterCd: ms3MethodParameterCd), ms3MethodBegHours[caseDex], ms3MethodEndHours[caseDex]));

                    MpParameters.FacilityMethodRecords.Add(SetValues.DateHour(new VwMonitorMethodRow(monLocId: cs4MonLocId, parameterCd: cs4MethodParameterCd), cs4MethodBegHours[caseDex], cs4MethodEndHour));
                }
                MpParameters.UnitStackConfigurationRecords = new CheckDataView<VwUnitStackConfigurationRow>();
                {
                    if ((ms2UscBegDates[caseDex] != null) || (ms2UscEndDates[caseDex] != null))
                        MpParameters.UnitStackConfigurationRecords.Add(new VwUnitStackConfigurationRow(monLocId: un1MonLocId, stackName: ms2Name, stackPipeMonLocId: ms2MonLocId, beginDate: ms2UscBegDates[caseDex], endDate: ms2UscEndDates[caseDex]));

                    if ((ms3UscBegDates[caseDex] != null) || (ms3UscEndDates[caseDex] != null))
                        MpParameters.UnitStackConfigurationRecords.Add(new VwUnitStackConfigurationRow(monLocId: un1MonLocId, stackName: ms3Name, stackPipeMonLocId: ms3MonLocId, beginDate: ms3UscBegDates[caseDex], endDate: ms3UscEndDates[caseDex]));

                    MpParameters.UnitStackConfigurationRecords.Add(new VwUnitStackConfigurationRow(monLocId: un1MonLocId, stackName: cs4Name, stackPipeMonLocId: cs4MonLocId, beginDate: cs4UscBegHour, endDate: cs4UscEndHour));
                }

                /* Initialize Output Parameters */
                MpParameters.ProgramMethodParameterDescription = null;


                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                /* Initialize variables needed to run the check. */
                bool log = false;
                string actual;

                /* Run Check */
                actual = target.PROGRAM11(category, ref log);

                /* Check Results */
                Assert.AreEqual(string.Empty, actual, $"actual {caseDex}");
                Assert.AreEqual(false, log, $"log {caseDex}");
                Assert.AreEqual(resultList[caseDex], category.CheckCatalogResult, $"category.CheckCatalogResult {caseDex}");
                Assert.AreEqual(prgParameterDescription, MpParameters.ProgramMethodParameterDescription, $"ProgramMethodParameterDescription {caseDex}");
            }
        }

        /// <summary>
        /// 
        /// 
        /// 
        /// Date Ranges:
        /// 
        ///     Evaluation Begin    : 2020-03-17
        ///     Evaluation End      : 2020-06-18
        /// 
        /// 
        ///      |          UN1 Method           |          MS2 Method           |         MS2 USC         ||
        /// | ## | BeginDate     | EndDate       | BeginDate     | EndDate       | BeginDate  | EndDate    || Result || Notes
        /// | -- | ------------- | ------------- | ------------- | ------------- | ---------- | ---------- || ------ || --------------------------------------------------------------------------------------------------------------------
        /// |  0 | 2020-03-17 00 | 2020-05-02 12 | 2020-05-02 13 | 2020-06-18 23 | 2020-01-01 | null       || null   || Unit and MS cover.  Result Type is None not invovled.
        /// |  1 | 2020-03-17 00 | 2020-05-02 12 | 2020-05-02 13 | 2020-06-17 23 | 2020-01-01 | null       || B      || Unit coveres, but MS only partially covers.  Result Type is None not invovled.
        /// |  2 | 2020-03-17 00 | 2020-05-02 12 | null          | null          | 2020-01-01 | null       || B      || Unit coveres, but MS covers nothing.  Result Type is None is invovled.
        /// |  3 | 2020-03-17 00 | 2020-05-02 11 | 2020-05-02 13 | 2020-06-18 23 | 2020-01-01 | null       || B      || Unit partially covers and MS covers.  Result Type is None not invovled.
        /// |  4 | 2020-03-17 00 | 2020-05-02 11 | 2020-05-02 13 | 2020-06-17 23 | 2020-01-01 | null       || B      || Unit and MS partially cover halves.  Result Type is None not invovled.
        /// |  5 | 2020-03-17 00 | 2020-05-02 11 | null          | null          | 2020-01-01 | null       || B      || Unit partially covers and MS covers nothing.  Result Type is None is invovled.
        /// |  6 | null          | null          | 2020-05-02 13 | 2020-06-18 23 | 2020-01-01 | null       || B      || Unit covers nothing, and MS covers half.  Result Type is None not invovled.
        /// |  7 | null          | null          | 2020-05-02 13 | 2020-06-17 23 | 2020-01-01 | null       || B      || Unit covers nothing, and MS partially covers half.  Result Type is None not invovled.
        /// |  8 | null          | null          | null          | null          | 2020-01-01 | null       || A      || Unit covers nothing, and MS covers m=nothing.  Result Type is None not invovled.
        /// |  9 | 2020-03-17 00 | 2020-05-02 12 | 2020-05-02 13 | 2020-06-18 23 | 2020-01-01 | 2020-05-02 || B      || Unit covers half, but although MS covers other half, it is not linked to the unit.  Result Type is None is invovled.
        /// | 10 | null          | null          | null          | null          | 2020-01-01 | 2020-05-02 || A      || Unit covers nothing, and MS covers half, but not linked to unit.  Result Type is None is invovled.
        /// </summary>
        [TestMethod]
        public void Program11_ResultTypeIsNone()
        {
            /* Initialize objects generally needed for testing checks. */
            cMpCheckParameters mpCheckParameters = new cMpCheckParameters();
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory(mpCheckParameters);
            cProgramChecks target = new cProgramChecks(mpCheckParameters);

            MpParameters.Init(category.Process);
            MpParameters.Category = category;


            /* Helper Values */
            DateTime evalB = new DateTime(2020, 3, 17);
            DateTime evalE = new DateTime(2020, 6, 18);

            DateTime? mar17_00 = new DateTime(2020, 3, 17, 0, 0, 0);
            DateTime? may02_11 = new DateTime(2020, 5, 2, 11, 0, 0);
            DateTime? may02_12 = new DateTime(2020, 5, 2, 12, 0, 0);
            DateTime? may02_13 = new DateTime(2020, 5, 2, 13, 0, 0);
            DateTime? jun17_23 = new DateTime(2020, 6, 17, 23, 0, 0);
            DateTime? jun18_23 = new DateTime(2020, 6, 18, 23, 0, 0);

            DateTime? jan01 = new DateTime(2020, 1, 1);
            DateTime? may02 = new DateTime(2020, 5, 2);


            string prgCd = "ARP";
            string prgClassCd = "P1";
            string prgParameterCd = "SO2";
            string prgParameterDescription = "SO2 (or SO2M for an LME Unit)";

            string un1MonLocId = "UntLocId";
            string un1MethodParameterCd = "SO2";
            DateTime? un1MethodBegHour = new DateTime(2020, 3, 17, 23, 0, 0);

            string ms2Name = "MS2";
            string ms2MonLocId = "Ms2LocId";
            string ms2MethodParameterCd = "SO2";


            /* Input Parameter Values */
            DateTime?[] un1MethodBegHours = { mar17_00, mar17_00, mar17_00, mar17_00, mar17_00, mar17_00, null, null, null, mar17_00, null };
            DateTime?[] un1MethodEndHours = { may02_12, may02_12, may02_12, may02_11, may02_11, may02_11, null, null, null, may02_12, null };
            DateTime?[] ms2MethodBegHours = { may02_13, may02_13, null, may02_13, may02_13, null, may02_13, may02_13, null, may02_13, null };
            DateTime?[] ms2MethodEndHours = { jun18_23, jun17_23, null, jun18_23, jun17_23, null, jun18_23, jun17_23, null, jun18_23, null };

            DateTime?[] ms2UscBegDates = { jan01, jan01, jan01, jan01, jan01, jan01, jan01, jan01, jan01, jan01, jan01 };
            DateTime?[] ms2UscEndDates = { null, null, null, null, null, null, null, null, null, may02, may02 };

            /* Expected Values */
            string[] resultList = { null, "B", "B", "B", "B", "B", "B", "B", "A", "B", "A" };

            /* Test Case Count */
            int caseCount = 11;

            /* Check array lengths */
            Assert.AreEqual(caseCount, un1MethodBegHours.Length, "un1MethodBegHours length");
            Assert.AreEqual(caseCount, un1MethodEndHours.Length, "un1MethodEndHours length");
            Assert.AreEqual(caseCount, ms2MethodBegHours.Length, "ms2MethodBegHours length");
            Assert.AreEqual(caseCount, ms2MethodEndHours.Length, "ms2MethodEndHours length");
            Assert.AreEqual(caseCount, resultList.Length, "resultList length");


            /* Setup Unchanging Input Parameters */
            PopulateProgramCrosschecksInit();

            MpParameters.CurrentProgramParameter = new UnitProgramParameter(prgCd: prgCd, classCd: prgClassCd, parameterCd: prgParameterCd, requiredInd: 1, monLocId: un1MonLocId);
            MpParameters.CurrentProgramParameterActive = true;
            MpParameters.ProgramParameterEvaluationBeginDate = evalB.Date;
            MpParameters.ProgramParameterEvaluationEndDate = evalE.Date;


            /* Run Cases */
            for (int caseDex = 0; caseDex < caseCount; caseDex++)
            {
                /* Initialize Input Parameters */
                MpParameters.FacilityMethodRecords = new CheckDataView<VwMonitorMethodRow>();
                {
                    if ((un1MethodBegHours[caseDex] != null) || (un1MethodEndHours[caseDex] != null))
                        MpParameters.FacilityMethodRecords.Add(SetValues.DateHour(new VwMonitorMethodRow(monLocId: un1MonLocId, parameterCd: un1MethodParameterCd), un1MethodBegHours[caseDex], un1MethodEndHours[caseDex]));

                    if ((ms2MethodBegHours[caseDex] != null) || (ms2MethodEndHours[caseDex] != null))
                        MpParameters.FacilityMethodRecords.Add(SetValues.DateHour(new VwMonitorMethodRow(monLocId: ms2MonLocId, parameterCd: ms2MethodParameterCd), ms2MethodBegHours[caseDex], ms2MethodEndHours[caseDex]));
                }
                MpParameters.UnitStackConfigurationRecords = new CheckDataView<VwUnitStackConfigurationRow>();
                {
                    MpParameters.UnitStackConfigurationRecords.Add(new VwUnitStackConfigurationRow(monLocId: un1MonLocId, stackName: ms2Name, stackPipeMonLocId: ms2MonLocId, beginDate: ms2UscBegDates[caseDex], endDate: ms2UscEndDates[caseDex]));
                }

                /* Initialize Output Parameters */
                MpParameters.ProgramMethodParameterDescription = null;


                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                /* Initialize variables needed to run the check. */
                bool log = false;
                string actual;

                /* Run Check */
                actual = target.PROGRAM11(category, ref log);

                /* Check Results */
                Assert.AreEqual(string.Empty, actual, $"actual {caseDex}");
                Assert.AreEqual(false, log, $"log {caseDex}");
                Assert.AreEqual(resultList[caseDex], category.CheckCatalogResult, $"category.CheckCatalogResult {caseDex}");
                Assert.AreEqual(prgParameterDescription, MpParameters.ProgramMethodParameterDescription, $"ProgramMethodParameterDescription {caseDex}");
            }
        }

        /// <summary>
        ///A test for PROGRAM11_Severity
        ///</summary>()
        [TestMethod()]
        public void PROGRAM11_Severity()
        {
            cMpCheckParameters mpCheckParameters = new cMpCheckParameters();
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory(mpCheckParameters);
            cProgramChecks target = new cProgramChecks(mpCheckParameters);

            MpParameters.Init(category.Process);
            MpParameters.Category = category;

            // Variables
            bool log = false;
            string actual;
            string[] testSeverityList = { null, "NONCRIT", "INFORM" };

            // Init Input
            //allowable stack types for program
            MpParameters.CrosscheckProgramparametertolocationtype = new CheckDataView<ProgramParameterToLocationTypeRow>
              (new ProgramParameterToLocationTypeRow(programParameterCd: "PARAMGOOD", commonLocationTypeList: "CS", multipleLocationTypeList: "MS"));
            //Valid MethodParam codes for ProgramParameter
            MpParameters.CrosscheckProgramparametertomethodparameter = new CheckDataView<ProgramParameterToMethodParameterRow>
                    (new ProgramParameterToMethodParameterRow(programParameterCd: "PARAMGOOD", methodParameterList: "PARAMGOOD"));
            MpParameters.ProgramMethodParameterDescription = "";
            // ParamCd/MethodCd - none for MATS
            MpParameters.CrosscheckProgramparametertomethodcode = new CheckDataView<ProgramParameterToMethodCodeRow>();
            // Severity Cd
            MpParameters.CurrentProgram = new VwLocationProgramRow(prgCd: "MATS", classCd: "CLASSGOOD");
            MpParameters.ProgramParameterEvaluationBeginDate = DateTime.Today.AddDays(-10);
            MpParameters.ProgramParameterEvaluationEndDate = DateTime.Today;

            // Method missing, severity codes
            {
                MpParameters.CurrentProgramParameter = new ECMPS.Checks.Data.Ecmps.CheckMp.Function.UnitProgramParameter
                        (parameterCd: "PARAMGOOD", requiredInd: 1, classCd: "CLASSGOOD", monLocId: "UNITLOC1");
                MpParameters.CurrentProgramParameterActive = true;
                MpParameters.UnitStackConfigurationRecords = new CheckDataView<VwUnitStackConfigurationRow>
                    (new VwUnitStackConfigurationRow(monLocId: "UNITLOC1", stackPipeMonLocId: "SPLOC1", stackName: "CS1", stackPipeId: "GUID",
                    beginDate: DateTime.Today.AddDays(-10), endDate: DateTime.Today));
                MpParameters.FacilityMethodRecords = new CheckDataView<VwMonitorMethodRow>(); //not found

                foreach (string testSeverityCode in testSeverityList)
                {
                    MpParameters.CrosscheckProgramparametertoseverity = new CheckDataView<ProgramParameterToSeverityRow>
                      (new ProgramParameterToSeverityRow(programParameterCd: "PARAMGOOD", severityCd: testSeverityCode));

                    // Init Output
                    category.CheckCatalogResult = null;

                    // Run Checks
                    actual = target.PROGRAM11(category, ref log);

                    //// Check Results
                    Assert.AreEqual(string.Empty, actual);
                    Assert.AreEqual(false, log);

                    if (testSeverityCode == "NONCRIT")
                    {
                        Assert.AreEqual("C", category.CheckCatalogResult, "Result");
                    }
                    else if (testSeverityCode == "INFORM")
                    {
                        Assert.AreEqual("E", category.CheckCatalogResult, "Result");
                    }
                    else
                    {
                        Assert.AreEqual("A", category.CheckCatalogResult, "Result");
                    }

                }

                //Incomplete, severity codes
                {
                    MpParameters.CurrentProgramParameter = new ECMPS.Checks.Data.Ecmps.CheckMp.Function.UnitProgramParameter
                            (parameterCd: "PARAMGOOD", requiredInd: 1, classCd: "CLASSGOOD", monLocId: "UNITLOC1");
                    MpParameters.CurrentProgramParameterActive = true;
                    MpParameters.UnitStackConfigurationRecords = new CheckDataView<VwUnitStackConfigurationRow>
                      (new VwUnitStackConfigurationRow(monLocId: "UNITLOC1", stackPipeMonLocId: "SPLOC1", stackName: "CS1", stackPipeId: "GUID", beginDate: DateTime.Today.AddDays(-10), endDate: DateTime.Today));
                    MpParameters.FacilityMethodRecords = new CheckDataView<VwMonitorMethodRow>
                    (new VwMonitorMethodRow(monLocId: "UNITLOC1", stackName: "CS1", parameterCd: "PARAMGOOD", methodCd: "METHODGOOD",
                        beginDate: DateTime.Today.AddDays(-5), beginHour: 0, beginDatehour: DateTime.Today.AddDays(-5), endDate: DateTime.Today, endHour: 23, endDatehour: DateTime.Today.AddHours(23)));

                    foreach (string testSeverityCode in testSeverityList)
                    {
                        MpParameters.CrosscheckProgramparametertoseverity = new CheckDataView<ProgramParameterToSeverityRow>
                          (new ProgramParameterToSeverityRow(programParameterCd: "PARAMGOOD", severityCd: testSeverityCode));

                        // Init Output
                        category.CheckCatalogResult = null;

                        // Run Checks
                        actual = target.PROGRAM11(category, ref log);

                        //// Check Results
                        Assert.AreEqual(string.Empty, actual);
                        Assert.AreEqual(false, log);
                        if (testSeverityCode == "NONCRIT")
                        {
                            Assert.AreEqual("D", category.CheckCatalogResult, "Result");
                        }
                        else if (testSeverityCode == "INFORM")
                        {
                            Assert.AreEqual("F", category.CheckCatalogResult, "Result");
                        }
                        else
                        {
                            Assert.AreEqual("B", category.CheckCatalogResult, "Result");
                        }

                    }

                }
            }
        }

        /// <summary>
        /// Program Code, Required Ind, Class Cd and Active combinations.
        /// </summary>
        [TestMethod()]
        public void Program11_ProgramReqClassActiveCombos()
        {
            /* Setup Checks, Category and Parameters */
            cProgramChecks target = new cProgramChecks(new cMpCheckParameters()); // Old instantiated parameters are not used
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            MpParameters.Init(category.Process);
            MpParameters.Category = category;


            /* Setup Crosscheck Parameters */
            MpParameters.CrosscheckProgramparametertolocationtype = new CheckDataView<ProgramParameterToLocationTypeRow>();
            MpParameters.CrosscheckProgramparametertomethodcode = new CheckDataView<ProgramParameterToMethodCodeRow>();
            MpParameters.CrosscheckProgramparametertomethodparameter = new CheckDataView<ProgramParameterToMethodParameterRow>();
            MpParameters.CrosscheckProgramparametertoseverity = new CheckDataView<ProgramParameterToSeverityRow>();

            /* Initialize General Parameters */
            MpParameters.FacilityMethodRecords = new CheckDataView<VwMonitorMethodRow>();
            MpParameters.ProgramParameterEvaluationBeginDate = new DateTime(2014, 1, 1);
            MpParameters.ProgramParameterEvaluationEndDate = new DateTime(2014, 12, 31);
            MpParameters.UnitStackConfigurationRecords = new CheckDataView<VwUnitStackConfigurationRow>();

            foreach (string prgCd in UnitTestStandardLists.ProgramCodeList)
                foreach (int? requiredInd in UnitTestStandardLists.IndicatorList)
                    foreach (string classCd in UnitTestStandardLists.ClassCodeList)
                        foreach (bool? active in UnitTestStandardLists.ValidList)
                        {
                            /*  Initialize Input Parameters*/
                            MpParameters.CurrentProgramParameter = new UnitProgramParameter(monLocId: "TestUnit", prgCd: prgCd, parameterCd: "PrgPar", requiredInd: requiredInd, classCd: classCd);
                            MpParameters.CurrentProgramParameterActive = active;

                            /* Initialize Output Parameters */
                            MpParameters.ProgramMethodParameterDescription = null;

                            /* Check Result Label */
                            string resultPrefix = string.Format("Required/Class/Active [Req: {0}, Class: {1}, Act: {2}]", requiredInd, classCd, active);

                            /* Expected Results */
                            string result = (prgCd.NotInList("MATS,NSPS4T") && (requiredInd == 1) && classCd.NotInList("N,NA,NB") && active.Default(false)) ? "A" : null;

                            /* Init Cateogry Result */
                            category.CheckCatalogResult = null;

                            /* Initialize variables needed to run the check. */
                            bool log = false;
                            string actual;

                            /* Run Check */
                            actual = target.PROGRAM11(category, ref log);

                            /* Check results */
                            Assert.AreEqual(string.Empty, actual, resultPrefix + ".Error");
                            Assert.AreEqual(false, log, resultPrefix + ".Log");

                            Assert.AreEqual(result, category.CheckCatalogResult, resultPrefix + ".Result");
                            Assert.AreEqual(null, MpParameters.ProgramMethodParameterDescription, resultPrefix + ".ProgramMethodParameterDescription");
                        }
        }

        /// <summary>
        /// Result Combinations for a single Unit
        /// 
        /// Test includes combinations of:
        /// 
        /// 1) Program Parameter
        /// 2) Severity Code
        /// 3) Method Parameter
        /// 4) Span Test Cases
        /// </summary>
        [TestMethod()]
        public void Program11_Unit_ResultCombos()
        {
            /* Setup Checks, Category and Parameters */
            cProgramChecks target = new cProgramChecks(new cMpCheckParameters()); // Old instantiated parameters are not used
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            MpParameters.Init(category.Process);
            MpParameters.Category = category;

            /* Method Parameter Cd and Location Type combinations */
            {
                /* Initialize General Variables */
                DateTime evalBeginDate = new DateTime(2014, 1, 1);
                DateTime evalEndDate = new DateTime(2014, 12, 31);
                string unitMonLocId = "UnitMonLocId";

                List<SpanTestCase> spanTestCases = new List<SpanTestCase>();
                {
                    /* The end of the period is adjusted to the 24th hour of the end date. */
                    spanTestCases.Add(new SpanTestCase(evalBeginDate, evalEndDate.AddHours(23), SpanPeriod.Hour, SpanCaseType.Spans));
                    spanTestCases.Add(new SpanTestCase(evalBeginDate, evalEndDate.AddHours(23), SpanPeriod.Hour, SpanCaseType.NotActive));
                    spanTestCases.Add(new SpanTestCase(evalBeginDate, evalEndDate.AddHours(23), SpanPeriod.Hour, SpanCaseType.GapBetween));
                    /* Begin gaps must be the 24th hour of the begin date */
                    spanTestCases.Add(new SpanTestCase(evalBeginDate.AddHours(23), evalEndDate, SpanPeriod.Hour, SpanCaseType.GapBefore));
                    /* End gaps must be the 1st hour of the end date */
                    spanTestCases.Add(new SpanTestCase(evalBeginDate, evalEndDate, SpanPeriod.Hour, SpanCaseType.GapAfter));
                }

                /* Setup Crosscheck Parameters */
                MpParameters.CrosscheckProgramparametertomethodcode = new CheckDataView<ProgramParameterToMethodCodeRow>();


                /* Initialize General Parameters */
                MpParameters.CurrentProgramParameterActive = true;
                MpParameters.ProgramParameterEvaluationBeginDate = evalBeginDate;
                MpParameters.ProgramParameterEvaluationEndDate = evalEndDate;
                MpParameters.UnitStackConfigurationRecords = new CheckDataView<VwUnitStackConfigurationRow>();

                foreach (ProgramParameter programParameter in UnitTestStandardLists.ProgramParameterRequiredList)
                {
                    string methodParameterList = GeneralUtilites.MethodParametersForProgramParameter(programParameter.ParameterCd);

                    MpParameters.CrosscheckProgramparametertolocationtype
                      = new CheckDataView<ProgramParameterToLocationTypeRow>
                        (
                          new ProgramParameterToLocationTypeRow(programParameterCd: programParameter.ParameterCd, commonLocationTypeList: "CS", multipleLocationTypeList: "MS")
                        );

                    MpParameters.CrosscheckProgramparametertomethodparameter
                      = new CheckDataView<ProgramParameterToMethodParameterRow>
                        (
                          new ProgramParameterToMethodParameterRow
                          (
                            programParameterCd: programParameter.ParameterCd,
                            methodParameterList: methodParameterList,
                            methodParameterDescription: string.Format("{0} Parameter Description", programParameter.ParameterCd)
                          )
                        );

                    foreach (string severityCd in UnitTestStandardLists.SeverityCodeList)
                    {
                        MpParameters.CrosscheckProgramparametertoseverity
                          = new CheckDataView<ProgramParameterToSeverityRow>
                            (
                              new ProgramParameterToSeverityRow(programParameterCd: programParameter.ParameterCd, severityCd: severityCd)
                            );

                        foreach (string methodParameterCd in UnitTestStandardLists.MethodParameterCodeList)
                            foreach (SpanTestCase spanTestCase in spanTestCases)
                            {
                                /*  Initialize Input Parameters*/
                                MpParameters.CurrentProgramParameter = new UnitProgramParameter(monLocId: unitMonLocId, prgCd: programParameter.PrgCd, parameterCd: programParameter.ParameterCd, requiredInd: 1, classCd: "A");

                                MpParameters.FacilityMethodRecords = new CheckDataView<VwMonitorMethodRow>
                                    (
                                      new VwMonitorMethodRow
                                      (
                                        monLocId: unitMonLocId,
                                        parameterCd: methodParameterCd,
                                        beginDatehour: spanTestCase.BeginRange.Begin, beginDate: spanTestCase.BeginRange.Begin.Value.Date, beginHour: spanTestCase.BeginRange.Begin.Value.Hour,
                                        endDatehour: spanTestCase.BeginRange.End, endDate: spanTestCase.BeginRange.End.Value.Date, endHour: spanTestCase.BeginRange.End.Value.Hour
                                      ),
                                      new VwMonitorMethodRow
                                      (
                                        monLocId: unitMonLocId,
                                        parameterCd: methodParameterCd,
                                        beginDatehour: spanTestCase.EndRange.Begin, beginDate: spanTestCase.EndRange.Begin.Value.Date, beginHour: spanTestCase.EndRange.Begin.Value.Hour,
                                        endDatehour: spanTestCase.EndRange.End, endDate: spanTestCase.EndRange.End.Value.Date, endHour: spanTestCase.EndRange.End.Value.Hour
                                      )
                                    );

                                /* Initialize Output Parameters */
                                MpParameters.ProgramMethodParameterDescription = null;

                                /* Check Result Label */
                                string resultPrefix = $"ResultCombinations [Prg: {programParameter.PrgCd}, Param: {programParameter.ParameterCd}, Sev: {severityCd}, MethP: {methodParameterCd}, Span: {spanTestCase}]";

                                /* Expected Results */
                                string result = null;
                                {
                                    if (programParameter.PrgCd.InList("MATS,NSPS4T"))
                                    {
                                        result = null;
                                    }
                                    else if (methodParameterCd.NotInList(methodParameterList))
                                    {
                                        switch (severityCd)
                                        {
                                            case "NONCRIT": result = "C"; break;
                                            case "INFORM": result = "E"; break;
                                            default: result = "A"; break;
                                        }
                                    }
                                    else
                                    {
                                        switch (spanTestCase.CaseType)
                                        {
                                            case SpanCaseType.NotActive:
                                                {
                                                    switch (severityCd)
                                                    {
                                                        case "NONCRIT": result = "C"; break;
                                                        case "INFORM": result = "E"; break;
                                                        default: result = "A"; break;
                                                    }
                                                }
                                                break;

                                            case SpanCaseType.GapBefore:
                                            case SpanCaseType.GapBetween:
                                            case SpanCaseType.GapAfter:
                                                {
                                                    switch (severityCd)
                                                    {
                                                        case "NONCRIT": result = "D"; break;
                                                        case "INFORM": result = "F"; break;
                                                        default: result = "B"; break;
                                                    }
                                                }
                                                break;
                                        }
                                    }
                                }

                                /* Init Category Result */
                                category.CheckCatalogResult = null;

                                /* Initialize variables needed to run the check. */
                                bool log = false;
                                string actual;

                                /* Run Check */
                                actual = target.PROGRAM11(category, ref log);

                                /* Check results */
                                Assert.AreEqual(string.Empty, actual, resultPrefix + ".Error");
                                Assert.AreEqual(false, log, resultPrefix + ".Log");

                                Assert.AreEqual(result, category.CheckCatalogResult, resultPrefix + ".Result");
                                Assert.AreEqual(null, MpParameters.ProgramMethodParameterDescription, resultPrefix + ".ProgramMethodParameterDescription");
                            }
                    }
                }
            }
        }


        #region Helper Methods

        private void PopulateProgramCrosschecksInit()
        {
            MpParameters.CrosscheckProgramparametertolocationtype = new CheckDataView<ProgramParameterToLocationTypeRow>
                (
                    new ProgramParameterToLocationTypeRow(programParameterCd: "CO2", commonLocationTypeList: "CS,CP", multipleLocationTypeList: "MS,MP"),
                    new ProgramParameterToLocationTypeRow(programParameterCd: "HCL", commonLocationTypeList: "CS", multipleLocationTypeList: "MS"),
                    new ProgramParameterToLocationTypeRow(programParameterCd: "HF", commonLocationTypeList: "CS", multipleLocationTypeList: "MS"),
                    new ProgramParameterToLocationTypeRow(programParameterCd: "HG", commonLocationTypeList: "CS", multipleLocationTypeList: "MS"),
                    new ProgramParameterToLocationTypeRow(programParameterCd: "HI", commonLocationTypeList: "CS,CP", multipleLocationTypeList: "MS,MP"),
                    new ProgramParameterToLocationTypeRow(programParameterCd: "NOX", commonLocationTypeList: "CS,CP", multipleLocationTypeList: "MS,MP"),
                    new ProgramParameterToLocationTypeRow(programParameterCd: "NOXR", commonLocationTypeList: "CS,CP", multipleLocationTypeList: "MS,MP"),
                    new ProgramParameterToLocationTypeRow(programParameterCd: "OP", commonLocationTypeList: "CS", multipleLocationTypeList: "MS"),
                    new ProgramParameterToLocationTypeRow(programParameterCd: "SO2", commonLocationTypeList: "CS,CP", multipleLocationTypeList: "MS,MP")
                );
            MpParameters.CrosscheckProgramparametertomethodcode = new CheckDataView<ProgramParameterToMethodCodeRow>
                (
                    new ProgramParameterToMethodCodeRow(programParameterCd: "OP", methodCdList: "LME")
                );
            MpParameters.CrosscheckProgramparametertomethodparameter = new CheckDataView<ProgramParameterToMethodParameterRow>
                (
                    new ProgramParameterToMethodParameterRow(programParameterCd: "CO2", methodParameterList: "CO2,CO2M", methodParameterDescription: "CO2 (or CO2M for an LME Unit)"),
                    new ProgramParameterToMethodParameterRow(programParameterCd: "H2O", methodParameterList: "H2O", methodParameterDescription: "H2O"),
                    new ProgramParameterToMethodParameterRow(programParameterCd: "HCL", methodParameterList: "HCLRE,HCLRH,SO2RE,SO2RH,MATSSUP", methodParameterDescription: "HCLRE (or HCLRH version for heat input based method, or equivalent SO2 method, or HCL Supplemental Method)"),
                    new ProgramParameterToMethodParameterRow(programParameterCd: "HF", methodParameterList: "HFRE,HFRH,MATSSUP", methodParameterDescription: "HFRE (or HFRH version for heat input based method or HF Supplemental Method)"),
                    new ProgramParameterToMethodParameterRow(programParameterCd: "HG", methodParameterList: "HGRE,HGRH,MATSSUP", methodParameterDescription: "HGRE (or HGRH version for heat input based method or HG Supplemental Method)"),
                    new ProgramParameterToMethodParameterRow(programParameterCd: "HI", methodParameterList: "HI,HIT", methodParameterDescription: "HI (or HIT for an LME Unit)"),
                    new ProgramParameterToMethodParameterRow(programParameterCd: "NOX", methodParameterList: "NOX,NOXM", methodParameterDescription: "NOX (or NOXM for an LME Unit)"),
                    new ProgramParameterToMethodParameterRow(programParameterCd: "NOXR", methodParameterList: "NOXM,NOXR", methodParameterDescription: "NOXR (or NOXM for an LME Unit)"),
                    new ProgramParameterToMethodParameterRow(programParameterCd: "OP", methodParameterList: "OP", methodParameterDescription: "OP"),
                    new ProgramParameterToMethodParameterRow(programParameterCd: "SO2", methodParameterList: "SO2,SO2M", methodParameterDescription: "SO2 (or SO2M for an LME Unit)")
                );
            MpParameters.CrosscheckProgramparametertoseverity = new CheckDataView<ProgramParameterToSeverityRow>
                (
                    new ProgramParameterToSeverityRow(programParameterCd: "CO2", severityCd: "CRIT1"),
                    new ProgramParameterToSeverityRow(programParameterCd: "HCL", severityCd: "INFORM"),
                    new ProgramParameterToSeverityRow(programParameterCd: "HF", severityCd: "INFORM"),
                    new ProgramParameterToSeverityRow(programParameterCd: "HG", severityCd: "INFORM"),
                    new ProgramParameterToSeverityRow(programParameterCd: "HI", severityCd: "CRIT1"),
                    new ProgramParameterToSeverityRow(programParameterCd: "NOX", severityCd: "CRIT1"),
                    new ProgramParameterToSeverityRow(programParameterCd: "NOXR", severityCd: "CRIT1"),
                    new ProgramParameterToSeverityRow(programParameterCd: "OP", severityCd: "NONCRIT"),
                    new ProgramParameterToSeverityRow(programParameterCd: "SO2", severityCd: "CRIT1")
                );
        }

        #endregion

        #endregion


        /// <summary>
        /// 
        /// EvalB (Date) : 2016-01-27
        /// EvalB (Hour) : 2016-01-27 23
        /// EvalE (Date) : 2016-06-17
        /// EvalE (Hour) : 2016-06-17 00
        /// EvalM (Date) : 2016-04-02
        /// EvalM (Hour) : 2016-04-02 16
        /// 
        /// |    |                 |        |           Unit Method           |               Stack/Pipe USC and Method               ||        ||
        /// | ## | PrgCd | ClassCd | Active | Meth    | BegHr     | EndHr     | SP   | UscBeg  | UscEnd  | MethCd | MethBeg | MethEnd || Result || Note
        /// |  0 | ARP   | A       | true   | HGRE    | EvalB     | null      | null | null    | null    | null   | null    | null    || null   || Not MATS.
        /// |  1 | MATS  | N       | true   | HGRE    | EvalB     | null      | null | null    | null    | null   | null    | null    || null   || Not affected.
        /// |  2 | MATS  | A       | false  | HGRE    | EvalB     | null      | null | null    | null    | null   | null    | null    || null   || Not active.
        /// |  3 | MATS  | A       | true   | HGRE    | EvalB     | null      | null | null    | null    | null   | null    | null    || null   || Unit method spans the program evaluation period.
        /// |  4 | MATS  | A       | true   | HGRE    | EvalB     | EvalE     | null | null    | null    | null   | null    | null    || null   || Unit method spans the program evaluation period.
        /// |  5 | MATS  | A       | true   | HGRE    | EvalB+1   | EvalE     | null | null    | null    | null   | null    | null    || B      || Method begins after start of evaluation period.
        /// |  6 | MATS  | A       | true   | HGRE    | EvalB     | EvalE-1   | null | null    | null    | null   | null    | null    || B      || Method ends before end of evaluation period.
        /// |  7 | MATS  | A       | true   | HGRE    | EvalE     | EvalE+2   | null | null    | null    | null   | null    | null    || B      || Method starts on last hour of evalution period.
        /// |  8 | MATS  | A       | true   | HGRE    | EvalB-2   | EvalB     | null | null    | null    | null   | null    | null    || B      || Method ends on first hour of evaluation period.
        /// |  9 | MATS  | A       | true   | HGRE    | EvalE23+1 | Eval23E+2 | null | null    | null    | null   | null    | null    || A      || Method starts after last hour of evalution period.
        /// | 10 | MATS  | A       | true   | HGRE    | EvalB00-2 | EvalB00-1 | null | null    | null    | null   | null    | null    || A      || Method ends before first hour of evaluation period.
        /// | 11 | MATS  | A       | true   | HGRH    | EvalB     | EvalE     | null | null    | null    | null   | null    | null    || null   || Unit method spans the program evaluation period.
        /// | 12 | MATS  | A       | true   | HCLRE   | EvalB     | EvalE     | null | null    | null    | null   | null    | null    || null   || Unit method spans the program evaluation period.
        /// | 13 | MATS  | A       | true   | HCLRH   | EvalB     | EvalE     | null | null    | null    | null   | null    | null    || null   || Unit method spans the program evaluation period.
        /// | 14 | MATS  | A       | true   | HFRE    | EvalB     | EvalE     | null | null    | null    | null   | null    | null    || null   || Unit method spans the program evaluation period.
        /// | 15 | MATS  | A       | true   | HFRH    | EvalB     | EvalE     | null | null    | null    | null   | null    | null    || null   || Unit method spans the program evaluation period.
        /// | 16 | MATS  | A       | true   | SO2RE   | EvalB     | EvalE     | null | null    | null    | null   | null    | null    || null   || Unit method spans the program evaluation period.
        /// | 17 | MATS  | A       | true   | SO2RH   | EvalB     | EvalE     | null | null    | null    | null   | null    | null    || null   || Unit method spans the program evaluation period.
        /// | 18 | MATS  | A       | true   | MATSSUP | EvalB     | EvalE     | null | null    | null    | null   | null    | null    || null   || Unit method spans the program evaluation period.
        /// | 19 | MATS  | A       | true   | HGRE    | EvalB     | EvalM     | CS1  | EvalB   | null    | HGRE   | EvalM+1 | EvalE   || null   || Unit and CS methods span the program evaluation period.
        /// | 20 | MATS  | A       | true   | HGRE    | EvalB     | EvalM     | MS1  | EvalB   | null    | HGRE   | EvalM+1 | EvalE   || null   || Unit and MS methods span the program evaluation period.
        /// | 21 | MATS  | A       | true   | HGRE    | EvalB     | EvalM     | CP1  | EvalB   | null    | HGRE   | EvalM+1 | EvalE   || B      || Unit and CP methods span the program evaluation period, but CP methods are not allowed to cover.
        /// | 22 | MATS  | A       | true   | HGRE    | EvalB     | EvalM     | MP1  | EvalB   | null    | HGRE   | EvalM+1 | EvalE   || B      || Unit and MP methods span the program evaluation period, but CP methods are not allowed to cover.
        /// | 23 | MATS  | A       | true   | HGRE    | EvalB     | EvalM     | CS1  | EvalB   | null    | HGRE   | EvalM+2 | EvalE   || B      || Unit and CS methods have gap between them but otherwise span the program evaluation period.
        /// | 24 | MATS  | A       | true   | HGRE    | EvalB     | EvalM     | CS1  | EvalE   | null    | HGRE   | EvalM+1 | EvalE   || null   || Unit and CS methods span the program evaluation period, and USC begins on last day of eval peirod.
        /// | 25 | MATS  | A       | true   | HGRE    | EvalB     | EvalM     | CS1  | EvalE+1 | null    | HGRE   | EvalM+1 | EvalE   || B      || Unit and CS methods span the program evaluation period, and USC begins after last day of eval peirod.
        /// | 26 | MATS  | A       | true   | HGRE    | EvalB     | EvalM     | CS1  | EvalB-2 | EvalB   | HGRE   | EvalM+1 | EvalE   || null   || Unit and CS methods span the program evaluation period, and USC ends on first day of eval peirod.
        /// | 27 | MATS  | A       | true   | HGRE    | EvalB     | EvalM     | CS1  | EvalB-2 | EvalB-1 | HGRE   | EvalM+1 | EvalE   || B      || Unit and CS methods span the program evaluation period, and USC ends befpre first day of eval peirod.
        /// | 28 | MATS  | A       | true   | HGRE    | EvalB     | EvalM     | CS1  | EvalB   | null    | HGRE   | EvalM+1 | EvalE-1 || B      || Unit and CS methods span the program evaluation period, and USC ends befpre first day of eval peirod.
        /// 
        /// </summary>
        [TestMethod()]
        public void Program12()
        {
            /* Initialize objects generally needed for testing checks. */
            cMpCheckParameters mpCheckParameters = new cMpCheckParameters();
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory(mpCheckParameters);
            cProgramChecks target = new cProgramChecks(mpCheckParameters);

            MpParameters.Init(category.Process);
            MpParameters.Category = category;


            /* Input Parameter Values */
            string unLocId = "unKey";
            string spLocId = "spKey";

            DateTime evalB = new DateTime(2016, 1, 27, 23, 0, 0);
            DateTime evalE = new DateTime(2016, 6, 17, 0, 0, 0);
            DateTime evalM = new DateTime(2016, 4, 2, 16, 0, 0);

            string[] prgList = { "ARP", "MATS", "MATS", "MATS", "MATS", "MATS", "MATS", "MATS", "MATS", "MATS",
                                 "MATS", "MATS", "MATS", "MATS", "MATS", "MATS", "MATS", "MATS", "MATS", "MATS",
                                 "MATS", "MATS", "MATS", "MATS", "MATS", "MATS", "MATS", "MATS", "MATS" };
            string[] classList = { "A", "N", "A", "A", "A", "A", "A", "A", "A", "A",
                                   "A", "A", "A", "A", "A", "A", "A", "A", "A", "A",
                                   "A", "A", "A", "A", "A", "A", "A", "A", "A" };
            bool[] activeList = { true, true, false, true, true, true, true, true, true, true,
                                  true, true, true, true, true, true, true, true, true, true,
                                  true, true, true, true, true, true, true, true, true };
            string[] unitMethList = { "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE",
                                      "HGRE", "HGRH", "HCLRE", "HCLRH", "HFRE", "HFRH", "SO2RE", "SO2RH", "MATSSUP", "HGRE",
                                      "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE" };
            DateTime?[] unitMethBegList = { evalB, evalB, evalB, evalB, evalB, evalB.AddHours(1), evalB, evalE, evalB.AddHours(-2), evalE.Date.AddDays(1),
                                            evalB.Date.AddDays(-1).AddHours(-1), evalB, evalB, evalB, evalB, evalB, evalB, evalB, evalB, evalB,
                                            evalB, evalB, evalB, evalB, evalB, evalB, evalB, evalB, evalB };
            DateTime?[] unitMethEndList = { null, null, null, null, evalE, evalE, evalE.AddHours(-1), evalE.AddHours(2), evalB, evalE.Date.AddDays(1).AddHours(1),
                                            evalB.Date.AddDays(-1), evalE, evalE, evalE, evalE, evalE, evalE, evalE, evalE, evalM,
                                            evalM, evalM, evalM, evalM, evalM, evalM, evalM, evalM, evalM };
            string[] spIdentList = { null, null, null, null, null, null, null, null, null, null,
                                     null, null, null, null, null, null, null, null, null, "CS1",
                                     "MS1", "CP1", "MP1", "CS1", "CS1", "CS1", "CS1", "CS1", "CS1" };
            DateTime?[] uscBegList = { null, null, null, null, null, null, null, null, null, null,
                                       null, null, null, null, null, null, null, null, null, evalB.Date,
                                       evalB.Date, evalB.Date, evalB.Date, evalB.Date, evalE.Date, evalE.Date.AddDays(1), evalB.Date.AddDays(-2), evalB.Date.AddDays(-2), evalB };
            DateTime?[] uscEndList = { null, null, null, null, null, null, null, null, null, null,
                                       null, null, null, null, null, null, null, null, null, evalB.Date,
                                       null, null, null, null, null, null, evalB.Date, evalB.Date.AddDays(-1), null };
            string[] spMethList = { null, null, null, null, null, null, null, null, null, null,
                                    null, null, null, null, null, null, null, null, null, "HGRE",
                                    "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE" };
            DateTime?[] spMethBegList = { null, null, null, null, null, null, null, null, null, null,
                                          null, null, null, null, null, null, null, null, null, evalM.AddHours(1),
                                          evalM.AddHours(1), evalM.AddHours(1), evalM.AddHours(1), evalM.AddHours(2), evalM.AddHours(1), evalM.AddHours(1), evalM.AddHours(1), evalM.AddHours(1), evalM.AddHours(1) };
            DateTime?[] spMethEndList = { null, null, null, null, null, null, null, null, null, null,
                                          null, null, null, null, null, null, null, null, null, evalE,
                                          evalE, evalE, evalE, evalE, evalE, evalE, evalE, evalE, evalE.AddHours(-1) };

            /* Expected Values */
            string[] expResultList = { null, null, null, null, null, "B", "B", "B", "B", "A",
                                       "A", null, null, null, null, null, null, null, null, null,
                                       null, "B", "B", "B", null, "B", null, "B", "B" };

            /* Test Case Count */
            int caseCount = 29;

            /* Check array lengths */
            Assert.AreEqual(caseCount, prgList.Length, "prgList length");
            Assert.AreEqual(caseCount, classList.Length, "classList length");
            Assert.AreEqual(caseCount, activeList.Length, "activeList length");
            Assert.AreEqual(caseCount, unitMethList.Length, "unitMethList length");
            Assert.AreEqual(caseCount, unitMethBegList.Length, "unitMethBegList length");
            Assert.AreEqual(caseCount, unitMethEndList.Length, "unitMethEndList length");
            Assert.AreEqual(caseCount, spIdentList.Length, "spIdentList length");
            Assert.AreEqual(caseCount, uscBegList.Length, "uscBegList length");
            Assert.AreEqual(caseCount, uscEndList.Length, "uscEndList length");
            Assert.AreEqual(caseCount, spMethList.Length, "spMethList length");
            Assert.AreEqual(caseCount, spMethBegList.Length, "spMethBegList length");
            Assert.AreEqual(caseCount, spMethEndList.Length, "spMethEndList length");
            Assert.AreEqual(caseCount, expResultList.Length, "expResultList length");

            /* Run Cases */
            for (int caseDex = 0; caseDex < caseCount; caseDex++)
            {
                /* Initialize Input Parameters */
                MpParameters.CombinedFacilityMethodRecords = new CheckDataView<CombinedMethods>();
                {
                    MpParameters.CombinedFacilityMethodRecords.Add(SetValues.DateHour(new CombinedMethods(monLocId: unLocId, crosscheckParameter: unitMethList[caseDex]), unitMethBegList[caseDex], unitMethEndList[caseDex]));

                    MpParameters.CombinedFacilityMethodRecords.Add(SetValues.DateHour(new CombinedMethods(monLocId: "CP0", crosscheckParameter: "HGRE"), evalB, null));
                    MpParameters.CombinedFacilityMethodRecords.Add(SetValues.DateHour(new CombinedMethods(monLocId: "CS0", crosscheckParameter: "HGRE"), evalB, null));
                    MpParameters.CombinedFacilityMethodRecords.Add(SetValues.DateHour(new CombinedMethods(monLocId: "MP0", crosscheckParameter: "HGRE"), evalB, null));
                    MpParameters.CombinedFacilityMethodRecords.Add(SetValues.DateHour(new CombinedMethods(monLocId: "MS0", crosscheckParameter: "HGRE"), evalB, null));

                    if (spIdentList[caseDex] != null)
                        MpParameters.CombinedFacilityMethodRecords.Add(SetValues.DateHour(new CombinedMethods(monLocId: spLocId, crosscheckParameter: spMethList[caseDex]), spMethBegList[caseDex], spMethEndList[caseDex]));
                }
                MpParameters.CurrentProgram = new VwLocationProgramRow(prgCd: prgList[caseDex], classCd: classList[caseDex], monLocId: unLocId);
                MpParameters.CurrentProgramActive = activeList[caseDex];
                MpParameters.ProgramEvaluationBeginDate = evalB.Date;
                MpParameters.ProgramEvaluationEndDate = evalE.Date;
                MpParameters.UnitStackConfigurationRecords = new CheckDataView<VwUnitStackConfigurationRow>();
                {
                    MpParameters.UnitStackConfigurationRecords.Add(new VwUnitStackConfigurationRow(stackName: "CP2", monLocId: unLocId, stackPipeMonLocId: "CP2", beginDate: evalB));
                    MpParameters.UnitStackConfigurationRecords.Add(new VwUnitStackConfigurationRow(stackName: "CS2", monLocId: unLocId, stackPipeMonLocId: "CS2", beginDate: evalB));
                    MpParameters.UnitStackConfigurationRecords.Add(new VwUnitStackConfigurationRow(stackName: "MP2", monLocId: unLocId, stackPipeMonLocId: "MP2", beginDate: evalB));
                    MpParameters.UnitStackConfigurationRecords.Add(new VwUnitStackConfigurationRow(stackName: "MS2", monLocId: unLocId, stackPipeMonLocId: "MS2", beginDate: evalB));

                    if (spIdentList != null)
                        MpParameters.UnitStackConfigurationRecords.Add(new VwUnitStackConfigurationRow(stackName: spIdentList[caseDex], monLocId: unLocId, stackPipeMonLocId: spLocId, beginDate: uscBegList[caseDex], endDate: uscEndList[caseDex]));
                }


                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                /* Initialize variables needed to run the check. */
                bool log = false;
                string actual;

                /* Run Check */
                actual = target.PROGRAM12(category, ref log);

                /* Check Results */
                Assert.AreEqual(string.Empty, actual, string.Format("actual {0}", caseDex));
                Assert.AreEqual(false, log, string.Format("log {0}", caseDex));
                Assert.AreEqual(expResultList[caseDex], category.CheckCatalogResult, string.Format("category.CheckCatalogResult {0}", caseDex));
            }

        }


        #region Program-13

        /// <summary>
        /// 
        /// BegHr : 2017-06-17 22
        /// EndHr : 2018-06-17 21
        /// MidHB : 2017-12-17 22
        /// MidHE : 2017-12-17 21
        /// 
        /// 
        ///      |       Method 1        |       Method 2        ||      |       Gap 1        |         Gap 2         |       Gap 3        ||
        /// | ## | Begin     | End       | Begin     | End       || Gaps | Begin  | End       | Begin     | End       | Begin     | End    || Note
        /// |  0 | BegHr     | MidHE     | MidHB     | EndHr     ||    0 | (none) | (none)    | (none)    | (none)    | (none)    | (none) || No gaps exist.
        /// |  1 | BegHr-1   | MidHE     | MidHB     | null      ||    0 | (none) | (none)    | (none)    | (none)    | (none)    | (none) || No gaps exist and coverage begins before the period and does not end.
        /// |  2 | BegHr     | MidHE     | MidHB     | EndHr+1   ||    0 | (none) | (none)    | (none)    | (none)    | (none)    | (none) || No gaps exist and coverage ends after the period.
        /// |  3 | BegHr+708 | MidHE-708 | MidHB+732 | EndHr-708 ||    3 | BegHr  | BegHr+707 | MidHE-707 | MidHB+731 | EndHr-707 | EndHr  || Three gaps exists, at the beginning and end of the range, and in the middle.
        /// </summary>
        [TestMethod]
        public void Program13_GetMethodGaps()
        {
            /* Initialize objects generally needed for testing checks. */
            cMpCheckParameters mpCheckParameters = new cMpCheckParameters();
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory(mpCheckParameters);
            cProgramChecks target = new cProgramChecks(mpCheckParameters);


            /* Input Parameter Values */
            DateTime begHr = new DateTime(2017, 06, 17, 22, 0, 0);
            DateTime endHr = new DateTime(2018, 06, 17, 21, 0, 0);
            DateTime midHB = new DateTime(2017, 12, 17, 22, 0, 0);
            DateTime midHE = new DateTime(2017, 12, 17, 21, 0, 0);

            DateTime?[] meth1BeginList = { begHr, begHr.AddHours(-1), begHr, begHr.AddHours(708) };
            DateTime?[] meth1EndList = { midHE, midHE, midHE, midHE.AddHours(-708) };
            DateTime?[] meth2BeginList = { midHB, midHB, midHB, midHB.AddHours(732) };
            DateTime?[] meth2EndList = { endHr, null, endHr.AddHours(1), endHr.AddHours(-708)};

            /* Expected Values */
            int[] gapsList = { 0, 0, 0, 3 };
            DateTime?[] gap1BeginList = { null, null, null, begHr };
            DateTime?[] gap1EndList = { null, null, null, begHr.AddHours(707) };
            DateTime?[] gap2BeginList = { null, null, null, midHE.AddHours(-707) };
            DateTime?[] gap2EndList = { null, null, null, midHB.AddHours(731) };
            DateTime?[] gap3BeginList = { null, null, null, endHr.AddHours(-707) };
            DateTime?[] gap3EndList = { null, null, null, endHr };

            /* Test Case Count */
            int caseCount = 4;

            /* Check array lengths */
            Assert.AreEqual(caseCount, meth1BeginList.Length, "meth1BeginList length");
            Assert.AreEqual(caseCount, meth1EndList.Length, "meth1EndList length");
            Assert.AreEqual(caseCount, meth2BeginList.Length, "meth2BeginList length");
            Assert.AreEqual(caseCount, meth2EndList.Length, "meth2EndList length");
            Assert.AreEqual(caseCount, gapsList.Length, "gapsList length");
            Assert.AreEqual(caseCount, gap1BeginList.Length, "gap1BeginList length");
            Assert.AreEqual(caseCount, gap1EndList.Length, "gap1EndList length");
            Assert.AreEqual(caseCount, gap2BeginList.Length, "gap2BeginList length");
            Assert.AreEqual(caseCount, gap2EndList.Length, "gap2EndList length");
            Assert.AreEqual(caseCount, gap3BeginList.Length, "gap3BeginList length");
            Assert.AreEqual(caseCount, gap3EndList.Length, "gap3EndList length");


            /* Run Cases */
            for (int caseDex = 0; caseDex < caseCount; caseDex++)
            {
                /* Initialize Input Parameters */
                CheckDataView<VwMonitorMethodRow> facilityMethodRecords = new CheckDataView<VwMonitorMethodRow>();
                {
                    if (meth1BeginList[caseDex] != null)
                        facilityMethodRecords.Add(SetValues.DateHour(new VwMonitorMethodRow(monLocId: "TestLoc"), meth1BeginList[caseDex], meth1EndList[caseDex]));
                    if (meth2BeginList[caseDex] != null)
                        facilityMethodRecords.Add(SetValues.DateHour(new VwMonitorMethodRow(monLocId: "TestLoc"), meth2BeginList[caseDex], meth2EndList[caseDex]));
                };

                /* Run Method */
                cHourRangeCollection result = target.GetMethodGaps(facilityMethodRecords, begHr, endHr);

                /* Check Results */
                Assert.AreEqual(gapsList[caseDex], result.Count, string.Format("Gaps {0}", caseDex));

                if (gapsList[caseDex] >= 1)
                {
                    Assert.AreEqual(gap1BeginList[caseDex], result.Item(0).BeganDateHour, string.Format("Gap 1 BeganDateHour {0}", caseDex));
                    Assert.AreEqual(gap1EndList[caseDex], result.Item(0).EndedDateHour, string.Format("Gap 1 EndedDateHour {0}", caseDex));
                }

                if (gapsList[caseDex] >= 2)
                {
                    Assert.AreEqual(gap2BeginList[caseDex], result.Item(1).BeganDateHour, string.Format("Gap 2 BeganDateHour {0}", caseDex));
                    Assert.AreEqual(gap2EndList[caseDex], result.Item(1).EndedDateHour, string.Format("Gap 2 EndedDateHour {0}", caseDex));
                }

                if (gapsList[caseDex] >= 3)
                {
                    Assert.AreEqual(gap3BeginList[caseDex], result.Item(2).BeganDateHour, string.Format("Gap 3 BeganDateHour {0}", caseDex));
                    Assert.AreEqual(gap3EndList[caseDex], result.Item(2).EndedDateHour, string.Format("Gap 3 EndedDateHour {0}", caseDex));
                }
            }

        }

        /// <summary>
        /// 
        /// BegHr : 2017-06-17 22
        /// EndHr : 2018-06-17 21
        /// MidHB : 2017-12-17 22
        /// MidHE : 2017-12-17 21
        /// 
        ///                   |     Method 1      |     Method 2      ||
        /// | ## | Meth State | Begin   | End     | Begin   | End     || Meth State || Note
        /// |  0 | NONE       | (null)  | (null)  | (null)  | (null)  || MISSING    || Initial update with no covering methods.
        /// |  1 | NONE       | BegHr+1 | MidHE   | MidHB   | EndHr   || INCOMPLETE || Initial update with gap at beginning of range.
        /// |  2 | NONE       | BegHr   | MidHE-1 | MidHB   | EndHr   || INCOMPLETE || Initial update with gap in middle of range.
        /// |  3 | NONE       | BegHr   | MidHE   | MidHB   | EndHr-1 || INCOMPLETE || Initial update with gap at end of range.
        /// |  4 | NONE       | BegHr   | MidHE   | MidHB   | EndHr   || SPANS      || Initial update with covered range.
        /// |  5 | MISSING    | (null)  | (null)  | (null)  | (null)  || MISSING    || All previous ranges had no covering methods, and update has no covering methods.
        /// |  6 | MISSING    | BegHr+1 | MidHE   | MidHB   | EndHr   || INCOMPLETE || All previous ranges had no covering methods, and update contains gap at beginning of range.
        /// |  7 | MISSING    | BegHr   | MidHE-1 | MidHB   | EndHr   || INCOMPLETE || All previous ranges had no covering methods, and update contains gap in middle of range.
        /// |  8 | MISSING    | BegHr   | MidHE   | MidHB   | EndHr-1 || INCOMPLETE || All previous ranges had no covering methods, and update contains gap at end of range.
        /// |  9 | MISSING    | BegHr   | MidHE   | MidHB   | EndHr   || INCOMPLETE || All previous ranges had no covering methods, and update is for a covered range.
        /// | 10 | SPANS      | (null)  | (null)  | (null)  | (null)  || INCOMPLETE || All previous ranges are covered by methods, and update has no covering methods.
        /// | 11 | SPANS      | BegHr+1 | MidHE   | MidHB   | EndHr   || INCOMPLETE || All previous ranges are covered by methods, and update with gap at beginning of range.
        /// | 12 | SPANS      | BegHr   | MidHE-1 | MidHB   | EndHr   || INCOMPLETE || All previous ranges are covered by methods, and update with gap in middle of range.
        /// | 13 | SPANS      | BegHr   | MidHE   | MidHB   | EndHr-1 || INCOMPLETE || All previous ranges are covered by methods, and update with gap at end of range.
        /// | 14 | SPANS      | BegHr   | MidHE   | MidHB   | EndHr   || SPANS      || All previous ranges are covered by methods, and update is for a covered range.
        /// | 15 | INCOMPLETE | (null)  | (null)  | (null)  | (null)  || INCOMPLETE || Previous ranges are covered but not completely cover, and update has no covering methods.
        /// | 16 | INCOMPLETE | BegHr+1 | MidHE   | MidHB   | EndHr   || INCOMPLETE || Previous ranges are covered but not completely cover, and update contains gap at beginning of range.
        /// | 17 | INCOMPLETE | BegHr   | MidHE-1 | MidHB   | EndHr   || INCOMPLETE || Previous ranges are covered but not completely cover, and update contains gap in middle of range.
        /// | 18 | INCOMPLETE | BegHr   | MidHE   | MidHB   | EndHr-1 || INCOMPLETE || Previous ranges are covered but not completely cover, and update contains gap at end of range.
        /// | 19 | INCOMPLETE | BegHr   | MidHE   | MidHB   | EndHr   || INCOMPLETE || Previous ranges are covered but not completely cover, and update is for a covered range.
        /// </summary>
        [TestMethod]
        public void Program13_UpdateMethodState()
        {
            /* Initialize objects generally needed for testing checks. */
            cMpCheckParameters mpCheckParameters = new cMpCheckParameters();
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory(mpCheckParameters);
            cProgramChecks target = new cProgramChecks(mpCheckParameters);

            MpParameters.Init(category.Process);
            MpParameters.Category = category;


            /* Input Parameter Values */
            DateTime begHr = new DateTime(2017, 06, 17, 22, 0, 0);
            DateTime endHr = new DateTime(2018, 06, 17, 21, 0, 0);
            DateTime midHB = new DateTime(2017, 12, 17, 22, 0, 0);
            DateTime midHE = new DateTime(2017, 12, 17, 21, 0, 0);

            eTimespanCoverageState[] methStateList = { eTimespanCoverageState.None, eTimespanCoverageState.None, eTimespanCoverageState.None, eTimespanCoverageState.None, eTimespanCoverageState.None,
                                                       eTimespanCoverageState.Missing, eTimespanCoverageState.Missing, eTimespanCoverageState.Missing, eTimespanCoverageState.Missing, eTimespanCoverageState.Missing,
                                                       eTimespanCoverageState.Spans, eTimespanCoverageState.Spans, eTimespanCoverageState.Spans, eTimespanCoverageState.Spans, eTimespanCoverageState.Spans,
                                                       eTimespanCoverageState.Incomplete, eTimespanCoverageState.Incomplete, eTimespanCoverageState.Incomplete, eTimespanCoverageState.Incomplete, eTimespanCoverageState.Incomplete };
            DateTime?[] meth1BeginList = { null, begHr.AddHours(1), begHr, begHr, begHr,
                                           null, begHr.AddHours(1), begHr, begHr, begHr,
                                           null, begHr.AddHours(1), begHr, begHr, begHr,
                                           null, begHr.AddHours(1), begHr, begHr, begHr};
            DateTime?[] meth1EndList = { null, midHE, midHE.AddHours(-1), midHE, midHE,
                                         null, midHE, midHE.AddHours(-1), midHE, midHE,
                                         null, midHE, midHE.AddHours(-1), midHE, midHE,
                                         null, midHE, midHE.AddHours(-1), midHE, midHE};
            DateTime?[] meth2BeginList = { null, midHB, midHB, midHB, midHB,
                                           null, midHB, midHB, midHB, midHB,
                                           null, midHB, midHB, midHB, midHB,
                                           null, midHB, midHB, midHB, midHB};
            DateTime?[] meth2EndList = { null, endHr, endHr, endHr.AddHours(-1), endHr,
                                         null, endHr, endHr, endHr.AddHours(-1), endHr,
                                         null, endHr, endHr, endHr.AddHours(-1), endHr,
                                         null, endHr, endHr, endHr.AddHours(-1), endHr};

            /* Expected Values */
            eTimespanCoverageState[] expMethStateList = { eTimespanCoverageState.Missing, eTimespanCoverageState.Incomplete, eTimespanCoverageState.Incomplete, eTimespanCoverageState.Incomplete, eTimespanCoverageState.Spans,
                                                          eTimespanCoverageState.Missing, eTimespanCoverageState.Incomplete, eTimespanCoverageState.Incomplete, eTimespanCoverageState.Incomplete, eTimespanCoverageState.Incomplete,
                                                          eTimespanCoverageState.Incomplete, eTimespanCoverageState.Incomplete, eTimespanCoverageState.Incomplete, eTimespanCoverageState.Incomplete, eTimespanCoverageState.Spans,
                                                          eTimespanCoverageState.Incomplete, eTimespanCoverageState.Incomplete, eTimespanCoverageState.Incomplete, eTimespanCoverageState.Incomplete, eTimespanCoverageState.Incomplete };

            /* Test Case Count */
            int caseCount = 20;

            /* Check array lengths */
            Assert.AreEqual(caseCount, methStateList.Length, "methStateList length");
            Assert.AreEqual(caseCount, meth1BeginList.Length, "meth1BeginList length");
            Assert.AreEqual(caseCount, meth1EndList.Length, "meth1EndList length");
            Assert.AreEqual(caseCount, meth2BeginList.Length, "meth2BeginList length");
            Assert.AreEqual(caseCount, meth2EndList.Length, "meth2EndList length");
            Assert.AreEqual(caseCount, expMethStateList.Length, "expMethStateList length");


            /* Run Cases */
            for (int caseDex = 0; caseDex < caseCount; caseDex++)
            {
                /* Initialize Input Parameters */
                eTimespanCoverageState methodState = methStateList[caseDex];

                CheckDataView<VwMonitorMethodRow> facilityMethodRecords = new CheckDataView<VwMonitorMethodRow>();
                {
                    if (meth1BeginList[caseDex] != null)
                        facilityMethodRecords.Add(SetValues.DateHour(new VwMonitorMethodRow(monLocId: "TestLoc"), meth1BeginList[caseDex], meth1EndList[caseDex]));
                    if (meth2BeginList[caseDex] != null)
                        facilityMethodRecords.Add(SetValues.DateHour(new VwMonitorMethodRow(monLocId: "TestLoc"), meth2BeginList[caseDex], meth2EndList[caseDex]));
                };

                /* Run Method */
                eTimespanCoverageState result = target.UpdateMethodState(methodState, facilityMethodRecords, begHr, endHr, category);

                /* Check Results */
                Assert.AreEqual(expMethStateList[caseDex], result, string.Format("Case {0}", caseDex));
            }

        }

        /// <summary>
        /// Program-13 Conditions
        /// 
        /// 
        /// CrossCheckProgramParameterToMethodParameter:
        /// 
        ///     |-PrgParam-|-MethodParamList-|---Description----|
        ///     | BAD      | BAD,BADM        | Bad Stuff        |
        ///     | CO2      | CO2,CO2M        | Carbon Dioxide   |
        ///     | MAL      | MAL, MALM       | Bad French Stuff |
        /// 
        /// CurrentProgramParameter
        ///     MonLocId                        : UnitLocKey
        ///     ParameterCd                     : CO2
        ///     UnitId                          : 13
        /// FacilityMethodRecords               : no rows
        /// ProgramParameterBeginDate           : 2016-06-17
        /// ProgramParameterEndDate             : 2018-06-17
        /// UnitStackConfiguration              : no rows
        /// 
        /// | ## | PrgCode | Class | ReqInd | Active || Result | Description    || Note
        /// |  0 | NSPS4T  | A     | 1      | True   || A      | Carbon Dioxide || All 'run check' conditions met but no methods exist.
        /// |  1 | MATS    | A     | 1      | True   || null   | null           || Incorrect program.
        /// |  2 | NSPS4T  | N     | 1      | True   || null   | null           || Class is not-affected.
        /// |  3 | NSPS4T  | A     | 0      | True   || null   | null           || Parameter is not required.
        /// |  4 | NSPS4T  | A     | 1      | False  || null   | null           || Program Parameter is not active.
        /// 
        /// </summary>
        [TestMethod]
        public void Program13_Conditions()
        {
            /* Initialize objects generally needed for testing checks. */
            cMpCheckParameters mpCheckParameters = new cMpCheckParameters();
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory(mpCheckParameters);
            cProgramChecks target = new cProgramChecks(mpCheckParameters);

            MpParameters.Init(category.Process);
            MpParameters.Category = category;


            /* Input Parameter Values */
            string[] prgList = { "NSPS4T", "MATS", "NSPS4T", "NSPS4T", "NSPS4T" };
            string[] classList = { "A", "A", "N", "A", "A" };
            int?[] reqIndList = { 1, 1, 1, 0, 1 };
            bool[] activeList = { true, true, true, true, false };

            /* Expected Values */
            string[] expResultList = { "A", null, null, null, null };
            string[] expDescriptionList = { "Carbon Dioxide", null, null, null, null };

            /* Test Case Count */
            int caseCount = 5;

            /* Check array lengths */
            Assert.AreEqual(caseCount, prgList.Length, "prgList length");
            Assert.AreEqual(caseCount, classList.Length, "classList length");
            Assert.AreEqual(caseCount, reqIndList.Length, "reqIndList length");
            Assert.AreEqual(caseCount, activeList.Length, "activeList length");
            Assert.AreEqual(caseCount, expResultList.Length, "expResultList length");
            Assert.AreEqual(caseCount, expDescriptionList.Length, "expDescriptionList length");


            /* Run Cases */
            for (int caseDex = 0; caseDex < caseCount; caseDex++)
            {
                /* Initialize Input Parameters */
                MpParameters.CrosscheckProgramparametertomethodparameter = new CheckDataView<ProgramParameterToMethodParameterRow>
                    (
                        new ProgramParameterToMethodParameterRow(programParameterCd: "BAD", methodParameterList: "BAD,BADM", methodParameterDescription: "Bad Stuff"),
                        new ProgramParameterToMethodParameterRow(programParameterCd: "CO2", methodParameterList: "CO2,CO2M", methodParameterDescription: "Carbon Dioxide"),
                        new ProgramParameterToMethodParameterRow(programParameterCd: "MAL", methodParameterList: "MAL,MALM", methodParameterDescription: "Bad French Stuff")
                    );
                MpParameters.CurrentProgramParameter = new UnitProgramParameter(monLocId: "UnitLocKey", unitId: 13, prgCd: prgList[caseDex], parameterCd: "CO2", classCd: classList[caseDex], requiredInd: reqIndList[caseDex]);
                MpParameters.CurrentProgramParameterActive = activeList[caseDex];
                MpParameters.FacilityMethodRecords = new CheckDataView<VwMonitorMethodRow>();
                MpParameters.ProgramParameterEvaluationBeginDate = new DateTime(2016, 6, 17, 23, 0, 0);
                MpParameters.ProgramParameterEvaluationEndDate = new DateTime(2018, 6, 17, 22, 0, 0);
                MpParameters.UnitStackConfigurationRecords = new CheckDataView<VwUnitStackConfigurationRow>();


                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                /* Initialize variables needed to run the check. */
                bool log = false;
                string actual;

                /* Run Check */
                actual = target.PROGRAM13(category, ref log);

                /* Check Results */
                Assert.AreEqual(string.Empty, actual, string.Format("actual {0}", caseDex));
                Assert.AreEqual(false, log, string.Format("log {0}", caseDex));

                Assert.AreEqual(expResultList[caseDex], category.CheckCatalogResult, string.Format("category.CheckCatalogResult {0}", caseDex));
                Assert.AreEqual(expDescriptionList[caseDex], MpParameters.Nsps4tMethodParameterDescription, string.Format("Nsps4tMethodParameterDescription {0}", caseDex));
            }

        }

        /// <summary>
        /// Program-13 Single Unit Configurations
        /// 
        /// 
        /// CrossCheckProgramParameterToMethodParameter:
        /// 
        ///     |-PrgParam-|-MethodParamList-|---Description----|
        ///     | BAD      | BAD,BADM        | Bad Stuff        |
        ///     | CO2      | CO2,CO2M        | Carbon Dioxide   |
        ///     | MAL      | MAL, MALM       | Bad French Stuff |
        /// 
        /// CurrentProgramParameter
        ///     classCd                         : A
        ///     MonLocId                        : UnitLocKey
        ///     ParameterCd                     : CO2
        ///     PrgCd                           : NSPS4T
        ///     RequiredInd                     : 1
        ///     UnitId                          : 13
        /// CurrentProgramParameterActive       : True
        /// ProgramParameterEvaluationBeginDate : 2016-06-17
        /// ProgramParameterEvaluationEndDate   : 2018-06-17
        /// UnitStackConfiguration              : no rows
        /// 
        /// EvalB   : 2016-06-17 00
        /// EvalE   : 2018-06-17 23
        /// CovrB  : 2016-06-17 23
        /// CovrE  : 2018-06-17 00
        /// MidB    : 2017-06-17 22
        /// MidE    : 2017-06-17 21
        /// 
        /// 
        ///      |                     Method 1                     |                     Method 2                     ||
        /// | ## | UnitId | MonLocId | Parmeter | BegHr   | EndHr   | UnitId | MonLocId | Parmeter | BegHr   | EndHr   || Result | Description    || Note
        /// |  0 | 13     | MonLoc13 | CO2      | EvalB   | MidE    | 13     | MonLoc13 | CO2M     | MidB    | Null    || null   | Carbon Dioxide || Methods exists and span
        /// |  1 | 13     | MonLoc13 | CO2      | EvalB   | MidE    | 13     | MonLoc13 | CO2M     | MidB    | EvalE   || null   | Carbon Dioxide || Methods exists and span
        /// |  2 | 14     | MonLoc14 | CO2      | EvalB   | MidE    | 14     | MonLoc14 | CO2M     | MidB    | Null    || A      | Carbon Dioxide || Methods do not exist (UnitId)
        /// |  3 | 13     | MonLoc13 | BAD      | EvalB   | MidE    | 13     | MonLoc13 | BADM     | MidB    | Null    || A      | Carbon Dioxide || Methods do not exist (Parameter)
        /// |  4 | 13     | MonLoc13 | CO2      | EvalE+1 | EvalE+2 | 13     | MonLoc13 | CO2M     | EvalE+3 | EvalE+4 || A      | Carbon Dioxide || Methods do not exist (Dates After)
        /// |  5 | 13     | MonLoc13 | CO2      | EvalB-4 | EvalB-3 | 13     | MonLoc13 | CO2M     | EvalB-2 | EvalB-1 || A      | Carbon Dioxide || Methods do not exist (Dates Before)
        /// |  6 | 13     | MonLoc13 | CO2      | CovrB+1 | MidE    | 13     | MonLoc13 | CO2M     | MidB    | Null    || B      | Carbon Dioxide || Incomplete, gap at beginning
        /// |  7 | 13     | MonLoc13 | CO2      | EvalB   | MidE    | 13     | MonLoc13 | CO2M     | MidB    | CovrE-1 || B      | Carbon Dioxide || Incomplete, gap at end
        /// |  8 | 13     | MonLoc13 | CO2      | EvalB   | MidE    | 13     | MonLoc13 | CO2M     | MidB+1  | Null    || B      | Carbon Dioxide || Incomplete, gap in middle
        /// 
        /// </summary>
        [TestMethod]
        public void Program13_SingleUnit()
        {
            /* Initialize objects generally needed for testing checks. */
            cMpCheckParameters mpCheckParameters = new cMpCheckParameters();
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory(mpCheckParameters);
            cProgramChecks target = new cProgramChecks(mpCheckParameters);

            MpParameters.Init(category.Process);
            MpParameters.Category = category;


            /* Input Parameter Values */
            DateTime evalB = new DateTime(2016, 6, 17, 0, 0, 0);
            DateTime evalE = new DateTime(2018, 6, 17, 23, 0, 0);
            DateTime covrB = new DateTime(2016, 6, 17, 23, 0, 0);
            DateTime covrE = new DateTime(2018, 6, 17, 0, 0, 0);
            DateTime midB = new DateTime(2017, 6, 17, 22, 0, 0);
            DateTime midE = new DateTime(2017, 6, 17, 21, 0, 0);

            int?[] meth1UnitIdList = { 13, 13, 14, 13, 13, 13, 13, 13, 13 };
            string[] meth1LocIdList = { "MonLoc13", "MonLoc13", "MonLoc14", "MonLoc13", "MonLoc13", "MonLoc13", "MonLoc13", "MonLoc13", "MonLoc13" };
            string[] meth1ParameterList = { "CO2", "CO2", "CO2", "BAD", "CO2", "CO2", "CO2", "CO2", "CO2" };
            DateTime?[] meth1BeginList = { evalB, evalB, evalB, evalB, evalE.AddHours(1), evalB.AddHours(-4), covrB.AddHours(1), evalB, evalB };
            DateTime?[] meth1EndList = { midE, midE, midE, midE, evalE.AddHours(2), evalB.AddHours(-3), midE, midE, midE };
            int?[] meth2UnitIdList = { 13, 13, 14, 13, 13, 13, 13, 13, 13 };
            string[] meth2LocIdList = { "MonLoc13", "MonLoc13", "MonLoc14", "MonLoc13", "MonLoc13", "MonLoc13", "MonLoc13", "MonLoc13", "MonLoc13" };
            string[] meth2ParameterList = { "CO2M", "CO2M", "CO2M", "BADM", "CO2M", "CO2M", "CO2M", "CO2M", "CO2M" };
            DateTime?[] meth2BeginList = { midB, midB, midB, midB, evalE.AddHours(3), evalB.AddHours(-2), midB, midB, midB.AddHours(1) };
            DateTime?[] meth2EndList = { null, evalE, null, null, evalE.AddHours(4), evalB.AddHours(-1), null, covrE.AddHours(-1), null };

            /* Expected Values */
            string[] expResultList = { null, null, "A", "A", "A", "A", "B", "B", "B" };
            string[] expDescriptionList = { "Carbon Dioxide", "Carbon Dioxide", "Carbon Dioxide", "Carbon Dioxide", "Carbon Dioxide", "Carbon Dioxide", "Carbon Dioxide", "Carbon Dioxide", "Carbon Dioxide" };

            /* Test Case Count */
            int caseCount = 9;

            /* Check array lengths */
            Assert.AreEqual(caseCount, meth1UnitIdList.Length, "meth1UnitIdList length");
            Assert.AreEqual(caseCount, meth1LocIdList.Length, "meth1LocIdList length");
            Assert.AreEqual(caseCount, meth1ParameterList.Length, "meth1ParameterList length");
            Assert.AreEqual(caseCount, meth1BeginList.Length, "meth1BeginList length");
            Assert.AreEqual(caseCount, meth1EndList.Length, "meth1EndList length");
            Assert.AreEqual(caseCount, meth2UnitIdList.Length, "meth2UnitIdList length");
            Assert.AreEqual(caseCount, meth2LocIdList.Length, "meth2LocIdList length");
            Assert.AreEqual(caseCount, meth2ParameterList.Length, "meth2ParameterList length");
            Assert.AreEqual(caseCount, meth2BeginList.Length, "meth2BeginList length");
            Assert.AreEqual(caseCount, meth2EndList.Length, "meth2EndList length");
            Assert.AreEqual(caseCount, expResultList.Length, "expResultList length");
            Assert.AreEqual(caseCount, expDescriptionList.Length, "expDescriptionList length");


            /* Run Cases */
            for (int caseDex = 0; caseDex < caseCount; caseDex++)
            {
                /* Initialize Input Parameters */
                MpParameters.CrosscheckProgramparametertomethodparameter = new CheckDataView<ProgramParameterToMethodParameterRow>
                    (
                        new ProgramParameterToMethodParameterRow(programParameterCd: "BAD", methodParameterList: "BAD,BADM", methodParameterDescription: "Bad Stuff"),
                        new ProgramParameterToMethodParameterRow(programParameterCd: "CO2", methodParameterList: "CO2,CO2M", methodParameterDescription: "Carbon Dioxide"),
                        new ProgramParameterToMethodParameterRow(programParameterCd: "MAL", methodParameterList: "MAL,MALM", methodParameterDescription: "Bad French Stuff")
                    );
                MpParameters.CurrentProgramParameter = new UnitProgramParameter(monLocId: "MonLoc13", unitId: 13, prgCd: "NSPS4T", parameterCd: "CO2", classCd: "A", requiredInd: 1);
                MpParameters.CurrentProgramParameterActive = true;
                MpParameters.FacilityMethodRecords = new CheckDataView<VwMonitorMethodRow>
                    (
                        SetValues.DateHour(new VwMonitorMethodRow(unitId: 12, monLocId: "Unit12Key", parameterCd: "CO2"), evalB, null),
                        SetValues.DateHour(new VwMonitorMethodRow(unitId: meth1UnitIdList[caseDex], monLocId: meth1LocIdList[caseDex], parameterCd: meth1ParameterList[caseDex]), meth1BeginList[caseDex], meth1EndList[caseDex]),
                        SetValues.DateHour(new VwMonitorMethodRow(unitId: meth2UnitIdList[caseDex], monLocId: meth2LocIdList[caseDex], parameterCd: meth2ParameterList[caseDex]), meth2BeginList[caseDex], meth2EndList[caseDex]),
                        SetValues.DateHour(new VwMonitorMethodRow(unitId: 15, monLocId: "Unit15Key", parameterCd: "CO2"), evalB, null)
                    );
                MpParameters.ProgramParameterEvaluationBeginDate = evalB.Date;
                MpParameters.ProgramParameterEvaluationEndDate = evalE.Date;
                MpParameters.UnitStackConfigurationRecords = new CheckDataView<VwUnitStackConfigurationRow>();


                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                /* Initialize variables needed to run the check. */
                bool log = false;
                string actual;

                /* Run Check */
                actual = target.PROGRAM13(category, ref log);

                /* Check Results */
                Assert.AreEqual(string.Empty, actual, string.Format("actual {0}", caseDex));
                Assert.AreEqual(false, log, string.Format("log {0}", caseDex));

                Assert.AreEqual(expResultList[caseDex], category.CheckCatalogResult, string.Format("category.CheckCatalogResult {0}", caseDex));
                Assert.AreEqual(expDescriptionList[caseDex], MpParameters.Nsps4tMethodParameterDescription, string.Format("Nsps4tMethodParameterDescription {0}", caseDex));
            }

        }

        /// <summary>
        /// Program-13 Stack and Pipe Parameters
        /// 
        /// Input Parameters:
        /// 
        ///     CrossCheckProgramParameterToMethodParameter:
        /// 
        ///         |-PrgParam-|-MethodParamList-|---Description----|
        ///         | BAD      | BAD,BADM        | Bad Stuff        |
        ///         | CO2      | CO2,CO2M        | Carbon Dioxide   |
        ///         | MAL      | MAL, MALM       | Bad French Stuff |
        /// 
        ///     CurrentProgramParameter
        ///         classCd                         : A
        ///         MonLocId                        : UnitLocKey
        ///         ParameterCd                     : CO2
        ///         PrgCd                           : NSPS4T
        ///         RequiredInd                     : 1
        ///         UnitId                          : 13
        ///     CurrentProgramParameterActive       : True
        ///     ProgramParameterEvaluationBeginDate : 2016-06-17
        ///     ProgramParameterEvaluationEndDate   : 2018-06-17
        /// 
        ///     UnitStackConfiguration:
        /// 
        ///         | UnitId | StackId | StackName  | BegDt   | EndDt   |
        ///         | 13     | loc1    | CS1 or CP1 | Range1B | Range1E |
        ///         | 13     | loc2    | CS2 or CP2 | Range1B | Range1E |
        ///         | 13     | loc3    | MS1 or MP1 | Range2B | Range2E |
        ///         | 13     | loc4    | MS2 or MP2 | Range2B | Range2E |
        /// 
        /// Output Parameters:
        /// 
        ///     Nsps4tMetodParameterDescription     : Carbon Dioxide
        /// 
        /// 
        /// EvalB   : 2016-06-17 00
        /// EvalE   : 2018-06-17 23
        /// Range1B : 2016-06-17 22
        /// Range1E : 2017-06-17 21
        /// Range2B : 2017-06-17 22
        /// Range2E : 2018-06-17 21
        /// 
        /// 
        ///              |   Range 1   |   Range 2   ||
        /// | ## | Type  | Loc1 | Loc2 | Loc3 | Loc4 || Result || Note
        /// |  0 | Stack | CO2  | CO2M | CO2  | CO2M || null   || Parameters exist at ranges covered by common and multiple.
        /// |  1 | Pipe  | CO2  | CO2M | CO2  | CO2M || null   || Parameters exist at ranges covered by common and multiple.
        /// |  2 | Stack | BAD  | MALM | BAD  | MALM || A      || No parameters match.
        /// |  3 | Pipe  | MAL  | BADM | MAL  | BADM || A      || No parameters match.
        /// |  4 | Stack | BAD  | CO2M | CO2  | CO2M || B      || Loc1 (stack) parameter does not match.
        /// |  5 | Pipe  | MAL  | CO2M | CO2  | CO2M || B      || Loc1 (pipe) parameter does not match.
        /// |  6 | Stack | CO2  | MALM | CO2  | CO2M || B      || Loc2 (stack) parameter does not match.
        /// |  7 | Pipe  | CO2  | BADM | CO2  | CO2M || B      || Loc2 (pipe) parameter does not match.
        /// |  8 | Stack | CO2  | CO2M | BAD  | CO2M || B      || Loc3 (stack) parameter does not match.
        /// |  9 | Pipe  | CO2  | CO2M | MAL  | CO2M || B      || Loc3 (pipe) parameter does not match.
        /// | 10 | Stack | CO2  | CO2M | CO2  | MALM || B      || Loc4 (stack) parameter does not match.
        /// | 11 | Pipe  | CO2  | CO2M | CO2  | BADM || B      || Loc4 (pipe) parameter does not match.
        /// </summary>
        [TestMethod]
        public void Program13_StackAndPipeParameters()
        {
            /* Initialize objects generally needed for testing checks. */
            cMpCheckParameters mpCheckParameters = new cMpCheckParameters();
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory(mpCheckParameters);
            cProgramChecks target = new cProgramChecks(mpCheckParameters);

            MpParameters.Init(category.Process);
            MpParameters.Category = category;


            /* Input Parameter Values */
            DateTime evalB = new DateTime(2016, 6, 17, 0, 0, 0);
            DateTime evalE = new DateTime(2018, 6, 17, 23, 0, 0);
            DateTime range1Beg = new DateTime(2016, 6, 17, 22, 0, 0);
            DateTime range1End = new DateTime(2017, 6, 17, 21, 0, 0);
            DateTime range2Beg = new DateTime(2017, 6, 17, 22, 0, 0);
            DateTime range2End = new DateTime(2018, 6, 17, 21, 0, 0);

            bool[] IsStackList = { true, false, true, false, true, false, true, false, true, false, true, false };
            string[] loc1ParameterList = { "CO2", "CO2", "BAD", "MAL", "BAD", "MAL", "CO2", "CO2", "CO2", "CO2", "CO2", "CO2" };
            string[] loc2ParameterList = { "CO2M", "CO2M", "MALM", "BADM", "CO2M", "CO2M", "MALM", "BADM", "CO2M", "CO2M", "CO2M", "CO2M" };
            string[] loc3ParameterList = { "CO2", "CO2", "BAD", "MAL", "CO2", "CO2", "CO2", "CO2", "BAD", "MAL", "CO2", "CO2" };
            string[] loc4ParameterList = { "CO2M", "CO2M", "MALM", "BADM", "CO2M", "CO2M", "CO2M", "CO2M", "CO2M", "CO2M", "MALM", "BADM" };

            /* Expected Values */
            string[] expResultList = { null, null, "A", "A", "B", "B", "B", "B", "B", "B", "B", "B" };

            /* Test Case Count */
            int caseCount = 12;

            /* Check array lengths */
            Assert.AreEqual(caseCount, IsStackList.Length, "IsStackList length");
            Assert.AreEqual(caseCount, loc1ParameterList.Length, "loc1ParameterList length");
            Assert.AreEqual(caseCount, loc2ParameterList.Length, "loc2ParameterList length");
            Assert.AreEqual(caseCount, loc3ParameterList.Length, "loc3ParameterList length");
            Assert.AreEqual(caseCount, loc4ParameterList.Length, "loc4ParameterList length");
            Assert.AreEqual(caseCount, expResultList.Length, "expResultList length");


            /* Run Cases */
            for (int caseDex = 0; caseDex < caseCount; caseDex++)
            {
                /* Initialize Input Parameters */
                MpParameters.CrosscheckProgramparametertomethodparameter = new CheckDataView<ProgramParameterToMethodParameterRow>
                    (
                        new ProgramParameterToMethodParameterRow(programParameterCd: "BAD", methodParameterList: "BAD,BADM", methodParameterDescription: "Bad Stuff"),
                        new ProgramParameterToMethodParameterRow(programParameterCd: "CO2", methodParameterList: "CO2,CO2M", methodParameterDescription: "Carbon Dioxide"),
                        new ProgramParameterToMethodParameterRow(programParameterCd: "MAL", methodParameterList: "MAL,MALM", methodParameterDescription: "Bad French Stuff")
                    );
                MpParameters.CurrentProgramParameter = new UnitProgramParameter(monLocId: "UnitLocKey", unitId: 13, prgCd: "NSPS4T", parameterCd: "CO2", classCd: "A", requiredInd: 1);
                MpParameters.CurrentProgramParameterActive = true;
                MpParameters.FacilityMethodRecords = new CheckDataView<VwMonitorMethodRow>
                    (
                        SetValues.DateHour(new VwMonitorMethodRow(stackPipeId: "stp1", monLocId: "loc1", stackName: (IsStackList[caseDex] ? "CS1" : "CP1"), parameterCd: loc1ParameterList[caseDex]), range1Beg, range1End),
                        SetValues.DateHour(new VwMonitorMethodRow(stackPipeId: "stp2", monLocId: "loc2", stackName: (IsStackList[caseDex] ? "CS2" : "CP2"), parameterCd: loc2ParameterList[caseDex]), range1Beg, range1End),
                        SetValues.DateHour(new VwMonitorMethodRow(stackPipeId: "stp3", monLocId: "loc3", stackName: (IsStackList[caseDex] ? "MS1" : "MP1"), parameterCd: loc3ParameterList[caseDex]), range2Beg, range2End),
                        SetValues.DateHour(new VwMonitorMethodRow(stackPipeId: "stp4", monLocId: "loc4", stackName: (IsStackList[caseDex] ? "MS2" : "MP2"), parameterCd: loc4ParameterList[caseDex]), range2Beg, range2End)
                    );
                MpParameters.ProgramParameterEvaluationBeginDate = evalB.Date;
                MpParameters.ProgramParameterEvaluationEndDate = evalE.Date;
                MpParameters.UnitStackConfigurationRecords = new CheckDataView<VwUnitStackConfigurationRow>
                    (
                        new VwUnitStackConfigurationRow(unitId: 13, stackPipeId: "stp1", stackPipeMonLocId: "loc1", stackName: (IsStackList[caseDex] ? "CS1" : "CP1"), beginDate: range1Beg.Date, endDate: range1End.Date),
                        new VwUnitStackConfigurationRow(unitId: 13, stackPipeId: "stp2", stackPipeMonLocId: "loc2", stackName: (IsStackList[caseDex] ? "CS2" : "CP2"), beginDate: range1Beg.Date, endDate: range1End.Date),
                        new VwUnitStackConfigurationRow(unitId: 13, stackPipeId: "stp3", stackPipeMonLocId: "loc3", stackName: (IsStackList[caseDex] ? "MS1" : "MP1"), beginDate: range2Beg.Date, endDate: range2End.Date),
                        new VwUnitStackConfigurationRow(unitId: 13, stackPipeId: "stp4", stackPipeMonLocId: "loc4", stackName: (IsStackList[caseDex] ? "MS2" : "MP2"), beginDate: range2Beg.Date, endDate: range2End.Date)
                    );


                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                /* Initialize variables needed to run the check. */
                bool log = false;
                string actual;

                /* Run Check */
                actual = target.PROGRAM13(category, ref log);

                /* Check Results */
                Assert.AreEqual(string.Empty, actual, string.Format("actual {0}", caseDex));
                Assert.AreEqual(false, log, string.Format("log {0}", caseDex));

                Assert.AreEqual(expResultList[caseDex], category.CheckCatalogResult, string.Format("category.CheckCatalogResult {0}", caseDex));
                Assert.AreEqual("Carbon Dioxide", MpParameters.Nsps4tMethodParameterDescription, string.Format("Nsps4tMethodParameterDescription {0}", caseDex));
            }

        }

        /// <summary>
        /// Program-13 Stack And Pipe Dates
        /// 
        /// Input Parameters:
        /// 
        ///     CrossCheckProgramParameterToMethodParameter:
        /// 
        ///         |-PrgParam-|-MethodParamList-|---Description----|
        ///         | BAD      | BAD,BADM        | Bad Stuff        |
        ///         | CO2      | CO2,CO2M        | Carbon Dioxide   |
        ///         | MAL      | MAL, MALM       | Bad French Stuff |
        /// 
        ///     CurrentProgramParameter
        ///         classCd                         : A
        ///         MonLocId                        : UnitLocKey
        ///         ParameterCd                     : CO2
        ///         PrgCd                           : NSPS4T
        ///         RequiredInd                     : 1
        ///         UnitId                          : 13
        ///     CurrentProgramParameterActive       : True
        ///     ProgramParameterEvaluationBeginDate : 2016-06-17
        ///     ProgramParameterEvaluationEndDate   : 2018-06-17
        /// 
        ///     UnitStackConfiguration:
        /// 
        ///         | UnitId | StackId | StackName  | BegDt   | EndDt   |
        ///         | 13     | loc1    | CS1 or CP1 | Range1B | Range1E |
        ///         | 13     | loc2    | CS2 or CP2 | Range1B | Range1E |
        ///         | 13     | loc3    | MS1 or MP1 | Range2B | Range2E |
        ///         | 13     | loc4    | MS2 or MP2 | Range2B | Range2E |
        /// 
        /// Output Parameters:
        /// 
        ///     Nsps4tMetodParameterDescription     : Carbon Dioxide
        /// 
        /// 
        /// EvalB  : 2016-06-17 00
        /// EvalE : 2018-06-17 23
        /// CovrB : 2016-06-17 23
        /// CovrE : 2018-06-17 00
        /// MidB  : 2017-06-17 22
        /// MidE  : 2017-06-17 21
        /// MidBC : 2017-06-17 23
        /// MidEC : 2017-06-17 00
        /// 
        /// 
        ///      |                 Unit                  |            SP1            |              SP2              |        USC        || 
        /// | ## | Beg1Hr  | End1Hr  | Beg2Hr  | End2Hr  | Loc | BegHour  | EndHour  | Loc | BegHour    | EndHour    | BegDate | EnDate  || Result || Note
        /// 
        /// |  0 | EvalB-2 | EvalB-1 | (none)  | (none)  | CS1 | EvalB-2  | EvalB-1  | MS2 | EvalB-2    | EvalB-1    | CovrB   | MidE    || A      || No methods exist for the unit, SP1 or SP2.
        /// |  1 | EvalE+1 | EvalE+2 | (none)  | (none)  | CS1 | EvalE+1  | EvalE+2  | MS2 | EvalE+1    | EvalE+2    | MidB    | CovrE   || A      || No methods exist for the unit, SP1 or SP2.
        /// |  2 | MidB    | EvalE   | (none)  | (none)  | CS1 | EvalB-2  | EvalB-1  | MS2 | EvalB-2    | EvalB-1    | CovrB   | MidE    || B      || No methods exist for SP1 or SP2.
        /// |  3 | EvalB   | MidE    | (none)  | (none)  | CS1 | EvalE+1  | EvalE+2  | MS2 | EvalE+1    | EvalE+2    | MidB    | CovrE   || B      || No methods exist for SP1 or SP2.
        /// 
        /// |  4 | (none)  | (none)  | (none)  | (none)  | CS1 | EvalB    | EvalE    | MS2 | EvalB      | EvalE      | CovrB   | CovrE   || null   || SP1 and SP2 methods cover the entire period.
        /// |  5 | (none)  | (none)  | (none)  | (none)  | CP1 | EvalB    | EvalE    | MP2 | EvalB      | EvalE      | CovrB   | CovrE   || null   || SP1 and SP2 methods cover the entire period.
        /// |  6 | (none)  | (none)  | (none)  | (none)  | CS1 | EvalB    | EvalE    | MS2 | CovrB      | CovrE      | CovrB   | CovrE   || null   || SP1 methods cover the entire period, SP2 covers minimum period.
        /// |  7 | (none)  | (none)  | (none)  | (none)  | CP1 | EvalB    | EvalE    | MP2 | CovrB      | CovrE      | CovrB   | CovrE   || null   || SP1 methods cover the entire period, SP2 covers minimum period.
        /// |  8 | (none)  | (none)  | (none)  | (none)  | MS1 | EvalB    | EvalE    | CS2 | CovrB      | CovrE      | CovrB   | CovrE   || null   || SP1 methods cover the entire period, SP2 covers minimum period.
        /// |  9 | (none)  | (none)  | (none)  | (none)  | MP1 | EvalB    | EvalE    | CP2 | CovrB      | CovrE      | CovrB   | CovrE   || null   || SP1 methods cover the entire period, SP2 covers minimum period.
        /// | 10 | (none)  | (none)  | (none)  | (none)  | CS1 | EvalB    | EvalE    | MS2 | CovrB+1    | CovrE      | CovrB   | CovrE   || B      || SP1 methods cover the entire period, SP2 does not cover the first required hour.
        /// | 11 | (none)  | (none)  | (none)  | (none)  | CP1 | EvalB    | EvalE    | MP2 | CovrB+1    | CovrE      | CovrB   | CovrE   || B      || SP1 methods cover the entire period, SP2 does not cover the first required hour.
        /// | 12 | (none)  | (none)  | (none)  | (none)  | MS1 | EvalB    | EvalE    | CS2 | CovrB      | CovrE-1    | CovrB   | CovrE   || B      || SP1 methods cover the entire period, SP2 does not cover the last required hour.
        /// | 13 | (none)  | (none)  | (none)  | (none)  | MP1 | EvalB    | EvalE    | CP2 | CovrB      | CovrE-1    | CovrB   | CovrE   || B      || SP1 methods cover the entire period, SP2 does not cover the last required hour.
        /// 
        /// | 14 | MidB    | EvalE   | (none)  | (none)  | CS1 | EvalB    | MidE     | MS2 | CovrB      | MidEC      | CovrB   | MidE    || null   || Stacks cover beginning range of period.
        /// | 15 | MidB    | EvalE   | (none)  | (none)  | CS1 | EvalB    | MidE     | MS2 | CovrB+1    | MidEC      | CovrB   | MidE    || B      || SP2 does not cover first required hour of beginning range of period.
        /// | 16 | MidB    | EvalE   | (none)  | (none)  | CS1 | EvalB    | MidE     | MS2 | CovrB      | MidEC-1    | CovrB   | MidE    || B      || SP2 does not cover last required hour of beginning range of period.
        /// | 17 | MidB    | EvalE   | (none)  | (none)  | CS1 | EvalB    | MidE     | MS2 | EvalB-2    | EvalB-1    | CovrB   | MidE    || B      || SP1 covers beginning range but no methods exist for SP2.
        /// 
        /// | 18 | EvalB   | MidE    | (none)  | (none)  | CS1 | MidB     | CovrE    | MS2 | MidBC      | CovrE      | MidB    | CovrE   || null   || Stacks cover ending range of period.
        /// | 19 | EvalB   | MidE    | (none)  | (none)  | CS1 | MidB     | CovrE    | MS2 | MidBC+1    | CovrE      | MidB    | CovrE   || B      || SP2 does not cover first required hour of ending range of period.
        /// | 20 | EvalB   | MidE    | (none)  | (none)  | CS1 | MidB     | CovrE    | MS2 | MidBC      | CovrE-1    | MidB    | CovrE   || B      || SP2 does not cover last required hour of ending range of period.
        /// | 21 | EvalB   | MidE    | (none)  | (none)  | CS1 | MidB     | CovrE    | MS2 | EvalE+1    | EvalE+2    | MidB    | CovrE   || B      || SP1 covers ending range but no methods exist for SP2.
        /// 
        /// | 22 | EvalB   | MidE-31 | MidB+30 | EvalE   | CS1 | MidE-30  | MidB+29  | MS2 | MidE-30c   | MidB+29c   | MidE-30 | MidB+29 || null   || Stacks cover middle range of period.
        /// | 23 | EvalB   | MidE-31 | MidB+30 | EvalE   | CS1 | MidE-30  | MidB+29  | MS2 | MidE-30c+1 | MidB+29c   | MidE-30 | MidB+29 || B      || SP2 does not cover first required hour of the middle range of period.
        /// | 24 | EvalB   | MidE-31 | MidB+30 | EvalE   | CS1 | MidE-30  | MidB+29  | MS2 | MidE-30c   | MidB+29c-1 | MidE-30 | MidB+29 || B      || SP2 does not cover last required hour of the middle range of period.
        /// | 25 | EvalB   | MidE-31 | MidB+30 | EvalE   | CS1 | MidE-30  | MidB+29  | MS2 | EvalB-2    | EvalB-1    | MidE-30 | MidB+29 || B      || SP1 covers middle range but no methods exist for SP2.
        /// 
        /// | 26 | (none)  | (none)  | (none)  | (none)  | CS1 | EvalB    | EvalE    | MP2 | CovrB+1    | CovrE      | CovrB   | CovrE   || null   || SP1 methods cover the entire period, SP2 does not cover the first required hour, but SP have different types.
        /// | 27 | (none)  | (none)  | (none)  | (none)  | CP1 | EvalB    | EvalE    | MS2 | CovrB+1    | CovrE      | CovrB   | CovrE   || null   || SP1 methods cover the entire period, SP2 does not cover the first required hour, but SP have different types..
        /// | 28 | (none)  | (none)  | (none)  | (none)  | MS1 | EvalB    | EvalE    | CP2 | CovrB      | CovrE-1    | CovrB   | CovrE   || null   || SP1 methods cover the entire period, SP2 does not cover the last required hour, but SP have different types..
        /// | 29 | (none)  | (none)  | (none)  | (none)  | MP1 | EvalB    | EvalE    | CS2 | CovrB      | CovrE-1    | CovrB   | CovrE   || null   || SP1 methods cover the entire period, SP2 does not cover the last required hour, but SP have different types..
        /// </summary>
        [TestMethod]
        public void Program13_StackAndPipeDates()
        {
            /* Initialize objects generally needed for testing checks. */
            cMpCheckParameters mpCheckParameters = new cMpCheckParameters();
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory(mpCheckParameters);
            cProgramChecks target = new cProgramChecks(mpCheckParameters);

            MpParameters.Init(category.Process);
            MpParameters.Category = category;


            /* Input Parameter Values */
            DateTime evalB = new DateTime(2016, 6, 17, 0, 0, 0);
            DateTime evalE = new DateTime(2018, 6, 17, 23, 0, 0);
            DateTime covrB = new DateTime(2016, 6, 17, 23, 0, 0);
            DateTime covrE = new DateTime(2018, 6, 17, 0, 0, 0);
            DateTime midB = new DateTime(2017, 6, 17, 22, 0, 0);
            DateTime midE = new DateTime(2017, 6, 17, 21, 0, 0);
            DateTime midBC = new DateTime(2017, 6, 17, 23, 0, 0);
            DateTime midEC = new DateTime(2017, 6, 17, 0, 0, 0);

            DateTime?[] unitRange1BegList = { evalB.AddHours(-2), evalE.AddHours(1), midB, evalB,
                                              null, null, null, null, null, null, null, null, null, null,
                                              midB, midB, midB, midB,
                                              evalB, evalB, evalB, evalB,
                                              evalB, evalB, evalB, evalB,
                                              null, null, null, null };
            DateTime?[] unitRange1EndList = { evalB.AddHours(-1), evalE.AddHours(2), evalE, midE,
                                              null, null, null, null, null, null, null, null, null, null,
                                              evalE, evalE, evalE, evalE,
                                              midE, midE, midE, midE,
                                              midE.AddHours(-31), midE.AddHours(-31), midE.AddHours(-31), midE.AddHours(-31),
                                              null, null, null, null };
            DateTime?[] unitRange2BegList = { null, null, null, null,
                                              null, null, null, null, null, null, null, null, null, null,
                                              null, null, null, null,
                                              null, null, null, null,
                                              midB.AddHours(30), midB.AddHours(30), midB.AddHours(30), midB.AddHours(30),
                                              null, null, null, null };
            DateTime?[] unitRange2EndList = { null, null, null, null,
                                              null, null, null, null, null, null, null, null, null, null,
                                              null, null, null, null,
                                              null, null, null, null,
                                              evalE, evalE, evalE, evalE,
                                              null, null, null, null };

            string[] sp1NameList = { "CS1", "CS1", "CS1", "CS1",
                                     "CS1", "CP1", "CS1", "CP1", "MS1", "MP1", "CS1", "CP1", "MS1", "MP1",
                                     "CS1", "CS1", "CS1", "CS1",
                                     "CS1", "CS1", "CS1", "CS1",
                                     "CS1", "CS1", "CS1", "CS1",
                                     "CS1", "CP1", "MS1", "MP1" };
            DateTime?[] sp1BegList = { evalB.AddHours(-2), evalE.AddHours(1), evalB.AddHours(-2), evalE.AddHours(1),
                                       evalB, evalB, evalB, evalB, evalB, evalB, evalB, evalB, evalB, evalB,
                                       evalB, evalB, evalB, evalB,
                                       midB, midB, midB, midB,
                                       midE.AddHours(-30), midE.AddHours(-30), midE.AddHours(-30), midE.AddHours(-30),
                                       evalB, evalB, evalB, evalB };
            DateTime?[] sp1EndList = { evalB.AddHours(-1), evalE.AddHours(2), evalB.AddHours(-1), evalE.AddHours(2),
                                       evalE, evalE, evalE, evalE, evalE, evalE, evalE, evalE, evalE, evalE,
                                       midE, midE, midE, midE,
                                       covrE, covrE, covrE, covrE,
                                       midB.AddHours(29), midB.AddHours(29), midB.AddHours(29), midB.AddHours(29),
                                       evalE, evalE, evalE, evalE };

            string[] sp2NameList = { "MS2", "MS2", "MS2", "MS2",
                                     "MS2", "MP2", "MS2", "MP2", "CS2", "CP2", "MS2", "MP2", "CS2", "CP2",
                                     "MS2", "MS2", "MS2", "MS2",
                                     "MS2", "MS2", "MS2", "MS2",
                                     "MS2", "MS2", "MS2", "MS2",
                                     "MP2", "MS2", "CP2", "CS2" };
            DateTime?[] sp2BegList = { evalB.AddHours(-2), evalE.AddHours(1), evalB.AddHours(-2), evalE.AddHours(1),
                                       evalB, evalB, covrB, covrB, covrB, covrB, covrB.AddHours(1), covrB.AddHours(1), covrB, covrB,
                                       covrB, covrB.AddHours(1), covrB, evalB.AddHours(-2),
                                       midBC, midBC.AddHours(1), midBC, evalE.AddHours(1),
                                       midE.AddHours(-30).Date.AddHours(23), midE.AddHours(-30).Date.AddHours(24), midE.AddHours(-30).Date.AddHours(23), evalB.AddHours(-2),
                                       covrB.AddHours(1), covrB.AddHours(1), covrB, covrB };
            DateTime?[] sp2EndList = { evalB.AddHours(-1), evalE.AddHours(2), evalB.AddHours(-1), evalE.AddHours(2),
                                       evalE, evalE, covrE, covrE, covrE, covrE, covrE, covrE, covrE.AddHours(-1), covrE.AddHours(-1),
                                       midEC, midEC, midEC.AddHours(-1), evalB.AddHours(-1),
                                       covrE, covrE, covrE.AddHours(-1), evalE.AddHours(2),
                                       midB.AddHours(29).Date, midB.AddHours(29).Date, midB.AddHours(29).Date.AddHours(-1), evalB.AddHours(-1).Date,
                                       covrE, covrE, covrE.AddHours(-1), covrE.AddHours(-1) };

            DateTime?[] uscBegList = { covrB.Date, midB.Date, covrB.Date, midB.Date,
                                       covrB.Date, covrB.Date, covrB.Date, covrB.Date, covrB.Date, covrB.Date, covrB.Date, covrB.Date, covrB.Date, covrB.Date,
                                       covrB.Date, covrB.Date, covrB.Date, covrB.Date,
                                       midB.Date, midB.Date, midB.Date, midB.Date,
                                       midE.AddHours(-30).Date, midE.AddHours(-30).Date, midE.AddHours(-30).Date, midE.AddHours(-30).Date,
                                       covrB.Date, covrB.Date, covrB.Date, covrB.Date };
            DateTime?[] uscEndList = { midE.Date, covrE.Date, midE.Date, covrE.Date,
                                       covrE.Date, covrE.Date, covrE.Date, covrE.Date, covrE.Date, covrE.Date, covrE.Date, covrE.Date, covrE.Date, covrE.Date,
                                       midE.Date, midE.Date, midE.Date, midE.Date,
                                       covrE.Date, covrE.Date, covrE.Date, covrE.Date,
                                       midB.AddHours(29).Date, midB.AddHours(29).Date, midB.AddHours(29).Date, midB.AddHours(29).Date,
                                       covrE.Date, covrE.Date, covrE.Date, covrE.Date, };

            /* Expected Values */
            string[] expResultList = { "A", "A", "B", "B",
                                       null, null, null, null, null, null, "B", "B", "B", "B",
                                       null, "B", "B", "B",
                                       null, "B", "B", "B",
                                       null, "B", "B", "B",
                                       null, null, null, null };

            /* Test Case Count */
            int caseCount = 30;

            /* Check array lengths */
            Assert.AreEqual(caseCount, unitRange1BegList.Length, "unitRange1BegList length");
            Assert.AreEqual(caseCount, unitRange1EndList.Length, "unitRange1EndList length");
            Assert.AreEqual(caseCount, unitRange2BegList.Length, "unitRange2BegList length");
            Assert.AreEqual(caseCount, unitRange2EndList.Length, "unitRange2EndList length");
            Assert.AreEqual(caseCount, sp1NameList.Length, "sp1NameList length");
            Assert.AreEqual(caseCount, sp1BegList.Length, "sp1BegList length");
            Assert.AreEqual(caseCount, sp1EndList.Length, "sp1EndList length");
            Assert.AreEqual(caseCount, sp2NameList.Length, "sp2NameList length");
            Assert.AreEqual(caseCount, sp2BegList.Length, "sp2BegList length");
            Assert.AreEqual(caseCount, sp2EndList.Length, "sp2EndList length");
            Assert.AreEqual(caseCount, uscBegList.Length, "uscBegList length");
            Assert.AreEqual(caseCount, uscEndList.Length, "uscEndList length");
            Assert.AreEqual(caseCount, expResultList.Length, "expResultList length");


            /* Run Cases */
            for (int caseDex = 0; caseDex < caseCount; caseDex++)
            {
                /* Initialize Input Parameters */
                MpParameters.CrosscheckProgramparametertomethodparameter = new CheckDataView<ProgramParameterToMethodParameterRow>
                    (
                        new ProgramParameterToMethodParameterRow(programParameterCd: "BAD", methodParameterList: "BAD,BADM", methodParameterDescription: "Bad Stuff"),
                        new ProgramParameterToMethodParameterRow(programParameterCd: "CO2", methodParameterList: "CO2,CO2M", methodParameterDescription: "Carbon Dioxide"),
                        new ProgramParameterToMethodParameterRow(programParameterCd: "MAL", methodParameterList: "MAL,MALM", methodParameterDescription: "Bad French Stuff")
                    );
                MpParameters.CurrentProgramParameter = new UnitProgramParameter(monLocId: "UnitLocKey", unitId: 13, prgCd: "NSPS4T", parameterCd: "CO2", classCd: "A", requiredInd: 1);
                MpParameters.CurrentProgramParameterActive = true;
                MpParameters.FacilityMethodRecords = new CheckDataView<VwMonitorMethodRow>();
                {
                    if (unitRange1BegList[caseDex] != null)
                        MpParameters.FacilityMethodRecords.Add(SetValues.DateHour(new VwMonitorMethodRow(unitId: 13, monLocId: "UnitLocKey", parameterCd: "CO2"), unitRange1BegList[caseDex], unitRange1EndList[caseDex]));
                    if (unitRange2BegList[caseDex] != null)
                        MpParameters.FacilityMethodRecords.Add(SetValues.DateHour(new VwMonitorMethodRow(unitId: 13, monLocId: "UnitLocKey", parameterCd: "CO2M"), unitRange2BegList[caseDex], unitRange2EndList[caseDex]));

                    MpParameters.FacilityMethodRecords.Add(SetValues.DateHour(new VwMonitorMethodRow(stackPipeId: "SP1", monLocId: "loc1", parameterCd: "CO2"), sp1BegList[caseDex], sp1EndList[caseDex]));
                    MpParameters.FacilityMethodRecords.Add(SetValues.DateHour(new VwMonitorMethodRow(stackPipeId: "SP2", monLocId: "loc2", parameterCd: "CO2M"), sp2BegList[caseDex], sp2EndList[caseDex]));
                };
                MpParameters.ProgramParameterEvaluationBeginDate = evalB.Date;
                MpParameters.ProgramParameterEvaluationEndDate = evalE.Date;
                MpParameters.UnitStackConfigurationRecords = new CheckDataView<VwUnitStackConfigurationRow>
                    (
                        new VwUnitStackConfigurationRow(unitId: 13, stackPipeId: "SP1", stackPipeMonLocId: "loc1", stackName: sp1NameList[caseDex], beginDate: uscBegList[caseDex].Value, endDate: uscEndList[caseDex].Value),
                        new VwUnitStackConfigurationRow(unitId: 13, stackPipeId: "SP2", stackPipeMonLocId: "loc2", stackName: sp2NameList[caseDex], beginDate: uscBegList[caseDex].Value, endDate: uscEndList[caseDex].Value)
                    );


                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                /* Initialize variables needed to run the check. */
                bool log = false;
                string actual;

                /* Run Check */
                actual = target.PROGRAM13(category, ref log);

                /* Check Results */
                Assert.AreEqual(string.Empty, actual, string.Format("actual {0}", caseDex));
                Assert.AreEqual(false, log, string.Format("log {0}", caseDex));

                Assert.AreEqual(expResultList[caseDex], category.CheckCatalogResult, string.Format("category.CheckCatalogResult {0}", caseDex));
                Assert.AreEqual("Carbon Dioxide", MpParameters.Nsps4tMethodParameterDescription, string.Format("Nsps4tMethodParameterDescription {0}", caseDex));
            }

        }

        #endregion

    }
}
