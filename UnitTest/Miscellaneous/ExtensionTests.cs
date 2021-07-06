using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ECMPS.Definitions.Extensions;


namespace UnitTest.Miscellaneous
{
    [TestClass]
    public class ExtensionTests
    {

        /// <summary>
        /// cExtensions.MatsSignificantDigitsFormat
        /// </summary>
        [TestMethod]
        public void MatsSignificantDigitsFormat()
        {
            DateTime september8th2020 = new DateTime(2020, 9, 8);
            DateTime september9th2020 = new DateTime(2020, 9, 9);

            MatsSignificantDigitsFormatDo(null, september8th2020, "1.234E-1", null);
            MatsSignificantDigitsFormatDo(null, september9th2020, "1.234E-1", null);

            MatsSignificantDigitsFormatDo(0m, september8th2020, null, "0.00E0");
            MatsSignificantDigitsFormatDo(0m, september8th2020, "1.2", "0.00E0");
            MatsSignificantDigitsFormatDo(0m, september8th2020, "1.2E-1", "0.00E0");
            MatsSignificantDigitsFormatDo(0m, september8th2020, "1.23E-1", "0.00E0");
            MatsSignificantDigitsFormatDo(0m, september8th2020, "1.234E-1", "0.00E0");
            MatsSignificantDigitsFormatDo(0m, september9th2020, null, "0.00E0");
            MatsSignificantDigitsFormatDo(0m, september9th2020, "1.23", "0.00E0");
            MatsSignificantDigitsFormatDo(0m, september9th2020, "1.2E-1", "0.0E0");
            MatsSignificantDigitsFormatDo(0m, september9th2020, "1.23E-1", "0.00E0");
            MatsSignificantDigitsFormatDo(0m, september9th2020, "1.234E-1", "0.00E0");

            MatsSignificantDigitsFormatDo(0.004321m, september8th2020, null, "4.32E-3");
            MatsSignificantDigitsFormatDo(0.004321m, september8th2020, "1.2", "4.32E-3");
            MatsSignificantDigitsFormatDo(0.004321m, september8th2020, "1.2E-1", "4.32E-3");
            MatsSignificantDigitsFormatDo(0.004321m, september8th2020, "1.23E-1", "4.32E-3");
            MatsSignificantDigitsFormatDo(0.004321m, september8th2020, "1.234E-1", "4.32E-3");
            MatsSignificantDigitsFormatDo(0.004321m, september9th2020, null, "4.32E-3");
            MatsSignificantDigitsFormatDo(0.004321m, september9th2020, "1.2", "4.32E-3");
            MatsSignificantDigitsFormatDo(0.004321m, september9th2020, "1.2E-1", "4.3E-3");
            MatsSignificantDigitsFormatDo(0.004321m, september9th2020, "1.23E-1", "4.32E-3");
            MatsSignificantDigitsFormatDo(0.004321m, september9th2020, "1.234E-1", "4.32E-3");
        }

        /// <summary>
        /// Formats the assertion for MatsSignificantDigitsValid.
        /// </summary>
        /// <param name="decimalValue">The decimal value to convert.</param>
        /// <param name="recordDate">The associated date of the scientific notation.</param>
        /// <param name="model">The model scientific notation that will determine the significant digits for recordDate on or after 9/9/2020.</param>
        /// <param name="expected">The expected values</param>
        private void MatsSignificantDigitsFormatDo(decimal? decimalValue, DateTime recordDate, string model, string expected)
        {
            Assert.AreEqual(expected, cExtensions.MatsSignificantDigitsFormat(decimalValue, recordDate, model), $"[ value:{decimalValue}, date: {recordDate}, model: {model} ]");
        }



        /// <summary>
        /// cExtensions.MatsSignificantDigitsValid
        /// 
        /// Checks whether the method correctly determines whether string values 
        /// represent scientific notation with the correct number of significant 
        /// digits based on the date associated with the values.
        /// </summary>
        [TestMethod]
        public void MatsSignificantDigitsValid()
        {
            {
                DateTime september9th2020 = new DateTime(2020, 9, 9);

                MatsSignificantDigitsValidDo(false, "0", september9th2020);
                MatsSignificantDigitsValidDo(false, "0.0", september9th2020);
                MatsSignificantDigitsValidDo(false, "0E0", september9th2020);
                MatsSignificantDigitsValidDo(false, "0.00E00", september9th2020);
                MatsSignificantDigitsValidDo(false, "0.00 E0", september9th2020);
                MatsSignificantDigitsValidDo(false, "0.000E0", september9th2020);
                MatsSignificantDigitsValidDo(false, "1.234E-5", september9th2020);
                MatsSignificantDigitsValidDo(false, "0.99E0", september9th2020);
                MatsSignificantDigitsValidDo(false, "9.99E-0", september9th2020);
                MatsSignificantDigitsValidDo(false, "10.0E0", september9th2020);

                MatsSignificantDigitsValidDo(true, "-1.23E1", september9th2020);
                MatsSignificantDigitsValidDo(true, "0.0E0", september9th2020);
                MatsSignificantDigitsValidDo(true, "0.00E0", september9th2020);
                MatsSignificantDigitsValidDo(true, "1.2E-3", september9th2020);
                MatsSignificantDigitsValidDo(true, "1.23E-4", september9th2020);
                MatsSignificantDigitsValidDo(true, "1.01E-1324343", september9th2020);
                MatsSignificantDigitsValidDo(true, "9.99E0", september9th2020);
            }

            {
                DateTime september8th2020 = new DateTime(2020, 9, 8);

                MatsSignificantDigitsValidDo(false, "0", september8th2020);
                MatsSignificantDigitsValidDo(false, "0.0", september8th2020);
                MatsSignificantDigitsValidDo(false, "0E0", september8th2020);
                MatsSignificantDigitsValidDo(false, "0.0E0", september8th2020);
                MatsSignificantDigitsValidDo(false, "0.00E00", september8th2020);
                MatsSignificantDigitsValidDo(false, "0.00 E0", september8th2020);
                MatsSignificantDigitsValidDo(false, "0.000E0", september8th2020);
                MatsSignificantDigitsValidDo(false, "1.234E-5", september8th2020);
                MatsSignificantDigitsValidDo(false, "1.2E-3", september8th2020);
                MatsSignificantDigitsValidDo(false, "0.99E0", september8th2020);
                MatsSignificantDigitsValidDo(false, "9.99E-0", september8th2020);
                MatsSignificantDigitsValidDo(false, "10.0E0", september8th2020);

                MatsSignificantDigitsValidDo(true, "-1.23E1", september8th2020);
                MatsSignificantDigitsValidDo(true, "0.00E0", september8th2020);
                MatsSignificantDigitsValidDo(true, "1.23E-4", september8th2020);
                MatsSignificantDigitsValidDo(true, "1.01E-1324343", september8th2020);
                MatsSignificantDigitsValidDo(true, "9.99E0", september8th2020);
            }
        }

        /// <summary>
        /// Formats the assertion for MatsSignificantDigitsValid.
        /// </summary>
        /// <param name="expected">The expected values</param>
        /// <param name="scientificNotation">The scientific notation being checks.</param>
        /// <param name="recordDate">The associated date of the scientific notation.</param>
        private void MatsSignificantDigitsValidDo(bool expected, string scientificNotation, DateTime recordDate)
        {
            Assert.AreEqual(expected, cExtensions.MatsSignificantDigitsValid(scientificNotation, recordDate), $"[ {scientificNotation} ]");
        }



    }
}
