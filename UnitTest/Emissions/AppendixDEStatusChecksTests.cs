using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ECMPS.Checks.CheckEngine;
using ECMPS.Checks.Data.Ecmps.CheckEm.Function;
using ECMPS.Checks.Data.Ecmps.Dbo.View;
using ECMPS.Checks.Em.Parameters;
using ECMPS.Checks.EmissionsChecks;
using ECMPS.Checks.EmissionsReport;
using ECMPS.Definitions.Enumerations;

using UnitTest.UtilityClasses;


namespace UnitTest.Emissions
{
    [TestClass]
    public class AppendixDEStatusChecksTests
    {

        /// <summary>
        /// Test cAppendixDEStatusChecks.GetOpHourCountTrySystemThenFuel(int year, int quarter)
        /// 
        /// 1) Test the selection only for "OP" Op Supp Data Type Code for System Op Supp Data.
        /// 2) Test the selection only for "OPHOURS" Op Type Code for Fuel-Specific Op Supp Data.
        /// 3) Test the selection only for "OPHOURS" Op Type Code for Location-Level Op Supp Data.
        /// 4) Test the selection hierarchy with System first, Fuel-Specific second, and no selection of Location-Level.
        /// </summary>
        [TestMethod]
        public void GetOpHourCountTrySystemThenFuel()
        {
            /* Initialize objects generally needed for testing checks. */
            cAppendixDEStatusChecks target = new cAppendixDEStatusChecks(new cEmissionsCheckParameters());
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            EmParameters.Init(category.Process);
            EmParameters.Category = category;


            /* Initialize General Variables */
            string monSysId = "GoodSysId";
            int quarter = 2;
            int year = 2020;


            /* Check System Op Supp Data Codes Cases */
            foreach (string systemOpSuppDataTypeCd in UnitTestStandardLists.OpSuppDataTypeCodeList)
            {
                /* Initialize Parameters */
                EmParameters.CurrentFuelFlowRecord = new VwMpHrlyFuelFlowRow(fuelCd: "GFUEL");
                EmParameters.FuelFlowComponentRecordToCheck = new VwMpMonitorSystemComponentRow(monSysId: monSysId);
                EmParameters.OperatingSuppDataRecordsByLocation = new CheckDataView<VwMpOpSuppDataRow>
                    (
                        new VwMpOpSuppDataRow(calendarYear: year, quarter: quarter, fuelCd: "BFUEL", opTypeCd: "BAD", opValue: 2200m),
                        new VwMpOpSuppDataRow(calendarYear: year, quarter: quarter, fuelCd: null, opTypeCd: "BAD", opValue: 2200m)
                    );
                EmParameters.SystemOperatingSuppDataRecordsByLocation = new CheckDataView<SystemOpSuppData>();
                {
                    foreach (string addCd in UnitTestStandardLists.OpSuppDataTypeCodeList)
                    {
                        if (addCd == systemOpSuppDataTypeCd)
                        {
                            EmParameters.SystemOperatingSuppDataRecordsByLocation.Add
                                (
                                    new SystemOpSuppData(monSysId: monSysId, calendarYear: year, quarter: quarter, opSuppDataTypeCd: addCd, hours: 168)
                                );
                        }
                        else if (addCd != "OP")
                        {
                            EmParameters.SystemOperatingSuppDataRecordsByLocation.Add
                                (
                                    new SystemOpSuppData(monSysId: monSysId, calendarYear: year, quarter: quarter, opSuppDataTypeCd: addCd, hours: 2200)
                                );
                        }
                    }
                }


                /* Run Case */
                int? opHourCount = cAppendixDEStatusChecks.GetOpHourCountTrySystemThenFuel(year, quarter);


                /* Check Results */
                Assert.AreEqual((systemOpSuppDataTypeCd == "OP" ? 168 : (int?)null), opHourCount, $"Check System Op Supp Data Codes: {systemOpSuppDataTypeCd}");
            }


            /* Check Fuel-Specific Op Supp Data Codes Cases */
            foreach (string opTypeCd in UnitTestStandardLists.OpTypeCodeList)
            {
                /* Initialize Parameters */
                EmParameters.CurrentFuelFlowRecord = new VwMpHrlyFuelFlowRow(fuelCd: "GFUEL");
                EmParameters.FuelFlowComponentRecordToCheck = new VwMpMonitorSystemComponentRow(monSysId: monSysId);
                EmParameters.OperatingSuppDataRecordsByLocation = new CheckDataView<VwMpOpSuppDataRow>();
                {
                    foreach (string addCd in UnitTestStandardLists.OpTypeCodeList)
                    {
                        if (addCd == opTypeCd)
                        {
                            EmParameters.OperatingSuppDataRecordsByLocation.Add
                                (
                                    new VwMpOpSuppDataRow(calendarYear: year, quarter: quarter, fuelCd: "GFUEL", opTypeCd: addCd, opValue: 168m)
                                );
                        }
                        else if (addCd != "OPHOURS")
                        {
                            EmParameters.OperatingSuppDataRecordsByLocation.Add
                                (
                                    new VwMpOpSuppDataRow(calendarYear: year, quarter: quarter, fuelCd: "GFUEL", opTypeCd: addCd, opValue: 2200m)
                                );
                        }
                    }
                }
                EmParameters.SystemOperatingSuppDataRecordsByLocation = new CheckDataView<SystemOpSuppData>
                    (
                        new SystemOpSuppData(monSysId: monSysId, calendarYear: year, quarter: quarter, opSuppDataTypeCd: "BAD", hours: 2200)
                    );


                /* Run Case */
                int? opHourCount = cAppendixDEStatusChecks.GetOpHourCountTrySystemThenFuel(year, quarter);


                /* Check Results */
                Assert.AreEqual((opTypeCd == "OPHOURS" ? 168 : (int?)null), opHourCount, $"Check Fuel-Specific Op Supp Data Codes: {opTypeCd}");
            }


            /* Check Location-Level Op Supp Data Codes Cases */
            foreach (string opTypeCd in UnitTestStandardLists.OpTypeCodeList)
            {
                /* Initialize Parameters */
                EmParameters.CurrentFuelFlowRecord = new VwMpHrlyFuelFlowRow(fuelCd: "GFUEL");
                EmParameters.FuelFlowComponentRecordToCheck = new VwMpMonitorSystemComponentRow(monSysId: monSysId);
                EmParameters.OperatingSuppDataRecordsByLocation = new CheckDataView<VwMpOpSuppDataRow>();
                {
                    foreach (string addCd in UnitTestStandardLists.OpTypeCodeList)
                    {
                        if (addCd == opTypeCd)
                        {
                            EmParameters.OperatingSuppDataRecordsByLocation.Add
                                (
                                    new VwMpOpSuppDataRow(calendarYear: year, quarter: quarter, fuelCd: null, opTypeCd: addCd, opValue: 168m)
                                );
                        }
                        else if (addCd != "OPHOURS")
                        {
                            EmParameters.OperatingSuppDataRecordsByLocation.Add
                                (
                                    new VwMpOpSuppDataRow(calendarYear: year, quarter: quarter, fuelCd: null, opTypeCd: addCd, opValue: 2200m)
                                );
                        }
                    }
                }
                EmParameters.SystemOperatingSuppDataRecordsByLocation = new CheckDataView<SystemOpSuppData>
                    (
                        new SystemOpSuppData(monSysId: monSysId, calendarYear: year, quarter: quarter, opSuppDataTypeCd: "BAD", hours: 2200)
                    );


                /* Run Case */
                int? opHourCount = cAppendixDEStatusChecks.GetOpHourCountTrySystemThenFuel(year, quarter);


                /* Check Results */
                Assert.AreEqual(null, opHourCount, $"Check Location-Level Op Supp Data Codes: {opTypeCd}");
            }


            /* Check Combined Supplemental Data Sources Cases */
            short[] indicatorList = { 0, 1 };
            short[] threeLevelList = { -1, 0, 1 };

            foreach (short systemOpIndicator in indicatorList)
                foreach (short fuelSpecificIndicator in threeLevelList)
                    foreach (short locationLevelIndicator in indicatorList)
                    {
                        /* Initialize Parameters */
                        EmParameters.CurrentFuelFlowRecord = new VwMpHrlyFuelFlowRow(fuelCd: "GFUEL");
                        EmParameters.FuelFlowComponentRecordToCheck = new VwMpMonitorSystemComponentRow(monSysId: monSysId);
                        EmParameters.OperatingSuppDataRecordsByLocation = new CheckDataView<VwMpOpSuppDataRow>();
                        {
                            if (fuelSpecificIndicator != 0)
                            {
                                EmParameters.OperatingSuppDataRecordsByLocation.Add
                                    (
                                        new VwMpOpSuppDataRow(calendarYear: year, quarter: quarter, fuelCd: "GFUEL", opTypeCd: "OPHOURS", opValue: (fuelSpecificIndicator == 1) ? 169m : (decimal?)null)
                                    );
                            }

                            if (locationLevelIndicator != 0)
                            {
                                EmParameters.OperatingSuppDataRecordsByLocation.Add
                                    (
                                        new VwMpOpSuppDataRow(calendarYear: year, quarter: quarter, fuelCd: null, opTypeCd: "OPHOURS", opValue: (locationLevelIndicator == 1) ? 2197m : (decimal?)null)
                                    );
                            }
                        }
                        EmParameters.SystemOperatingSuppDataRecordsByLocation = new CheckDataView<SystemOpSuppData>();
                        {
                            if (systemOpIndicator == 1)
                            {
                                EmParameters.SystemOperatingSuppDataRecordsByLocation.Add
                                    (
                                        new SystemOpSuppData(monSysId: monSysId, calendarYear: year, quarter: quarter, opSuppDataTypeCd: "OP", hours: 13)
                                    );
                            }
                        }


                        /* Run Case */
                        int? opHourCount = cAppendixDEStatusChecks.GetOpHourCountTrySystemThenFuel(year, quarter);


                        /* Expected Results */
                        int? expOpHourCount;
                        {
                            if (systemOpIndicator == 1) expOpHourCount = 13;
                            else if (fuelSpecificIndicator == 1) expOpHourCount = 169;
                            else if (locationLevelIndicator == 1) expOpHourCount = null;
                            else expOpHourCount = null;
                        }


                        /* Check Results */
                        Assert.AreEqual(expOpHourCount, opHourCount, $"Check Combined Supplemental Data Sources: System {systemOpIndicator}, Fuel-Specific {fuelSpecificIndicator}, Location-Level {locationLevelIndicator}");
                    }
        }


        /// <summary>
        /// Test cAppendixDEStatusChecks.GetOpHourCountTrySystemThenFuel(int year, int quarter)
        /// 
        /// 1) Test the selection only for "OP" Op Supp Data Type Code for System Op Supp Data.
        /// 2) Test the selection only for "OPHOURS" Op Type Code for Fuel-Specific Op Supp Data.
        /// 3) Test the selection only for "OPHOURS" Op Type Code for Location-Level Op Supp Data.
        /// 4) Test the selection hierarchy with System first, Fuel-Specific second, and no selection of Location-Level.
        /// </summary>
        [TestMethod]
        public void GetOpHourCountTrySystemThenFuelThenLocation()
        {
            /* Initialize objects generally needed for testing checks. */
            cAppendixDEStatusChecks target = new cAppendixDEStatusChecks(new cEmissionsCheckParameters());
            cCategory category = new UnitTest.UtilityClasses.UnitTestCategory();

            EmParameters.Init(category.Process);
            EmParameters.Category = category;


            /* Initialize General Variables */
            string monSysId = "GoodSysId";
            int quarter = 2;
            int year = 2020;


            /* Check System Op Supp Data Codes Cases */
            foreach (string systemOpSuppDataTypeCd in UnitTestStandardLists.OpSuppDataTypeCodeList)
            {
                /* Initialize Parameters */
                EmParameters.CurrentFuelFlowRecord = new VwMpHrlyFuelFlowRow(fuelCd: "GFUEL");
                EmParameters.FuelFlowComponentRecordToCheck = new VwMpMonitorSystemComponentRow(monSysId: monSysId);
                EmParameters.OperatingSuppDataRecordsByLocation = new CheckDataView<VwMpOpSuppDataRow>
                    (
                        new VwMpOpSuppDataRow(calendarYear: year, quarter: quarter, fuelCd: "BFUEL", opTypeCd: "BAD", opValue: 2200m),
                        new VwMpOpSuppDataRow(calendarYear: year, quarter: quarter, fuelCd: null, opTypeCd: "BAD", opValue: 2200m)
                    );
                EmParameters.SystemOperatingSuppDataRecordsByLocation = new CheckDataView<SystemOpSuppData>();
                {
                    foreach (string addCd in UnitTestStandardLists.OpSuppDataTypeCodeList)
                    {
                        if (addCd == systemOpSuppDataTypeCd)
                        {
                            EmParameters.SystemOperatingSuppDataRecordsByLocation.Add
                                (
                                    new SystemOpSuppData(monSysId: monSysId, calendarYear: year, quarter: quarter, opSuppDataTypeCd: addCd, hours: 168)
                                );
                        }
                        else if (addCd != "OP")
                        {
                            EmParameters.SystemOperatingSuppDataRecordsByLocation.Add
                                (
                                    new SystemOpSuppData(monSysId: monSysId, calendarYear: year, quarter: quarter, opSuppDataTypeCd: addCd, hours: 2200)
                                );
                        }
                    }
                }


                /* Run Case */
                eSourceSupplementalData? sourceSupplementalData;
                int? opHourCount = cAppendixDEStatusChecks.GetOpHourCountTrySystemThenFuelThenLocation(year, quarter, out sourceSupplementalData);


                /* Check Results */
                Assert.AreEqual((systemOpSuppDataTypeCd == "OP" ? 168 : (int?)null), opHourCount, $"Check System Op Supp Data Codes (Count): {systemOpSuppDataTypeCd}");
                Assert.AreEqual((systemOpSuppDataTypeCd == "OP" ? eSourceSupplementalData.SystemOpSuppData : (eSourceSupplementalData?)null), sourceSupplementalData, $"Check System Op Supp Data Codes (Source): {systemOpSuppDataTypeCd}");
            }


            /* Check Fuel-Specific Op Supp Data Codes Cases */
            foreach (string opTypeCd in UnitTestStandardLists.OpTypeCodeList)
            {
                /* Initialize Parameters */
                EmParameters.CurrentFuelFlowRecord = new VwMpHrlyFuelFlowRow(fuelCd: "GFUEL");
                EmParameters.FuelFlowComponentRecordToCheck = new VwMpMonitorSystemComponentRow(monSysId: monSysId);
                EmParameters.OperatingSuppDataRecordsByLocation = new CheckDataView<VwMpOpSuppDataRow>();
                {
                    foreach (string addCd in UnitTestStandardLists.OpTypeCodeList)
                    {
                        if (addCd == opTypeCd)
                        {
                            EmParameters.OperatingSuppDataRecordsByLocation.Add
                                (
                                    new VwMpOpSuppDataRow(calendarYear: year, quarter: quarter, fuelCd: "GFUEL", opTypeCd: addCd, opValue: 168m)
                                );
                        }
                        else if (addCd != "OPHOURS")
                        {
                            EmParameters.OperatingSuppDataRecordsByLocation.Add
                                (
                                    new VwMpOpSuppDataRow(calendarYear: year, quarter: quarter, fuelCd: "GFUEL", opTypeCd: addCd, opValue: 2200m)
                                );
                        }
                    }
                }
                EmParameters.SystemOperatingSuppDataRecordsByLocation = new CheckDataView<SystemOpSuppData>
                    (
                        new SystemOpSuppData(monSysId: monSysId, calendarYear: year, quarter: quarter, opSuppDataTypeCd: "BAD", hours: 2200)
                    );


                /* Run Case */
                eSourceSupplementalData? sourceSupplementalData;
                int? opHourCount = cAppendixDEStatusChecks.GetOpHourCountTrySystemThenFuelThenLocation(year, quarter, out sourceSupplementalData);


                /* Check Results */
                Assert.AreEqual((opTypeCd == "OPHOURS" ? 168 : (int?)null), opHourCount, $"Check Fuel-Specific Op Supp Data Codes (Count): {opTypeCd}");
                Assert.AreEqual((opTypeCd == "OPHOURS" ? eSourceSupplementalData.FuelSpecificOpSuppData : (eSourceSupplementalData?)null), sourceSupplementalData, $"Check Fuel-Specific Op Supp Data Codes (Source): {opTypeCd}");
            }


            /* Check Location-Level Op Supp Data Codes Cases */
            foreach (string opTypeCd in UnitTestStandardLists.OpTypeCodeList)
            {
                /* Initialize Parameters */
                EmParameters.CurrentFuelFlowRecord = new VwMpHrlyFuelFlowRow(fuelCd: "GFUEL");
                EmParameters.FuelFlowComponentRecordToCheck = new VwMpMonitorSystemComponentRow(monSysId: monSysId);
                EmParameters.OperatingSuppDataRecordsByLocation = new CheckDataView<VwMpOpSuppDataRow>();
                {
                    foreach (string addCd in UnitTestStandardLists.OpTypeCodeList)
                    {
                        if (addCd == opTypeCd)
                        {
                            EmParameters.OperatingSuppDataRecordsByLocation.Add
                                (
                                    new VwMpOpSuppDataRow(calendarYear: year, quarter: quarter, fuelCd: null, opTypeCd: addCd, opValue: 168m)
                                );
                        }
                        else if (addCd != "OPHOURS")
                        {
                            EmParameters.OperatingSuppDataRecordsByLocation.Add
                                (
                                    new VwMpOpSuppDataRow(calendarYear: year, quarter: quarter, fuelCd: null, opTypeCd: addCd, opValue: 2200m)
                                );
                        }
                    }
                }
                EmParameters.SystemOperatingSuppDataRecordsByLocation = new CheckDataView<SystemOpSuppData>
                    (
                        new SystemOpSuppData(monSysId: monSysId, calendarYear: year, quarter: quarter, opSuppDataTypeCd: "BAD", hours: 2200)
                    );


                /* Run Case */
                eSourceSupplementalData? sourceSupplementalData;
                int? opHourCount = cAppendixDEStatusChecks.GetOpHourCountTrySystemThenFuelThenLocation(year, quarter, out sourceSupplementalData);


                /* Check Results */
                Assert.AreEqual((opTypeCd == "OPHOURS" ? 168 : (int?)null), opHourCount, $"Check Location-Level Op Supp Data Codes (Count): {opTypeCd}");
                Assert.AreEqual((opTypeCd == "OPHOURS" ? eSourceSupplementalData.LocationLevelOpSuppData : (eSourceSupplementalData?)null), sourceSupplementalData, $"Check Location-Level Op Supp Data Codes (Source): {opTypeCd}");
            }


            /* Check Combined Supplemental Data Sources Cases */
            short[] indicatorList = { 0, 1 };
            short[] threeLevelList = { -1, 0, 1 };

            foreach (short systemOpIndicator in indicatorList)
                foreach (short fuelSpecificIndicator in threeLevelList)
                    foreach (short locationLevelIndicator in indicatorList)
                    {
                        /* Initialize Parameters */
                        EmParameters.CurrentFuelFlowRecord = new VwMpHrlyFuelFlowRow(fuelCd: "GFUEL");
                        EmParameters.FuelFlowComponentRecordToCheck = new VwMpMonitorSystemComponentRow(monSysId: monSysId);
                        EmParameters.OperatingSuppDataRecordsByLocation = new CheckDataView<VwMpOpSuppDataRow>();
                        {
                            if (fuelSpecificIndicator != 0)
                            {
                                EmParameters.OperatingSuppDataRecordsByLocation.Add
                                    (
                                        new VwMpOpSuppDataRow(calendarYear: year, quarter: quarter, fuelCd: "GFUEL", opTypeCd: "OPHOURS", opValue: (fuelSpecificIndicator == 1) ? 169m : (decimal?)null)
                                    );
                            }

                            if (locationLevelIndicator != 0)
                            {
                                EmParameters.OperatingSuppDataRecordsByLocation.Add
                                    (
                                        new VwMpOpSuppDataRow(calendarYear: year, quarter: quarter, fuelCd: null, opTypeCd: "OPHOURS", opValue: (locationLevelIndicator == 1) ? 2197m : (decimal?)null)
                                    );
                            }
                        }
                        EmParameters.SystemOperatingSuppDataRecordsByLocation = new CheckDataView<SystemOpSuppData>();
                        {
                            if (systemOpIndicator == 1)
                            {
                                EmParameters.SystemOperatingSuppDataRecordsByLocation.Add
                                    (
                                        new SystemOpSuppData(monSysId: monSysId, calendarYear: year, quarter: quarter, opSuppDataTypeCd: "OP", hours: 13)
                                    );
                            }
                        }


                        /* Expected Results */
                        int? expOpHourCount;
                        eSourceSupplementalData? expSourceSuppData;
                        {
                            if (systemOpIndicator == 1)
                            {
                                expOpHourCount = 13;
                                expSourceSuppData = eSourceSupplementalData.SystemOpSuppData;
                            }
                            else if (fuelSpecificIndicator == 1)
                            {
                                expOpHourCount = 169;
                                expSourceSuppData = eSourceSupplementalData.FuelSpecificOpSuppData;
                            }
                            else if (locationLevelIndicator == 1)
                            {
                                expOpHourCount = 2197;
                                expSourceSuppData = eSourceSupplementalData.LocationLevelOpSuppData;
                            }
                            else
                            {
                                expOpHourCount = null;
                                expSourceSuppData = null;
                            }
                        }


                        /* Run Case */
                        eSourceSupplementalData? sourceSupplementalData;
                        int? opHourCount = cAppendixDEStatusChecks.GetOpHourCountTrySystemThenFuelThenLocation(year, quarter, out sourceSupplementalData);


                        /* Check Results */
                        Assert.AreEqual(expOpHourCount, opHourCount, $"Check Combined Supplemental Data Sources (Count): System {systemOpIndicator}, Fuel-Specific {fuelSpecificIndicator}, Location-Level {locationLevelIndicator}");
                        Assert.AreEqual(expSourceSuppData, sourceSupplementalData, $"Check Combined Supplemental Data Sources (Source): System {systemOpIndicator}, Fuel-Specific {fuelSpecificIndicator}, Location-Level {locationLevelIndicator}");
                    }
        }

    }
}
