using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

using ECMPS.Checks.CheckEngine;
using ECMPS.Checks.CheckEngine.SpecialParameterClasses;

using ECMPS.Checks.Data.Ecmps.CheckEm.Function;
using ECMPS.Checks.Data.Ecmps.Dbo.View;
using ECMPS.Checks.Data.Ecmps.Dbo.Table;

using ECMPS.Checks.Em.Parameters;
using ECMPS.Checks.EmissionsChecks;
using ECMPS.Checks.EmissionsReport;
using ECMPS.Checks.Parameters;
using ECMPS.Definitions.Extensions;

using UnitTest.UtilityClasses;
using ECMPS.Checks.TypeUtilities;


namespace UnitTest.Emissions
{
	[TestClass]
	public class DailyCalibrationTest
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


		#region Test Cases

		[TestMethod]
		public void DayCal1()
		{
			/* Initialize objects generally needed for testing checks. */
			cEmissionsCheckParameters parameters = UnitTestCheckParameters.InstantiateEmParameters();
			cDailyCalibrationChecks target = new cDailyCalibrationChecks(parameters);
			cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

			EmParameters.Init(category.Process);
			EmParameters.Category = category;

			/* Initialize General Variables */
			int?[] componentCaseList = { null, 0, 1 };
            DateTime?[] dailyTestDateList = { new DateTime(2020, 9, 9), new DateTime(2020, 9, 10) };

			/* Initialize variables needed to run the check. */
			bool log = false;
			string actual;

            /* Run Test Cases */
            foreach (bool hasComponentIdentifier in UnitTestStandardLists.BooleanList)
                foreach (string componentTypeCd in UnitTestStandardLists.ComponentTypeCodeList)
                    foreach (int? onlineOfflineInd in UnitTestStandardLists.IndicatorList)
                        foreach (DateTime? dailyTestDate in dailyTestDateList)
                        {
                            /*  Initialize Input Parameters*/
                            EmParameters.CurrentDailyCalibrationTest = SetValues.DateTime(new DailyCalibrationTestPeriodData(componentIdentifier: (hasComponentIdentifier ? "cmpId2" : null), componentTypeCd: componentTypeCd, onlineOfflineInd: onlineOfflineInd), dailyTestDate);

                            /* Initialize Output Parameters */
                            EmParameters.DailyCalComponentTypeValid = null;

                            EmParameters.DailyCalCalcResult = "Wha?";
                            EmParameters.DailyCalFailDate = DateTime.Now.Date;
                            EmParameters.DailyCalFailHour = DateTime.Now.Hour;

                            /* Init Cateogry Result */
                            category.CheckCatalogResult = null;

                            // Run Checks
                            actual = target.DAYCAL1(category, ref log);

                            /* Expected Results */
                            string result = null;
                            {
                                if (!hasComponentIdentifier)
                                    result = "A";
                                else if (!componentTypeCd.InList("CO2,FLOW,HCL,HG,NOX,O2,SO2"))
                                    result = "B";
                                else if (((componentTypeCd == "HG" && dailyTestDate < new DateTime(2020,9,9)) || (componentTypeCd == "HCL")) && (onlineOfflineInd != 1))
                                    result = "C";
                            }

                            bool? expectedValid = (result == null);

                            /* Check Result Label */
                            string resultPrefix = string.Format("[hasComponentId: {0}, componentTypeCd: {1}, onlineOfflineInd: {2}, dailyTestTime: {3}]", hasComponentIdentifier, componentTypeCd, onlineOfflineInd, dailyTestDate);

                            // Check Results
                            Assert.AreEqual(string.Empty, actual);
                            Assert.AreEqual(false, log);
                            Assert.AreEqual(result, category.CheckCatalogResult, resultPrefix + ".Result");

                            Assert.AreEqual(expectedValid, EmParameters.DailyCalComponentTypeValid, resultPrefix + ".DailyCalCalcResult");
                            Assert.AreEqual(null, EmParameters.DailyCalCalcResult, resultPrefix + ".DailyCalCalcResult");
                            Assert.AreEqual(null, EmParameters.DailyCalFailDate, resultPrefix + ".DailyCalFailDate");
                            Assert.AreEqual(null, EmParameters.DailyCalFailHour, resultPrefix + ".DailyCalFailHour");
                        }
		}


        /// <summary>
        /// 
        /// | ## | Type | OnOff | ZeroOp | UpscaleOp | DayCalDate || Result | CalcInd | Ignore | CalcRes || Note
        /// |  0 | HG   | null  |   1.00 |      1.00 | 2020-09-08 || A      | 1       | null   | null    || Online-Offline Indicator is null, zero and upscale op times are 1.00.
        /// |  1 | HG   | 1     |   1.00 |      1.00 | 2020-09-08 || null   | 1       | null   | null    || Online-Offline Indicator is 1, zero and upscale op times are 1.00.
        /// |  2 | HG   | 0     |   1.00 |      1.00 | 2020-09-08 || null   | 1       | null   | null    || Online-Offline Indicator is 0, zero and upscale op times are 1.00.
        /// |  3 | HG   | null  |   0.00 |      0.00 | 2020-09-08 || A      | 0       | null   | null    || Online-Offline Indicator is null, zero and upscale op times are 0.00.
        /// |  4 | HG   | null  |   0.00 |      0.00 | 2020-09-09 || A      | 0       | null   | null    || Online-Offline Indicator is null, zero and upscale op times are 0.00, on 9/9/2020.
        /// |  5 | HG   | 1     |   0.00 |      0.00 | 2020-09-08 || E      | 0       | null   | null    || Online-Offline Indicator is 1, zero and upscale op times are 0.00.
        /// |  6 | HG   | 1     |   0.00 |      0.00 | 2020-09-09 || B      | 0       | null   | null    || Online-Offline Indicator is 1, zero and upscale op times are 0.00, on 9/9/2020.
        /// |  7 | HG   | 0     |   0.00 |      0.00 | 2020-09-08 || null   | 0       | null   | null    || Online-Offline Indicator is 0, zero and upscale op times are 0.00.
        /// |  8 | HG   | 0     |   0.00 |      0.00 | 2020-09-09 || null   | 0       | null   | null    || Online-Offline Indicator is 0, zero and upscale op times are 0.00, on 9/9/2020.
        /// |  9 | HG   | 1     |   1.00 |      0.00 | 2020-09-08 || E      | 0       | null   | null    || Online-Offline Indicator is 1, zero op time is 1.00 and upscale op time is 0.00.
        /// | 10 | HG   | 1     |   0.00 |      1.00 | 2020-09-08 || E      | 0       | null   | null    || Online-Offline Indicator is 1, zero op time is 0.00 and upscale op time is 1.00.
        /// | 11 | HG   | 1     |   1.00 |      0.99 | 2020-09-08 || null   | 1       | null   | null    || Online-Offline Indicator is 1, zero op time is 1.00 and upscale op time is 0.99.
        /// | 12 | HG   | 1     |   1.00 |      0.01 | 2020-09-08 || null   | 1       | null   | null    || Online-Offline Indicator is 1, zero op time is 1.00 and upscale op time is 0.01.
        /// | 13 | HG   | 1     |   0.99 |      1.00 | 2020-09-08 || null   | 1       | null   | null    || Online-Offline Indicator is 1, zero op time is 0.99 and upscale op time is 1.00.
        /// | 14 | HG   | 1     |   0.01 |      1.00 | 2020-09-08 || null   | 1       | null   | null    || Online-Offline Indicator is 1, zero op time is 0.01 and upscale op time is 1.00.
        /// | 15 | HCL  | null  |   1.00 |      1.00 | 2020-09-08 || A      | 1       | null   | null    || Online-Offline Indicator is null, zero and upscale op times are 1.00.
        /// | 16 | HCL  | 1     |   1.00 |      1.00 | 2020-09-08 || null   | 1       | null   | null    || Online-Offline Indicator is 1, zero and upscale op times are 1.00.
        /// | 17 | HCL  | 0     |   1.00 |      1.00 | 2020-09-08 || null   | 1       | null   | null    || Online-Offline Indicator is 0, zero and upscale op times are 1.00.
        /// | 18 | HCL  | null  |   0.00 |      0.00 | 2020-09-08 || A      | 0       | null   | null    || Online-Offline Indicator is null, zero and upscale op times are 0.00.
        /// | 19 | HCL  | null  |   0.00 |      0.00 | 2020-09-09 || A      | 0       | null   | null    || Online-Offline Indicator is null, zero and upscale op times are 0.00, on 9/9/2020.
        /// | 20 | HCL  | 1     |   0.00 |      0.00 | 2020-09-08 || E      | 0       | null   | null    || Online-Offline Indicator is 1, zero and upscale op times are 0.00.
        /// | 21 | HCL  | 1     |   0.00 |      0.00 | 2020-09-09 || E      | 0       | null   | null    || Online-Offline Indicator is 1, zero and upscale op times are 0.00, on 9/9/2020.
        /// | 22 | HCL  | 0     |   0.00 |      0.00 | 2020-09-08 || null   | 0       | null   | null    || Online-Offline Indicator is 0, zero and upscale op times are 0.00.
        /// | 23 | HCL  | 0     |   0.00 |      0.00 | 2020-09-09 || null   | 0       | null   | null    || Online-Offline Indicator is 0, zero and upscale op times are 0.00, on 9/9/2020.
        /// | 24 | HCL  | 1     |   1.00 |      0.00 | 2020-09-08 || E      | 0       | null   | null    || Online-Offline Indicator is 0, zero op time is 1.00 and upscale op time is 0.00.
        /// | 25 | HCL  | 1     |   0.00 |      1.00 | 2020-09-08 || E      | 0       | null   | null    || Online-Offline Indicator is 1, zero op time is 0.00 and upscale op time is 1.00.
        /// | 26 | HCL  | 1     |   1.00 |      0.99 | 2020-09-08 || null   | 1       | null   | null    || Online-Offline Indicator is 1, zero op time is 1.00 and upscale op time is 0.99.
        /// | 27 | HCL  | 1     |   1.00 |      0.01 | 2020-09-08 || null   | 1       | null   | null    || Online-Offline Indicator is 1, zero op time is 1.00 and upscale op time is 0.01.
        /// | 28 | HCL  | 1     |   0.99 |      1.00 | 2020-09-08 || null   | 1       | null   | null    || Online-Offline Indicator is 1, zero op time is 0.99 and upscale op time is 1.00.
        /// | 29 | HCL  | 1     |   0.01 |      1.00 | 2020-09-08 || null   | 1       | null   | null    || Online-Offline Indicator is 1, zero op time is 0.01 and upscale op time is 1.00.
        /// | 30 | HG   | 0     |   0.01 |      0.99 | 2020-09-08 || null   | 0       | null   | null    || Online-Offline Indicator is 0, zero op time is 0.01 and upscale op time is 0.99.
        /// | 31 | HCL  | 0     |   0.01 |      0.99 | 2020-09-08 || null   | 0       | null   | null    || Online-Offline Indicator is 0, zero op time is 0.01 and upscale op time is 0.99.
        /// | 32 | HG   | 0     |   0.01 |      0.99 | 2020-09-09 || null   | 0       | null   | null    || Online-Offline Indicator is 0, zero op time is 0.01 and upscale op time is 0.99, on 9/9/2020.
        /// | 33 | HCL  | 0     |   0.01 |      0.99 | 2020-09-09 || null   | 0       | null   | null    || Online-Offline Indicator is 0, zero op time is 0.01 and upscale op time is 0.99, on 9/9/2020.
        /// </summary>
        [TestMethod]
        public void DayCal3_HgAndHcl()
        {
            /* Initialize objects generally needed for testing checks. */
            cEmissionsCheckParameters parameters = UnitTestCheckParameters.InstantiateEmParameters();
            cDailyCalibrationChecks target = new cDailyCalibrationChecks(parameters);
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            EmParameters.Init(category.Process);
            EmParameters.Category = category;


            /* Initialize General Variables */
            DateTime d20200908 = new DateTime(2020, 09, 08);
            DateTime d20200909 = new DateTime(2020, 09, 09);


            /* Input Parameter Values */
            string[] typeList = { "HG", "HG", "HG", "HG", "HG", "HG", "HG", "HG", "HG", "HG",
                                  "HG", "HG", "HG", "HG", "HG", "HCL", "HCL", "HCL", "HCL", "HCL",
                                  "HCL", "HCL", "HCL", "HCL", "HCL", "HCL", "HCL", "HCL", "HCL", "HCL",
                                  "HG", "HCL", "HG", "HCL" };
            int?[] onOffList = { null, 1, 0, null, null, 1, 1, 0, 0, 1,
                                 1, 1, 1, 1, 1, null, 1, 0, null, null,
                                 1, 1, 0, 0, 1, 1, 1, 1, 1, 1,
                                 0, 0, 0, 0 };
            decimal?[] zeroOpList = { 1.00m, 1.00m, 1.00m, 0.00m, 0.00m, 0.00m, 0.00m, 0.00m, 0.00m, 1.00m,
                                      0.00m, 1.00m, 1.00m, 0.99m, 0.01m, 1.00m, 1.00m, 1.00m, 0.00m, 0.00m,
                                      0.00m, 0.00m, 0.00m, 0.00m, 1.00m, 0.00m, 1.00m, 1.00m, 0.99m, 0.01m,
                                      0.01m, 0.01m, 0.01m, 0.01m };
            decimal?[] upscaleList = { 1.00m, 1.00m, 1.00m, 0.00m, 0.00m, 0.00m, 0.00m, 0.00m, 0.00m, 0.00m,
                                       1.00m, 0.99m, 0.01m, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m, 0.00m, 0.00m,
                                       0.00m, 0.00m, 0.00m, 0.00m, 0.00m, 1.00m, 0.99m, 0.01m, 1.00m, 1.00m,
                                       0.99m, 0.99m, 0.99m, 0.99m };
            DateTime?[] dailyTestDateList = { d20200908, d20200908, d20200908, d20200908, d20200909, d20200908, d20200909, d20200908, d20200909, d20200908,
                                              d20200908, d20200908, d20200908, d20200908, d20200908, d20200908, d20200908, d20200908, d20200908, d20200909,
                                              d20200908, d20200909, d20200908, d20200909, d20200908, d20200908, d20200908, d20200908, d20200908, d20200908,
                                              d20200908, d20200908, d20200909, d20200909 };


            /* Expected Values */
            string[] expResultList = { "A", null, null, "A", "A", "E", "B", null, null, "E",
                                       "E", null, null, null, null, "A", null, null, "A", "A",
                                       "E", "E", null, null, "E", "E", null, null, null, null,
                                       null, null, null, null };
            int?[] expCalcIndList = { 1, 1, 1, 0, 0, 0, 0, 0, 0, 0,
                                      0, 1, 1, 1, 1, 1, 1, 1, 0, 0,
                                      0, 0, 0, 0, 0, 0, 1, 1, 1, 1,
                                      0, 0, 0, 0 };
            bool?[] expIgnoreList = { null, null, null, null, null, null, null, null, null, null,
                                      null, null, null, null, null, null, null, null, null, null,
                                      null, null, null, null, null, null, null, null, null, null,
                                      null, null, null, null };
            string[] expCalcResultList = { null, null, null, null, null, null, null, null, null, null,
                                           null, null, null, null, null, null, null, null, null, null,
                                           null, null, null, null, null, null, null, null, null, null,
                                           null, null, null, null };

            /* Test Case Count */
            int caseCount = 34;

            /* Check array lengths */
            Assert.AreEqual(caseCount, typeList.Length, "typeList length");
            Assert.AreEqual(caseCount, onOffList.Length, "onOffList length");
            Assert.AreEqual(caseCount, zeroOpList.Length, "zeroOpList length");
            Assert.AreEqual(caseCount, upscaleList.Length, "upscaleList length");
            Assert.AreEqual(caseCount, dailyTestDateList.Length, "dailyTestDateList length");
            Assert.AreEqual(caseCount, expResultList.Length, "expResultList length");
            Assert.AreEqual(caseCount, expCalcIndList.Length, "expCalcIndList length");
            Assert.AreEqual(caseCount, expIgnoreList.Length, "expIgnoreList length");
            Assert.AreEqual(caseCount, expCalcResultList.Length, "expCalcResultList length");


            /* Run Cases */
            for (int caseDex = 0; caseDex < caseCount; caseDex++)
            {
                /* Initialize Input Parameters */
                EmParameters.CurrentDailyCalibrationTest
                    = SetValues.DateTime(new DailyCalibrationTestPeriodData(componentTypeCd: typeList[caseDex], onlineOfflineInd: onOffList[caseDex], zeroOpTime: zeroOpList[caseDex], upscaleOpTime: upscaleList[caseDex]), dailyTestDateList[caseDex]);
                EmParameters.OocTestRecordsByLocation = new CheckDataView<OnOffCalibrationTestAllData>();
                EmParameters.QaCertificationEventRecords = new CheckDataView<VwQaCertEventRow>();

                /* Initialize Optional Parameters */
                EmParameters.DailyCalCalcResult = null;
                EmParameters.IgnoredDailyCalibrationTests = null;

                /* Initialize Output Parameters */
                EmParameters.DailyCalCalcOnlineInd = -1;


                /* Initialize variables needed to run the check. */
                bool log = false;
                string actual;
                
                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                // Run Checks
                actual = target.DAYCAL3(category, ref log);

                /* Check Results */
                Assert.AreEqual(string.Empty, actual, string.Format("actual {0}", caseDex));
                Assert.AreEqual(false, log, string.Format("log {0}", caseDex));

                Assert.AreEqual(expResultList[caseDex], category.CheckCatalogResult, $"CheckCatalogResult {caseDex}");
                Assert.AreEqual(expCalcIndList[caseDex], EmParameters.DailyCalCalcOnlineInd, $"DailyCalCalcOnlineInd {caseDex}");
                Assert.AreEqual(expIgnoreList[caseDex], EmParameters.IgnoredDailyCalibrationTests, $"IgnoredDailyCalibrationTests {caseDex}");
                Assert.AreEqual(expCalcResultList[caseDex], EmParameters.DailyCalCalcResult, $"DailyCalCalcResult {caseDex}");
            }
        }

        [TestMethod]
        public void DayCal4ABandD()
        {
            /* Initialize objects generally needed for testing checks. */
            cEmissionsCheckParameters parameters = UnitTestCheckParameters.InstantiateEmParameters();
            cDailyCalibrationChecks target = new cDailyCalibrationChecks(parameters);
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            EmParameters.Init(category.Process);
            EmParameters.Category = category;

            /* Initialize variables needed to run the check. */
            bool log = false;
            string actual;

            /* Run Test Cases */
            foreach (string ComponentTypeCode in UnitTestStandardLists.ComponentTypeCodeList)
                foreach (string SpanScaleCode in UnitTestStandardLists.SpanScaleCodeList)
                {
                    /*  Initialize Input Parameters*/
                    EmParameters.CurrentDailyCalibrationTest = new DailyCalibrationTestPeriodData(componentTypeCd: ComponentTypeCode,  spanScaleCd: SpanScaleCode);
                    EmParameters.DailyCalComponentTypeValid = true;

                    /*  Initialize Output Parameters*/
                    EmParameters.DailyCalSpanScaleValid = null;

                    /* Init Cateogry Result */
                    category.CheckCatalogResult = null; 

                    // Run Checks
                    actual = target.DAYCAL4(category, ref log);
                     
                    switch (ComponentTypeCode)
                    {
                        case "FLOW":
                            if (!string.IsNullOrEmpty(SpanScaleCode)){
                                Assert.AreEqual(false, cDBConvert.ToBoolean(EmParameters.DailyCalSpanScaleValid), "DailyCalSpanScaleValid");
                                Assert.AreEqual("D", category.CheckCatalogResult, "Result");
                            }
                            break;

                        case "HG":
                        case "HCL":
                            if (string.IsNullOrEmpty(SpanScaleCode)){
                                Assert.AreEqual(false, cDBConvert.ToBoolean(EmParameters.DailyCalSpanScaleValid), "DailyCalSpanScaleValid");
                                Assert.AreEqual("A", category.CheckCatalogResult, "Result");
                            }
                            else if (SpanScaleCode != "H"){
                                Assert.AreEqual(false, cDBConvert.ToBoolean(EmParameters.DailyCalSpanScaleValid), "DailyCalSpanScaleValid");
                                Assert.AreEqual("B", category.CheckCatalogResult, "Result");
                            }
                            break;
                        default:
                            if (string.IsNullOrEmpty(SpanScaleCode)){
                                Assert.AreEqual(false, cDBConvert.ToBoolean(EmParameters.DailyCalSpanScaleValid), "DailyCalSpanScaleValid");
                                Assert.AreEqual("A", category.CheckCatalogResult, "Result");
                            }
                            else if (!SpanScaleCode.InList("H,L")){
                                Assert.AreEqual(false, cDBConvert.ToBoolean(EmParameters.DailyCalSpanScaleValid), "DailyCalSpanScaleValid");
                                Assert.AreEqual("B", category.CheckCatalogResult, "Result");
                            }
                            else{
                                // Do EM Test to check for result C.
                            }
                            break;
                    } 
                    // Check Results
                    Assert.AreEqual(string.Empty, actual);
                    Assert.AreEqual(false, log);
                }
        }


        /// <summary>
        /// PGVP-2 Cylinder Id Format
        /// 
        /// 1) Start with a seed Cylinder Id that contains each number and capital letter.
        /// 2) Select a position in the seed id, and successively replace the character at that position with each ASCII character.
        /// 3) AETB-11 should return a result of A if the replacement character is not a number or a capital letter.
        /// </summary>
        [TestMethod()]
        public void DayCal27_CylinderIdFormat()
        {
            /* Initialize objects generally needed for testing checks. */
            cEmissionsCheckParameters emCheckParameters = new cEmissionsCheckParameters();
            cDailyCalibrationChecks target = new cDailyCalibrationChecks(emCheckParameters);
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory(emCheckParameters);

            EmParameters.Init(category.Process);
            EmParameters.Category = category;


            /* General Values */
            string seed1 = "ABC9DE8FG7HI6JK5LM";
            string seed2 = "NO4PQ3RS2TU1VW0XYZ";
            string cylinderId;
            string expList;
            string expResult = null;
            char testChar;

            for (int ascii = 0; ascii <= 255; ascii++)
            {
                /* Setup Variables */
                testChar = (char)ascii;
                {
                    cylinderId = seed1 + testChar + seed2;

                    switch (testChar)
                    {
                        case 'A': case 'B': case 'C': case 'D': case 'E': case 'F': case 'G': case 'H': case 'I': case 'J': case 'K': case 'L': case 'M':
                        case 'N': case 'O': case 'P': case 'Q': case 'R': case 'S': case 'T': case 'U': case 'V': case 'W': case 'X': case 'Y': case 'Z':
                        case '1': case '2': case '3': case '4': case '5': case '6': case '7': case '8': case '9': case '0': case '-': case '&': case '.':
                            {
                                expList = "OldId,AaaId";
                            }
                            break;
                        default:
                            {
                                expList = string.Format("OldId,AaaId,{0}", cylinderId);
                            }
                            break;
                    }
                }


                /* Initialize Input Parameters */
                EmParameters.CurrentDailyCalibrationTest = new DailyCalibrationTestPeriodData(cylinderId: cylinderId, upscaleGasTypeCd: "GASGOOD");
                EmParameters.EvaluateUpscaleInjection = true;
                EmParameters.UpscaleGasTypeValid = true;

                /* Initialize Input-Output Parameters */
                EmParameters.InvalidCylinderIdList = new List<string> { "OldId", "AaaId" };


                /* Init Cateogry Result */
                category.CheckCatalogResult = null;

                /* Initialize variables needed to run the check. */
                bool log = false;
                string actual;

                /* Run Check */
                actual = target.DAYCAL27(category, ref log);


                /* Check Results */
                Assert.AreEqual(string.Empty, actual, string.Format("actual [{0} => {1}]", ascii, cylinderId));
                Assert.AreEqual(false, log, string.Format("log [{0} => {1}]", ascii, cylinderId));
                Assert.AreEqual(expResult, category.CheckCatalogResult, string.Format("CheckCatalogResult [{0} => {1}]", ascii, cylinderId));
                Assert.AreEqual(expList, EmParameters.InvalidCylinderIdList.DelimitedList(","), string.Format("InvalidCylinderIdList [{0} => {1}]", ascii, cylinderId));
            }
        }


        /// <summary>
        ///A test for DAYCAL28 - Extend PGVP expiration to 8 years
        ///</summary>()
        [TestMethod]
		public void DAYCAL28()
		{
			//instantiated checks and old instantiated parameters setup

			cEmissionsCheckParameters emCheckParameters = UnitTestCheckParameters.InstantiateEmParameters();
			cDailyCalibrationChecks target = new cDailyCalibrationChecks(emCheckParameters);
			
			// Init Variables
			bool log = false;
			string actual;

			// Init Input
			EmParameters.EvaluateUpscaleInjection = true;
			EmParameters.ProtocolGasVendorLookupTable = new CheckDataView<ProtocolGasVendorRow>(new ProtocolGasVendorRow(vendorId: "NONPGVP"));
			EmParameters.UpscaleGasTypeValid = true;

			//Result D
			{
				EmParameters.CurrentDailyCalibrationTest = new DailyCalibrationTestPeriodData(vendorId: "NONPGVP", dailyTestDate: DateTime.Today, dailyTestDatetime: DateTime.Today, dailyTestHour: 0, dailyTestMin: 0, upscaleGasTypeCd: "NOTNULL");
				EmParameters.DailyCalPgvpRuleDate = DateTime.Today.AddDays(-60).AddYears(-8);

				// Init Output
				EmParameters.Category.CheckCatalogResult = null;

				// Run Checks
				actual = target.DAYCAL28(EmParameters.Category, ref log);

				// Check Results
				Assert.AreEqual(string.Empty, actual);
				Assert.AreEqual(false, log);
				Assert.AreEqual("D", EmParameters.Category.CheckCatalogResult, "Result");
			}

			//Pass
			{
				EmParameters.CurrentDailyCalibrationTest = new DailyCalibrationTestPeriodData(vendorId: "NONPGVP", dailyTestDate: DateTime.Today, dailyTestDatetime: DateTime.Today, dailyTestHour: 0, dailyTestMin: 0, upscaleGasTypeCd: "NOTNULL");
				EmParameters.DailyCalPgvpRuleDate = DateTime.Today.AddDays(-59).AddYears(-8);

				// Init Output
				EmParameters.Category.CheckCatalogResult = null;

				// Run Checks
				actual = target.DAYCAL28(EmParameters.Category, ref log);

				// Check Results
				Assert.AreEqual(string.Empty, actual);
				Assert.AreEqual(false, log);
				Assert.AreEqual(null, EmParameters.Category.CheckCatalogResult, "Result");
			}

            //Result G
            {
                EmParameters.CurrentDailyCalibrationTest = new DailyCalibrationTestPeriodData(vendorId: "NONPGVP", dailyTestDate: DateTime.Today.AddYears(-8).AddDays(-1), dailyTestDatetime: DateTime.Today.AddYears(-8).AddDays(-1), dailyTestHour: 0, dailyTestMin: 0, upscaleGasTypeCd: "NOTNULL");
                EmParameters.ProtocolGasVendorLookupTable = new CheckDataView<ProtocolGasVendorRow>(
                        new ProtocolGasVendorRow(vendorId: "NONPGVP", activationDate: DateTime.Today.AddDays(-1)));
                EmParameters.DailyCalPgvpRuleDate = DateTime.Today.AddDays(-60).AddYears(-8);

                // Init Output
                EmParameters.Category.CheckCatalogResult = null;

                // Run Checks
                actual = target.DAYCAL28(EmParameters.Category, ref log);

                // Check Results
                Assert.AreEqual(string.Empty, actual);
                Assert.AreEqual(false, log);
                Assert.AreEqual("G", EmParameters.Category.CheckCatalogResult, "Result");
            }

            //Result G
            {
                EmParameters.CurrentDailyCalibrationTest = new DailyCalibrationTestPeriodData(vendorId: "V01", dailyTestDate: DateTime.Today.AddYears(-8).AddDays(-1), dailyTestDatetime: DateTime.Today.AddYears(-8).AddDays(-1), dailyTestHour: 0, dailyTestMin: 0, upscaleGasTypeCd: "NOTNULL");
                EmParameters.ProtocolGasVendorLookupTable = new CheckDataView<ProtocolGasVendorRow>(
                        new ProtocolGasVendorRow(vendorId: "V01", activationDate: DateTime.Today.AddDays(-1)));
                //EmParameters.DailyCalPgvpRuleDate = DateTime.Today.AddDays(-60).AddYears(-8);

                // Init Output
                EmParameters.Category.CheckCatalogResult = null;

                // Run Checks
                actual = target.DAYCAL28(EmParameters.Category, ref log);

                // Check Results
                Assert.AreEqual(string.Empty, actual);
                Assert.AreEqual(false, log);
                Assert.AreEqual("G", EmParameters.Category.CheckCatalogResult, "Result");
            }

        }

		#endregion

	}
}
