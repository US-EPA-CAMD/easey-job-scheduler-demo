using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using ECMPS.Checks.CheckEngine;
using ECMPS.Checks.CheckEngine.SpecialParameterClasses;
using ECMPS.Checks.Data.Ecmps.Dbo.View;
using ECMPS.Checks.Em.Parameters;
using ECMPS.Checks.EmissionsChecks;
using ECMPS.Definitions.Extensions;

using UnitTest.UtilityClasses;


namespace UnitTest.Emissions
{
    [TestClass()]
    public class cHourlyOperatingDataChecksTest
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

        #region HOUROP-3
        /// <summary>
        ///A test for HOUROP-3_AddMATSParams
        ///</summary>()
        [TestMethod()]
        public void HOUROP3_AddMATSParams()
        {
            //static check setup
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            EmParameters.Init(category.Process);
            EmParameters.Category = category;

            // Variables
            bool log = false;
            string actual;

            // Init Input
            EmParameters.CurrentMonitorPlanLocationRecord = new VwCeMpMonitorLocationRow(locationName: "LOCATION1", monLocId: "LOC1");
            EmParameters.CurrentOperatingDate = DateTime.Now.Date;
            EmParameters.EarliestLocationReportDate = DateTime.Today.AddYears(-1);
            EmParameters.LocationProgramRecordsByHourLocation = new CheckDataView<VwMpLocationProgramRow>();
            EmParameters.MonitorQualificationRecordsByHour = new CheckDataView<VwMpMonitorQualificationRow>();
            EmParameters.UnitStackConfigurationRecordsByHourLocation = new CheckDataView<VwMpUnitStackConfigurationRow>();

            // Init Output
            category.CheckCatalogResult = null;

            // Run Checks
            actual = cHourlyOperatingDataChecks.HOUROP3(category, ref log);

            // Check Results
            Assert.AreEqual(string.Empty, actual);
            Assert.AreEqual(false, log);
            Assert.AreEqual(null, category.CheckCatalogResult, "Result");
            Assert.AreEqual(false, EmParameters.Co2DiluentNeededForMats, "Co2DiluentNeededForMats");
            Assert.AreEqual(false, EmParameters.O2DryNeededForMats, "O2DryNeededForMats");
            Assert.AreEqual(false, EmParameters.O2WetNeededForMats, "O2WetNeededForMats");
            Assert.AreEqual(null, EmParameters.Co2cMhvModc, "Co2cMhvModc");
            Assert.AreEqual(null, EmParameters.H2oDhvModc, "H2oDhvModc");
            Assert.AreEqual(null, EmParameters.H2oMhvModc, "H2oMhvModc");
            Assert.AreEqual(null, EmParameters.O2DryModc, "O2DryModc");
            Assert.AreEqual(null, EmParameters.O2WetModc, "O2WetModc");

            Assert.AreEqual(false, EmParameters.So2HpffExists, "So2HpffExists");
            Assert.AreEqual(false, EmParameters.Co2HpffExists, "Co2HpffExists");
            Assert.AreEqual(false, EmParameters.HiHpffExists, "HiHpffExists");

            Assert.AreEqual(false, EmParameters.FlowNeededForPart75, "FlowNeededForPart75Exists");
        }
        #endregion

        /// <summary>
        /// HOUROP-18
        /// 
        /// Test combinations of the following parameters:
        /// 
        /// 1) SO2 Monitor Hourly Value Records By Hour and Location: Contains two rows with changing combinations of parameter codes.
        /// 2) SO2 CEM Method Active for Hour: true, false and null.
        /// 3) MATS SO2 Needed: true, false and null.
        /// 4) Unit Hourly Operational Status: true, false and null.
        /// </summary>()
        [TestMethod()]
        public void HOUROP18()
        {
            /* Initialize objects generally needed for testing checks. */
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            EmParameters.Init(category.Process);
            EmParameters.Category = category;

            /* Initialize variables needed to run the check. */
            bool log = false;
            string actual;

            /* Loop Through Parmeter Combinations*/
            foreach (bool? unitHourlyOperationalStatus in UnitTestStandardLists.ValidList)
                foreach (bool? so2CemMethodActiveForHour in UnitTestStandardLists.ValidList)
                    foreach (bool? matsSo2cNeeded in UnitTestStandardLists.ValidList)
                        for (int so2MonitorHourlyCount = 0; so2MonitorHourlyCount <= 2; so2MonitorHourlyCount++)
                        {
                            /*  Initialize Input Parameters*/
                            EmParameters.UnitHourlyOperationalStatus = unitHourlyOperationalStatus;
                            EmParameters.So2CemMethodActiveForHour = so2CemMethodActiveForHour;
                            EmParameters.MatsSo2cNeeded = matsSo2cNeeded;

                            EmParameters.So2MonitorHourlyValueRecordsByHourLocation = new CheckDataView<VwMpMonitorHrlyValueSo2cRow>();
                            {
                                for (int dex = 0; dex < so2MonitorHourlyCount; dex++)
                                {
                                    EmParameters.So2MonitorHourlyValueRecordsByHourLocation.Add(new VwMpMonitorHrlyValueSo2cRow(monitorHrlyValId: dex.ToString(), parameterCd: "SO2C"));
                                }
                            }

                            /* Initialize Output Parameters */
                            EmParameters.CurrentSo2MonitorHourlyRecord = null;
                            EmParameters.So2MonitorHourlyCount = null;

                            /* Expected Results */
                            string result = null;
                            VwMpMonitorHrlyValueSo2cRow expRecord = null;
                            {
                                if (unitHourlyOperationalStatus.Default(false))
                                {
                                    if ((so2MonitorHourlyCount > 0) && !so2CemMethodActiveForHour.Default(false) && !matsSo2cNeeded.Default(false))
                                        result = "A";
                                    else if (so2MonitorHourlyCount > 1)
                                        result = "B";
                                    else if (so2MonitorHourlyCount == 1)
                                        expRecord = EmParameters.So2MonitorHourlyValueRecordsByHourLocation[0];
                                }
                                else
                                {
                                    if (so2MonitorHourlyCount > 0)
                                        result = "C";
                                }
                            }

                            /* Init Cateogry Result */
                            category.CheckCatalogResult = null;

                            // Run Checks
                            actual = cHourlyOperatingDataChecks.HOUROP18(category, ref log);

                            /* Check Result Label */
                            string resultPrefix = string.Format("[OpStatus: {0}, so2Cem: {1}, matsSo2: {2}, count: {3}]",
                                                                unitHourlyOperationalStatus,
                                                                so2CemMethodActiveForHour,
                                                                matsSo2cNeeded,
                                                                so2MonitorHourlyCount);

                            /* Validate Results */
                            Assert.AreEqual(string.Empty, actual);
                            Assert.AreEqual(false, log);
                            Assert.AreEqual(result, category.CheckCatalogResult, resultPrefix + ".Result");

                            if (expRecord != null && EmParameters.CurrentSo2MonitorHourlyRecord != null)
                                Assert.AreEqual(expRecord.MonitorHrlyValId, EmParameters.CurrentSo2MonitorHourlyRecord.MonitorHrlyValId, resultPrefix + ".CurrentSo2MonitorHourlyRecord(1)");
                            else
                                Assert.AreEqual(expRecord, EmParameters.CurrentSo2MonitorHourlyRecord, resultPrefix + ".CurrentSo2MonitorHourlyRecord(2)");

                            Assert.AreEqual(so2MonitorHourlyCount, EmParameters.So2MonitorHourlyCount, resultPrefix + "So2MonitorHourlyCount");
                        }
        }

        #region HOUROP-23
        /// <summary>
        ///A test for HOUROP-23_MATSDiluentNeeded
        ///</summary>()
        [TestMethod()]
        public void HOUROP23_MATSDiluentNeeded()
        {
            //static check setup
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            EmParameters.Init(category.Process);
            EmParameters.Category = category;

            // Variables
            bool log = false;
            string actual;

            // Init Input
            EmParameters.Co2CemMethodActiveForHour = null;
            EmParameters.Co2cDerivedHourlyRecordsByHourLocation = new CheckDataView<VwMpDerivedHrlyValueCo2cRow>();
            EmParameters.Co2cMonitorHourlyRecordsByHourLocation = new CheckDataView<VwMpMonitorHrlyValueCo2cRow>();
            EmParameters.DerivedHourlyChecksNeeded = false;
            EmParameters.FcFactorNeeded = false;
            EmParameters.FdFactorNeeded = false;
            EmParameters.MoistureNeeded = false;
            EmParameters.MonitorFormulaRecordsByHourLocation = new CheckDataView<VwMpMonitorFormulaRow>();
            EmParameters.CurrentHourlyOpRecord = new VwMpHrlyOpDataRow(opTime: 1);

            //Result B
            {
                EmParameters.Co2DiluentChecksNeededForNoxRateCalc = false;
                EmParameters.Co2ConcChecksNeededForHeatInput = false;
                EmParameters.Co2ConcChecksNeededForCo2MassCalc = false;
                EmParameters.Co2DiluentNeededForMats = true;
                EmParameters.Co2DiluentNeededForMatsCalculation = true;
                EmParameters.Co2ConcMonitorHourlyCount = 0;

                // Init Output
                category.CheckCatalogResult = null;

                // Run Checks
                actual = cHourlyOperatingDataChecks.HOUROP23(category, ref log);

                // Check Results
                Assert.AreEqual(string.Empty, actual);
                Assert.AreEqual(false, log);
                Assert.AreEqual("B", category.CheckCatalogResult, "Result");
            }

            //Result F
            {
                EmParameters.Co2DiluentChecksNeededForNoxRateCalc = false;
                EmParameters.Co2ConcChecksNeededForHeatInput = false;
                EmParameters.Co2ConcChecksNeededForCo2MassCalc = false;
                EmParameters.Co2DiluentNeededForMats = true;
                EmParameters.Co2DiluentNeededForMatsCalculation = false;
                EmParameters.Co2ConcMonitorHourlyCount = 0;

                // Init Output
                category.CheckCatalogResult = null;

                // Run Checks
                actual = cHourlyOperatingDataChecks.HOUROP23(category, ref log);

                // Check Results
                Assert.AreEqual(string.Empty, actual);
                Assert.AreEqual(false, log);
                Assert.AreEqual("F", category.CheckCatalogResult, "Result");
            }

            //Result C
            // Note: Check is calculating some of this data instead of requesting it from the parameter
            {
                EmParameters.Co2ConcMonitorHourlyCount = 2;  //check not using this parameter for if statement
                EmParameters.Co2ConcDerivedHourlyCount = 0; //check not using this parameter for if statement
                EmParameters.Co2DiluentChecksNeededForNoxRateCalc = false;
                EmParameters.Co2DiluentNeededForMats = true;
                EmParameters.Co2DiluentNeededForMatsCalculation = true;
                EmParameters.Co2ConcChecksNeededForHeatInput = false;
                EmParameters.Co2ConcChecksNeededForCo2MassCalc = true;
                EmParameters.CurrentCo2ConcMonitorHourlyRecord = new VwMpMonitorHrlyValueCo2cRow(); //check not using this parameter for if statement
                EmParameters.CurrentCo2ConcMissingDataMonitorHourlyRecord = new VwMpMonitorHrlyValueCo2cRow(); //check not using this parameter for if statement
                EmParameters.Co2cMonitorHourlyRecordsByHourLocation = new CheckDataView<VwMpMonitorHrlyValueCo2cRow>(
                    new VwMpMonitorHrlyValueCo2cRow(monitorHrlyValId: "1"),
                    new VwMpMonitorHrlyValueCo2cRow(monitorHrlyValId: "2")
                    );

                // Init Output
                category.CheckCatalogResult = null;

                // Run Checks
                actual = cHourlyOperatingDataChecks.HOUROP23(category, ref log);

                // Check Results
                Assert.AreEqual(string.Empty, actual);
                Assert.AreEqual(false, log);
                Assert.AreEqual("C", category.CheckCatalogResult, "Result");
            }
        }
        #endregion

        #region HOUROP-40
        /// <summary>
        ///A test for HOUROP-40_MATSO2Needed
        ///</summary>()
        [TestMethod()]
        public void HOUROP40_MATSO2Needed()
        {
            //static check setup
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            EmParameters.Init(category.Process);
            EmParameters.Category = category;

            // Variables
            bool log = false;
            string actual;

            // Init Input
            EmParameters.O2DryNeededToSupportCo2Calculation = false;
            EmParameters.O2DryChecksNeededForH2o = false;
            EmParameters.O2NullMonitorHourlyCount = 0;
            EmParameters.O2NullMonitorHourlyValueRecordsByHourLocation = new CheckDataView<VwMpMonitorHrlyValueO2NullRow>();
            EmParameters.O2WetChecksNeededForHeatInput = false;
            EmParameters.O2WetChecksNeededForH2o = false;
            EmParameters.O2WetChecksNeededForNoxRateCalc = false;
            EmParameters.O2WetMonitorHourlyCount = 0;
            EmParameters.O2WetNeededToSupportCo2Calculation = false;
            EmParameters.CurrentHourlyOpRecord = new VwMpHrlyOpDataRow(opTime: 1);

            //Result A
            {
                // Init Input
                EmParameters.O2DryChecksNeededForHeatInput = false;
                EmParameters.O2DryNeededForMats = true;
                EmParameters.O2WetNeededForMats = true;
                EmParameters.O2WetNeededForMatsCalculation = true;

                // Init Output
                category.CheckCatalogResult = null;

                // Run Checks
                actual = cHourlyOperatingDataChecks.HOUROP40(category, ref log);

                // Check Results
                Assert.AreEqual(string.Empty, actual);
                Assert.AreEqual(false, log);
                Assert.AreEqual("A", category.CheckCatalogResult, "Result");
            }

            //Result G
            {
                // Init Input
                EmParameters.O2DryChecksNeededForHeatInput = false;
                EmParameters.O2DryNeededForMats = true;
                EmParameters.O2WetNeededForMats = true;
                EmParameters.O2WetNeededForMatsCalculation = false;

                // Init Output
                category.CheckCatalogResult = null;

                // Run Checks
                actual = cHourlyOperatingDataChecks.HOUROP40(category, ref log);

                // Check Results
                Assert.AreEqual(string.Empty, actual);
                Assert.AreEqual(false, log);
                Assert.AreEqual("G", category.CheckCatalogResult, "Result");
            }

            //Pass, O2 Dry Checks Needed
            {
                // Init Input
                EmParameters.O2DryChecksNeededForHeatInput = true;
                EmParameters.O2DryNeededForMats = true;
                EmParameters.O2WetNeededForMats = false;
                EmParameters.O2DryMonitorHourlyCount = 2;
                EmParameters.O2NullMonitorHourlyCount = 0;
                EmParameters.O2NullMonitorHourlyValueRecordsByHourLocation = new CheckDataView<VwMpMonitorHrlyValueO2NullRow>(
                    new VwMpMonitorHrlyValueO2NullRow(monitorHrlyValId: "1", modcCd: "01"));
                EmParameters.O2DryMonitorHourlyValueRecordsByHourLocation = new CheckDataView<VwMpMonitorHrlyValueO2DryRow>(
                    new VwMpMonitorHrlyValueO2DryRow(monitorHrlyValId: "2", modcCd: "99"));

                // Init Output
                category.CheckCatalogResult = null;

                // Run Checks
                actual = cHourlyOperatingDataChecks.HOUROP40(category, ref log);

                // Check Results
                Assert.AreEqual(string.Empty, actual);
                Assert.AreEqual(false, log);
                Assert.AreEqual(null, category.CheckCatalogResult, "Result");
                Assert.AreEqual(true, EmParameters.O2DryMonitorHourlyChecksNeeded, "O2DryMonitorHourlyChecksNeeded");
            }

            //Result E
            {
                // Init Input
                EmParameters.O2DryChecksNeededForHeatInput = false;
                EmParameters.O2DryNeededForMats = false;
                EmParameters.O2WetNeededForMats = false;
                EmParameters.O2DryMonitorHourlyCount = 0;
                EmParameters.O2NullMonitorHourlyCount = 2;
                EmParameters.O2NullMonitorHourlyValueRecordsByHourLocation = new CheckDataView<VwMpMonitorHrlyValueO2NullRow>(
                    new VwMpMonitorHrlyValueO2NullRow(monitorHrlyValId: "1"),
                    new VwMpMonitorHrlyValueO2NullRow(monitorHrlyValId: "2")
                    );

                // Init Output
                category.CheckCatalogResult = null;

                // Run Checks
                actual = cHourlyOperatingDataChecks.HOUROP40(category, ref log);

                // Check Results
                Assert.AreEqual(string.Empty, actual);
                Assert.AreEqual(false, log);
                Assert.AreEqual("E", category.CheckCatalogResult, "Result");
            }

        }
        #endregion

        #region HOUROP-41
        /// <summary>
        ///A test for HOUROP-41_MATSO2Needed
        ///</summary>()
        [TestMethod()]
        public void HOUROP41_MATSO2Needed()
        {
            //static check setup
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            EmParameters.Init(category.Process);
            EmParameters.Category = category;

            // Variables
            bool log = false;
            string actual;

            // Init Input
            EmParameters.O2DryChecksNeededForH2o = false;
            EmParameters.O2DryChecksNeededForNoxRateCalc = false;
            EmParameters.O2DryNeededToSupportCo2Calculation = false;
            EmParameters.O2NullMonitorHourlyValueRecordsByHourLocation = new CheckDataView<VwMpMonitorHrlyValueO2NullRow>();
            EmParameters.O2WetChecksNeededForH2o = false;
            EmParameters.O2WetChecksNeededForHeatInput = false;
            EmParameters.O2WetChecksNeededForNoxRateCalc = false;
            EmParameters.O2WetMonitorHourlyCount = 0;
            EmParameters.O2WetMonitorHourlyValueRecordsByHourLocation = new CheckDataView<VwMpMonitorHrlyValueO2WetRow>();
            EmParameters.O2WetNeededToSupportCo2Calculation = false;
            EmParameters.CurrentHourlyOpRecord = new VwMpHrlyOpDataRow(opTime: 1);

            //Result A
            {
                // Init Input
                EmParameters.O2DryChecksNeededForHeatInput = false;
                EmParameters.O2DryMonitorHourlyCount = 0;
                EmParameters.O2DryNeededForMats = true;
                EmParameters.O2DryNeededForMatsCalculation = true;
                EmParameters.O2NullMonitorHourlyCount = 0;
                EmParameters.O2WetNeededForMats = true;

                // Init Output
                category.CheckCatalogResult = null;

                // Run Checks
                actual = cHourlyOperatingDataChecks.HOUROP41(category, ref log);

                // Check Results
                Assert.AreEqual(string.Empty, actual);
                Assert.AreEqual(false, log);
                Assert.AreEqual("A", category.CheckCatalogResult, "Result");
            }

            //Result E
            {
                // Init Input
                EmParameters.O2DryChecksNeededForHeatInput = false;
                EmParameters.O2DryMonitorHourlyCount = 0;
                EmParameters.O2DryNeededForMats = true;
                EmParameters.O2DryNeededForMatsCalculation = false;
                EmParameters.O2NullMonitorHourlyCount = 0;
                EmParameters.O2WetNeededForMats = true;

                // Init Output
                category.CheckCatalogResult = null;

                // Run Checks
                actual = cHourlyOperatingDataChecks.HOUROP41(category, ref log);

                // Check Results
                Assert.AreEqual(string.Empty, actual);
                Assert.AreEqual(false, log);
                Assert.AreEqual("E", category.CheckCatalogResult, "Result");
            }

            //Pass, O2 Wet Checks Needed
            {
                // Init Input
                EmParameters.O2WetChecksNeededForHeatInput = true;
                EmParameters.O2DryNeededForMats = true;
                EmParameters.O2WetNeededForMats = true;
                EmParameters.O2WetMonitorHourlyCount = 2;
                EmParameters.O2NullMonitorHourlyCount = 0;
                EmParameters.O2NullMonitorHourlyValueRecordsByHourLocation = new CheckDataView<VwMpMonitorHrlyValueO2NullRow>(
                    new VwMpMonitorHrlyValueO2NullRow(monitorHrlyValId: "1", modcCd: "01"));
                EmParameters.O2WetMonitorHourlyValueRecordsByHourLocation = new CheckDataView<VwMpMonitorHrlyValueO2WetRow>(
                    new VwMpMonitorHrlyValueO2WetRow(monitorHrlyValId: "2", modcCd: "99"));

                // Init Output
                category.CheckCatalogResult = null;

                // Run Checks
                actual = cHourlyOperatingDataChecks.HOUROP41(category, ref log);

                // Check Results
                Assert.AreEqual(string.Empty, actual);
                Assert.AreEqual(false, log);
                Assert.AreEqual(null, category.CheckCatalogResult, "Result");
                Assert.AreEqual(true, EmParameters.O2WetMonitorHourlyChecksNeeded, "O2WetMonitorHourlyChecksNeeded");
            }
        }
        #endregion

        /// <summary>
        /// HourO-p43
        /// 
        /// ApportionmentStackFlowArray: [0m, 1.4m, 0m]
        /// CurrentMonitorPlanLocationPosition: 1
        /// 
        /// | ## | Needed | Allowed | Count || Result | Record | Needed | FlowArray    || Note
        /// |  0 | null   | null    |     0 || null   | No     | null   | [0, null, 0] || Checks not needed (null) and no Flow rows
        /// |  1 | null   | null    |     1 || C      | No     | null   | [0, null, 0] || Checks not needed (null), but one Flow row exists.
        /// |  2 | null   | null    |     2 || C      | No     | null   | [0, null, 0] || Checks not needed (null), but two Flow rows exist.
        /// |  3 | false  | null    |     0 || null   | No     | false  | [0, null, 0] || Checks not needed and no Flow rows
        /// |  4 | false  | null    |     1 || C      | No     | false  | [0, null, 0] || Checks not needed, but one Flow row exists.
        /// |  5 | false  | null    |     2 || C      | No     | false  | [0, null, 0] || Checks not needed, but two Flow rows exist.
        /// |  6 | true   | null    |     0 || A      | No     | false  | [0, null, 0] || Checks needed, but no Flow rows.
        /// |  7 | true   | null    |     1 || null   | Yes    | true   | [0, 1.3, 0]  || Checks needed and one Flow row exists.
        /// |  8 | true   | null    |     2 || B      | No     | true   | [0, null, 0] || Checks needed, but two Flow rows exist.
        /// |  9 | false  | false   |     0 || null   | No     | false  | [0, null, 0] || Checks not needed and no Flow rows
        /// | 10 | false  | false   |     1 || C      | No     | false  | [0, null, 0] || Checks not needed, but one Flow row exists.
        /// | 11 | false  | false   |     2 || C      | No     | false  | [0, null, 0] || Checks not needed, but two Flow rows exist.
        /// | 12 | false  | true    |     0 || null   | No     | false  | [0, null, 0] || Checks not needed and no Flow rows, but Flow MHV allowed.
        /// | 13 | false  | true    |     1 || null   | Yes    | true   | [0, 1.3, 0]  || Checks needed and one Flow row exists, but Flow MHV allowed.
        /// | 14 | false  | true    |     2 || B      | No     | true   | [0, null, 0] || Checks needed, but two Flow rows exist and Flow MHV allowed.
        /// | 15 | true   | false   |     0 || A      | No     | false  | [0, null, 0] || Checks needed, but no Flow rows.
        /// | 16 | true   | false   |     1 || null   | Yes    | true   | [0, 1.3, 0]  || Checks needed and one Flow row exists.
        /// | 17 | true   | false   |     2 || B      | No     | true   | [0, null, 0] || Checks needed, but two Flow rows exist.
        /// </summary>
        [TestMethod()]
        public void HourOp43()
        {
            /* Initialize objects generally needed for testing checks. */
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            EmParameters.Init(category.Process);
            EmParameters.Category = category;

            category.Process.ProcessParameters.RegisterParameter(3618, "Apportionment_Stack_Flow_Array"); // Currently cannot access arrays using the new check parameter access.


            /* Input Parameter Values */
            bool?[] neededList = { null, null, null, false, false, false, true, true, true, false, false, false, false, false, false, true, true, true };
            bool?[] allowedList = { null, null, null, null, null, null, null, null, null, false, false, false, true, true, true, false, false, false };
            int?[] countList = { 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2 };

            /* Expected Values */
            string[] expResultList = { null, "C", "C", null, "C", "C", "A", null, "B", null, "C", "C", null, null, "B", "A", null, "B" };
            bool[] expRecordList = { false, false, false, false, false, false, false, true, false, false, false, false, false, true, false, false, true, false };
            bool?[] expNeededList = { null, null, null, false, false, false, false, true, true, false, false, false, false, true, true, false, true, true };
            decimal?[][] expFlowArrayList = { new decimal?[] {0m, null, 0m }, new decimal?[] { 0m, null, 0m }, new decimal?[] { 0m, null, 0m }, new decimal?[] { 0m, null, 0m }, new decimal?[] { 0m, null, 0m },
                                              new decimal?[] {0m, null, 0m }, new decimal?[] { 0m, null, 0m }, new decimal?[] { 0m, 1.3m, 0m }, new decimal?[] { 0m, null, 0m }, new decimal?[] { 0m, null, 0m },
                                              new decimal?[] {0m, null, 0m }, new decimal?[] { 0m, null, 0m }, new decimal?[] { 0m, null, 0m }, new decimal?[] { 0m, 1.3m, 0m }, new decimal?[] { 0m, null, 0m },
                                              new decimal?[] {0m, null, 0m }, new decimal?[] { 0m, 1.3m, 0m }, new decimal?[] { 0m, null, 0m } };

            /* Test Case Count */
            int caseCount = 18;

            /* Check array lengths */
            Assert.AreEqual(caseCount, neededList.Length, "neededList length");
            Assert.AreEqual(caseCount, allowedList.Length, "allowedList length");
            Assert.AreEqual(caseCount, countList.Length, "countList length");
            Assert.AreEqual(caseCount, expResultList.Length, "resultList length");
            Assert.AreEqual(caseCount, expNeededList.Length, "expNeededList length");
            Assert.AreEqual(caseCount, expFlowArrayList.Length, "expFlowArrayList length");


            /* Run Cases */
            for (int caseDex = 0; caseDex < caseCount; caseDex++)
            {
                /*  Initialize Required Parameters */
                EmParameters.CurrentMonitorPlanLocationPostion = 1;
                EmParameters.FlowMhvOptionallyAllowed = allowedList[caseDex];
                EmParameters.FlowMonitorHourlyChecksNeeded = neededList[caseDex];
                EmParameters.FlowMonitorHourlyCount = countList[caseDex];
                EmParameters.FlowMonitorHourlyValueRecordsByHourLocation = new CheckDataView<VwMpMonitorHrlyValueFlowRow>();
                {
                    for (int rowDex = 0; rowDex < countList[caseDex]; rowDex++)
                        EmParameters.FlowMonitorHourlyValueRecordsByHourLocation.Add(new VwMpMonitorHrlyValueFlowRow(monitorHrlyValId: rowDex.ToString(), unadjustedHrlyValue: 1.3m));
                }

                /* Initialize Input-Output Parameters */
                category.SetCheckParameter("Apportionment_Stack_Flow_Array", new decimal?[] { 0m, 1.4m, 0m });

                /*  Initialize Output Parameters */
                EmParameters.CurrentFlowMonitorHourlyRecord = null;


                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                /* Initialize variables needed to run the check. */
                bool log = false;
                string actual;

                /* Run Check */
                actual = cHourlyOperatingDataChecks.HOUROP43(category, ref log);

                /* Check results */
                Assert.AreEqual(string.Empty, actual, string.Format("actual {0}", caseDex));
                Assert.AreEqual(false, log, string.Format("log {0}", caseDex));

                Assert.AreEqual(expResultList[caseDex], category.CheckCatalogResult, String.Format("Result [case {0}]", caseDex));
                Assert.AreEqual(expRecordList[caseDex], ((EmParameters.CurrentFlowMonitorHourlyRecord != null ? EmParameters.CurrentFlowMonitorHourlyRecord.MonitorHrlyValId : null) == "0"), string.Format("CurrentFlowMonitorHourlyRecord [case {0}]", caseDex));
                Assert.AreEqual(expNeededList[caseDex], EmParameters.FlowMonitorHourlyChecksNeeded, string.Format("FlowMonitorHourlyChecksNeeded [case {0}]", caseDex));

                decimal?[] apportionmentStackFlowArray = (decimal?[])category.GetCheckParameter("Apportionment_Stack_Flow_Array").ParameterValue;
                {
                    Assert.AreEqual(expFlowArrayList[caseDex].Length, apportionmentStackFlowArray.Length, string.Format("ApportionmentStackFlowArray.Length [case {0}]", caseDex));
                    Assert.AreEqual(expFlowArrayList[caseDex][0], apportionmentStackFlowArray[0], string.Format("ApportionmentStackFlowArray[0] [case {0}]", caseDex));
                    Assert.AreEqual(expFlowArrayList[caseDex][1], apportionmentStackFlowArray[1], string.Format("ApportionmentStackFlowArray[1] [case {0}]", caseDex));
                    Assert.AreEqual(expFlowArrayList[caseDex][2], apportionmentStackFlowArray[2], string.Format("ApportionmentStackFlowArray[2] [case {0}]", caseDex));
                }
            }

        }


        /// <summary>
        /// HourOp-44
        /// 
        /// 
        /// | ## | HrRange | CsRange | HrLoad | OpTime | FlowCnt | OilCnt | GasCnt | FlowNeed | NoxcNeed | NoxrNeed | So2Hpff | Co2Hpff | HiHpff | Needed | LoadBased | Type | LmeAnnual | LmeOs || Result | CheckHr | CheckCs || Note
        /// |  0 |    null |    null |   1234 |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | false  | true      | Unit | false     | false || null   | false   | false   || DerivedHourlyChecksNeeded is true.
        /// |  1 |    null |    null |   1234 |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | Unit | false     | false || A      | false   | false   || LoadRange required but both are null.
        /// |  2 |    null |    null |   1234 |   0.01 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | Unit | false     | false || A      | false   | false   || LoadRange required but both are null.
        /// |  3 |    null |    null |      1 |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | Unit | false     | false || A      | false   | false   || LoadRange required but both are null.
        /// |  4 |       1 |    null |   1234 |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | Unit | false     | false || null   | true    | false   || Unit with LoadRange reported.
        /// |  5 |    null |       2 |   1234 |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | Unit | false     | false || E      | false   | false   || Unit with CommonStackLoadRange reported.
        /// |  6 |       3 |       4 |   1234 |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | Unit | false     | false || E      | true    | false   || Unit with LoadRange and CommonStackLoadRange reported.
        /// |  7 |       1 |    null |   1234 |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | MS   | false     | false || null   | true    | false   || MS with LoadRange reported.
        /// |  8 |    null |       2 |   1234 |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | MS   | false     | false || E      | false   | false   || MS with CommonStackLoadRange reported.
        /// |  9 |       3 |       4 |   1234 |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | MS   | false     | false || E      | true    | false   || MS with LoadRange and CommonStackLoadRange reported.
        /// | 10 |       1 |    null |   1234 |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | MP   | false     | false || null   | true    | false   || MP with LoadRange reported.
        /// | 11 |    null |       2 |   1234 |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | MP   | false     | false || E      | false   | false   || MP with CommonStackLoadRange reported.
        /// | 12 |       3 |       4 |   1234 |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | MP   | false     | false || E      | true    | false   || MP with LoadRange and CommonStackLoadRange reported.
        /// | 13 |       1 |    null |   1234 |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | CS   | false     | false || null   | true    | false   || CS with LoadRange reported and no flow.
        /// | 14 |       1 |    null |   1234 |   1.00 |       1 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | CS   | false     | false || null   | true    | false   || CS with LoadRange reported and with flow.
        /// | 15 |    null |       2 |   1234 |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | CS   | false     | false || C      | false   | false   || CS with CommonStackLoadRange reported and no flow.
        /// | 16 |    null |       2 |   1234 |   1.00 |       1 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | CS   | false     | false || null   | false   | true    || CS with CommonStackLoadRange reported and with flow.
        /// | 17 |       3 |       4 |   1234 |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | CS   | false     | false || C      | true    | false   || CS with LoadRange and CommonStackLoadRange reported and no flow.
        /// | 18 |       3 |       4 |   1234 |   1.00 |       1 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | CS   | false     | false || null   | true    | true    || CS with LoadRange and CommonStackLoadRange reported and with flow.
        /// | 19 |       1 |    null |   1234 |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | CP   | false     | false || null   | true    | false   || CP with LoadRange reported and no fuel flow.
        /// | 20 |       1 |    null |   1234 |   1.00 |       0 |      1 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | CP   | false     | false || null   | true    | false   || CP with LoadRange reported and with oil fuel flow.
        /// | 21 |       1 |    null |   1234 |   1.00 |       0 |      0 |      1 | true     | true     | true     | true    | true    | true   | true   | true      | CP   | false     | false || null   | true    | false   || CP with LoadRange reported and with gas fuel flow.
        /// | 22 |       1 |    null |   1234 |   1.00 |       0 |      1 |      1 | true     | true     | true     | true    | true    | true   | true   | true      | CP   | false     | false || null   | true    | false   || CP with LoadRange reported and with oil and gas fuel flow.
        /// | 23 |    null |       2 |   1234 |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | CP   | false     | false || D      | false   | false   || CP with CommonStackLoadRange reported and no fuel flow.
        /// | 24 |    null |       2 |   1234 |   1.00 |       0 |      1 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | CP   | false     | false || null   | false   | true    || CP with CommonStackLoadRange reported and with oil fuel flow.
        /// | 25 |    null |       2 |   1234 |   1.00 |       0 |      0 |      1 | true     | true     | true     | true    | true    | true   | true   | true      | CP   | false     | false || null   | false   | true    || CP with CommonStackLoadRange reported and with gas fuel flow.
        /// | 26 |    null |       2 |   1234 |   1.00 |       0 |      1 |      1 | true     | true     | true     | true    | true    | true   | true   | true      | CP   | false     | false || null   | false   | true    || CP with CommonStackLoadRange reported and with oil and gas fuel flow.
        /// | 27 |       3 |       4 |   1234 |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | CP   | false     | false || D      | true    | false   || CP with LoadRange and CommonStackLoadRange reported and no fuel flow.
        /// | 28 |       3 |       4 |   1234 |   1.00 |       0 |      1 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | CP   | false     | false || null   | true    | true    || CP with LoadRange and CommonStackLoadRange reported and with oil fuel flow.
        /// | 29 |       3 |       4 |   1234 |   1.00 |       0 |      0 |      1 | true     | true     | true     | true    | true    | true   | true   | true      | CP   | false     | false || null   | true    | true    || CP with LoadRange and CommonStackLoadRange reported and with gas fuel flow.
        /// | 30 |       3 |       4 |   1234 |   1.00 |       0 |      1 |      1 | true     | true     | true     | true    | true    | true   | true   | true      | CP   | false     | false || null   | true    | true    || CP with LoadRange and CommonStackLoadRange reported and with oil and gas fuel flow.
        /// | 31 |    null |    null |   1234 |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | false     | Unit | false     | false || null   | false   | false   || Non load-based without LoadRange and CommonStackLoadRange.
        /// | 32 |       1 |    null |   1234 |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | false     | Unit | false     | false || null   | false   | false   || Non load-based with LoadRange.
        /// | 33 |    null |       2 |   1234 |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | false     | Unit | false     | false || null   | false   | false   || Non load-based with CommonStackLoadRange.
        /// | 34 |    null |    null |   1234 |   0.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | Unit | false     | false || null   | false   | false   || Non operating without LoadRange and CommonStackLoadRange.
        /// | 35 |       1 |    null |   1234 |   0.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | Unit | false     | false || B      | false   | false   || Non operating with LoadRange.
        /// | 36 |    null |       2 |   1234 |   0.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | Unit | false     | false || B      | false   | false   || Non operating with CommonStackLoadRange.
        /// | 37 |    null |    null |   null |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | Unit | false     | false || null   | false   | false   || No hour load and without LoadRange and CommonStackLoadRange.
        /// | 38 |       1 |    null |   null |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | Unit | false     | false || B      | false   | false   || No hour load with LoadRange.
        /// | 39 |    null |       2 |   null |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | Unit | false     | false || B      | false   | false   || No hour load with CommonStackLoadRange.
        /// | 40 |    null |    null |     -1 |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | Unit | false     | false || null   | false   | false   || Negative hour load without LoadRange and CommonStackLoadRange.
        /// | 41 |       1 |    null |     -1 |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | Unit | false     | false || B      | false   | false   || Negative hour load with LoadRange.
        /// | 42 |    null |       2 |     -1 |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | Unit | false     | false || B      | false   | false   || Negative hour load with CommonStackLoadRange.
        /// | 43 |    null |    null |   1234 |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | Unit | true      | false || null   | false   | false   || Annual LME without LoadRange and CommonStackLoadRange.
        /// | 44 |       1 |    null |   1234 |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | Unit | true      | false || B      | false   | false   || Annual LME with LoadRange.
        /// | 45 |    null |       2 |   1234 |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | Unit | true      | false || B      | false   | false   || Annual LME with CommonStackLoadRange.
        /// | 46 |    null |    null |   1234 |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | Unit | false     | true  || null   | false   | false   || Annual LME without LoadRange and CommonStackLoadRange.
        /// | 47 |       1 |    null |   1234 |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | Unit | false     | true  || B      | false   | false   || Annual LME with LoadRange.
        /// | 48 |    null |       2 |   1234 |   1.00 |       0 |      0 |      0 | true     | true     | true     | true    | true    | true   | true   | true      | Unit | false     | true  || B      | false   | false   || Annual LME with CommonStackLoadRange.
        /// | 49 |    null |       2 |   1234 |   1.00 |       0 |      0 |      0 | true     | false    | false    | false   | false   | false  | true   | true      | Unit | false     | false || E      | false   | false   || Unit with Flow Needed and CommonStackLoadRange reported.
        /// | 50 |    null |       2 |   1234 |   1.00 |       0 |      0 |      0 | false    | true     | false    | false   | false   | false  | true   | true      | Unit | false     | false || E      | false   | false   || Unit with NOxC (for NOx Mass Rate) Needed and CommonStackLoadRange reported.
        /// | 51 |    null |       2 |   1234 |   1.00 |       0 |      0 |      0 | false    | false    | true     | false   | false   | false  | true   | true      | Unit | false     | false || E      | false   | false   || Unit with NOxR Needed and CommonStackLoadRange reported.
        /// | 52 |       1 |    null |   1234 |   1.00 |       0 |      0 |      0 | null     | null     | null     | null    | null    | null   | true   | true      | Unit | false     | false || F      | false   | false   || Unit without FLow, NOxC, NOxR or SO2 or CO2 HPFF (needed parameters are null), but with LoadRange.
        /// | 53 |       1 |    null |   1234 |   1.00 |       0 |      0 |      0 | false    | false    | false    | false   | false   | false  | true   | true      | Unit | false     | false || F      | false   | false   || Unit without FLow, NOxC, NOxR or SO2 or CO2 HPFF, but with LoadRange.
        /// | 54 |    null |       2 |   1234 |   1.00 |       0 |      0 |      0 | false    | false    | false    | false   | false   | false  | true   | true      | Unit | false     | false || F      | false   | false   || Unit without FLow, NOxC, NOxR or SO2 or CO2 HPFF, but with CommonStackLoadRange.
        /// | 55 |    null |    null |   1234 |   1.00 |       0 |      0 |      0 | false    | false    | false    | false   | false   | false  | true   | true      | Unit | false     | false || null   | false   | false   || Unit without FLow, NOxC, NOxR or SO2 or CO2 HPFF, and without LoadRange and CommonStackLoadRange.
        /// | 56 |    null |       2 |   1234 |   1.00 |       0 |      0 |      0 | false    | false    | false    | true    | false   | false  | true   | true      | Unit | false     | false || E      | false   | false   || Unit where SO2 HPFF exists and CommonStackLoadRange reported.
        /// | 57 |    null |       2 |   1234 |   1.00 |       0 |      0 |      0 | false    | false    | false    | false   | true    | false  | true   | true      | Unit | false     | false || E      | false   | false   || Unit where CO2 HPFF exists and CommonStackLoadRange reported.
        /// | 58 |    null |       2 |   1234 |   1.00 |       0 |      0 |      0 | false    | false    | false    | false   | false   | true   | true   | true      | Unit | false     | false || E      | false   | false   || Unit where HI HPFF exists and CommonStackLoadRange reported.
        /// </summary>
        [TestMethod()]
        public void HourOp44()
        {
            /* Initialize objects generally needed for testing checks. */
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            EmParameters.Init(category.Process);
            EmParameters.Category = category;


            /* Input Parameter Values */
            int?[] hrRangeList = { null, null, null, null, 1, null, 3, 1, null, 3,
                                   1, null, 3, 1, 1, null, null, 3, 3, 1,
                                   1, 1, 1, null, null, null, null, 3, 3, 3,
                                   3, null, 1, null, null, 1, null, null, 1, null,
                                   null, 1, null, null, 1, null, null, 1, null, null,
                                   null, null, 1, 1, null, null, null, null, null };
            int?[] csRangeList = { null, null, null, null, null, 2, 4, null, 2, 4,
                                   null, 2, 4, null, null, 2, 2, 4, 4, null,
                                   null, null, null, 2, 2, 2, 2, 4, 4, 4,
                                   4, null, null, 2, null, null, 2, null, null, 2,
                                   null, null, 2, null, null, 2, null, null, 2, 2,
                                   2, 2, null, null, 2, null, 2, 2, 2 };
            int?[] hrLoadList = { 1234, 1234, 1234,    1, 1234, 1234, 1234, 1234, 1234, 1234,
                                  1234, 1234, 1234, 1234, 1234, 1234, 1234, 1234, 1234, 1234,
                                  1234, 1234, 1234, 1234, 1234, 1234, 1234, 1234, 1234, 1234,
                                  1234, 1234, 1234, 1234, 1234, 1234, 1234, null, null, null,
                                    -1,   -1,   -1, 1234, 1234, 1234, 1234, 1234, 1234, 1234,
                                  1234, 1234, 1234, 1234, 1234, 1234, 1234, 1234, 1234  };
            decimal?[] opTimeList = { 1.00m, 1.00m, 0.01m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m,
                                      1.00m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m,
                                      1.00m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m,
                                      1.00m, 1.00m, 1.00m, 1.00m, 0.00m, 0.00m, 0.00m, 1.00m, 1.00m, 1.00m,
                                      1.00m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m,
                                      1.00m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m };
            int?[] flowCntList = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                   0, 0, 0, 0, 1, 0, 1, 0, 1, 0,
                                   0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                   0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                   0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                   0, 0, 0, 0, 0, 0, 0, 0, 0  };
            int?[] oilCntList = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                  0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                  1, 0, 1, 0, 1, 0, 1, 0, 1, 0,
                                  1, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                  0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                  0, 0, 0, 0, 0, 0, 0, 0, 0  };
            int?[] gasCntList = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                  0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                  0, 1, 1, 0, 0, 1, 1, 0, 0, 1,
                                  1, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                  0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                  0, 0, 0, 0, 0, 0, 0, 0, 0  };
            bool?[] flowNeededList = { true, true, true, true, true, true, true, true, true, true,
                                       true, true, true, true, true, true, true, true, true, true,
                                       true, true, true, true, true, true, true, true, true, true,
                                       true, true, true, true, true, true, true, true, true, true,
                                       true, true, true, true, true, true, true, true, true, true,
                                       false, false, null, false, false, false, false, false, false };
            bool?[] noxcNeededList = { true, true, true, true, true, true, true, true, true, true,
                                       true, true, true, true, true, true, true, true, true, true,
                                       true, true, true, true, true, true, true, true, true, true,
                                       true, true, true, true, true, true, true, true, true, true,
                                       true, true, true, true, true, true, true, true, true, false,
                                       true, false, null, false, false, false, false, false, false };
            bool?[] noxrNeededList = { true, true, true, true, true, true, true, true, true, true,
                                       true, true, true, true, true, true, true, true, true, true,
                                       true, true, true, true, true, true, true, true, true, true,
                                       true, true, true, true, true, true, true, true, true, true,
                                       true, true, true, true, true, true, true, true, true, false,
                                       false, true, null, false, false, false, false, false, false  };
            bool?[] so2HpffExistsList = { true, true, true, true, true, true, true, true, true, true,
                                          true, true, true, true, true, true, true, true, true, true,
                                          true, true, true, true, true, true, true, true, true, true,
                                          true, true, true, true, true, true, true, true, true, true,
                                          true, true, true, true, true, true, true, true, true, false,
                                          false, false, null, false, false, false, true, false, false };
            bool?[] co2HpffExistsList = { true, true, true, true, true, true, true, true, true, true,
                                          true, true, true, true, true, true, true, true, true, true,
                                          true, true, true, true, true, true, true, true, true, true,
                                          true, true, true, true, true, true, true, true, true, true,
                                          true, true, true, true, true, true, true, true, true, false,
                                          false, false, null, false, false, false, false, true, false };
            bool?[] hiHpffExistsList = { true, true, true, true, true, true, true, true, true, true,
                                         true, true, true, true, true, true, true, true, true, true,
                                         true, true, true, true, true, true, true, true, true, true,
                                         true, true, true, true, true, true, true, true, true, true,
                                         true, true, true, true, true, true, true, true, true, false,
                                         false, false, null, false, false, false, false, false, true };
            bool?[] derivedNeededList = { false, true, true, true, true, true, true, true, true, true,
                                          true, true, true, true, true, true, true, true, true, true,
                                          true, true, true, true, true, true, true, true, true, true,
                                          true, true, true, true, true, true, true, true, true, true,
                                          true, true, true, true, true, true, true, true, true, true,
                                          true, true, true, true, true, true, true, true, true };
            bool?[] loadBasedList = { true, true, true, true, true, true, true, true, true, true,
                                      true, true, true, true, true, true, true, true, true, true,
                                      true, true, true, true, true, true, true, true, true, true,
                                      true, false, false, false, true, true, true, true, true, true,
                                      true, true, true, true, true, true, true, true, true, true,
                                      true, true, true, true, true, true, true, true, true };
            string[] entityTypeList = { "Unit", "Unit", "Unit", "Unit", "Unit", "Unit", "Unit", "MS", "MS", "MS",
                                        "MP", "MP", "MP", "CS", "CS", "CS", "CS", "CS", "CS", "CP",
                                        "CP", "CP", "CP", "CP", "CP", "CP", "CP", "CP", "CP", "CP",
                                        "CP", "Unit", "Unit", "Unit", "Unit", "Unit", "Unit", "Unit", "Unit", "Unit",
                                        "Unit", "Unit", "Unit", "Unit", "Unit", "Unit", "Unit", "Unit", "Unit", "Unit",
                                        "Unit", "Unit", "Unit", "Unit", "Unit", "Unit", "Unit", "Unit", "Unit" };
            bool?[] lmeAnnual = { false, false, false, false, false, false, false, false, false, false,
                                  false, false, false, false, false, false, false, false, false, false,
                                  false, false, false, false, false, false, false, false, false, false,
                                  false, false, false, false, false, false, false, false, false, false,
                                  false, false, false, true, true, true, false, false, false, false,
                                  false, false, false, false, false, false, false, false, false };
            bool?[] lmeOs = { false, false, false, false, false, false, false, false, false, false,
                              false, false, false, false, false, false, false, false, false, false,
                              false, false, false, false, false, false, false, false, false, false,
                              false, false, false, false, false, false, false, false, false, false,
                              false, false, false, false, false, false, true, true, true, false,
                              false, false, false, false, false, false, false, false, false };

            /* Expected Values */
            string[] expResultList = { null, "A", "A", "A", null, "E", "E", null, "E", "E",
                                       null, "E", "E", null, null, "C", null, "C", null, null,
                                       null, null, null, "D", null, null, null, "D", null, null,
                                       null, null, null, null, null, "B", "B", null, "B", "B",
                                       null, "B", "B", null, "B", "B", null, "B", "B", "E",
                                       "E", "E", "F", "F", "F", null, "E", "E", "E" };
            bool?[] expCheckHrList = { false, false, false, false, true, false, true, true, false, true,
                                       true, false, true, true, true, false, false, true, true, true,
                                       true, true, true, false, false, false, false, true, true, true,
                                       true, false, false, false, false, false, false, false, false, false,
                                       false, false, false, false, false, false, false, false, false, false,
                                       false, false, false, false, false, false, false, false, false };
            bool?[] expCheckCsList = { false, false, false, false, false, false, false, false, false, false,
                                       false, false, false, false, false, false, true, false, true, false,
                                       false, false, false, false, true, true, true, false, true, true,
                                       true, false, false, false, false, false, false, false, false, false,
                                       false, false, false, false, false, false, false, false, false, false,
                                       false, false, false, false, false, false, false, false, false };

            /* Test Case Count */
            int caseCount = 59;

            /* Check array lengths */
            Assert.AreEqual(caseCount, hrRangeList.Length, "hrRangeList length");
            Assert.AreEqual(caseCount, csRangeList.Length, "csRangeList length");
            Assert.AreEqual(caseCount, hrLoadList.Length, "hrLoadList length");
            Assert.AreEqual(caseCount, opTimeList.Length, "opTimeList length");
            Assert.AreEqual(caseCount, flowCntList.Length, "flowCntList length");
            Assert.AreEqual(caseCount, oilCntList.Length, "oilCntList length");
            Assert.AreEqual(caseCount, gasCntList.Length, "gasCntList length");
            Assert.AreEqual(caseCount, flowNeededList.Length, "flowNeededList length");
            Assert.AreEqual(caseCount, noxcNeededList.Length, "noxcNeededList length");
            Assert.AreEqual(caseCount, noxrNeededList.Length, "noxrNeededList length");
            Assert.AreEqual(caseCount, so2HpffExistsList.Length, "so2HpffExistsList length");
            Assert.AreEqual(caseCount, co2HpffExistsList.Length, "co2HpffExistsList length");
            Assert.AreEqual(caseCount, hiHpffExistsList.Length, "hiHpffExistsList length");
            Assert.AreEqual(caseCount, derivedNeededList.Length, "derivedNeededList length");
            Assert.AreEqual(caseCount, loadBasedList.Length, "loadBasedList length");
            Assert.AreEqual(caseCount, entityTypeList.Length, "entityTypeList length");
            Assert.AreEqual(caseCount, lmeAnnual.Length, "lmeAnnual length");
            Assert.AreEqual(caseCount, lmeOs.Length, "lmeOs length");
            Assert.AreEqual(caseCount, expResultList.Length, "expResultList length");
            Assert.AreEqual(caseCount, expCheckHrList.Length, "expCheckHrList length");
            Assert.AreEqual(caseCount, expCheckCsList.Length, "expCheckCsList length");


            /* Run Cases */
            for (int caseDex = 0; caseDex < caseCount; caseDex++)
            {
                /*  Initialize Input Parameters*/
                EmParameters.Co2HpffExists = co2HpffExistsList[caseDex];
                EmParameters.CurrentEntityType = entityTypeList[caseDex];
                EmParameters.CurrentHourlyOpRecord = new VwMpHrlyOpDataRow(loadRange: hrRangeList[caseDex], commonStackLoadRange: csRangeList[caseDex], hrLoad: hrLoadList[caseDex], opTime: opTimeList[caseDex]);
                EmParameters.DerivedHourlyChecksNeeded = derivedNeededList[caseDex];
                EmParameters.FlowMonitorHourlyChecksNeeded = flowNeededList[caseDex];
                EmParameters.FlowMonitorHourlyCount = flowCntList[caseDex];
                EmParameters.HiHpffExists = hiHpffExistsList[caseDex];
                EmParameters.HourlyFuelFlowCountForGas = gasCntList[caseDex];
                EmParameters.HourlyFuelFlowCountForOil = oilCntList[caseDex];
                EmParameters.LmeAnnual = lmeAnnual[caseDex];
                EmParameters.LmeOs = lmeOs[caseDex];
                EmParameters.NoxConcNeededForNoxMassCalc = noxcNeededList[caseDex];
                EmParameters.NoxrDerivedHourlyChecksNeeded = noxrNeededList[caseDex];
                EmParameters.So2HpffExists = so2HpffExistsList[caseDex];
                EmParameters.UnitIsLoadBased = loadBasedList[caseDex];

                /*  Initialize Input Parameters*/
                EmParameters.CheckCsLoadRangeValue = null;
                EmParameters.CheckLoadRangeValue = null;


                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                /* Initialize variables needed to run the check. */
                bool log = false;
                string actual;

                /* Run Check */
                actual = cHourlyOperatingDataChecks.HOUROP44(category, ref log);

                /* Check results */
                Assert.AreEqual(string.Empty, actual, string.Format("actual {0}", caseDex));
                Assert.AreEqual(false, log, string.Format("log {0}", caseDex));
                Assert.AreEqual(expResultList[caseDex], category.CheckCatalogResult, String.Format("CheckCatalogResult [case {0}]", caseDex));

                Assert.AreEqual(expCheckCsList[caseDex], EmParameters.CheckCsLoadRangeValue, String.Format("CheckCsLoadRangeValue [case {0}]", caseDex));
                Assert.AreEqual(expCheckHrList[caseDex], EmParameters.CheckLoadRangeValue, String.Format("CheckLoadRangeValue [case {0}]", caseDex));
            }

        }


        /// <summary>
        /// HourOp-45
        /// 
        /// 
        /// | ## | Check | Range | Load | MaxLoad || Result || Note
        /// |  0 | null  |     0 |    0 |    1234 || null   || CheckLoadRangeValue is null.
        /// |  1 | false |     0 |    0 |    1234 || null   || CheckLoadRangeValue is false.
        /// |  2 | true  |     0 |    0 |    1234 || A      || LoadRange equals 0.
        /// |  3 | true  |     1 |    0 |    1234 || null   || LoadRange equals 1 and Load is in range 1.
        /// |  4 | true  |     2 |    0 |    1234 || B      || LoadRange equals 2, but Load is 0 and therefore range must be 1.
        /// |  5 | true  |     1 |    1 |    1234 || null   || LoadRange equals 1 and Load is in range 1.
        /// |  6 | true  |     1 |  125 |    1234 || null   || LoadRange equals 1 and Load is in range 2 but within border region.
        /// |  7 | true  |     1 |  126 |    1234 || D      || LoadRange equals 1, but Load is in range 2.
        /// |  8 | true  |     2 |  121 |    1234 || D      || LoadRange equals 2, but Load is in range 1.
        /// |  9 | true  |     2 |  122 |    1234 || null   || LoadRange equals 2 and Load is in range 1 but within border region.
        /// | 10 | true  |     9 | 1112 |    1234 || null   || LoadRange equals 9 and Load is in range 10 but within border region.
        /// | 11 | true  |     9 | 1113 |    1234 || D      || LoadRange equals 9, but Load is in range 10.
        /// | 12 | true  |    10 | 1108 |    1234 || D      || LoadRange equals 10, but Load is in range 9.
        /// | 13 | true  |    10 | 1109 |    1234 || null   || LoadRange equals 10 and Load is in range 9 but within border region.
        /// | 14 | true  |    10 | 1233 |    1234 || null   || LoadRange equals 10 and Load is in range 10.
        /// | 15 | true  |     9 | 1234 |    1234 || C      || LoadRange equals 9, but the Load equals MaxLoad and therefore range must be 10.
        /// | 16 | true  |    10 | 1234 |    1234 || null   || LoadRange equals 10 and the Load is in range 10.
        /// | 17 | true  |     9 | 1235 |    1234 || C      || LoadRange equals 9, but the Load is greater than MaxLoad and therefore range must be 10.
        /// | 18 | true  |    10 | 1235 |    1234 || null   || LoadRange equals 10 and the Load is in range 10.
        /// </summary>
        [TestMethod()]
        public void HourOp45()
        {
            /* Initialize objects generally needed for testing checks. */
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            EmParameters.Init(category.Process);
            EmParameters.Category = category;


            /* Input Parameter Values */
            bool?[] checkList = { null, false, true, true, true, true, true, true, true, true,
                                  true, true, true, true, true, true, true, true, true };
            int?[] rangeList = { 0, 0, 0, 1, 2, 1, 1, 1, 2, 2,
                                 9, 9, 10, 10, 10, 9, 10, 9, 10 };
            int?[] loadList = { 0, 0, 0, 0, 0, 1, 125, 126, 121, 122,
                                1112, 1113, 1108, 1109, 1233, 1234, 1234, 1235, 1235 };
            int?[] maxList = { 1234, 1234, 1234, 1234, 1234, 1234, 1234, 1234, 1234, 1234,
                               1234, 1234, 1234, 1234, 1234, 1234, 1234, 1234, 1234 };

            /* Expected Values */
            string[] expResultList = { null, null, "A", null, "B", null, null, "D", "D", null,
                                       null, "D", "D", null, null, "C", null, "C", null,  };

            /* Test Case Count */
            int caseCount = 19;

            /* Check array lengths */
            Assert.AreEqual(caseCount, checkList.Length, "checkList length");
            Assert.AreEqual(caseCount, rangeList.Length, "rangeList length");
            Assert.AreEqual(caseCount, loadList.Length, "loadList length");
            Assert.AreEqual(caseCount, maxList.Length, "maxList length");
            Assert.AreEqual(caseCount, expResultList.Length, "expResultList length");


            /* Run Cases */
            for (int caseDex = 0; caseDex < caseCount; caseDex++)
            {
                /*  Initialize Input Parameters*/
                EmParameters.CheckLoadRangeValue = checkList[caseDex];
                EmParameters.CurrentHourlyOpRecord = new VwMpHrlyOpDataRow(loadRange: rangeList[caseDex], hrLoad: loadList[caseDex]);
                EmParameters.CurrentMaximumLoadValue = maxList[caseDex];


                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                /* Initialize variables needed to run the check. */
                bool log = false;
                string actual;

                /* Run Check */
                actual = cHourlyOperatingDataChecks.HOUROP45(category, ref log);

                /* Check results */
                Assert.AreEqual(string.Empty, actual, string.Format("actual {0}", caseDex));
                Assert.AreEqual(false, log, string.Format("log {0}", caseDex));
                Assert.AreEqual(expResultList[caseDex], category.CheckCatalogResult, String.Format("CheckCatalogResult [case {0}]", caseDex));
            }

        }


        /// <summary>
        /// HourOp-46
        /// 
        /// 
        /// | ## | Check | Range | Load | MaxLoad || Result || Note
        /// |  0 | null  |     0 |    0 |    1234 || null   || CheckLoadRangeValue is null.
        /// |  1 | false |     0 |    0 |    1234 || null   || CheckLoadRangeValue is false.
        /// |  2 | true  |     0 |    0 |    1234 || A      || LoadRange equals 0.
        /// |  3 | true  |     1 |    0 |    1234 || null   || LoadRange equals 1 and Load is in range 1.
        /// |  4 | true  |     2 |    0 |    1234 || B      || LoadRange equals 2, but Load is 0 and therefore range must be 1.
        /// |  5 | true  |     1 |    1 |    1234 || null   || LoadRange equals 1 and Load is in range 1.
        /// |  6 | true  |     1 |   63 |    1234 || null   || LoadRange equals 1 and Load is in range 2 but within border region.
        /// |  7 | true  |     1 |   64 |    1234 || D      || LoadRange equals 1, but Load is in range 2.
        /// |  8 | true  |     2 |   59 |    1234 || D      || LoadRange equals 2, but Load is in range 1.
        /// |  9 | true  |     2 |   60 |    1234 || null   || LoadRange equals 2 and Load is in range 1 but within border region.
        /// | 10 | true  |    19 | 1174 |    1234 || null   || LoadRange equals 19 and Load is in range 20 but within border region.
        /// | 11 | true  |    19 | 1175 |    1234 || D      || LoadRange equals 19, but Load is in range 20.
        /// | 12 | true  |    20 | 1170 |    1234 || D      || LoadRange equals 20, but Load is in range 19.
        /// | 13 | true  |    20 | 1171 |    1234 || null   || LoadRange equals 20 and Load is in range 19 but within border region.
        /// | 14 | true  |    20 | 1233 |    1234 || null   || LoadRange equals 20 and Load is in range 20.
        /// | 15 | true  |    19 | 1234 |    1234 || C      || LoadRange equals 19, but the Load equals MaxLoad and therefore range must be 20.
        /// | 16 | true  |    20 | 1234 |    1234 || null   || LoadRange equals 20 and the Load is in range 20.
        /// | 17 | true  |    19 | 1235 |    1234 || C      || LoadRange equals 19, but the Load is greater than MaxLoad and therefore range must be 20.
        /// | 18 | true  |    20 | 1235 |    1234 || null   || LoadRange equals 20 and the Load is in range 20.
        /// </summary>
        [TestMethod()]
        public void HourOp46()
        {
            /* Initialize objects generally needed for testing checks. */
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            EmParameters.Init(category.Process);
            EmParameters.Category = category;


            /* Input Parameter Values */
            bool?[] checkList = { null, false, true, true, true, true, true, true, true, true,
                                  true, true, true, true, true, true, true, true, true };
            int?[] rangeList = { 0, 0, 0, 1, 2, 1, 1, 1, 2, 2,
                                 19, 19, 20, 20, 20, 19, 20, 19, 20 };
            int?[] loadList = { 0, 0, 0, 0, 0, 1, 63, 64, 59, 60,
                                1174, 1175, 1170, 1171, 1233, 1234, 1234, 1235, 1235 };
            int?[] maxList = { 1234, 1234, 1234, 1234, 1234, 1234, 1234, 1234, 1234, 1234,
                               1234, 1234, 1234, 1234, 1234, 1234, 1234, 1234, 1234 };

            /* Expected Values */
            string[] expResultList = { null, null, "A", null, "B", null, null, "D", "D", null,
                                       null, "D", "D", null, null, "C", null, "C", null,  };

            /* Test Case Count */
            int caseCount = 19;

            /* Check array lengths */
            Assert.AreEqual(caseCount, checkList.Length, "checkList length");
            Assert.AreEqual(caseCount, rangeList.Length, "rangeList length");
            Assert.AreEqual(caseCount, loadList.Length, "loadList length");
            Assert.AreEqual(caseCount, maxList.Length, "maxList length");
            Assert.AreEqual(caseCount, expResultList.Length, "expResultList length");


            /* Run Cases */
            for (int caseDex = 0; caseDex < caseCount; caseDex++)
            {
                /*  Initialize Input Parameters*/
                EmParameters.CheckCsLoadRangeValue = checkList[caseDex];
                EmParameters.CurrentHourlyOpRecord = new VwMpHrlyOpDataRow(commonStackLoadRange: rangeList[caseDex], hrLoad: loadList[caseDex]);
                EmParameters.CurrentMaximumLoadValue = maxList[caseDex];


                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                /* Initialize variables needed to run the check. */
                bool log = false;
                string actual;

                /* Run Check */
                actual = cHourlyOperatingDataChecks.HOUROP46(category, ref log);

                /* Check results */
                Assert.AreEqual(string.Empty, actual, string.Format("actual {0}", caseDex));
                Assert.AreEqual(false, log, string.Format("log {0}", caseDex));
                Assert.AreEqual(expResultList[caseDex], category.CheckCatalogResult, String.Format("CheckCatalogResult [case {0}]", caseDex));
            }

        }


        /// <summary>
        /// HourOp-54
        /// 
        /// CurrentMonitorLocationId: GoodId
        /// CurrentOperatingDateHour: 2020-06-17 22 (CurHr)
        /// 
        /// Test #1: MON_LOC_ID = Other1Id and begin and end hours = CurrentOperatingDateHour
        /// Test #3: MON_LOC_ID = Other3Id and begin and end hours = CurrentOperatingDateHour
        /// 
        /// 
        ///                                                    |     Test #0       |     Test #2       |     Test #4       ||
        /// | ## | Needed | OpTime | Pos | ExistsArray         | BegHr   | EndHr   | BegHr   | EndHr   | BegHr   | EndHr   || Result | List                 || Note
        /// |  0 | null   |   0.00 |   1 | [false,true,false]  | CurHr   | CurHr   | CurHr   | CurHr   | CurHr   | CurHr   || null   | empty                || DerivedHourlyChecksNeeded is false.
        /// |  1 | false  |   0.00 |   1 | [false,true,false]  | CurHr   | CurHr   | CurHr   | CurHr   | CurHr   | CurHr   || null   | empty                || DerivedHourlyChecksNeeded is false.
        /// |  2 | true   |   0.01 |   1 | [false,true,false]  | CurHr   | CurHr   | CurHr   | CurHr   | CurHr   | CurHr   || null   | empty                || OpTime is not zero.
        /// |  3 | true   |   0.00 |   1 | [false,false,false] | CurHr   | CurHr   | CurHr   | CurHr   | CurHr   | CurHr   || null   | empty                || LinearityExistsLocationArray indicates that linearities do not exist for the location.
        /// |  4 | true   |   0.00 |   1 | [false,true,false]  | CurHr   | CurHr   | CurHr   | CurHr   | CurHr   | CurHr   || A      | '#0', '#2', and '#4' || Three non-op test exist.
        /// |  5 | true   |   0.00 |   0 | [false,true,false]  | CurHr   | CurHr   | CurHr   | CurHr   | CurHr   | CurHr   || null   | empty                || LinearityExistsLocationArray indicates that linearities do not exist for the location.
        /// |  6 | true   |   0.00 |   2 | [false,true,false]  | CurHr   | CurHr   | CurHr   | CurHr   | CurHr   | CurHr   || null   | empty                || LinearityExistsLocationArray indicates that linearities do not exist for the location.
        /// |  7 | true   |   0.00 |   0 | [true,false,false]  | CurHr   | CurHr   | CurHr   | CurHr   | CurHr   | CurHr   || A      | '#0', '#2', and '#4' || Three non-op tests exist.
        /// |  8 | true   |   0.00 |   2 | [false,false,true]  | CurHr   | CurHr   | CurHr   | CurHr   | CurHr   | CurHr   || A      | '#0', '#2', and '#4' || Three non-op tests exist.
        /// |  9 | true   |   0.00 |   1 | [false,true,false]  | CurHr   | CurHr+1 | CurHr   | CurHr   | CurHr-1 | CurHr   || A      | '#0', '#2', and '#4' || Three non-op tests exist.
        /// | 10 | true   |   0.00 |   1 | [false,true,false]  | CurHr+1 | CurHr+1 | CurHr   | CurHr   | CurHr-1 | CurHr-1 || A      | '#2'                 || One non-op test exists.
        /// | 11 | true   |   0.00 |   1 | [false,true,false]  | CurHr   | CurHr+1 | CurHr-1 | CurHr+1 | CurHr-1 | CurHr   || A      | '#0' and '#4'        || Two non-op tests exist.  Begin and end surrond no-op for other test.
        /// </summary>
        [TestMethod()]
        public void HourOp54()
        {
            /* Initialize objects generally needed for testing checks. */
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            EmParameters.Init(category.Process);
            EmParameters.Category = category;


            /* Parameter Value Variables */
            string curLoc = "GoodId";
            DateTime curHr = new DateTime(2020, 6, 17, 22, 0, 0);

            /* Input Parameter Values */
            bool?[] neededList = { null, false, true, true, true, true, true, true, true, true, true, true };
            decimal?[] opTimeList = { 0.00m, 0.00m, 0.01m, 0.00m, 0.00m, 0.00m, 0.00m, 0.00m, 0.00m, 0.00m, 0.00m, 0.00m };
            int[] positionList = { 1, 1, 1, 1, 1, 0, 2, 0, 2, 1, 1, 1 };
            bool[][] existsArrayList = { new bool[] { false, true, false },
                                         new bool[] { false, true, false },
                                         new bool[] { false, true, false },
                                         new bool[] { false, false, false },
                                         new bool[] { false, true, false },
                                         new bool[] { false, true, false },
                                         new bool[] { false, true, false },
                                         new bool[] { true, false, false },
                                         new bool[] { false, false, true },
                                         new bool[] { false, true, false },
                                         new bool[] { false, true, false },
                                         new bool[] { false, true, false } };
            DateTime[][] testBegList = { new DateTime[] { curHr, curHr, curHr, curHr, curHr, curHr, curHr, curHr, curHr, curHr, curHr.AddHours(1), curHr },
                                         new DateTime[] { curHr, curHr, curHr, curHr, curHr, curHr, curHr, curHr, curHr, curHr, curHr, curHr.AddHours(-1) },
                                         new DateTime[] { curHr, curHr, curHr, curHr, curHr, curHr, curHr, curHr, curHr, curHr.AddHours(-1), curHr.AddHours(-1), curHr.AddHours(-1) } };
            DateTime[][] testEndList = { new DateTime[] { curHr, curHr, curHr, curHr, curHr, curHr, curHr, curHr, curHr, curHr.AddHours(1), curHr.AddHours(1), curHr.AddHours(1) },
                                         new DateTime[] { curHr, curHr, curHr, curHr, curHr, curHr, curHr, curHr, curHr, curHr, curHr, curHr.AddHours(1) },
                                         new DateTime[] { curHr, curHr, curHr, curHr, curHr, curHr, curHr, curHr, curHr, curHr, curHr.AddHours(-1), curHr } };

            /* Expected Values */
            string[] expResultList = { null, null, null, null, "A", null, null, "A", "A", "A", "A", "A" };
            string[] expListList = { "", "", "", "", "'#0', '#2', and '#4'", "", "", "'#0', '#2', and '#4'", "'#0', '#2', and '#4'", "'#0', '#2', and '#4'", "'#2'", "'#0' and '#4'" };

            /* Test Case Count */
            int caseCount = 12;

            /* Check array lengths */
            Assert.AreEqual(caseCount, neededList.Length, "neededList length");
            Assert.AreEqual(caseCount, opTimeList.Length, "opTimeList length");
            Assert.AreEqual(caseCount, positionList.Length, "positionList length");
            Assert.AreEqual(caseCount, existsArrayList.Length, "existsArrayList length");
            Assert.AreEqual(3, testBegList.Length, "testBegList length");
            Assert.AreEqual(caseCount, testBegList[0].Length, "testBegList[0] length");
            Assert.AreEqual(caseCount, testBegList[1].Length, "testBegList[1] length");
            Assert.AreEqual(caseCount, testBegList[2].Length, "testBegList[2] length");
            Assert.AreEqual(3, testEndList.Length, "testEndList length");
            Assert.AreEqual(caseCount, testEndList[0].Length, "testEndList[0] length");
            Assert.AreEqual(caseCount, testEndList[1].Length, "testEndList[1] length");
            Assert.AreEqual(caseCount, testEndList[2].Length, "testEndList[2] length");
            Assert.AreEqual(caseCount, expResultList.Length, "expResultList length");
            Assert.AreEqual(caseCount, expListList.Length, "checkList length");


            /* Run Cases */
            for (int caseDex = 0; caseDex < caseCount; caseDex++)
            {
                /*  Initialize Input Parameters*/
                EmParameters.CurrentMonitorLocationId = curLoc;
                EmParameters.CurrentMonitorPlanLocationPostion = positionList[caseDex];
                EmParameters.CurrentOperatingDatehour = curHr;
                EmParameters.CurrentOperatingTime = opTimeList[caseDex];
                EmParameters.DerivedHourlyChecksNeeded = neededList[caseDex];
                EmParameters.LinearityExistsLocationArray = existsArrayList[caseDex];
                EmParameters.LinearityTestRecordsByLocationForQaStatus = new CheckDataView<VwQaSuppDataHourlyStatusRow>
                    (
                        SetValues.DateHour(new VwQaSuppDataHourlyStatusRow(monLocId: curLoc, testNum: "#0"), testBegList[0][caseDex], testEndList[0][caseDex]),
                        SetValues.DateHour(new VwQaSuppDataHourlyStatusRow(monLocId: "Other1Id", testNum: "#1"), curHr, curHr),
                        SetValues.DateHour(new VwQaSuppDataHourlyStatusRow(monLocId: curLoc, testNum: "#2"), testBegList[1][caseDex], testEndList[1][caseDex]),
                        SetValues.DateHour(new VwQaSuppDataHourlyStatusRow(monLocId: "Other3Id", testNum: "#3"), curHr, curHr),
                        SetValues.DateHour(new VwQaSuppDataHourlyStatusRow(monLocId: curLoc, testNum: "#4"), testBegList[2][caseDex], testEndList[2][caseDex])
                    );

                /*  Initialize Output Parameters*/
                EmParameters.LinearityOfflineList = "Bad List";


                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                /* Initialize variables needed to run the check. */
                bool log = false;
                string actual;

                /* Run Check */
                actual = cHourlyOperatingDataChecks.HOUROP54(category, ref log);

                /* Check results */
                Assert.AreEqual(string.Empty, actual, string.Format("actual {0}", caseDex));
                Assert.AreEqual(false, log, string.Format("log {0}", caseDex));
                Assert.AreEqual(expResultList[caseDex], category.CheckCatalogResult, String.Format("CheckCatalogResult [case {0}]", caseDex));
                Assert.AreEqual(expListList[caseDex], EmParameters.LinearityOfflineList, String.Format("LinearityOfflineList [case {0}]", caseDex));
            }
        }


        /// <summary>
        /// 
        /// </summary>
        [TestMethod()]
        public void HourOp55()
        {
            /* Initialize objects generally needed for testing checks. */
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            EmParameters.Init(category.Process);
            EmParameters.Category = category;


            /* Variables */
            int caseDex, locationPosition;

            /* Initial Parameter Values */
            EmParameters.CurrentMonitorPlanLocationPostion = 1;
            EmParameters.CurrentOperatingDatehour = new DateTime(2020, 6, 17);
            EmParameters.MissingDataPmaTracking = new MissingDataPmaTracking(3);

            /* Missing MODC for ALL */
            EmParameters.CurrentCo2ConcDerivedHourlyRecord = new VwMpDerivedHrlyValueCo2Row( modcCd: "06", pctAvailable: 80m );
            EmParameters.CurrentH2oDerivedHourlyRecord = new VwMpDerivedHrlyValueH2oRow(modcCd: "08", pctAvailable: 81m);
            EmParameters.CurrentNoxrDerivedHourlyRecord = new VwMpDerivedHrlyValueNoxrRow(modcCd: "09", pctAvailable: 82m);
            EmParameters.CurrentCo2ConcMissingDataMonitorHourlyRecord = new VwMpMonitorHrlyValueCo2cRow(modcCd: "10", pctAvailable: 83m);
            EmParameters.CurrentFlowMonitorHourlyRecord = new VwMpMonitorHrlyValueFlowRow(modcCd: "11", pctAvailable: 84m);
            EmParameters.CurrentH2oMonitorHourlyRecord = new VwMpMonitorHrlyValueH2oRow(modcCd: "12", pctAvailable: 85m);
            EmParameters.CurrentNoxConcMonitorHourlyRecord = new VwMpMonitorHrlyValueRow(modcCd: "13", pctAvailable: 86m);
            EmParameters.CurrentO2DryMissingDataMonitorHourlyRecord = new VwMpMonitorHrlyValueRow(modcCd: "15", pctAvailable: 87m);
            EmParameters.CurrentO2WetMissingDataMonitorHourlyRecord = new VwMpMonitorHrlyValueRow(modcCd: "18", pctAvailable: 88m);
            EmParameters.CurrentSo2MonitorHourlyRecord = new VwMpMonitorHrlyValueSo2cRow(modcCd: "23", pctAvailable: 89m);

            /* Not Set */
            EmParameters.CurrentCo2ConcMonitorHourlyRecord = null;
            EmParameters.CurrentO2DryMonitorHourlyRecord = null;
            EmParameters.CurrentO2WetMonitorHourlyRecord = null;


            /* DerivedHourlyChecksNeeded false and CurrentOperatingTime 0 case */
            {
                caseDex = 0;

                EmParameters.CurrentOperatingDatehour = EmParameters.CurrentOperatingDatehour.Value.AddHours(1);
                EmParameters.CurrentOperatingTime = 0m;
                EmParameters.DerivedHourlyChecksNeeded = false;

                // Setup and Run Check
                category.CheckCatalogResult = null;
                bool log = false;
                string actual = cHourlyOperatingDataChecks.HOUROP55(category, ref log);

                // Check Results
                Assert.AreEqual(string.Empty, actual, $"actual {caseDex}");
                Assert.AreEqual(false, log, $"log {caseDex}");

                locationPosition = EmParameters.CurrentMonitorPlanLocationPostion.Value;
                Assert.AreEqual(0, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedCo2c), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, DerivedCo2c )");
                Assert.AreEqual(null, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedCo2c), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, DerivedCo2c )");
                Assert.AreEqual(0, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedH2o), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, DerivedH2o )");
                Assert.AreEqual(null, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedH2o), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, DerivedH2o )");
                Assert.AreEqual(0, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedNoxr), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, DerivedNoxr )");
                Assert.AreEqual(null, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedNoxr), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, DerivedNoxr )");
                Assert.AreEqual(0, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorCo2c), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorCo2c )");
                Assert.AreEqual(null, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorCo2c), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorCo2c )");
                Assert.AreEqual(0, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorFlow), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorFlow )");
                Assert.AreEqual(null, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorFlow), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorFlow )");
                Assert.AreEqual(0, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorH2o), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorH2o )");
                Assert.AreEqual(null, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorH2o), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorH2o )");
                Assert.AreEqual(0, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorNoxc), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorNoxc )");
                Assert.AreEqual(null, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorNoxc), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorNoxc )");
                Assert.AreEqual(0, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2d), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorO2d )");
                Assert.AreEqual(null, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2d), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorO2d )");
                Assert.AreEqual(0, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2w), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorO2w )");
                Assert.AreEqual(null, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2w), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorO2w )");
                Assert.AreEqual(0, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorSo2c), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorSo2c )");
                Assert.AreEqual(null, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorSo2c), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorSo2c )");
            }

            /* DerivedHourlyChecksNeeded true and CurrentOperatingTime 0 case */
            {
                caseDex = 1;

                EmParameters.CurrentOperatingDatehour = EmParameters.CurrentOperatingDatehour.Value.AddHours(1);
                EmParameters.CurrentOperatingTime = 0m;
                EmParameters.DerivedHourlyChecksNeeded = true;

                // Setup and Run Check
                category.CheckCatalogResult = null;
                bool log = false;
                string actual = cHourlyOperatingDataChecks.HOUROP55(category, ref log);

                // Check Results
                Assert.AreEqual(string.Empty, actual, $"actual {caseDex}");
                Assert.AreEqual(false, log, $"log {caseDex}");

                locationPosition = EmParameters.CurrentMonitorPlanLocationPostion.Value;
                Assert.AreEqual(0, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedCo2c), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, DerivedCo2c )");
                Assert.AreEqual(null, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedCo2c), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, DerivedCo2c )");
                Assert.AreEqual(0, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedH2o), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, DerivedH2o )");
                Assert.AreEqual(null, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedH2o), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, DerivedH2o )");
                Assert.AreEqual(0, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedNoxr), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, DerivedNoxr )");
                Assert.AreEqual(null, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedNoxr), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, DerivedNoxr )");
                Assert.AreEqual(0, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorCo2c), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorCo2c )");
                Assert.AreEqual(null, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorCo2c), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorCo2c )");
                Assert.AreEqual(0, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorFlow), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorFlow )");
                Assert.AreEqual(null, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorFlow), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorFlow )");
                Assert.AreEqual(0, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorH2o), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorH2o )");
                Assert.AreEqual(null, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorH2o), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorH2o )");
                Assert.AreEqual(0, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorNoxc), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorNoxc )");
                Assert.AreEqual(null, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorNoxc), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorNoxc )");
                Assert.AreEqual(0, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2d), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorO2d )");
                Assert.AreEqual(null, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2d), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorO2d )");
                Assert.AreEqual(0, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2w), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorO2w )");
                Assert.AreEqual(null, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2w), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorO2w )");
                Assert.AreEqual(0, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorSo2c), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorSo2c )");
                Assert.AreEqual(null, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorSo2c), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorSo2c )");
            }

            /* DerivedHourlyChecksNeeded false and CurrentOperatingTime 0.01 case */
            {
                caseDex = 2;

                EmParameters.CurrentOperatingDatehour = EmParameters.CurrentOperatingDatehour.Value.AddHours(1);
                EmParameters.CurrentOperatingTime = 0.01m;
                EmParameters.DerivedHourlyChecksNeeded = false;

                // Setup and Run Check
                category.CheckCatalogResult = null;
                bool log = false;
                string actual = cHourlyOperatingDataChecks.HOUROP55(category, ref log);

                // Check Results
                Assert.AreEqual(string.Empty, actual, $"actual {caseDex}");
                Assert.AreEqual(false, log, $"log {caseDex}");

                locationPosition = EmParameters.CurrentMonitorPlanLocationPostion.Value;
                Assert.AreEqual(0, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedCo2c), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, DerivedCo2c )");
                Assert.AreEqual(null, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedCo2c), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, DerivedCo2c )");
                Assert.AreEqual(0, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedH2o), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, DerivedH2o )");
                Assert.AreEqual(null, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedH2o), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, DerivedH2o )");
                Assert.AreEqual(0, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedNoxr), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, DerivedNoxr )");
                Assert.AreEqual(null, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedNoxr), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, DerivedNoxr )");
                Assert.AreEqual(0, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorCo2c), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorCo2c )");
                Assert.AreEqual(null, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorCo2c), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorCo2c )");
                Assert.AreEqual(0, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorFlow), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorFlow )");
                Assert.AreEqual(null, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorFlow), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorFlow )");
                Assert.AreEqual(0, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorH2o), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorH2o )");
                Assert.AreEqual(null, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorH2o), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorH2o )");
                Assert.AreEqual(0, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorNoxc), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorNoxc )");
                Assert.AreEqual(null, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorNoxc), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorNoxc )");
                Assert.AreEqual(0, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2d), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorO2d )");
                Assert.AreEqual(null, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2d), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorO2d )");
                Assert.AreEqual(0, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2w), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorO2w )");
                Assert.AreEqual(null, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2w), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorO2w )");
                Assert.AreEqual(0, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorSo2c), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorSo2c )");
                Assert.AreEqual(null, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorSo2c), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorSo2c )");
            }

            /* DerivedHourlyChecksNeeded true and CurrentOperatingTime 0.01 case */
            {
                caseDex = 3;

                EmParameters.CurrentOperatingDatehour = EmParameters.CurrentOperatingDatehour.Value.AddHours(1);
                EmParameters.CurrentOperatingTime = 0.01m;
                EmParameters.DerivedHourlyChecksNeeded = true;

                // Setup and Run Check
                category.CheckCatalogResult = null;
                bool log = false;
                string actual = cHourlyOperatingDataChecks.HOUROP55(category, ref log);

                // Check Results
                Assert.AreEqual(string.Empty, actual, $"actual {caseDex}");
                Assert.AreEqual(false, log, $"log {caseDex}");

                locationPosition = EmParameters.CurrentMonitorPlanLocationPostion.Value;
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedCo2c), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, DerivedCo2c )");
                Assert.AreEqual(80m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedCo2c), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, DerivedCo2c )");
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedH2o), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, DerivedH2o )");
                Assert.AreEqual(81m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedH2o), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, DerivedH2o )");
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedNoxr), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, DerivedNoxr )");
                Assert.AreEqual(82m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedNoxr), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, DerivedNoxr )");
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorCo2c), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorCo2c )");
                Assert.AreEqual(83m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorCo2c), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorCo2c )");
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorFlow), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorFlow )");
                Assert.AreEqual(84m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorFlow), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorFlow )");
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorH2o), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorH2o )");
                Assert.AreEqual(85m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorH2o), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorH2o )");
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorNoxc), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorNoxc )");
                Assert.AreEqual(86m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorNoxc), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorNoxc )");
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2d), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorO2d )");
                Assert.AreEqual(87m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2d), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorO2d )");
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2w), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorO2w )");
                Assert.AreEqual(88m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2w), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorO2w )");
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorSo2c), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorSo2c )");
                Assert.AreEqual(89m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorSo2c), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorSo2c )");
            }


            /* Measured MODC for ALL */
            EmParameters.CurrentCo2ConcDerivedHourlyRecord = new VwMpDerivedHrlyValueCo2Row(modcCd: "01", pctAvailable: 80m);
            EmParameters.CurrentH2oDerivedHourlyRecord = new VwMpDerivedHrlyValueH2oRow(modcCd: "02", pctAvailable: 81m);
            EmParameters.CurrentNoxrDerivedHourlyRecord = new VwMpDerivedHrlyValueNoxrRow(modcCd: "03", pctAvailable: 82m);
            EmParameters.CurrentFlowMonitorHourlyRecord = new VwMpMonitorHrlyValueFlowRow(modcCd: "04", pctAvailable: 84m);
            EmParameters.CurrentH2oMonitorHourlyRecord = new VwMpMonitorHrlyValueH2oRow(modcCd: "05", pctAvailable: 85m);
            EmParameters.CurrentNoxConcMonitorHourlyRecord = new VwMpMonitorHrlyValueRow(modcCd: "07", pctAvailable: 86m);
            EmParameters.CurrentSo2MonitorHourlyRecord = new VwMpMonitorHrlyValueSo2cRow(modcCd: "14", pctAvailable: 89m);

            /*  Missing MODC for ALL */
            EmParameters.CurrentCo2ConcMonitorHourlyRecord = new VwMpMonitorHrlyValueCo2cRow(modcCd: "24", pctAvailable: 90m);
            EmParameters.CurrentO2DryMonitorHourlyRecord = new VwMpMonitorHrlyValueO2DryRow(modcCd: "25", pctAvailable: 91m);
            EmParameters.CurrentO2WetMonitorHourlyRecord = new VwMpMonitorHrlyValueO2WetRow(modcCd: "26", pctAvailable: 92m);

            /* Not Set */
            EmParameters.CurrentCo2ConcMissingDataMonitorHourlyRecord = null;
            EmParameters.CurrentO2DryMissingDataMonitorHourlyRecord = null;
            EmParameters.CurrentO2WetMissingDataMonitorHourlyRecord = null;


            /* CO2C and O2C non-missing data records */
            {
                caseDex = 4;

                EmParameters.CurrentOperatingDatehour = EmParameters.CurrentOperatingDatehour.Value.AddHours(1);
                EmParameters.CurrentOperatingTime = 0.01m;
                EmParameters.DerivedHourlyChecksNeeded = true;

                // Setup and Run Check
                category.CheckCatalogResult = null;
                bool log = false;
                string actual = cHourlyOperatingDataChecks.HOUROP55(category, ref log);

                // Check Results
                Assert.AreEqual(string.Empty, actual, $"actual {caseDex}");
                Assert.AreEqual(false, log, $"log {caseDex}");

                locationPosition = EmParameters.CurrentMonitorPlanLocationPostion.Value;
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedCo2c), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, DerivedCo2c )");
                Assert.AreEqual(80m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedCo2c), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, DerivedCo2c )");
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedH2o), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, DerivedH2o )");
                Assert.AreEqual(81m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedH2o), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, DerivedH2o )");
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedNoxr), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, DerivedNoxr )");
                Assert.AreEqual(82m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedNoxr), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, DerivedNoxr )");
                Assert.AreEqual(2, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorCo2c), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorCo2c )");
                Assert.AreEqual(90, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorCo2c), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorCo2c )");
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorFlow), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorFlow )");
                Assert.AreEqual(84m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorFlow), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorFlow )");
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorH2o), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorH2o )");
                Assert.AreEqual(85m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorH2o), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorH2o )");
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorNoxc), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorNoxc )");
                Assert.AreEqual(86m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorNoxc), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorNoxc )");
                Assert.AreEqual(2, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2d), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorO2d )");
                Assert.AreEqual(91m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2d), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorO2d )");
                Assert.AreEqual(2, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2w), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorO2w )");
                Assert.AreEqual(92m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2w), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorO2w )");
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorSo2c), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorSo2c )");
                Assert.AreEqual(89m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorSo2c), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorSo2c )");
            }


            EmParameters.CurrentCo2ConcDerivedHourlyRecord = null;
            EmParameters.CurrentH2oDerivedHourlyRecord = null;
            EmParameters.CurrentNoxrDerivedHourlyRecord = null;
            EmParameters.CurrentCo2ConcMissingDataMonitorHourlyRecord = null;
            EmParameters.CurrentCo2ConcMonitorHourlyRecord = null;
            EmParameters.CurrentH2oMonitorHourlyRecord = null;
            EmParameters.CurrentNoxConcMonitorHourlyRecord = null;
            EmParameters.CurrentO2DryMissingDataMonitorHourlyRecord = null;
            EmParameters.CurrentO2DryMonitorHourlyRecord = null;
            EmParameters.CurrentO2WetMissingDataMonitorHourlyRecord = null;
            EmParameters.CurrentO2WetMonitorHourlyRecord = null;
            EmParameters.CurrentSo2MonitorHourlyRecord = null;


            /* Check All Missing Data MODC for Flow */
            {
                caseDex = 5;

                EmParameters.CurrentOperatingTime = 0.01m;
                EmParameters.DerivedHourlyChecksNeeded = true;

                string[] modcList = { "06", "08", "09", "10", "11", "12", "13", "15", "18", "23", "24", "25", "26", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "48", "55" };

                foreach (string modcCd in modcList)
                {
                    EmParameters.CurrentFlowMonitorHourlyRecord = new VwMpMonitorHrlyValueFlowRow(modcCd: modcCd, pctAvailable: 80m);
                    EmParameters.CurrentOperatingDatehour = EmParameters.CurrentOperatingDatehour.Value.AddHours(1);


                    // Setup and Run Check
                    category.CheckCatalogResult = null;
                    bool log = false;
                    string actual = cHourlyOperatingDataChecks.HOUROP55(category, ref log);

                    // Check Results
                    Assert.AreEqual(string.Empty, actual, $"actual {caseDex} MODC: {modcCd}");
                    Assert.AreEqual(false, log, $"log {caseDex} MODC: {modcCd}");
                }

                // Check Results
                locationPosition = EmParameters.CurrentMonitorPlanLocationPostion.Value;
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedCo2c), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, DerivedCo2c )");
                Assert.AreEqual(80m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedCo2c), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, DerivedCo2c )");
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedH2o), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, DerivedH2o )");
                Assert.AreEqual(81m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedH2o), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, DerivedH2o )");
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedNoxr), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, DerivedNoxr )");
                Assert.AreEqual(82m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedNoxr), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, DerivedNoxr )");
                Assert.AreEqual(2, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorCo2c), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorCo2c )");
                Assert.AreEqual(90, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorCo2c), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorCo2c )");
                Assert.AreEqual(31, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorFlow), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorFlow )");
                Assert.AreEqual(80m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorFlow), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorFlow )");
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorH2o), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorH2o )");
                Assert.AreEqual(85m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorH2o), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorH2o )");
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorNoxc), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorNoxc )");
                Assert.AreEqual(86m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorNoxc), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorNoxc )");
                Assert.AreEqual(2, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2d), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorO2d )");
                Assert.AreEqual(91m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2d), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorO2d )");
                Assert.AreEqual(2, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2w), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorO2w )");
                Assert.AreEqual(92m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2w), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorO2w )");
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorSo2c), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorSo2c )");
                Assert.AreEqual(89m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorSo2c), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorSo2c )");
            }


            /* Check All Measured+ Data MODC for Flow */
            {
                caseDex = 6;

                EmParameters.CurrentOperatingTime = 0.01m;
                EmParameters.DerivedHourlyChecksNeeded = true;

                string[] modcList = { "01", "02", "03", "04", "05", "07", "14", "16", "17", "19", "20", "21", "22", "47", "53", "54" };

                foreach (string modcCd in modcList)
                {
                    EmParameters.CurrentFlowMonitorHourlyRecord = new VwMpMonitorHrlyValueFlowRow(modcCd: modcCd, pctAvailable: 80m);
                    EmParameters.CurrentOperatingDatehour = EmParameters.CurrentOperatingDatehour.Value.AddHours(1);


                    // Setup and Run Check
                    category.CheckCatalogResult = null;
                    bool log = false;
                    string actual = cHourlyOperatingDataChecks.HOUROP55(category, ref log);

                    // Check Results
                    Assert.AreEqual(string.Empty, actual, $"actual {caseDex} MODC: {modcCd}");
                    Assert.AreEqual(false, log, $"log {caseDex} MODC: {modcCd}");
                }

                // Check Results
                locationPosition = EmParameters.CurrentMonitorPlanLocationPostion.Value;
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedCo2c), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, DerivedCo2c )");
                Assert.AreEqual(80m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedCo2c), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, DerivedCo2c )");
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedH2o), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, DerivedH2o )");
                Assert.AreEqual(81m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedH2o), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, DerivedH2o )");
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedNoxr), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, DerivedNoxr )");
                Assert.AreEqual(82m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedNoxr), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, DerivedNoxr )");
                Assert.AreEqual(2, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorCo2c), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorCo2c )");
                Assert.AreEqual(90, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorCo2c), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorCo2c )");
                Assert.AreEqual(31, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorFlow), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorFlow )");
                Assert.AreEqual(80m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorFlow), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorFlow )");
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorH2o), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorH2o )");
                Assert.AreEqual(85m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorH2o), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorH2o )");
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorNoxc), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorNoxc )");
                Assert.AreEqual(86m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorNoxc), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorNoxc )");
                Assert.AreEqual(2, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2d), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorO2d )");
                Assert.AreEqual(91m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2d), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorO2d )");
                Assert.AreEqual(2, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2w), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorO2w )");
                Assert.AreEqual(92m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2w), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorO2w )");
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorSo2c), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorSo2c )");
                Assert.AreEqual(89m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorSo2c), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorSo2c )");
            }


            /* Missing MODC for ALL */
            EmParameters.CurrentCo2ConcDerivedHourlyRecord = new VwMpDerivedHrlyValueCo2Row(modcCd: "06", pctAvailable: 70m);
            EmParameters.CurrentH2oDerivedHourlyRecord = new VwMpDerivedHrlyValueH2oRow(modcCd: "08", pctAvailable: 70m);
            EmParameters.CurrentNoxrDerivedHourlyRecord = new VwMpDerivedHrlyValueNoxrRow(modcCd: "09", pctAvailable: 70m);
            EmParameters.CurrentCo2ConcMissingDataMonitorHourlyRecord = new VwMpMonitorHrlyValueCo2cRow(modcCd: "10", pctAvailable: 70m);
            EmParameters.CurrentFlowMonitorHourlyRecord = new VwMpMonitorHrlyValueFlowRow(modcCd: "11", pctAvailable: 70m);
            EmParameters.CurrentH2oMonitorHourlyRecord = new VwMpMonitorHrlyValueH2oRow(modcCd: "12", pctAvailable: 70m);
            EmParameters.CurrentNoxConcMonitorHourlyRecord = new VwMpMonitorHrlyValueRow(modcCd: "13", pctAvailable: 70m);
            EmParameters.CurrentO2DryMissingDataMonitorHourlyRecord = new VwMpMonitorHrlyValueRow(modcCd: "15", pctAvailable: 70m);
            EmParameters.CurrentO2WetMissingDataMonitorHourlyRecord = new VwMpMonitorHrlyValueRow(modcCd: "18", pctAvailable: 70m);
            EmParameters.CurrentSo2MonitorHourlyRecord = new VwMpMonitorHrlyValueSo2cRow(modcCd: "23", pctAvailable: 70m);

            /* Not Set */
            EmParameters.CurrentCo2ConcMonitorHourlyRecord = null;
            EmParameters.CurrentO2DryMonitorHourlyRecord = null;
            EmParameters.CurrentO2WetMonitorHourlyRecord = null;


            /* DerivedHourlyChecksNeeded false and CurrentOperatingTime 0 case */
            {
                caseDex = 0;

                EmParameters.CurrentOperatingDatehour = EmParameters.CurrentOperatingDatehour;
                EmParameters.CurrentOperatingTime = 0m;
                EmParameters.DerivedHourlyChecksNeeded = false;

                // Setup and Run Check
                category.CheckCatalogResult = null;
                bool log = false;
                string actual = cHourlyOperatingDataChecks.HOUROP55(category, ref log);

                // Check Results
                Assert.AreEqual(string.Empty, actual, $"actual {caseDex}");
                Assert.AreEqual(false, log, $"log {caseDex}");

                locationPosition = EmParameters.CurrentMonitorPlanLocationPostion.Value;
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedCo2c), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, DerivedCo2c )");
                Assert.AreEqual(80m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedCo2c), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, DerivedCo2c )");
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedH2o), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, DerivedH2o )");
                Assert.AreEqual(81m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedH2o), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, DerivedH2o )");
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedNoxr), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, DerivedNoxr )");
                Assert.AreEqual(82m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.DerivedNoxr), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, DerivedNoxr )");
                Assert.AreEqual(2, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorCo2c), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorCo2c )");
                Assert.AreEqual(90, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorCo2c), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorCo2c )");
                Assert.AreEqual(31, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorFlow), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorFlow )");
                Assert.AreEqual(80m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorFlow), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorFlow )");
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorH2o), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorH2o )");
                Assert.AreEqual(85m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorH2o), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorH2o )");
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorNoxc), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorNoxc )");
                Assert.AreEqual(86m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorNoxc), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorNoxc )");
                Assert.AreEqual(2, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2d), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorO2d )");
                Assert.AreEqual(91m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2d), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorO2d )");
                Assert.AreEqual(2, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2w), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorO2w )");
                Assert.AreEqual(92m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorO2w), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorO2w )");
                Assert.AreEqual(1, EmParameters.MissingDataPmaTracking.GetMissingDataHourCount(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorSo2c), $"GetMissingDataHourCount {caseDex} ( {locationPosition}, MonitorSo2c )");
                Assert.AreEqual(89m, EmParameters.MissingDataPmaTracking.GetLastPercentAvailable(locationPosition, MissingDataPmaTracking.eHourlyParameter.MonitorSo2c), $"GetLastPercentAvailable {caseDex} ( {locationPosition}, MonitorSo2c )");
            }

        }

        /// <summary>
        /// HourOp-56
        /// 
        /// Checks the Needed and MODC combinations for NOxC, CO2C, O2D and O2W.
        /// 
        /// 
        /// Constant Check Parameter Settings:
        ///     
        ///     DerivedHourlyChecksNeeded           : true
        ///     CurrentOperatingTime                : 1.00
        ///     PrimaryBypassActiveForHour          : false
        ///     
        ///     Co2ConcChecksNeededforCo2MassCalc   : false
        ///     Co2ConcChecksNeededforHeatInput     : false
        ///     Co2DiluentNeededforMats             : false
        ///     O2DryChecksNeededforH2o             : false
        ///     O2DryChecksNeededforHeatInput       : false
        ///     O2DryNeededforMats                  : false
        ///     O2DryNeededToSupportCo2Calculation  : false
        ///     O2WetChecksNeededForH2o             : false
        ///     O2WetChecksNeededforHeatInput       : false
        ///     O2WetNeededforMats                  : false
        ///     O2WetNeededToSupportCo2Calculation  : false
        /// 
        /// 
        ///      |           NOxC          |           CO2C          |          O2 Dry         |          O2 Wet         ||
        /// | ## | Needed | Exists | Modc  | Needed | Exists | Modc  | Needed | Exists | Modc  | Needed | Exists | Modc  || Result | Param46           | ParamNon          | Modc  || Note
        /// |  0 | false  | false  | null  | false  | false  | null  | false  | false  | null  | false  | false  | null  || null   | null              | null              | null  || NOxC and Diluents not needed and without.
        /// |  1 | true   | false  | null  | false  | false  | null  | false  | false  | null  | false  | false  | null  || null   | null              | null              | null  || NOxC needed without record, but Diluents are not needed and do not exist.
        /// |  2 | true   | true   | Not46 | false  | false  | null  | false  | false  | null  | false  | false  | null  || null   | null              | null              | null  || NOxC needed with non-46 MODC, but Diluents are not needed and do not exist.
        /// |  3 | true   | true   | 46    | false  | false  | null  | false  | false  | null  | false  | false  | null  || null   | null              | null              | null  || NOxC needed with 46 MODC, but Diluents are not needed and do not exist.
        /// |  4 | false  | false  | null  | true   | false  | null  | false  | false  | null  | false  | false  | null  || null   | null              | null              | null  || CO2C needed without record, NOxC and other Diluents not needed and record does not exist.
        /// |  5 | true   | false  | null  | true   | false  | null  | false  | false  | null  | false  | false  | null  || null   | null              | null              | null  || CO2C needed without record, NOxC needed without record, but other Diluents are not needed and do not exist.
        /// |  6 | true   | true   | Not46 | true   | false  | null  | false  | false  | null  | false  | false  | null  || null   | null              | null              | null  || CO2C needed without record, NOxC needed with non-46 MODC, but other Diluents are not needed and do not exist.
        /// |  7 | true   | true   | 46    | true   | false  | null  | false  | false  | null  | false  | false  | null  || null   | null              | null              | null  || CO2C needed without record, NOxC needed with 46 MODC, but other Diluents are not needed and do not exist.
        /// |  8 | false  | false  | null  | true   | true   | Not46 | false  | false  | null  | false  | false  | null  || null   | null              | null              | null  || CO2C needed with non-46 MODC, NOxC and other Diluents not needed and record does not exist.
        /// |  9 | true   | false  | null  | true   | true   | Not46 | false  | false  | null  | false  | false  | null  || null   | null              | null              | null  || CO2C needed with non-46 MODC, NOxC needed without record, but other Diluents are not needed and do not exist.
        /// | 10 | true   | true   | Not46 | true   | true   | Not46 | false  | false  | null  | false  | false  | null  || null   | null              | null              | null  || CO2C needed with non-46 MODC, NOxC needed with non-46 MODC, but other Diluents are not needed and do not exist.
        /// | 11 | true   | true   | 46    | true   | true   | Not46 | false  | false  | null  | false  | false  | null  || A      | NOx Concentration | CO2 Concentration | Not46 || CO2C needed with non-46 MODC, NOxC needed with 46 MODC, but other Diluents are not needed and do not exist.
        /// | 12 | false  | false  | null  | true   | true   | 46    | false  | false  | null  | false  | false  | null  || null   | null              | null              | null  || CO2C needed with 46 MODC, NOxC and other Diluents not needed and record does not exist.
        /// | 13 | true   | false  | null  | true   | true   | 46    | false  | false  | null  | false  | false  | null  || null   | null              | null              | null  || CO2C needed with 46 MODC, NOxC needed without record, but other Diluents are not needed and do not exist.
        /// | 14 | true   | true   | Not46 | true   | true   | 46    | false  | false  | null  | false  | false  | null  || A      | CO2 Concentration | NOx Concentration | Not46 || CO2C needed with 46 MODC, NOxC needed with non-46 MODC, but other Diluents are not needed and do not exist.
        /// | 15 | true   | true   | 46    | true   | true   | 46    | false  | false  | null  | false  | false  | null  || null   | null              | null              | null  || CO2C needed with 46 MODC, NOxC needed with 46 MODC, but other Diluents are not needed and do not exist.
        /// | 16 | false  | false  | null  | false  | false  | null  | true   | false  | null  | false  | false  | null  || null   | null              | null              | null  || O2D needed without record, NOxC and other Diluents not needed and record does not exist.
        /// | 17 | true   | false  | null  | false  | false  | null  | true   | false  | null  | false  | false  | null  || null   | null              | null              | null  || O2D needed without record, NOxC needed without record, but other Diluents are not needed and do not exist.
        /// | 18 | true   | true   | Not46 | false  | false  | null  | true   | false  | null  | false  | false  | null  || null   | null              | null              | null  || O2D needed without record, NOxC needed with non-46 MODC, but other Diluents are not needed and do not exist.
        /// | 19 | true   | true   | 46    | false  | false  | null  | true   | false  | null  | false  | false  | null  || null   | null              | null              | null  || O2D needed without record, NOxC needed with 46 MODC, but other Diluents are not needed and do not exist.
        /// | 20 | false  | false  | null  | false  | false  | null  | true   | true   | Not46 | false  | false  | null  || null   | null              | null              | null  || O2D needed with non-46 MODC, NOxC and other Diluents not needed and record does not exist.
        /// | 21 | true   | false  | null  | false  | false  | null  | true   | true   | Not46 | false  | false  | null  || null   | null              | null              | null  || O2D needed with non-46 MODC, NOxC needed without record, but other Diluents are not needed and do not exist.
        /// | 22 | true   | true   | Not46 | false  | false  | null  | true   | true   | Not46 | false  | false  | null  || null   | null              | null              | null  || O2D needed with non-46 MODC, NOxC needed with non-46 MODC, but other Diluents are not needed and do not exist.
        /// | 23 | true   | true   | 46    | false  | false  | null  | true   | true   | Not46 | false  | false  | null  || A      | NOx Concentration | O2 Dry            | Not46 || O2D needed with non-46 MODC, NOxC needed with 46 MODC, but other Diluents are not needed and do not exist.
        /// | 24 | false  | false  | null  | false  | false  | null  | true   | true   | 46    | false  | false  | null  || null   | null              | null              | null  || O2D needed with 46 MODC, NOxC and other Diluents not needed and record does not exist.
        /// | 25 | true   | false  | null  | false  | false  | null  | true   | true   | 46    | false  | false  | null  || null   | null              | null              | null  || O2D needed with 46 MODC, NOxC needed without record, but other Diluents are not needed and do not exist.
        /// | 26 | true   | true   | Not46 | false  | false  | null  | true   | true   | 46    | false  | false  | null  || A      | O2 Dry            | NOx Concentration | Not46 || O2D needed with 46 MODC, NOxC needed with non-46 MODC, but other Diluents are not needed and do not exist.
        /// | 27 | true   | true   | 46    | false  | false  | null  | true   | true   | 46    | false  | false  | null  || null   | null              | null              | null  || O2D needed with 46 MODC, NOxC needed with 46 MODC, but other Diluents are not needed and do not exist.
        /// | 28 | false  | false  | null  | false  | false  | null  | false  | false  | null  | true   | false  | null  || null   | null              | null              | null  || O2W needed without record, NOxC and other Diluents not needed and record does not exist.
        /// | 29 | true   | false  | null  | false  | false  | null  | false  | false  | null  | true   | false  | null  || null   | null              | null              | null  || O2W needed without record, NOxC needed without record, but other Diluents are not needed and do not exist.
        /// | 30 | true   | true   | Not46 | false  | false  | null  | false  | false  | null  | true   | false  | null  || null   | null              | null              | null  || O2W needed without record, NOxC needed with non-46 MODC, but other Diluents are not needed and do not exist.
        /// | 31 | true   | true   | 46    | false  | false  | null  | false  | false  | null  | true   | false  | null  || null   | null              | null              | null  || O2W needed without record, NOxC needed with 46 MODC, but other Diluents are not needed and do not exist.
        /// | 32 | false  | false  | null  | false  | false  | null  | false  | false  | null  | true   | true   | Not46 || null   | null              | null              | null  || O2W needed with non-46 MODC, NOxC and other Diluents not needed and record does not exist.
        /// | 33 | true   | false  | null  | false  | false  | null  | false  | false  | null  | true   | true   | Not46 || null   | null              | null              | null  || O2W needed with non-46 MODC, NOxC needed without record, but other Diluents are not needed and do not exist.
        /// | 34 | true   | true   | Not46 | false  | false  | null  | false  | false  | null  | true   | true   | Not46 || null   | null              | null              | null  || O2W needed with non-46 MODC, NOxC needed with non-46 MODC, but other Diluents are not needed and do not exist.
        /// | 35 | true   | true   | 46    | false  | false  | null  | false  | false  | null  | true   | true   | Not46 || A      | NOx Concentration | O2 Wet            | Not46 || O2W needed with non-46 MODC, NOxC needed with 46 MODC, but other Diluents are not needed and do not exist.
        /// | 36 | false  | false  | null  | false  | false  | null  | false  | false  | null  | true   | true   | 46    || null   | null              | null              | null  || O2W needed with 46 MODC, NOxC and other Diluents not needed and record does not exist.
        /// | 37 | true   | false  | null  | false  | false  | null  | false  | false  | null  | true   | true   | 46    || null   | null              | null              | null  || O2W needed with 46 MODC, NOxC needed without record, but other Diluents are not needed and do not exist.
        /// | 38 | true   | true   | Not46 | false  | false  | null  | false  | false  | null  | true   | true   | 46    || A      | O2 Wet            | NOx Concentration | Not46 || O2W needed with 46 MODC, NOxC needed with non-46 MODC, but other Diluents are not needed and do not exist.
        /// | 39 | true   | true   | 46    | false  | false  | null  | false  | false  | null  | true   | true   | 46    || null   | null              | null              | null  || O2W needed with 46 MODC, NOxC needed with 46 MODC, but other Diluents are not needed and do not exist.
        /// | 40 | null   | true   | 46    | true   | true   | Not46 | false  | false  | null  | false  | false  | null  || null   | null              | null              | Not46 || CO2C needed with non-46 MODC, NOxC needed null with 46 MODC, but other Diluents are not needed and do not exist.
        /// | 41 | true   | true   | 46    | null   | true   | Not46 | false  | false  | null  | false  | false  | null  || null   | null              | null              | Not46 || CO2C needed null with non-46 MODC, NOxC needed with 46 MODC, but other Diluents are not needed and do not exist.
        /// | 42 | null   | true   | 46    | false  | false  | null  | true   | true   | Not46 | false  | false  | null  || null   | null              | null              | Not46 || O2D needed with non-46 MODC, NOxC needed null with 46 MODC, but other Diluents are not needed and do not exist.
        /// | 43 | true   | true   | 46    | false  | false  | null  | null   | true   | Not46 | false  | false  | null  || null   | null              | null              | Not46 || O2D needed null with non-46 MODC, NOxC needed with 46 MODC, but other Diluents are not needed and do not exist.
        /// | 44 | null   | true   | 46    | false  | false  | null  | false  | false  | null  | true   | true   | Not46 || null   | null              | null              | Not46 || O2W needed with non-46 MODC, NOxC needed null with 46 MODC, but other Diluents are not needed and do not exist.
        /// | 45 | true   | true   | 46    | false  | false  | null  | false  | false  | null  | null   | true   | Not46 || null   | null              | null              | Not46 || O2W needed null with non-46 MODC, NOxC needed with 46 MODC, but other Diluents are not needed and do not exist.
        /// </summary>
        [TestMethod()]
        public void HourOp56_Core()
        {
            /* Initialize objects generally needed for testing checks. */
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            EmParameters.Init(category.Process);
            EmParameters.Category = category;


            /* Helper Values */
            string co2cParameter = "CO2 Concentration";
            string noxcParameter = "NOx Concentration";
            string o2cdParameter = "O2 Dry";
            string o2cwParameter = "O2 Wet";

            /* MODC List */
            string[] modcIs46List = { "46" };
            string[] modcNullList = { null };

            string[] modcNo46List;
            {
                int dex = 0;

                modcNo46List = new string[UnitTestStandardLists.ModcCodeList.Length - 1];

                foreach (string modcCd in UnitTestStandardLists.ModcCodeList)
                {
                    if (modcCd != "46")
                    {
                        modcNo46List[dex++] = modcCd;
                    }
                }
            }


            /* Expected Value Actions */
            Func<string, string, string, string, string, string> non46Modc = (non46Param, noxcModc, co2cModc, o2dModc, o2wModc) => non46Param == noxcParameter ? noxcModc : non46Param == co2cParameter ? co2cModc : non46Param == o2cdParameter ? o2dModc : non46Param == o2cwParameter ? o2wModc : null;
            Func<string, string, string, string, string, string> nullModc = (non46Param, noxcModc, co2cModc, o2dModc, o2wModc) => null;


            /* Input Parameter Values */
            bool?[] noxcNeededList = { false, true, true, true, false, true, true, true, false, true,
                                       true, true, false, true, true, true, false, true, true, true,
                                       false, true, true, true, false, true, true, true, false, true,
                                       true, true, false, true, true, true, false, true, true, true,
                                       null, true, null, true, null, true };

            bool[] noxcExistsList = { false, false, true, true, false, false, true, true, false, false,
                                      true, true, false, false, true, true, false, false, true, true,
                                      false, false, true, true, false, false, true, true, false, false,
                                      true, true, false, false, true, true, false, false, true, true,
                                      true, true, true, true, true, true };

            string[][] noxcModcList = { modcNullList, modcNullList, modcNo46List, modcIs46List, modcNullList, modcNullList, modcNo46List, modcIs46List, modcNullList, modcNullList,
                                        modcNo46List, modcIs46List, modcNullList, modcNullList, modcNo46List, modcIs46List, modcNullList, modcNullList, modcNo46List, modcIs46List,
                                        modcNullList, modcNullList, modcNo46List, modcIs46List, modcNullList, modcNullList, modcNo46List, modcIs46List, modcNullList, modcNullList,
                                        modcNo46List, modcIs46List, modcNullList, modcNullList, modcNo46List, modcIs46List, modcNullList, modcNullList, modcNo46List, modcIs46List,
                                        modcIs46List, modcIs46List, modcIs46List, modcIs46List, modcIs46List, modcIs46List };

            bool?[] co2cNeededList = { false, false, false, false, true, true, true, true, true, true,
                                       true, true, true, true, true, true, false, false, false, false,
                                       false, false, false, false, false, false, false, false, false, false,
                                       false, false, false, false, false, false, false, false, false, false,
                                       true, null, false, false, false, false };

            bool[] co2cExistsList = { false, false, false, false, false, false, false, false, true, true,
                                      true, true, true, true, true, true, false, false, false, false,
                                      false, false, false, false, false, false, false, false, false, false,
                                      false, false, false, false, false, false, false, false, false, false,
                                       true, true, false, false, false, false };

            string[][] co2cModcList = { modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNo46List, modcNo46List,
                                        modcNo46List, modcNo46List, modcIs46List, modcIs46List, modcIs46List, modcIs46List, modcNullList, modcNullList, modcNullList, modcNullList,
                                        modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList,
                                        modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList,
                                        modcNo46List, modcNo46List, modcNullList, modcNullList, modcNullList, modcNullList };

            bool?[] o2cdNeededList = { false, false, false, false, false, false, false, false, false, false,
                                       false, false, false, false, false, false, true, true, true, true,
                                       true, true, true, true, true, true, true, true, false, false,
                                       false, false, false, false, false, false, false, false, false, false,
                                       false, false, true, null, false, false };

            bool[] o2cdExistsList = { false, false, false, false, false, false, false, false, false, false,
                                      false, false, false, false, false, false, false, false, false, false,
                                      true, true, true, true, true, true, true, true, false, false,
                                      false, false, false, false, false, false, false, false, false, false,
                                      false, false, true, true, false, false };

            string[][] o2cdModcList = { modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList,
                                        modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList,
                                        modcNo46List, modcNo46List, modcNo46List, modcNo46List, modcIs46List, modcIs46List, modcIs46List, modcIs46List, modcNullList, modcNullList,
                                        modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList,
                                        modcNullList, modcNullList, modcNo46List, modcNo46List, modcNullList, modcNullList};

            bool?[] o2cwNeededList = { false, false, false, false, false, false, false, false, false, false,
                                       false, false, false, false, false, false, false, false, false, false,
                                       false, false, false, false, false, false, false, false, true, true,
                                       true, true, true, true, true, true, true, true, true, true,
                                       false, false, false, false, true, null };

            bool[] o2cwExistsList = { false, false, false, false, false, false, false, false, false, false,
                                      false, false, false, false, false, false, false, false, false, false,
                                      false, false, false, false, false, false, false, false, false, false,
                                      false, false, true, true, true, true, true, true, true, true,
                                      false, false, false, false, true, true };

            string[][] o2cwModcList = { modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList,
                                        modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList,
                                        modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList, modcNullList,
                                        modcNullList, modcNullList, modcNo46List, modcNo46List, modcNo46List, modcNo46List, modcIs46List, modcIs46List, modcIs46List, modcIs46List,
                                        modcNullList, modcNullList, modcNullList, modcNullList, modcNo46List, modcNo46List };

            /* Expected Values */
            string[] expResultList = { null, null, null, null, null, null, null, null, null, null,
                                       null, "A", null, null, "A", null, null, null, null, null,
                                       null, null, null, "A", null, null, "A", null, null, null,
                                       null, null, null, null, null, "A", null, null, "A", null,
                                       null, null, null, null, null, null };

            string[] expParamFor46List = { null, null, null, null, null, null, null, null, null, null,
                                           null, noxcParameter, null, null, co2cParameter, null, null, null, null, null,
                                           null, null, null, noxcParameter, null, null, o2cdParameter, null, null, null,
                                           null, null, null, null, null, noxcParameter, null, null, o2cwParameter, null,
                                           null, null, null, null, null, null  };

            string[] expParamNot46List = { null, null, null, null, null, null, null, null, null, null,
                                           null, co2cParameter, null, null, noxcParameter, null, null, null, null, null,
                                           null, null, null, o2cdParameter, null, null, noxcParameter, null, null, null,
                                           null, null, null, null, null, o2cwParameter, null, null, noxcParameter, null,
                                           null, null, null, null, null, null  };

            Func < string, string, string, string, string, string>[] expNon46ModcList = { nullModc, nullModc, nullModc, nullModc, nullModc, nullModc, nullModc, nullModc, nullModc, nullModc,
                                                                                          nullModc, non46Modc, nullModc, nullModc, non46Modc, nullModc, nullModc, nullModc, nullModc, nullModc,
                                                                                          nullModc, nullModc, nullModc, non46Modc, nullModc, nullModc, non46Modc, nullModc, nullModc, nullModc,
                                                                                          nullModc, nullModc, nullModc, nullModc, nullModc, non46Modc, nullModc, nullModc, non46Modc, nullModc,
                                                                                          non46Modc, non46Modc, non46Modc, non46Modc, non46Modc, non46Modc};

              /* Test Case Count */
              int caseCount = 46;

            /* Check array lengths */
            Assert.AreEqual(caseCount, noxcNeededList.Length, "noxcNeededList length");
            Assert.AreEqual(caseCount, noxcExistsList.Length, "noxcExistsList length");
            Assert.AreEqual(caseCount, noxcModcList.Length, "noxcModcList length");
            Assert.AreEqual(caseCount, co2cNeededList.Length, "co2cNeededList length");
            Assert.AreEqual(caseCount, co2cExistsList.Length, "co2cExistsList length");
            Assert.AreEqual(caseCount, co2cModcList.Length, "co2cModcList length");
            Assert.AreEqual(caseCount, o2cdNeededList.Length, "o2cdNeededList length");
            Assert.AreEqual(caseCount, o2cdExistsList.Length, "o2cdExistsList length");
            Assert.AreEqual(caseCount, o2cdModcList.Length, "o2cdModcList length");
            Assert.AreEqual(caseCount, o2cwNeededList.Length, "o2cwNeededList length");
            Assert.AreEqual(caseCount, o2cwExistsList.Length, "o2cwExistsList length");
            Assert.AreEqual(caseCount, o2cwModcList.Length, "o2cwModcList length");
            Assert.AreEqual(caseCount, expResultList.Length, "expResultList length");
            Assert.AreEqual(caseCount, expParamFor46List.Length, "expParamFor46List length");
            Assert.AreEqual(caseCount, expParamNot46List.Length, "expParamNot46List length");
            Assert.AreEqual(caseCount, expNon46ModcList.Length, "expNon46ModcList length");


            /* Run Cases */
            for (int caseDex = 0; caseDex < caseCount; caseDex++)
                foreach (string noxcModcCd in noxcModcList[caseDex])
                    foreach (string co2cModcCd in co2cModcList[caseDex])
                        foreach (string o2cdModcCd in o2cdModcList[caseDex])
                            foreach (string o2cwModcCd in o2cwModcList[caseDex])
                            {
                                /*  Initialize Input Parameters*/
                                EmParameters.CurrentOperatingTime = 1.00m;
                                EmParameters.DerivedHourlyChecksNeeded = true;
                                EmParameters.PrimaryBypassActiveForHour = false;

                                /*  Initialize Optional Parameters*/
                                EmParameters.Co2ConcChecksNeededForCo2MassCalc = false;
                                EmParameters.Co2ConcChecksNeededForHeatInput = false;
                                EmParameters.Co2DiluentChecksNeededForNoxRateCalc = co2cNeededList[caseDex];
                                EmParameters.Co2DiluentNeededForMats = false;
                                EmParameters.CurrentCo2ConcMonitorHourlyRecord = co2cExistsList[caseDex] ? new VwMpMonitorHrlyValueCo2cRow(modcCd: co2cModcCd) : null;
                                EmParameters.CurrentNoxConcMonitorHourlyRecord = noxcExistsList[caseDex] ? new VwMpMonitorHrlyValueRow(modcCd: noxcModcCd) : null;
                                EmParameters.CurrentO2DryMonitorHourlyRecord = o2cdExistsList[caseDex] ? new VwMpMonitorHrlyValueO2DryRow(modcCd: o2cdModcCd) : null;
                                EmParameters.CurrentO2WetMonitorHourlyRecord = o2cwExistsList[caseDex] ? new VwMpMonitorHrlyValueO2WetRow(modcCd: o2cwModcCd) : null;
                                EmParameters.NoxConcNeededForNoxRateCalc = noxcNeededList[caseDex];
                                EmParameters.O2DryChecksNeededForH2o = false;
                                EmParameters.O2DryChecksNeededForHeatInput = false;
                                EmParameters.O2DryChecksNeededForNoxRateCalc = o2cdNeededList[caseDex];
                                EmParameters.O2DryNeededForMats = false;
                                EmParameters.O2DryNeededToSupportCo2Calculation = false;
                                EmParameters.O2WetChecksNeededForH2o = false;
                                EmParameters.O2WetChecksNeededForHeatInput = false;
                                EmParameters.O2WetChecksNeededForNoxRateCalc = o2cwNeededList[caseDex];
                                EmParameters.O2WetNeededForMats = false;
                                EmParameters.O2WetNeededToSupportCo2Calculation = false;

                                /*  Initialize Output Parameters*/
                                EmParameters.MissingModc46Non46ModcCode = "Bad";
                                EmParameters.MissingModc46ParameterForModc46 = "Bad";
                                EmParameters.MissingModc46ParameterForNon46 = "Bad";


                                /* Init Cateogry Result */
                                category.CheckCatalogResult = null;

                                /* Initialize variables needed to run the check. */
                                bool log = false;
                                string actual;

                                /* Run Check */
                                actual = cHourlyOperatingDataChecks.HOUROP56(category, ref log);

                                /* Check results */
                                Assert.AreEqual(string.Empty, actual, $"actual {caseDex}");
                                Assert.AreEqual(false, log, $"log {caseDex}");
                                Assert.AreEqual(expResultList[caseDex], category.CheckCatalogResult, $"CheckCatalogResult [case {caseDex}  non-46 MODC {expNon46ModcList[caseDex](expParamNot46List[caseDex], noxcModcCd, co2cModcCd, o2cdModcCd, o2cwModcCd)}]");
                                Assert.AreEqual(expNon46ModcList[caseDex](expParamNot46List[caseDex], noxcModcCd, co2cModcCd, o2cdModcCd, o2cwModcCd), EmParameters.MissingModc46Non46ModcCode, $"MissingModc46Non46ModcCode [case {caseDex}  non-46 MODC {expNon46ModcList[caseDex](expParamNot46List[caseDex], noxcModcCd, co2cModcCd, o2cdModcCd, o2cwModcCd)}]");
                                Assert.AreEqual(expParamFor46List[caseDex], EmParameters.MissingModc46ParameterForModc46, $"MissingModc46ParameterForModc46 [case {caseDex}  non-46 MODC {expNon46ModcList[caseDex](expParamNot46List[caseDex], noxcModcCd, co2cModcCd, o2cdModcCd, o2cwModcCd)}]");
                                Assert.AreEqual(expParamNot46List[caseDex], EmParameters.MissingModc46ParameterForNon46, $"MissingModc46ParameterForNon46 [case {caseDex}  non-46 MODC {expNon46ModcList[caseDex](expParamNot46List[caseDex], noxcModcCd, co2cModcCd, o2cdModcCd, o2cwModcCd)}]");
                            }
        }

        /// <summary>
        /// HourOp-56
        /// 
        /// Checks the non NOxC, CO2C, O2D and O2W Needed, Op Time, Primary Bypass Active combinations.
        /// 
        /// 
        /// Constant Check Parameter Settings:
        ///     
        ///     Co2DiluentChecksNeededForNoxRateCalc    : true 
        ///     CurrentCo2ConcMonitorHourlyRecord       : MODC 01
        ///     CurrentNoxConcMonitorHourlyRecord       : MODC 46
        ///     CurrentO2DryMonitorHourlyRecord         : null
        ///     CurrentO2WetMonitorHourlyRecord         : null
        ///     NoxConcNeededForNoxRateCalc             : true
        ///     O2DryChecksNeededForNoxRateCalc         : false
        ///     O2WetChecksNeededForNoxRateCalc         : false
        /// 
        /// 
        /// | ## | Needed | OpTime | PBActive | Co2cCo2m | Co2cHi | Co2cMats | O2dH2o | O2dHi | O2dMats | O2dCo2c | O2dH2o | O2dHi | O2dMats | O2dCo2c || Result | Param46           | ParamNon          | Modc  || Note
        /// |  0 | true   | 1.00   | false    | false    | false  | false    | false  | false | false   | false   | false  | false | false   | false   || A      | NOx Concentration | CO2 Concentration | 01    || All (other) parameters allow core checking.
        /// |  1 | true   | 0.01   | null     | null     | null   | null     | null   | null  | null    | null    | null   | null  | null    | null    || A      | NOx Concentration | CO2 Concentration | 01    || All (other) parameters allow core checking.
        /// |  2 | false  | 1.00   | false    | false    | false  | false    | false  | false | false   | false   | false  | false | false   | false   || null   | null              | null              | null  || DerivedHourlyChecksNeeded prevents core checking.
        /// |  3 | null   | 1.00   | false    | false    | false  | false    | false  | false | false   | false   | false  | false | false   | false   || null   | null              | null              | null  || DerivedHourlyChecksNeeded prevents core checking.
        /// |  4 | true   | 0.00   | false    | false    | false  | false    | false  | false | false   | false   | false  | false | false   | false   || null   | null              | null              | null  || CurrentOperatingTime prevents core checking.
        /// |  5 | true   | 1.00   | true     | false    | false  | false    | false  | false | false   | false   | false  | false | false   | false   || null   | null              | null              | null  || PrimaryBypassActiveForHour prevents core checking.
        /// |  6 | true   | 1.00   | false    | true     | false  | false    | false  | false | false   | false   | false  | false | false   | false   || null   | null              | null              | null  || Co2ConcChecksNeededforCo2MassCalc prevents core checking.
        /// |  7 | true   | 1.00   | false    | false    | true   | false    | false  | false | false   | false   | false  | false | false   | false   || null   | null              | null              | null  || Co2ConcChecksNeededforHeatInput prevents core checking.
        /// |  8 | true   | 1.00   | false    | false    | false  | true     | false  | false | false   | false   | false  | false | false   | false   || null   | null              | null              | null  || Co2DiluentNeededforMats prevents core checking.
        /// |  9 | true   | 1.00   | false    | false    | false  | false    | true   | false | false   | false   | false  | false | false   | false   || null   | null              | null              | null  || O2DryChecksNeededforH2o prevents core checking.
        /// | 10 | true   | 1.00   | false    | false    | false  | false    | false  | true  | false   | false   | false  | false | false   | false   || null   | null              | null              | null  || O2DryChecksNeededforHeatInput prevents core checking.
        /// | 11 | true   | 1.00   | false    | false    | false  | false    | false  | false | true    | false   | false  | false | false   | false   || null   | null              | null              | null  || O2DryNeededforMats prevents core checking.
        /// | 12 | true   | 1.00   | false    | false    | false  | false    | false  | false | false   | true    | false  | false | false   | false   || null   | null              | null              | null  || O2DryNeededToSupportCo2Calculation prevents core checking.
        /// | 13 | true   | 1.00   | false    | false    | false  | false    | false  | false | false   | false   | true   | false | false   | false   || null   | null              | null              | null  || O2WetChecksNeededForH2o prevents core checking.
        /// | 14 | true   | 1.00   | false    | false    | false  | false    | false  | false | false   | false   | false  | true  | false   | false   || null   | null              | null              | null  || O2WetChecksNeededforHeatInput prevents core checking.
        /// | 15 | true   | 1.00   | false    | false    | false  | false    | false  | false | false   | false   | false  | false | true    | false   || null   | null              | null              | null  || O2WetNeededforMats prevents core checking.
        /// | 16 | true   | 1.00   | false    | false    | false  | false    | false  | false | false   | false   | false  | false | false   | true    || null   | null              | null              | null  || O2WetNeededToSupportCo2Calculation prevents core checking.
        /// </summary>
        [TestMethod()]
        public void HourOp56_Other()
        {
            /* Initialize objects generally needed for testing checks. */
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            EmParameters.Init(category.Process);
            EmParameters.Category = category;


            /* Helper Values */
            string co2cParameter = "CO2 Concentration";
            string noxcParameter = "NOx Concentration";

            /* Input Parameter Values */
            bool?[] neededList = { true, true, false, null, true, true, true, true, true, true, true, true, true, true, true, true, true };
            decimal?[] opTimeList = { 1.00m, 0.01m, 1.00m, 1.00m, 0.00m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m };
            bool?[] pbActiveList = { false, null, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false };
            bool?[] co2cCo2mList = { false, null, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false };
            bool?[] co2cHiList = { false, null, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false };
            bool?[] co2cMatsList = { false, null, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false };
            bool?[] o2dH2oList = { false, null, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false };
            bool?[] o2dHiList = { false, null, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false };
            bool?[] o2dMatsList = { false, null, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false };
            bool?[] o2dCo2cList = { false, null, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false };
            bool?[] o2wH2oList = { false, null, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false };
            bool?[] o2wHiList = { false, null, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false };
            bool?[] o2wMatsList = { false, null, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false };
            bool?[] o2wCo2cList = { false, null, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true };

            /* Expected Values */
            string[] expResultList = { "A", "A", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null };
            string[] expParamFor46List = { noxcParameter, noxcParameter, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null };
            string[] expParamNot46List = { co2cParameter, co2cParameter, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null };
            string[] expNon46ModcList = { "01", "01", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null };
            
            /* Test Case Count */
            int caseCount = 17;

            /* Check array lengths */
            Assert.AreEqual(caseCount, neededList.Length, "neededList length");
            Assert.AreEqual(caseCount, opTimeList.Length, "opTimeList length");
            Assert.AreEqual(caseCount, pbActiveList.Length, "pbActiveList length");
            Assert.AreEqual(caseCount, co2cCo2mList.Length, "co2cCo2mList length");
            Assert.AreEqual(caseCount, co2cHiList.Length, "co2cHiList length");
            Assert.AreEqual(caseCount, co2cMatsList.Length, "co2cMatsList length");
            Assert.AreEqual(caseCount, o2dH2oList.Length, "o2dH2oList length");
            Assert.AreEqual(caseCount, o2dHiList.Length, "o2dHiList length");
            Assert.AreEqual(caseCount, o2dMatsList.Length, "o2dMatsList length");
            Assert.AreEqual(caseCount, o2dCo2cList.Length, "o2dCo2cList length");
            Assert.AreEqual(caseCount, o2wH2oList.Length, "o2wH2oList length");
            Assert.AreEqual(caseCount, o2wHiList.Length, "o2wHiList length");
            Assert.AreEqual(caseCount, o2wMatsList.Length, "o2wMatsList length");
            Assert.AreEqual(caseCount, o2wCo2cList.Length, "o2wCo2cList length");
            Assert.AreEqual(caseCount, expResultList.Length, "expResultList length");
            Assert.AreEqual(caseCount, expParamFor46List.Length, "expParamFor46List length");
            Assert.AreEqual(caseCount, expParamNot46List.Length, "expParamNot46List length");
            Assert.AreEqual(caseCount, expNon46ModcList.Length, "expNon46ModcList length");


            /* Run Cases */
            for (int caseDex = 0; caseDex < caseCount; caseDex++)
            {
                /*  Initialize Input Parameters*/
                EmParameters.CurrentOperatingTime = opTimeList[caseDex];
                EmParameters.DerivedHourlyChecksNeeded = neededList[caseDex];
                EmParameters.PrimaryBypassActiveForHour = pbActiveList[caseDex];

                /*  Initialize Optional Parameters*/
                EmParameters.Co2ConcChecksNeededForCo2MassCalc = co2cCo2mList[caseDex];
                EmParameters.Co2ConcChecksNeededForHeatInput = co2cHiList[caseDex];
                EmParameters.Co2DiluentChecksNeededForNoxRateCalc = true;
                EmParameters.Co2DiluentNeededForMats = co2cMatsList[caseDex];
                EmParameters.CurrentCo2ConcMonitorHourlyRecord = new VwMpMonitorHrlyValueCo2cRow(modcCd: "01");
                EmParameters.CurrentNoxConcMonitorHourlyRecord = new VwMpMonitorHrlyValueRow(modcCd: "46");
                EmParameters.CurrentO2DryMonitorHourlyRecord = null;
                EmParameters.CurrentO2WetMonitorHourlyRecord = null;
                EmParameters.NoxConcNeededForNoxRateCalc = true;
                EmParameters.O2DryChecksNeededForH2o = o2dH2oList[caseDex];
                EmParameters.O2DryChecksNeededForHeatInput = o2dHiList[caseDex];
                EmParameters.O2DryChecksNeededForNoxRateCalc = false;
                EmParameters.O2DryNeededForMats = o2dMatsList[caseDex];
                EmParameters.O2DryNeededToSupportCo2Calculation = o2dCo2cList[caseDex];
                EmParameters.O2WetChecksNeededForH2o = o2wH2oList[caseDex];
                EmParameters.O2WetChecksNeededForHeatInput = o2wHiList[caseDex];
                EmParameters.O2WetChecksNeededForNoxRateCalc = false;
                EmParameters.O2WetNeededForMats = o2wMatsList[caseDex];
                EmParameters.O2WetNeededToSupportCo2Calculation = o2wCo2cList[caseDex];

                /*  Initialize Output Parameters*/
                EmParameters.MissingModc46Non46ModcCode = "Bad";
                EmParameters.MissingModc46ParameterForModc46 = "Bad";
                EmParameters.MissingModc46ParameterForNon46 = "Bad";


                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                /* Initialize variables needed to run the check. */
                bool log = false;
                string actual;

                /* Run Check */
                actual = cHourlyOperatingDataChecks.HOUROP56(category, ref log);

                /* Check results */
                Assert.AreEqual(string.Empty, actual, $"actual {caseDex}");
                Assert.AreEqual(false, log, $"log {caseDex}");
                Assert.AreEqual(expResultList[caseDex], category.CheckCatalogResult, $"CheckCatalogResult [case {caseDex}]");
                Assert.AreEqual(expNon46ModcList[caseDex], EmParameters.MissingModc46Non46ModcCode, $"MissingModc46Non46ModcCode [case {caseDex}]");
                Assert.AreEqual(expParamFor46List[caseDex], EmParameters.MissingModc46ParameterForModc46, $"MissingModc46ParameterForModc46 [case {caseDex}]");
                Assert.AreEqual(expParamNot46List[caseDex], EmParameters.MissingModc46ParameterForNon46, $"MissingModc46ParameterForNon46 [case {caseDex}]");
            }
        }
    }
}