using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using ECMPS.Checks.CheckEngine;
using ECMPS.Checks.FormulaChecks;
using ECMPS.Checks.MonitorPlan;
using ECMPS.Definitions.Extensions;

using ECMPS.Checks.Data.Ecmps.Dbo.Table;
using ECMPS.Checks.Data.Ecmps.Dbo.View;
using ECMPS.Checks.Data.EcmpsAux.CrossCheck.Virtual;
using ECMPS.Checks.Data.Ecmps.CrossCheck.Table;
using ECMPS.Checks.Mp.Parameters;

using UnitTest.UtilityClasses;

namespace UnitTest.MonitorPlan
{
    [TestClass]
    public class cFormulaChecksTest
    {
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
        /// </summary>
        [TestMethod()]
        public void Formula10()
        {
            /* Initialize objects generally needed for testing checks. */
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            MpParameters.Init(category.Process);
            MpParameters.Category = category;


            /* Setup */
            DateTime beginDateHour = new DateTime(2001, 10, 1);
            DateTime?[] endDateHourList = { null, new DateTime(2020, 6, 17) };
            string[] formulaEquationList = { null, "", " ", "Equation Text" };

            /* Generate Cases */
            foreach (string equationCd in UnitTestStandardLists.EquationCodeList)
                foreach (string formulaEquation in formulaEquationList)
                    foreach(DateTime? endDateHour in endDateHourList)
                    {
                        /* Initialize Input Parameters */
                        MpParameters.CurrentFormula = SetValues.DateHour(new VwMonitorFormulaRow(equationCd: equationCd, formulaEquation: formulaEquation), beginDateHour, endDateHour);

                        /* Determine Expected Result */
                        string expResult = (((equationCd == "N-GAS") || (equationCd == "N-OIL")) && string.IsNullOrWhiteSpace(formulaEquation) && (endDateHour == null)) ? "A" : null;

                        /* Init Cateogry Result */
                        category.CheckCatalogResult = null;

                        /* Initialize variables needed to run the check. */
                        bool log = false;
                        string actual;

                        /* Run Check */
                        actual = cFormulaChecks.FORMULA10(category, ref log);

                        /* Check Results */
                        Assert.AreEqual(string.Empty, actual, $"actual - Code '{equationCd}', Text '{formulaEquation}', End '{endDateHour}'");
                        Assert.AreEqual(false, log, $"log - Code '{equationCd}', Text '{formulaEquation}', End '{endDateHour}'");
                        Assert.AreEqual(expResult, category.CheckCatalogResult, $"Result - Code '{equationCd}', Text '{formulaEquation}', End '{endDateHour}'");
                    }
        }


        #region Formula-12

        #region Formula-13

        /// <summary>
        ///A test for FORMULA13_RemoveHG
        ///</summary>()
        [TestMethod()]
        public void FORMULA13_RemoveHG()
        {
            //static check setup
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            MpParameters.Init(category.Process);
            MpParameters.Category = category;

            // Variables
            bool log = false;
            string actual;
            string[] testParameterList = { "HGC", "HGM" };

            // Init Input
            MpParameters.FormulaCodeValid = true;
            MpParameters.FormulaEvaluationBeginDate = DateTime.Today.AddDays(-20);
            MpParameters.FormulaEvaluationBeginHour = 0;
            MpParameters.FormulaEvaluationEndDate = DateTime.Today;
            MpParameters.FormulaEvaluationEndHour = 23;
            MpParameters.FormulaParameterValid = true;
            MpParameters.FormulaRecords = new CheckDataView<VwMonitorFormulaRow>();
            MpParameters.LocationFuelRecords = new CheckDataView<VwLocationFuelRow>();
            MpParameters.LocationSystemComponentRecords = new CheckDataView<VwMonitorSystemComponentRow>();
            MpParameters.MethodRecords = new CheckDataView<VwMonitorMethodRow>();
            MpParameters.UnitMonitorSystemRecords = new CheckDataView<VwUnitMonitorSystemRow>();
            MpParameters.MonitorSystemRecords = new CheckDataView<VwMonitorSystemRow>();

            foreach (string testParameterCode in testParameterList)
            {
                MpParameters.CurrentFormula = new VwMonitorFormulaRow(parameterCd: testParameterCode, equationCd: "FORMULA");
                MpParameters.FormulaParameterAndComponentTypeAndBasisToFormulaCodeCrossCheckTable = new CheckDataView<FormulaParameterAndComponentTypeAndBasisToFormulaCodeRow>
                    (new FormulaParameterAndComponentTypeAndBasisToFormulaCodeRow(parameterCode: testParameterCode, formulaCode: "FORMULA", componentTypeAndBasis: "NOTNULL"));

                // Init Output
                category.CheckCatalogResult = null;

                // Run Checks
                actual = cFormulaChecks.FORMULA13(category, ref log);

                // Check Results
                Assert.AreEqual(string.Empty, actual);
                Assert.AreEqual(false, log);
                Assert.AreEqual("D", category.CheckCatalogResult, "Result");
                Assert.AreEqual("NOTNULL", MpParameters.AppropriateSystemOrComponentForFormula, "AppropriateSystemOrComponentForFormula");
            }
        }
        #endregion

        /// <summary>
        ///A test for FORMULA12_F2M_XCHECK
        ///</summary>()
        [TestMethod()]
        public void FORMULA12_F2M_XCheck()
        {
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            MpParameters.Init(category.Process);
            MpParameters.Category = category;

            // Variables
            bool log = false;
            string actual;
            //bool testTrue = false;

            // Init Input
            MpParameters.CurrentFormula = new VwMonitorFormulaRow(equationCd: "FORMULAGOOD", monLocId: "LOC1");
            MpParameters.FormulaParameterValid = true;
            MpParameters.FormulaCodeValid = true;
            MpParameters.FormulaToRequiredUnitFuelCrosscheck = new CheckDataView<FormulaToRequiredUnitFuelRow>();
            MpParameters.FormulaEvaluationBeginDate = DateTime.Today.AddDays(-10);
            MpParameters.FormulaEvaluationBeginHour = 0;
            MpParameters.FormulaEvaluationEndDate = DateTime.Today;
            MpParameters.FormulaEvaluationEndHour = 0;
            MpParameters.FormulaToRequiredMethodCrosscheck = new CheckDataView<FormulaToRequiredMethodRow>(
                new FormulaToRequiredMethodRow(methodCode: "METHOD1", formulaCode: "FORMULAGOOD", methodParameter: "PARAMETER1"),
                new FormulaToRequiredMethodRow(methodCode: "METHOD2", formulaCode: "FORMULAGOOD", methodParameter: "PARAMETER2"));
            MpParameters.LocationFuelRecords = new CheckDataView<VwLocationFuelRow>();

            // result A
            {
                MpParameters.MethodRecords = new CheckDataView<VwMonitorMethodRow>();


                // Init Output
                category.CheckCatalogResult = null;
                MpParameters.AppropriateMethodForFormula = null;

                // Run Checks
                actual = cFormulaChecks.FORMULA12(category, ref log);

                // Check Results
                Assert.AreEqual(string.Empty, actual);
                Assert.AreEqual(false, log);
                Assert.AreEqual("A", category.CheckCatalogResult, "Result");
                Assert.IsNotNull(MpParameters.AppropriateMethodForFormula);
            }

            // result not A
            {
                MpParameters.MethodRecords = new CheckDataView<VwMonitorMethodRow>(
                    new VwMonitorMethodRow(parameterCd: "PARAMETER1", methodCd: "METHOD1", monLocId: "LOC1",
                                beginDate: DateTime.Today.AddDays(-10), beginHour: 0, endDate: DateTime.Today.AddDays(-7), endHour: 23),
                        new VwMonitorMethodRow(parameterCd: "PARAMETER2", methodCd: "METHOD2", monLocId: "LOC1",
                                beginDate: DateTime.Today.AddDays(-6), beginHour: 0, endDate: DateTime.Today.AddDays(-3), endHour: 23)
                            );

                // Init Output
                category.CheckCatalogResult = null;
                MpParameters.AppropriateMethodForFormula = null;

                // Run Checks
                actual = cFormulaChecks.FORMULA12(category, ref log);

                // Check Results
                Assert.AreEqual(string.Empty, actual);
                Assert.AreEqual(false, log);
                Assert.AreEqual(null, category.CheckCatalogResult, "Result");
                Assert.IsNull(MpParameters.AppropriateMethodForFormula);
            }
        }

        /// <summary>
        ///A test for FORMULA12_F2UF_XCHECK
        ///</summary>()
        [TestMethod()]
        public void FORMULA12_F2UF_XCheck()
        {
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            MpParameters.Init(category.Process);
            MpParameters.Category = category;

            // Variables
            bool log = false;
            string actual;
            //bool testTrue = false;

            // Init Input
            MpParameters.CurrentFormula = new VwMonitorFormulaRow(equationCd: "FORMULAGOOD", monLocId: "LOC1");
            MpParameters.FormulaParameterValid = true;
            MpParameters.FormulaCodeValid = true;
            MpParameters.FormulaEvaluationBeginDate = DateTime.Today.AddDays(-10);
            MpParameters.FormulaEvaluationBeginHour = 0;
            MpParameters.FormulaEvaluationEndDate = DateTime.Today;
            MpParameters.FormulaEvaluationEndHour = 0;
            MpParameters.FormulaToRequiredMethodCrosscheck = new CheckDataView<FormulaToRequiredMethodRow>(
                    new FormulaToRequiredMethodRow(methodCode: "METHOD1", formulaCode: "FORMULAGOOD", methodParameter: "PARAMETER1"),
                    new FormulaToRequiredMethodRow(methodCode: "METHOD2", formulaCode: "FORMULAGOOD", methodParameter: "PARAMETER2"));
            MpParameters.MethodRecords = new CheckDataView<VwMonitorMethodRow>(
                new VwMonitorMethodRow(parameterCd: "PARAMETER1", methodCd: "METHOD1", monLocId: "LOC1",
                              beginDate: DateTime.Today.AddDays(-10), beginHour: 0, endDate: DateTime.Today.AddDays(-7), endHour: 23),
                  new VwMonitorMethodRow(parameterCd: "PARAMETER2", methodCd: "METHOD2", monLocId: "LOC1",
                              beginDate: DateTime.Today.AddDays(-6), beginHour: 0, endDate: DateTime.Today.AddDays(-3), endHour: 23)
                          );
            MpParameters.FormulaToRequiredUnitFuelCrosscheck = new CheckDataView<FormulaToRequiredUnitFuelRow>(
                new FormulaToRequiredUnitFuelRow(formulaCode: "FORMULAGOOD", unitFuelCode: "FUELGOOD"));

            // result B
            {
                MpParameters.LocationFuelRecords = new CheckDataView<VwLocationFuelRow>();


                // Init Output
                category.CheckCatalogResult = null;
                MpParameters.AppropriateMethodForFormula = null;

                // Run Checks
                actual = cFormulaChecks.FORMULA12(category, ref log);

                // Check Results
                Assert.AreEqual(string.Empty, actual);
                Assert.AreEqual(false, log);
                Assert.AreEqual("B", category.CheckCatalogResult, "Result");
                Assert.IsNull(MpParameters.AppropriateMethodForFormula);
            }

            // result not B
            {
                MpParameters.LocationFuelRecords = new CheckDataView<VwLocationFuelRow>(
                    new VwLocationFuelRow(fuelCd: "FUELGOOD", beginDate: DateTime.Today.AddDays(-10), endDate: DateTime.Today.AddHours(23)));

                // Init Output
                category.CheckCatalogResult = null;
                MpParameters.AppropriateMethodForFormula = null;

                // Run Checks
                actual = cFormulaChecks.FORMULA12(category, ref log);

                // Check Results
                Assert.AreEqual(string.Empty, actual);
                Assert.AreEqual(false, log);
                Assert.AreEqual(null, category.CheckCatalogResult, "Result");
                Assert.IsNull(MpParameters.AppropriateMethodForFormula);
            }
        }

        #endregion


        /// <summary>
        /// 
        /// Fourmula13_SpanChecking()
        /// 
        /// EvalB: 2020-06-01 12
        /// Cmp1E: 2020-06-08 17
        /// Cmp1B: 2020-06-08 18
        /// Sys1E: 2020-06-15 23
        /// Sys2B: 2020-06-16 00
        /// Cmp2E: 2020-06-23 05
        /// Cmp2B: 2020-06-23 06
        /// EvalE: 2020-06-30 11
        /// 
        /// 
        /// Formula to System and Component Type:
        /// 
        /// | Formula | SType | CType |
        /// | N-GAS   | GAS   | GFFM  |
        /// | N-OIL   | OILM  | OFFM  |
        /// 
        /// 
        /// Component 0 for each system: 
        ///
        ///  | Sys | Began   | Ended   |
        ///  |   1 | EvalB   | null    |
        ///  |   2 | Sys2B   | null    |
        /// 
        /// 
        /// 
        ///                      |                          System 1                         |                          System 2                         ||
        ///      |    Formula    |       System      |      Component 1  |      Component 2  |       System      |  Component 1      |  Component 2      ||
        /// | ## | Code  | EndDt | Began   | Ended   | Began   | Ended   | Began   | Ended   | Began   | Ended   | Began   | Ended   | Began   | Ended   || Result | Appropriate  || Notes
        /// |  0 | N-GAS | null  | EvalB   | Sys1E   | EvalB   | Cmp1E   | Cmp1B   | Sys1E   | Sys2B   | EvalE   | Sys2B   | Cmp2E   | Cmp2B   | EvalE   || null   | empty        || Systems with two components cover.
        /// |  1 | N-GAS | null  | EvalB+1 | Sys1E   | EvalB   | Cmp1E   | Cmp1B   | Sys1E   | Sys2B   | EvalE   | Sys2B   | Cmp2E   | Cmp2B   | EvalE   || C      | GAS or LTGS  || System 1 begin gap.
        /// |  2 | N-GAS | null  | EvalB   | Sys1E-1 | EvalB   | Cmp1E   | Cmp1B   | Sys1E   | Sys2B   | EvalE   | Sys2B   | Cmp2E   | Cmp2B   | EvalE   || C      | GAS or LTGS  || System 1 end gap.
        /// |  3 | N-GAS | null  | EvalB   | Sys1E   | EvalB+1 | Cmp1E   | Cmp1B   | Sys1E   | Sys2B   | EvalE   | Sys2B   | Cmp2E   | Cmp2B   | EvalE   || C      | GAS or LTGS  || System 1 Component 1 begin gap.
        /// |  4 | N-GAS | null  | EvalB   | Sys1E   | EvalB   | Cmp1E-1 | Cmp1B   | Sys1E   | Sys2B   | EvalE   | Sys2B   | Cmp2E   | Cmp2B   | EvalE   || C      | GAS or LTGS  || System 1 Component 1 end gap.
        /// |  5 | N-GAS | null  | EvalB   | Sys1E   | EvalB   | Cmp1E   | Cmp1B+1 | Sys1E   | Sys2B   | EvalE   | Sys2B   | Cmp2E   | Cmp2B   | EvalE   || C      | GAS or LTGS  || System 1 Component 2 begin gap.
        /// |  6 | N-GAS | null  | EvalB   | Sys1E   | EvalB   | Cmp1E   | Cmp1B   | Sys1E-1 | Sys2B   | EvalE   | Sys2B   | Cmp2E   | Cmp2B   | EvalE   || C      | GAS or LTGS  || System 1 Component 2 end gap.
        /// |  7 | N-GAS | null  | EvalB   | Sys1E   | EvalB   | Cmp1E   | Cmp1B   | Sys1E   | Sys2B+1 | EvalE   | Sys2B   | Cmp2E   | Cmp2B   | EvalE   || C      | GAS or LTGS  || System 2 begin gap.
        /// |  8 | N-GAS | null  | EvalB   | Sys1E   | EvalB   | Cmp1E   | Cmp1B   | Sys1E   | Sys2B   | EvalE-1 | Sys2B   | Cmp2E   | Cmp2B   | EvalE   || C      | GAS or LTGS  || System 2 end gap.
        /// |  9 | N-GAS | null  | EvalB   | Sys1E   | EvalB   | Cmp1E   | Cmp1B   | Sys1E   | Sys2B   | EvalE   | Sys2B+1 | Cmp2E   | Cmp2B   | EvalE   || C      | GAS or LTGS  || System 2 Component 1 begin gap.
        /// | 10 | N-GAS | null  | EvalB   | Sys1E   | EvalB   | Cmp1E   | Cmp1B   | Sys1E   | Sys2B   | EvalE   | Sys2B   | Cmp2E-1 | Cmp2B   | EvalE   || C      | GAS or LTGS  || System 2 Component 1 end gap.
        /// | 11 | N-GAS | null  | EvalB   | Sys1E   | EvalB   | Cmp1E   | Cmp1B   | Sys1E   | Sys2B   | EvalE   | Sys2B   | Cmp2E   | Cmp2B+1 | EvalE   || C      | GAS or LTGS  || System 2 Component 2 begin gap.
        /// | 12 | N-GAS | null  | EvalB   | Sys1E   | EvalB   | Cmp1E   | Cmp1B   | Sys1E   | Sys2B   | EvalE   | Sys2B   | Cmp2E   | Cmp2B   | EvalE-1 || C      | GAS or LTGS  || System 2 Component 2 end gap.
        /// | 13 | N-OIL | null  | EvalB   | Sys1E   | EvalB   | Cmp1E   | Cmp1B   | Sys1E   | Sys2B   | EvalE   | Sys2B   | Cmp2E   | Cmp2B   | EvalE   || null   | empty        || Systems with two components cover.
        /// | 14 | N-OIL | null  | EvalB+1 | Sys1E   | EvalB   | Cmp1E   | Cmp1B   | Sys1E   | Sys2B   | EvalE   | Sys2B   | Cmp2E   | Cmp2B   | EvalE   || C      | OFFM or BOFF || System 1 begin gap.
        /// | 15 | N-OIL | null  | EvalB   | Sys1E-1 | EvalB   | Cmp1E   | Cmp1B   | Sys1E   | Sys2B   | EvalE   | Sys2B   | Cmp2E   | Cmp2B   | EvalE   || C      | OFFM or BOFF || System 1 end gap.
        /// | 16 | N-OIL | null  | EvalB   | Sys1E   | EvalB+1 | Cmp1E   | Cmp1B   | Sys1E   | Sys2B   | EvalE   | Sys2B   | Cmp2E   | Cmp2B   | EvalE   || C      | OFFM or BOFF || System 1 Component 1 begin gap.
        /// | 17 | N-OIL | null  | EvalB   | Sys1E   | EvalB   | Cmp1E-1 | Cmp1B   | Sys1E   | Sys2B   | EvalE   | Sys2B   | Cmp2E   | Cmp2B   | EvalE   || C      | OFFM or BOFF || System 1 Component 1 end gap.
        /// | 18 | N-OIL | null  | EvalB   | Sys1E   | EvalB   | Cmp1E   | Cmp1B+1 | Sys1E   | Sys2B   | EvalE   | Sys2B   | Cmp2E   | Cmp2B   | EvalE   || C      | OFFM or BOFF || System 1 Component 2 begin gap.
        /// | 19 | N-OIL | null  | EvalB   | Sys1E   | EvalB   | Cmp1E   | Cmp1B   | Sys1E-1 | Sys2B   | EvalE   | Sys2B   | Cmp2E   | Cmp2B   | EvalE   || C      | OFFM or BOFF || System 1 Component 2 end gap.
        /// | 20 | N-OIL | null  | EvalB   | Sys1E   | EvalB   | Cmp1E   | Cmp1B   | Sys1E   | Sys2B+1 | EvalE   | Sys2B   | Cmp2E   | Cmp2B   | EvalE   || C      | OFFM or BOFF || System 2 begin gap.
        /// | 21 | N-OIL | null  | EvalB   | Sys1E   | EvalB   | Cmp1E   | Cmp1B   | Sys1E   | Sys2B   | EvalE-1 | Sys2B   | Cmp2E   | Cmp2B   | EvalE   || C      | OFFM or BOFF || System 2 end gap.
        /// | 22 | N-OIL | null  | EvalB   | Sys1E   | EvalB   | Cmp1E   | Cmp1B   | Sys1E   | Sys2B   | EvalE   | Sys2B+1 | Cmp2E   | Cmp2B   | EvalE   || C      | OFFM or BOFF || System 2 Component 1 begin gap.
        /// | 23 | N-OIL | null  | EvalB   | Sys1E   | EvalB   | Cmp1E   | Cmp1B   | Sys1E   | Sys2B   | EvalE   | Sys2B   | Cmp2E-1 | Cmp2B   | EvalE   || C      | OFFM or BOFF || System 2 Component 1 end gap.
        /// | 24 | N-OIL | null  | EvalB   | Sys1E   | EvalB   | Cmp1E   | Cmp1B   | Sys1E   | Sys2B   | EvalE   | Sys2B   | Cmp2E   | Cmp2B+1 | EvalE   || C      | OFFM or BOFF || System 2 Component 2 begin gap.
        /// | 25 | N-OIL | null  | EvalB   | Sys1E   | EvalB   | Cmp1E   | Cmp1B   | Sys1E   | Sys2B   | EvalE   | Sys2B   | Cmp2E   | Cmp2B   | EvalE-1 || C      | OFFM or BOFF || System 2 Component 2 end gap.
        /// | 26 | N-GAS | EvalE | EvalB   | Sys1E   | EvalB   | Cmp1E   | Cmp1B   | Sys1E   | Sys2B   | EvalE   | Sys2B   | Cmp2E   | Cmp2B   | EvalE-1 || null   | empty        || System 2 Component 2 end gap, inactive formula.
        /// | 27 | N-OIL | EvalE | EvalB   | Sys1E   | EvalB   | Cmp1E   | Cmp1B   | Sys1E   | Sys2B   | EvalE   | Sys2B   | Cmp2E   | Cmp2B   | EvalE-1 || null   | empty        || System 2 Component 2 end gap, inactive formula.
        /// </summary>
        [TestMethod()]
        public void Fourmula13_TwoComponentFormula_SpanChecking()
        {
            /* Initialize objects generally needed for testing checks. */
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            MpParameters.Init(category.Process);
            MpParameters.Category = category;


            /* Setup Value */
            DateTime evalB = new DateTime(2020, 06, 01, 12, 0, 0);
            DateTime cmp1E = new DateTime(2020, 06, 08, 17, 0, 0);
            DateTime cmp1B = new DateTime(2020, 06, 08, 18, 0, 0);
            DateTime sys1E = new DateTime(2020, 06, 15, 23, 0, 0);
            DateTime sys2B = new DateTime(2020, 06, 16, 00, 0, 0);
            DateTime cmp2E = new DateTime(2020, 06, 23, 05, 0, 0);
            DateTime cmp2B = new DateTime(2020, 06, 23, 06, 0, 0);
            DateTime evalE = new DateTime(2020, 06, 30, 11, 0, 0);


            /* Input Parameter Values */
            string[] formulaList = { "N-GAS", "N-GAS", "N-GAS", "N-GAS", "N-GAS", "N-GAS", "N-GAS", "N-GAS", "N-GAS", "N-GAS", "N-GAS", "N-GAS", "N-GAS",
                                     "N-OIL", "N-OIL", "N-OIL", "N-OIL", "N-OIL", "N-OIL", "N-OIL", "N-OIL", "N-OIL", "N-OIL", "N-OIL", "N-OIL", "N-OIL",
                                     "N-GAS", "N-OIL" };
            DateTime?[] formulaEndList = { null, null, null, null, null, null, null, null, null, null, null, null, null,
                                           null, null, null, null, null, null, null, null, null, null, null, null, null,
                                           evalE, evalE};
            DateTime[] sys1BeganList = { evalB, evalB.AddHours(1), evalB, evalB, evalB, evalB, evalB, evalB, evalB, evalB, evalB, evalB, evalB,
                                         evalB, evalB.AddHours(1), evalB, evalB, evalB, evalB, evalB, evalB, evalB, evalB, evalB, evalB, evalB,
                                         evalB, evalB};
            DateTime[] sys1EndedList = { sys1E, sys1E, sys1E.AddHours(-1), sys1E, sys1E, sys1E, sys1E, sys1E, sys1E, sys1E, sys1E, sys1E, sys1E,
                                         sys1E, sys1E, sys1E.AddHours(-1), sys1E, sys1E, sys1E, sys1E, sys1E, sys1E, sys1E, sys1E, sys1E, sys1E,
                                         sys1E, sys1E };
            DateTime[] sys1Cmp1BeganList = { evalB, evalB, evalB, evalB.AddHours(1), evalB, evalB, evalB, evalB, evalB, evalB, evalB, evalB, evalB,
                                             evalB, evalB, evalB, evalB.AddHours(1), evalB, evalB, evalB, evalB, evalB, evalB, evalB, evalB, evalB,
                                             evalB, evalB };
            DateTime[] sys1Cmp1EndedList = { cmp1E, cmp1E, cmp1E, cmp1E, cmp1E.AddHours(-1), cmp1E, cmp1E, cmp1E, cmp1E, cmp1E, cmp1E, cmp1E, cmp1E,
                                             cmp1E, cmp1E, cmp1E, cmp1E, cmp1E.AddHours(-1), cmp1E, cmp1E, cmp1E, cmp1E, cmp1E, cmp1E, cmp1E, cmp1E,
                                             cmp1E, cmp1E };
            DateTime[] sys1Cmp2BeganList = { cmp1B, cmp1B, cmp1B, cmp1B, cmp1B, cmp1B.AddHours(1), cmp1B, cmp1B, cmp1B, cmp1B, cmp1B, cmp1B, cmp1B,
                                             cmp1B, cmp1B, cmp1B, cmp1B, cmp1B, cmp1B.AddHours(1), cmp1B, cmp1B, cmp1B, cmp1B, cmp1B, cmp1B, cmp1B,
                                             cmp1B, cmp1B };
            DateTime[] sys1Cmp2EndedList = { sys1E, sys1E, sys1E, sys1E, sys1E, sys1E, sys1E.AddHours(-1), sys1E, sys1E, sys1E, sys1E, sys1E, sys1E,
                                             sys1E, sys1E, sys1E, sys1E, sys1E, sys1E, sys1E.AddHours(-1), sys1E, sys1E, sys1E, sys1E, sys1E, sys1E,
                                             sys1E, sys1E };
            DateTime[] sys2BeganList = { sys2B, sys2B, sys2B, sys2B, sys2B, sys2B, sys2B, sys2B.AddHours(1), sys2B, sys2B, sys2B, sys2B, sys2B,
                                         sys2B, sys2B, sys2B, sys2B, sys2B, sys2B, sys2B, sys2B.AddHours(1), sys2B, sys2B, sys2B, sys2B, sys2B,
                                         sys2B, sys2B };
            DateTime[] sys2EndedList = { evalE, evalE, evalE, evalE, evalE, evalE, evalE, evalE, evalE.AddHours(-1), evalE, evalE, evalE, evalE,
                                         evalE, evalE, evalE, evalE, evalE, evalE, evalE, evalE, evalE.AddHours(-1), evalE, evalE, evalE, evalE,
                                         evalE, evalE };
            DateTime[] sys2Cmp1BeganList = { sys2B, sys2B, sys2B, sys2B, sys2B, sys2B, sys2B, sys2B, sys2B, sys2B.AddHours(1), sys2B, sys2B, sys2B,
                                             sys2B, sys2B, sys2B, sys2B, sys2B, sys2B, sys2B, sys2B, sys2B, sys2B.AddHours(1), sys2B, sys2B, sys2B,
                                             sys2B, sys2B };
            DateTime[] sys2Cmp1EndedList = { cmp2E, cmp2E, cmp2E, cmp2E, cmp2E, cmp2E, cmp2E, cmp2E, cmp2E, cmp2E, cmp2E.AddHours(-1), cmp2E, cmp2E,
                                             cmp2E, cmp2E, cmp2E, cmp2E, cmp2E, cmp2E, cmp2E, cmp2E, cmp2E, cmp2E, cmp2E.AddHours(-1), cmp2E, cmp2E,
                                             cmp2E, cmp2E };
            DateTime[] sys2Cmp2BeganList = { cmp2B, cmp2B, cmp2B, cmp2B, cmp2B, cmp2B, cmp2B, cmp2B, cmp2B, cmp2B, cmp2B, cmp2B.AddHours(1), cmp2B,
                                             cmp2B, cmp2B, cmp2B, cmp2B, cmp2B, cmp2B, cmp2B, cmp2B, cmp2B, cmp2B, cmp2B, cmp2B.AddHours(1), cmp2B,
                                             cmp2B, cmp2B };
            DateTime[] sys2Cmp2EndedList = { evalE, evalE, evalE, evalE, evalE, evalE, evalE, evalE, evalE, evalE, evalE, evalE, evalE.AddHours(-1),
                                             evalE, evalE, evalE, evalE, evalE, evalE, evalE, evalE, evalE, evalE, evalE, evalE, evalE.AddHours(-1),
                                             evalE.AddHours(-1), evalE.AddHours(-1) };

            /* Expected Values */
            string[] expResultList = { null, "C", "C", "C", "C", "C", "C", "C", "C", "C", "C", "C", "C",
                                       null, "C", "C", "C", "C", "C", "C", "C", "C", "C", "C", "C", "C",
                                       null, null };
            string[] expAppropriatetList = { "", "GAS or LTGS", "GAS or LTGS", "GAS or LTGS", "GAS or LTGS", "GAS or LTGS", "GAS or LTGS", "GAS or LTGS", "GAS or LTGS", "GAS or LTGS", "GAS or LTGS", "GAS or LTGS", "GAS or LTGS",
                                             "", "OILM or OILV", "OILM or OILV", "OILM or OILV", "OILM or OILV", "OILM or OILV", "OILM or OILV", "OILM or OILV", "OILM or OILV", "OILM or OILV", "OILM or OILV", "OILM or OILV", "OILM or OILV",
                                              "", "" };


            /* Test Case Count */
            int caseCount = 28;

            /* Check Array Lengths */
            Assert.AreEqual(caseCount, formulaList.Length, "formulaList length");
            Assert.AreEqual(caseCount, formulaEndList.Length, "formulaEndList length");
            Assert.AreEqual(caseCount, sys1BeganList.Length, "sys1BeganList length");
            Assert.AreEqual(caseCount, sys1EndedList.Length, "sys1EndedList length");
            Assert.AreEqual(caseCount, sys1Cmp1BeganList.Length, "sys1Cmp1BeganList length");
            Assert.AreEqual(caseCount, sys1Cmp1EndedList.Length, "sys1Cmp1EndedList length");
            Assert.AreEqual(caseCount, sys1Cmp2BeganList.Length, "sys1Cmp2BeganList length");
            Assert.AreEqual(caseCount, sys1Cmp2EndedList.Length, "sys1Cmp2EndedList length");
            Assert.AreEqual(caseCount, sys2BeganList.Length, "sys2BeganList length");
            Assert.AreEqual(caseCount, sys2EndedList.Length, "sys2EndedList length");
            Assert.AreEqual(caseCount, sys2Cmp1BeganList.Length, "sys2Cmp1BeganList length");
            Assert.AreEqual(caseCount, sys2Cmp1EndedList.Length, "sys2Cmp1EndedList length");
            Assert.AreEqual(caseCount, sys2Cmp2BeganList.Length, "sys2Cmp2BeganList length");
            Assert.AreEqual(caseCount, sys2Cmp2EndedList.Length, "sys2Cmp2EndedList length");
            Assert.AreEqual(caseCount, expResultList.Length, "expResultList length");
            Assert.AreEqual(caseCount, expAppropriatetList.Length, "expAppropriatetList length");


            /* Run Cases */
            for (int caseDex = 0; caseDex < caseCount; caseDex++)
            {
                /* Set Derivitive Values */
                string systemTypeCd = formulaList[caseDex] == "N-GAS" ? "GAS" : formulaList[caseDex] == "N-OIL" ? "OILM" : null;
                string componentTypeCd = formulaList[caseDex] == "N-GAS" ? "GFFM" : formulaList[caseDex] == "N-OIL" ? "OFFM" : null;

                /* Initialize Input Parameters */
                MpParameters.CurrentFormula =  SetValues.DateHour(new VwMonitorFormulaRow(equationCd: formulaList[caseDex]), evalB, formulaEndList[caseDex]);
                MpParameters.FormulaCodeValid = true;
                MpParameters.FormulaEvaluationBeginDate = evalB.Date;
                MpParameters.FormulaEvaluationBeginHour = evalB.Hour;
                MpParameters.FormulaEvaluationEndDate = evalE.Date;
                MpParameters.FormulaEvaluationEndHour = evalE.Hour;
                MpParameters.FormulaParameterAndComponentTypeAndBasisToFormulaCodeCrossCheckTable = new CheckDataView<FormulaParameterAndComponentTypeAndBasisToFormulaCodeRow>();
                MpParameters.FormulaParameterValid = true;
                MpParameters.FormulaRecords = new CheckDataView<VwMonitorFormulaRow>();
                MpParameters.LocationFuelRecords = new CheckDataView<VwLocationFuelRow>();
                MpParameters.LocationSystemComponentRecords = new CheckDataView<VwMonitorSystemComponentRow>();
                {
                    MpParameters.LocationSystemComponentRecords.Add(SetValues.DateHour(new VwMonitorSystemComponentRow(monSysId: "Sys1", componentId: "CmpId:10", componentTypeCd: componentTypeCd), evalB, null));
                    MpParameters.LocationSystemComponentRecords.Add(SetValues.DateHour(new VwMonitorSystemComponentRow(monSysId: "Sys1", componentId: "CmpId:11", componentTypeCd: componentTypeCd), sys1Cmp1BeganList[caseDex], sys1Cmp1EndedList[caseDex]));
                    MpParameters.LocationSystemComponentRecords.Add(SetValues.DateHour(new VwMonitorSystemComponentRow(monSysId: "Sys1", componentId: "CmpId:12", componentTypeCd: componentTypeCd), sys1Cmp2BeganList[caseDex], sys1Cmp2EndedList[caseDex]));
                    MpParameters.LocationSystemComponentRecords.Add(SetValues.DateHour(new VwMonitorSystemComponentRow(monSysId: "Sys2", componentId: "CmpId:20", componentTypeCd: componentTypeCd), sys2B, null));
                    MpParameters.LocationSystemComponentRecords.Add(SetValues.DateHour(new VwMonitorSystemComponentRow(monSysId: "Sys2", componentId: "CmpId:22", componentTypeCd: componentTypeCd), sys2Cmp1BeganList[caseDex], sys2Cmp1EndedList[caseDex]));
                    MpParameters.LocationSystemComponentRecords.Add(SetValues.DateHour(new VwMonitorSystemComponentRow(monSysId: "Sys2", componentId: "CmpId:22", componentTypeCd: componentTypeCd), sys2Cmp2BeganList[caseDex], sys2Cmp2EndedList[caseDex]));
                }
                MpParameters.MethodRecords = new CheckDataView<VwMonitorMethodRow>();
                MpParameters.MonitorSystemRecords = new CheckDataView<VwMonitorSystemRow>();
                {
                    MpParameters.MonitorSystemRecords.Add(SetValues.DateHour(new VwMonitorSystemRow(monSysId: "Sys1", sysTypeCd: systemTypeCd), sys1BeganList[caseDex], sys1EndedList[caseDex]));
                    MpParameters.MonitorSystemRecords.Add(SetValues.DateHour(new VwMonitorSystemRow(monSysId: "Sys2", sysTypeCd: systemTypeCd), sys2BeganList[caseDex], sys2EndedList[caseDex]));
                }
                MpParameters.UnitMonitorSystemRecords = new CheckDataView<VwUnitMonitorSystemRow>();

                /* Initialize Input Parameters */
                MpParameters.AppropriateSystemOrComponentForFormula = null;


                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                /* Initialize variables needed to run the check. */
                bool log = false;
                string actual;

                /* Run Check */
                actual = cFormulaChecks.FORMULA13(category, ref log);

                /* Check Results */
                Assert.AreEqual(string.Empty, actual, string.Format("actual {0}", caseDex));
                Assert.AreEqual(false, log, string.Format("log {0}", caseDex));
                Assert.AreEqual(expResultList[caseDex], category.CheckCatalogResult, string.Format("category.CheckCatalogResult {0}", caseDex));
                Assert.AreEqual(expAppropriatetList[caseDex], MpParameters.AppropriateSystemOrComponentForFormula, string.Format("AppropriateSystemOrComponentForFormula {0}", caseDex));
            }
        }


        /// <summary>
        /// Fourmula13_TwoComponentFormula_TypeCodeChecking()
        /// 
        /// Checks system and component type combinations for for N-GAS and N-OIL equations.
        /// </summary>
        [TestMethod()]
        public void Fourmula13_TwoComponentFormula_TypeCodeChecking()
        {
            /* Initialize objects generally needed for testing checks. */
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            MpParameters.Init(category.Process);
            MpParameters.Category = category;


            /* Lists */
            string[] componentTypeCdShortList = { "GFFM", "OFFM" };
            string[] equationCdList = { "N-GAS", "N-OIL" };
            string systemTypeTargetList = "GAS,LTGS,OILM,OILV";


            /* Setup Value */
            DateTime evalB = new DateTime(2020, 06, 01, 12, 0, 0);
            DateTime evalE = new DateTime(2020, 06, 30, 11, 0, 0);
            DateTime systB = new DateTime(2020, 02, 12, 22, 0, 0);


            // Run Cases
            foreach (string equationCd in equationCdList)
                foreach (string systemTypeCd in UnitTestStandardLists.SystemTypeCodeList)
                {
                    string[] componentTypeCdGasList1 = systemTypeCd.InList(systemTypeTargetList) ? UnitTestStandardLists.ComponentTypeCodeList : componentTypeCdShortList;
                    string[] componentTypeCdGasList2 = systemTypeCd.InList(systemTypeTargetList) ? UnitTestStandardLists.ComponentTypeCodeList : componentTypeCdShortList;

                    foreach (string componentTypeCd1 in componentTypeCdGasList1)
                        foreach (string componentTypeCd2 in componentTypeCdGasList2)
                        {
                            /* Expected Values */
                            string expAppropriateList, expResult;
                            {
                                if (equationCd == "N-GAS")
                                {
                                    if (systemTypeCd.InList("GAS,LTGS") && componentTypeCd1.InList("GFFM,BGFF") && componentTypeCd2.InList("GFFM,BGFF"))
                                    {
                                        expAppropriateList = "";
                                        expResult = null;
                                    }
                                    else
                                    {
                                        expAppropriateList = "GAS or LTGS";
                                        expResult = "C";
                                    }
                                }
                                else if (equationCd == "N-OIL")
                                {
                                    if (systemTypeCd.InList("OILM,OILV") && componentTypeCd1.InList("OFFM,BOFF") && componentTypeCd2.InList("OFFM,BOFF"))
                                    {
                                        expAppropriateList = "";
                                        expResult = null;
                                    }
                                    else
                                    {
                                        expAppropriateList = "OILM or OILV";
                                        expResult = "C";
                                    }
                                }
                                else
                                {
                                    expAppropriateList = "";
                                    expResult = null;
                                }
                            }

                            /* Initialize Input Parameters */
                            MpParameters.CurrentFormula = new VwMonitorFormulaRow(equationCd: equationCd);
                            MpParameters.FormulaCodeValid = true;
                            MpParameters.FormulaEvaluationBeginDate = evalB.Date;
                            MpParameters.FormulaEvaluationBeginHour = evalB.Hour;
                            MpParameters.FormulaEvaluationEndDate = evalE.Date;
                            MpParameters.FormulaEvaluationEndHour = evalE.Hour;
                            MpParameters.FormulaParameterAndComponentTypeAndBasisToFormulaCodeCrossCheckTable = new CheckDataView<FormulaParameterAndComponentTypeAndBasisToFormulaCodeRow>();
                            MpParameters.FormulaParameterValid = true;
                            MpParameters.FormulaRecords = new CheckDataView<VwMonitorFormulaRow>();
                            MpParameters.LocationFuelRecords = new CheckDataView<VwLocationFuelRow>();
                            MpParameters.LocationSystemComponentRecords = new CheckDataView<VwMonitorSystemComponentRow>();
                            {
                                MpParameters.LocationSystemComponentRecords.Add(SetValues.DateHour(new VwMonitorSystemComponentRow(monSysId: "Sys1", componentId: "CmpId:Cmp1", componentTypeCd: componentTypeCd1), systB, null));
                                MpParameters.LocationSystemComponentRecords.Add(SetValues.DateHour(new VwMonitorSystemComponentRow(monSysId: "Sys1", componentId: "CmpId:Cmp2", componentTypeCd: componentTypeCd2), systB, null));
                            }
                            MpParameters.MethodRecords = new CheckDataView<VwMonitorMethodRow>();
                            MpParameters.MonitorSystemRecords = new CheckDataView<VwMonitorSystemRow>();
                            {
                                MpParameters.MonitorSystemRecords.Add(SetValues.DateHour(new VwMonitorSystemRow(monSysId: "Sys1", sysTypeCd: systemTypeCd), systB, null));
                            }
                            MpParameters.UnitMonitorSystemRecords = new CheckDataView<VwUnitMonitorSystemRow>();

                            /* Initialize Input Parameters */
                            MpParameters.AppropriateSystemOrComponentForFormula = null;


                            /* Init Cateogry Result */
                            category.CheckCatalogResult = null;

                            /* Initialize variables needed to run the check. */
                            bool log = false;
                            string actual;

                            /* Run Check */
                            actual = cFormulaChecks.FORMULA13(category, ref log);

                            /* Check Results */
                            Assert.AreEqual(string.Empty, actual, $"actual - Equation '{equationCd}', SysType '{systemTypeCd}', CmpType1 '{componentTypeCd1}', CmpType2 '{componentTypeCd2}'");
                            Assert.AreEqual(false, log, $"log - Equation '{equationCd}', SysType '{systemTypeCd}', CmpType1 '{componentTypeCd1}', CmpType2 '{componentTypeCd2}'");
                            Assert.AreEqual(expResult, category.CheckCatalogResult, $"category.CheckCatalogResult - Equation '{equationCd}', SysType '{systemTypeCd}', CmpType1 '{componentTypeCd1}', CmpType2 '{componentTypeCd2}'");
                            Assert.AreEqual(expAppropriateList, MpParameters.AppropriateSystemOrComponentForFormula, $"AppropriateSystemOrComponentForFormula - Equation '{equationCd}', SysType '{systemTypeCd}', CmpType1 '{componentTypeCd1}', CmpType2 '{componentTypeCd2}'");
                        }
                }
        }


        //#region Formula-[N]

        ///// <summary>
        /////A test for FORMULA[N]
        /////</summary>()
        //[TestMethod()]
        //public void FORMULA[N]()
        //{
        //    cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

        //    MpParameters.Init(category.Process);
        //    MpParameters.Category = category;

        //    // Variables
        //    bool log = false;
        //    string actual;
        //    //bool testTrue = false;

        //    // Init Input
        //    MpParameters.CurrentFormula = new CheckDataView<MonitorFormulaRow>();

        //    // Init Output
        //    category.CheckCatalogResult = null;

        //    // Run Checks
        //    actual = cFormulaChecks.FORMULA[N](category, ref log);

        //    // Check Results
        //    Assert.AreEqual(string.Empty, actual);
        //    Assert.AreEqual(false, log);
        //    Assert.AreEqual(null, category.CheckCatalogResult, "Result");

        //}
        //#endregion


        /// <summary>
        /// 
        /// Test Notes"
        /// 
        /// The setting of LocationType depends on the contents on Unit Stack Configuration.  The check uses the "US" location type 
        /// to determine that the current location is a unit and that it is linked to some combination of common stacks and multiple 
        /// stacks.  Once the check ensures the location type is "US", it only has to determine that no USC were for CS.  Some of the
        /// cases below abuse the inherent connection between location type and USC to ensure that only one of the paths to result B,
        /// can occur.
        /// 
        /// Test Cases:
        /// 
        /// * EvalB (FormulaEvaluationBeginDate/Hour) : 2016-06-17 22
        /// * EvalE (FormulaEvaluationEndDate/Hour)   : 2017-06-30 23
        /// 
        /// |    |   - Formula -    |             - Method -             |         |            - Configuration -            ||        || 
        /// | ## | Param | Equation | Param | Method | BegHr   | EndHr   | LocType | LocKey  | StackName | BegDt   | EndDt   || Result || Note
        /// |  0 | HGRE  | MS-1     | HGRH  | CALC   | EvalB   | null    | US      | GoodKey | MSONE     | EvalB   | null    || A      || MATS param with MS-1, but no corresponding param method.
        /// |  1 | HGRE  | MS-1     | HGRE  | CALC   | EvalB   | null    | US      | GoodKey | CSONE     | EvalB   | null    || B      || MATS Param with MS-1, an corresponding method, but only a CS USC.
        /// |  2 | HCLRE | MS-1     | HCLRH | CALC   | EvalB   | null    | US      | GoodKey | MSONE     | EvalB   | null    || A      || MATS param with MS-1, but no corresponding param method.
        /// |  3 | HCLRE | MS-1     | HCLRE | CALC   | EvalB   | null    | US      | GoodKey | CSONE     | EvalB   | null    || B      || MATS Param with MS-1, an corresponding method, but only a CS USC.
        /// |  4 | HFRE  | MS-1     | HFRH  | CALC   | EvalB   | null    | US      | GoodKey | MSONE     | EvalB   | null    || A      || MATS param with MS-1, but no corresponding param method.
        /// |  5 | HFRE  | MS-1     | HFRE  | CALC   | EvalB   | null    | US      | GoodKey | CSONE     | EvalB   | null    || B      || MATS Param with MS-1, an corresponding method, but only a CS USC.
        /// |  6 | SO2RE | MS-1     | SO2RH | CALC   | EvalB   | null    | US      | GoodKey | MSONE     | EvalB   | null    || A      || MATS param with MS-1, but no corresponding param method.
        /// |  7 | SO2RE | MS-1     | SO2RE | CALC   | EvalB   | null    | US      | GoodKey | CSONE     | EvalB   | null    || B      || MATS Param with MS-1, an corresponding method, but only a CS USC.
        /// |  8 | HGRH  | MS-1     | HGRE  | CALC   | EvalB   | null    | US      | GoodKey | MSONE     | EvalB   | null    || A      || MATS param with MS-1, but no corresponding param method.
        /// |  9 | HGRH  | MS-1     | HGRH  | CALC   | EvalB   | null    | US      | GoodKey | CSONE     | EvalB   | null    || B      || MATS Param with MS-1, an corresponding method, but only a CS USC.
        /// | 10 | HCLRH | MS-1     | HCLRE | CALC   | EvalB   | null    | US      | GoodKey | MSONE     | EvalB   | null    || A      || MATS param with MS-1, but no corresponding param method.
        /// | 11 | HCLRH | MS-1     | HCLRH | CALC   | EvalB   | null    | US      | GoodKey | CSONE     | EvalB   | null    || B      || MATS Param with MS-1, an corresponding method, but only a CS USC.
        /// | 12 | HFRH  | MS-1     | HFRE  | CALC   | EvalB   | null    | US      | GoodKey | MSONE     | EvalB   | null    || A      || MATS param with MS-1, but no corresponding param method.
        /// | 13 | HFRH  | MS-1     | HFRH  | CALC   | EvalB   | null    | US      | GoodKey | CSONE     | EvalB   | null    || B      || MATS Param with MS-1, an corresponding method, but only a CS USC.
        /// | 14 | SO2RH | MS-1     | SO2RE | CALC   | EvalB   | null    | US      | GoodKey | MSONE     | EvalB   | null    || A      || MATS param with MS-1, but no corresponding param method.
        /// | 15 | SO2RH | MS-1     | SO2RH | CALC   | EvalB   | null    | US      | GoodKey | CSONE     | EvalB   | null    || B      || MATS Param with MS-1, an corresponding method, but only a CS USC.
        /// | 16 | OTHER | MS-1     | null  | null   | null    | null    | U       | null    | null      | null    | null    || null   || Non MATS Parameter.
        /// | 17 | HGRE  | F-21C    | null  | null   | null    | null    | U       | null    | null      | null    | null    || null   || Non MATS Apportionment Equation.
        /// | 18 | HGRE  | MS-1     | null  | null   | null    | null    | U       | null    | null      | null    | null    || A      || MATS Parameter and Equation, but no method.
        /// | 19 | HGRE  | MS-1     | HGRE  | ADCALC | EvalB   | null    | U       | null    | null      | null    | null    || A      || MATS Parameter and Equation, but bad method code.
        /// | 20 | HGRE  | MS-1     | HGRE  | CALC   | EvalB-1 | EvalB   | U       | null    | null      | null    | null    || B      || Method ends on the eval begin hour, but no MS connected to the unit.
        /// | 21 | HGRE  | MS-1     | HGRE  | CALC   | EvalB-2 | EvalB-1 | U       | null    | null      | null    | null    || A      || Method ends before the eval begin hour, but no MS connected to the unit.
        /// | 22 | HGRE  | MS-1     | HGRE  | CALC   | EvalE   | EvalE+1 | U       | null    | null      | null    | null    || B      || Method begins on the eval end hour, but no MS connected to the unit.
        /// | 23 | HGRE  | MS-1     | HGRE  | CALC   | EvalE+1 | EvalE+2 | U       | null    | null      | null    | null    || A      || Method begins after the eval end hour, but no MS connected to the unit.
        /// | 24 | HGRE  | MS-1     | HGRE  | CALC   | EvalB   | null    | U       | GoodKey | MSONE     | EvalB   | null    || B      || Bad location type.
        /// | 25 | HGRE  | MS-1     | HGRE  | CALC   | EvalB   | null    | UP      | GoodKey | MSONE     | EvalB   | null    || B      || Bad location type.
        /// | 26 | HGRE  | MS-1     | HGRE  | CALC   | EvalB   | null    | UB      | GoodKey | MSONE     | EvalB   | null    || B      || Bad location type.
        /// | 27 | HGRE  | MS-1     | HGRE  | CALC   | EvalB   | null    | MS      | GoodKey | MSONE     | EvalB   | null    || B      || Bad location type.
        /// | 28 | HGRE  | MS-1     | HGRE  | CALC   | EvalB   | null    | CS      | GoodKey | MSONE     | EvalB   | null    || B      || Bad location type.
        /// | 29 | HGRE  | MS-1     | HGRE  | CALC   | EvalB   | null    | CP      | GoodKey | MSONE     | EvalB   | null    || B      || Bad location type.
        /// | 30 | HGRE  | MS-1     | HGRE  | CALC   | EvalB   | null    | MP      | GoodKey | MSONE     | EvalB   | null    || B      || Bad location type.
        /// | 31 | HGRE  | MS-1     | HGRE  | CALC   | EvalB   | null    | US      | BadKey  | CSONE     | EvalB   | null    || null   || CS not found because of mismatched unit MON_LOC_ID.
        /// | 32 | HGRE  | MS-1     | HGRE  | CALC   | EvalB   | null    | US      | GoodKey | CSONE     | EvalB-1 | EvalB   || B      || CS found because USC ends on the eval begin hour, but no MS connected to the unit.
        /// | 33 | HGRE  | MS-1     | HGRE  | CALC   | EvalB   | null    | US      | GoodKey | CSONE     | EvalB-2 | EvalB-1 || null   || CS not found because of USC ends before the eval begin hour, but no MS connected to the unit.
        /// | 34 | HGRE  | MS-1     | HGRE  | CALC   | EvalB   | null    | US      | GoodKey | CSONE     | EvalE   | EvalE+1 || B      || CS found because USC begins on the eval end hour, but no MS connected to the unit.
        /// | 35 | HGRE  | MS-1     | HGRE  | CALC   | EvalB   | null    | US      | GoodKey | CSONE     | EvalE+1 | EvalE+2 || null   || CS not found because of USC begins after the eval end hour, but no MS connected to the unit.
        /// | 36 | HGRE  | MS-1     | HGRE  | CALC   | EvalB   | null    | US      | GoodKey | CSONE     | EvalB   | null    || B      || CS found because USC begins on eval begin hour.
        /// | 37 | HGRE  | MS-1     | HGRE  | CALC   | EvalB   | null    | US      | GoodKey | MSONE     | EvalB   | null    || null   || MS USC found.
        /// </summary>
        [TestMethod()]
        public void Formula19()
        {
            /* Initialize objects generally needed for testing checks. */
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            MpParameters.Init(category.Process);
            MpParameters.Category = category;

            /* Input Parameter Values */
            DateTime eHrB = new DateTime(2016, 6, 17, 22, 0, 0);
            DateTime eHrE = new DateTime(2017, 6, 30, 23, 0, 0);
            DateTime eDtB = eHrB.Date;
            DateTime eDtE = eHrE.Date;
            String unitLocKey = "GoodKey";

            string[] frmParamList = { "HGRE", "HGRE", "HCLRE", "HCLRE", "HFRE", "HFRE", "SO2RE", "SO2RE", "HGRH", "HGRH", "HCLRH", "HCLRH", "HFRH", "HFRH", "SO2RH", "SO2RH", "OTHER", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE" };
            string[] frmEquatList = { "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "F-21C", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1" };
            string[] mthParamList = { "HGRH", "HGRE", "HCLRH", "HCLRE", "HFRH", "HFRE", "SO2RH", "SO2RE", "HGRE", "HGRH", "HCLRE", "HCLRH", "HFRE", "HFRH", "SO2RE", "SO2RH", null, null, null, "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE", "HGRE" };
            string[] mthCodeList = { "CALC", "CALC", "CALC", "CALC", "CALC", "CALC", "CALC", "CALC", "CALC", "CALC", "CALC", "CALC", "CALC", "CALC", "CALC", "CALC", null, null, null, "ADCALC", "CALC", "CALC", "CALC", "CALC", "CALC", "CALC", "CALC", "CALC", "CALC", "CALC", "CALC", "CALC", "CALC", "CALC", "CALC", "CALC", "CALC", "CALC" };
            DateTime?[] mthBegList = { eHrB, eHrB, eHrB, eHrB, eHrB, eHrB, eHrB, eHrB, eHrB, eHrB, eHrB, eHrB, eHrB, eHrB, eHrB, eHrB, null, null, null, eHrB, eHrB.AddHours(-1), eHrB.AddHours(-2), eHrE, eHrE.AddHours(1), eHrB, eHrB, eHrB, eHrB, eHrB, eHrB, eHrB, eHrB, eHrB, eHrB, eHrB, eHrB, eHrB, eHrB };
            DateTime?[] mthEndList = { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, eHrB, eHrB.AddHours(-1), eHrE.AddHours(1), eHrE.AddHours(2), null, null, null, null, null, null, null, null, null, null, null, null, null, null };
            string[] locTypeList = { "US", "US", "US", "US", "US", "US", "US", "US", "US", "US", "US", "US", "US", "US", "US", "US", "U", "U", "U", "U", "U", "U", "U", "U", "U", "UP", "UB", "MS", "CS", "CP", "MP", "US", "US", "US", "US", "US", "US", "US" };
            string[] uscLocKeyList = { "GoodKey", "GoodKey", "GoodKey", "GoodKey", "GoodKey", "GoodKey", "GoodKey", "GoodKey", "GoodKey", "GoodKey", "GoodKey", "GoodKey", "GoodKey", "GoodKey", "GoodKey", "GoodKey", null, null, null, null, null, null, null, null, "GoodKey", "GoodKey", "GoodKey", "GoodKey", "GoodKey", "GoodKey", "GoodKey", "BadKey", "GoodKey", "GoodKey", "GoodKey", "GoodKey", "GoodKey", "GoodKey" };
            string[] uscStpNameList = { "MSONE", "CSONE", "MSONE", "CSONE", "MSONE", "CSONE", "MSONE", "CSONE", "MSONE", "CSONE", "MSONE", "CSONE", "MSONE", "CSONE", "MSONE", "CSONE", null, null, null, null, null, null, null, null, "MSONE", "MSONE", "MSONE", "MSONE", "MSONE", "MSONE", "MSONE", "CSONE", "CSONE", "CSONE", "CSONE", "CSONE", "CSONE", "MSONE" };
            DateTime?[] uscBegList = { eDtB, eDtB, eDtB, eDtB, eDtB, eDtB, eDtB, eDtB, eDtB, eDtB, eDtB, eDtB, eDtB, eDtB, eDtB, eDtB, null, null, null, null, null, null, null, null, eDtB, eDtB, eDtB, eDtB, eDtB, eDtB, eDtB, eDtB, eDtB.AddDays(-1), eDtB.AddDays(-2), eDtE, eDtE.AddDays(1), eDtB, eDtB };
            DateTime?[] uscEndList = { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, eDtB, eDtB.AddDays(-1), eDtE.AddDays(1), eDtE.AddDays(2), null, null };

            /* Expected Values */
            string[] resultList = { "A", "B", "A", "B", "A", "B", "A", "B", "A", "B", "A", "B", "A", "B", "A", "B", null, null, "A", "A", "B", "A", "B", "A", "B", "B", "B", "B", "B", "B", "B", null, "B", null, "B", null, "B", null };

            /* Test Case Count */
            int caseCount = 38;

            /* Check array lengths */
            Assert.AreEqual(caseCount, frmParamList.Length, "frmParamList length");
            Assert.AreEqual(caseCount, frmEquatList.Length, "frmEquatList length");
            Assert.AreEqual(caseCount, mthParamList.Length, "mthParamList length");
            Assert.AreEqual(caseCount, mthCodeList.Length, "mthCodeList length");
            Assert.AreEqual(caseCount, mthBegList.Length, "mthBegList length");
            Assert.AreEqual(caseCount, mthEndList.Length, "mthEndList length");
            Assert.AreEqual(caseCount, locTypeList.Length, "locTypeList length");
            Assert.AreEqual(caseCount, uscLocKeyList.Length, "uscLocKeyList length");
            Assert.AreEqual(caseCount, uscStpNameList.Length, "uscStpNameList length");
            Assert.AreEqual(caseCount, uscBegList.Length, "uscBegList length");
            Assert.AreEqual(caseCount, uscEndList.Length, "uscEndList length");
            Assert.AreEqual(caseCount, resultList.Length, "resultList length");

            /* Run Cases */
            for (int caseDex = 0; caseDex < caseCount; caseDex++)
            {
                /* Initialize Input Parameters */
                MpParameters.CurrentFormula = new VwMonitorFormulaRow(monLocId: unitLocKey, parameterCd: frmParamList[caseDex], equationCd: frmEquatList[caseDex]);
                MpParameters.FormulaEvaluationBeginDate = eHrB.Date;
                MpParameters.FormulaEvaluationBeginHour = eHrB.Hour;
                MpParameters.FormulaEvaluationEndDate = eHrE.Date;
                MpParameters.FormulaEvaluationEndHour = eHrE.Hour;
                MpParameters.LocationType = locTypeList[caseDex];
                MpParameters.MethodRecords = new CheckDataView<VwMonitorMethodRow>();
                {
                    MpParameters.MethodRecords.Add(new VwMonitorMethodRow(parameterCd: "HI", methodCd: "CALC", beginDatehour: eHrB, beginDate: eHrB.Date, beginHour: eHrB.Hour));

                    if (mthParamList[caseDex] != null)
                        MpParameters.MethodRecords.Add(new VwMonitorMethodRow(parameterCd: mthParamList[caseDex], methodCd: mthCodeList[caseDex],
                                                                              beginDatehour: mthBegList[caseDex],
                                                                              beginDate: (mthBegList[caseDex] != null) ? mthBegList[caseDex].Value.Date : (DateTime?)null,
                                                                              beginHour: (mthBegList[caseDex] != null) ? mthBegList[caseDex].Value.Hour : (int?)null,
                                                                              endDatehour: mthEndList[caseDex],
                                                                              endDate: (mthEndList[caseDex] != null) ? mthEndList[caseDex].Value.Date : (DateTime?)null,
                                                                              endHour: (mthEndList[caseDex] != null) ? mthEndList[caseDex].Value.Hour : (int?)null));

                    MpParameters.MethodRecords.Add(new VwMonitorMethodRow(parameterCd: "HIT", methodCd: "CALC", beginDatehour: eHrB, beginDate: eHrB.Date, beginHour: eHrB.Hour));
                }
                MpParameters.UnitStackConfigurationRecords = new CheckDataView<VwUnitStackConfigurationRow>();
                {
                    MpParameters.UnitStackConfigurationRecords.Add(new VwUnitStackConfigurationRow(monLocId: "OneKey", stackName: "MSOTHER", beginDate: eDtB));

                    if (uscLocKeyList[caseDex] != null)
                        MpParameters.UnitStackConfigurationRecords.Add(new VwUnitStackConfigurationRow(monLocId: uscLocKeyList[caseDex], stackName: uscStpNameList[caseDex],
                                                                                                       beginDate: uscBegList[caseDex], endDate: uscEndList[caseDex]));

                    MpParameters.UnitStackConfigurationRecords.Add(new VwUnitStackConfigurationRow(monLocId: "TwoKey", stackName: "CSOTHER", beginDate: eDtB));
                }


                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                /* Initialize variables needed to run the check. */
                bool log = false;
                string actual;

                /* Run Check */
                actual = cFormulaChecks.FORMULA19(category, ref log);

                /* Check Results */
                Assert.AreEqual(string.Empty, actual, string.Format("actual {0}", caseDex));
                Assert.AreEqual(false, log, string.Format("log {0}", caseDex));
                Assert.AreEqual(resultList[caseDex], category.CheckCatalogResult, string.Format("category.CheckCatalogResult {0}", caseDex));
            }
        }


        /// <summary>
        /// Formula-20
        /// 
        /// | ## | Valid | EquationCd | LocationType || Result | ValidType       || Note
        /// |  0 | false | MS-2       | U            || null   | ""              || Equation code marked invalid so no result returned.
        /// |  1 | true  | MS-2       | U            || A      | multiple stacks || Location type is for a single unit, so result returned.
        /// |  2 | true  | MS-2       | US           || A      | multiple stacks || Location type is for a unit connected to a stack, so result returned.
        /// |  3 | true  | MS-2       | UP           || A      | multiple stacks || Location type is for a unit connected to a pipe, so result returned.
        /// |  4 | true  | MS-2       | UB           || A      | multiple stacks || Location type is for a unit connected to a stack and a pipe, so result returned.
        /// |  5 | true  | MS-2       | CS           || A      | multiple stacks || Location type is for a common stack, so result returned.
        /// |  6 | true  | MS-2       | CP           || A      | multiple stacks || Location type is for a common pipe, so result returned.
        /// |  7 | true  | MS-2       | MP           || A      | multiple stacks || Location type is for a multiple pipe, so result returned.
        /// |  8 | true  | MS-2       | MS           || null   | ""              || Location type is for a multiple stack, so no result is returned.
        /// |  9 | true  | MS-1       | U            || null   | ""              || MS-1 is not restricted by location type and there for does not return a result.
        /// | 10 | true  | MS-1       | US           || null   | ""              || MS-1 is not restricted by location type and there for does not return a result.
        /// | 11 | true  | MS-1       | UP           || null   | ""              || MS-1 is not restricted by location type and there for does not return a result.
        /// | 12 | true  | MS-1       | UB           || null   | ""              || MS-1 is not restricted by location type and there for does not return a result.
        /// | 13 | true  | MS-1       | CS           || null   | ""              || MS-1 is not restricted by location type and there for does not return a result.
        /// | 14 | true  | MS-1       | CP           || null   | ""              || MS-1 is not restricted by location type and there for does not return a result.
        /// | 15 | true  | MS-1       | MP           || null   | ""              || MS-1 is not restricted by location type and there for does not return a result.
        /// | 16 | true  | MS-1       | MS           || null   | ""              || MS-1 is not restricted by location type and there for does not return a result.
        /// 
        /// </summary>
        [TestMethod]
        public void Formula20()
        {
            /* Initialize objects generally needed for testing checks. */
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            MpParameters.Init(category.Process);
            MpParameters.Category = category;

            /* Input Parameter Values */
            bool?[] formulaCodeValidList = { false, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true };
            string[] equationCodeList = { "MS-2", "MS-2", "MS-2", "MS-2", "MS-2", "MS-2", "MS-2", "MS-2", "MS-2", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1", "MS-1" };
            string[] locationTypeList = { "U", "U", "US", "UP", "UB", "CS", "CP", "MP", "MS", "U", "US", "UP", "UB", "CS", "CP", "MP", "MS" };

            /* Expected Values */
            string[] resultList = { null, "A", "A", "A", "A", "A", "A", "A", null, null, null, null, null, null, null, null, null };
            string[] expValidLocationTypeList = { "", "multiple stacks", "multiple stacks", "multiple stacks", "multiple stacks", "multiple stacks", "multiple stacks", "multiple stacks", "", "", "", "", "", "", "", "", "" };

            /* Test Case Count */
            int caseCount = 17;

            /* Check array lengths */
            Assert.AreEqual(caseCount, formulaCodeValidList.Length, "formulaCodeValidList length");
            Assert.AreEqual(caseCount, equationCodeList.Length, "equationCodeList length");
            Assert.AreEqual(caseCount, locationTypeList.Length, "locationTypeList length");
            Assert.AreEqual(caseCount, resultList.Length, "resultList length");


            /* Run Cases */
            for (int caseDex = 0; caseDex < caseCount; caseDex++)
            {
                /* Initialize Input Parameters */
                MpParameters.CurrentFormula = new VwMonitorFormulaRow(equationCd: equationCodeList[caseDex]);
                MpParameters.FormulaCodeValid = formulaCodeValidList[caseDex];
                MpParameters.LocationType = locationTypeList[caseDex];

                /* Initialize Output Parameters */
                MpParameters.ValidLocationTypes = "Bad Locations";


                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                /* Initialize variables needed to run the check. */
                bool log = false;
                string actual;

                /* Run Check */
                actual = cFormulaChecks.FORMULA20(category, ref log);


                /* Check Results */
                Assert.AreEqual(string.Empty, actual, string.Format("actual {0}", caseDex));
                Assert.AreEqual(false, log, string.Format("log {0}", caseDex));
                Assert.AreEqual(resultList[caseDex], category.CheckCatalogResult, string.Format("category.CheckCatalogResult {0}", caseDex));

                Assert.AreEqual(expValidLocationTypeList[caseDex], MpParameters.ValidLocationTypes, string.Format("ValidLocationTypes {0}", caseDex));
            }

        }

    }
}
