using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ECMPS.Checks.TypeUtilities;

namespace UnitTest.DateFunctions
{
    [TestClass]
    public class DateFunctionTest
    {

        /// <summary>
        /// 
        /// All dates in 2020-06.
        /// 
        /// Initial Began   : 2020-06-01 12
        /// Initial Ended   : 2020-06-30 12
        /// 
        /// 
        /// | ## | Began1 | Ended1 | Began2 | Ended2 || Began0 | Ended0 | Began1 | Ended1 | Began2 | Ended2 | Began3 | Ended3 | Began4 | Ended4 || Notes
        /// |  0 | 17 08  | 17 20  | 17 22  | 17 22  || 01 12  | 17 07  | 17 08  | 17 20  | 17 21  | 17 21  | 17 22  | 17 22  | 17 23  | 30 12  || Second range after with gap and ends after first range with gap.
        /// |  1 | 17 08  | 17 20  | 17 21  | 17 22  || 01 12  | 17 07  | 17 08  | 17 20  | 17 21  | 17 22  | 17 23  | 30 12  |   -    |   -    || Second range after without gap and ends after first range with gap.
        /// |  2 | 17 08  | 17 20  | 17 20  | 17 22  || 01 12  | 17 07  | 17 08  | 17 19  | 17 20  | 17 20  | 17 21  | 17 22  | 17 23  | 30 12  || Second range begins on first's end and ends after first range with gap.
        /// |  3 | 17 08  | 17 20  | 17 19  | 17 22  || 01 12  | 17 07  | 17 08  | 17 18  | 17 19  | 17 20  | 17 21  | 17 22  | 17 23  | 30 12  || Second range begins hour before first ends and ends after first range with gap.
        /// |  4 | 17 08  | 17 20  | 17 18  | 17 22  || 01 12  | 17 07  | 17 08  | 17 17  | 17 18  | 17 20  | 17 21  | 17 22  | 17 23  | 30 12  || Second range begins before first ends with gap and ends after first range with gap.
        /// |  5 | 17 08  | 17 20  | 17 10  | 17 22  || 01 12  | 17 07  | 17 08  | 17 09  | 17 10  | 17 20  | 17 21  | 17 22  | 17 23  | 30 12  || Second range begins after first ends with gap and ends after first range with gap.
        /// |  6 | 17 08  | 17 20  | 17 09  | 17 22  || 01 12  | 17 07  | 17 08  | 17 08  | 17 09  | 17 20  | 17 21  | 17 22  | 17 23  | 30 12  || Second range begins hour after first begins with gap and ends after first range with gap.
        /// |  7 | 17 08  | 17 20  | 17 08  | 17 22  || 01 12  | 17 07  | 17 08  | 17 20  | 17 21  | 17 22  | 17 23  | 30 12  |   -    |   -    || Second range begins on first's begin and ends after first range with gap.
        /// |  8 | 17 08  | 17 20  | 17 07  | 17 22  || 01 12  | 17 06  | 17 07  | 17 07  | 17 08  | 17 20  | 17 21  | 17 22  | 17 23  | 30 12  || Second range begins hour before first begins and ends after first range with gap.
        /// |  9 | 17 08  | 17 20  | 17 06  | 17 22  || 01 12  | 17 05  | 17 06  | 17 07  | 17 08  | 17 20  | 17 21  | 17 22  | 17 23  | 30 12  || Second range begins hour before first begins and ends after first range with gap.
        /// | 10 | 17 08  | 17 20  | 17 21  | 17 21  || 01 12  | 17 07  | 17 08  | 17 20  | 17 21  | 17 21  | 17 22  | 30 12  |   -    |   -    || Second range after without gap and ends after first range without gap.
        /// | 11 | 17 08  | 17 20  | 17 20  | 17 21  || 01 12  | 17 07  | 17 08  | 17 19  | 17 20  | 17 20  | 17 21  | 17 21  | 17 22  | 30 12  || Second range begins on first's end and ends after first range without gap.
        /// | 12 | 17 08  | 17 20  | 17 19  | 17 21  || 01 12  | 17 07  | 17 08  | 17 18  | 17 19  | 17 20  | 17 21  | 17 21  | 17 22  | 30 12  || Second range begins hour before first ends and ends after first range without gap.
        /// | 13 | 17 08  | 17 20  | 17 18  | 17 21  || 01 12  | 17 07  | 17 08  | 17 17  | 17 18  | 17 20  | 17 21  | 17 21  | 17 22  | 30 12  || Second range begins before first ends with gap and ends after first range without gap.
        /// | 14 | 17 08  | 17 20  | 17 10  | 17 21  || 01 12  | 17 07  | 17 08  | 17 09  | 17 10  | 17 20  | 17 21  | 17 21  | 17 22  | 30 12  || Second range begins after first ends with gap and ends after first range without gap.
        /// | 15 | 17 08  | 17 20  | 17 09  | 17 21  || 01 12  | 17 07  | 17 08  | 17 08  | 17 09  | 17 20  | 17 21  | 17 21  | 17 22  | 30 12  || Second range begins hour after first begins with gap and ends after first range without gap.
        /// | 16 | 17 08  | 17 20  | 17 08  | 17 21  || 01 12  | 17 07  | 17 08  | 17 20  | 17 21  | 17 21  | 17 22  | 30 12  |   -    |   -    || Second range begins on first's begin and ends after first range without gap.
        /// | 17 | 17 08  | 17 20  | 17 07  | 17 21  || 01 12  | 17 06  | 17 07  | 17 07  | 17 08  | 17 20  | 17 21  | 17 21  | 17 22  | 30 12  || Second range begins hour before first begins and ends after first range without gap.
        /// | 18 | 17 08  | 17 20  | 17 06  | 17 21  || 01 12  | 17 05  | 17 06  | 17 07  | 17 08  | 17 20  | 17 21  | 17 21  | 17 22  | 30 12  || Second range begins hour before first begins and ends after first range without gap.
        /// | 19 | 17 08  | 17 20  | 17 20  | 17 20  || 01 12  | 17 07  | 17 08  | 17 19  | 17 20  | 17 20  | 17 21  | 30 12  |   -    |   -    || Second range begins on first's end and ends on the first range's end.
        /// | 20 | 17 08  | 17 20  | 17 19  | 17 20  || 01 12  | 17 07  | 17 08  | 17 18  | 17 19  | 17 20  | 17 21  | 30 12  |   -    |   -    || Second range begins hour before first ends and ends on the first range's end.
        /// | 21 | 17 08  | 17 20  | 17 18  | 17 20  || 01 12  | 17 07  | 17 08  | 17 17  | 17 18  | 17 20  | 17 21  | 30 12  |   -    |   -    || Second range begins before first ends with gap and ends on the first range's end.
        /// | 22 | 17 08  | 17 20  | 17 10  | 17 20  || 01 12  | 17 07  | 17 08  | 17 09  | 17 10  | 17 20  | 17 21  | 30 12  |   -    |   -    || Second range begins after first ends with gap and ends on the first range's end.
        /// | 23 | 17 08  | 17 20  | 17 09  | 17 20  || 01 12  | 17 07  | 17 08  | 17 08  | 17 09  | 17 20  | 17 21  | 30 12  |   -    |   -    || Second range begins hour after first begins with gap and ends on the first range's end.
        /// | 24 | 17 08  | 17 20  | 17 08  | 17 20  || 01 12  | 17 07  | 17 08  | 17 20  | 17 21  | 30 12  |   -    |   -    |   -    |   -    || Second range begins on first's begin and ends on the first range's end.
        /// | 25 | 17 08  | 17 20  | 17 07  | 17 20  || 01 12  | 17 06  | 17 07  | 17 07  | 17 08  | 17 20  | 17 21  | 30 12  |   -    |   -    || Second range begins hour before first begins and ends on the first range's end.
        /// | 26 | 17 08  | 17 20  | 17 06  | 17 20  || 01 12  | 17 05  | 17 06  | 17 07  | 17 08  | 17 20  | 17 21  | 30 12  |   -    |   -    || Second range begins hour before first begins and ends on the first range's end.
        /// | 27 | 17 08  | 17 20  | 17 19  | 17 19  || 01 12  | 17 07  | 17 08  | 17 18  | 17 19  | 17 19  | 17 20  | 17 20  | 17 21  | 30 12  || Second range begins hour before first ends and ends on the hour before the first range ends.
        /// | 28 | 17 08  | 17 20  | 17 18  | 17 19  || 01 12  | 17 07  | 17 08  | 17 17  | 17 18  | 17 19  | 17 20  | 17 20  | 17 21  | 30 12  || Second range begins before first ends with gap and ends on the hour before the first range ends.
        /// | 29 | 17 08  | 17 20  | 17 10  | 17 19  || 01 12  | 17 07  | 17 08  | 17 09  | 17 10  | 17 19  | 17 20  | 17 20  | 17 21  | 30 12  || Second range begins after first ends with gap and ends on the hour before the first range ends.
        /// | 30 | 17 08  | 17 20  | 17 09  | 17 19  || 01 12  | 17 07  | 17 08  | 17 08  | 17 09  | 17 19  | 17 20  | 17 20  | 17 21  | 30 12  || Second range begins hour after first begins with gap and ends on the hour before the first range ends.
        /// | 31 | 17 08  | 17 20  | 17 08  | 17 19  || 01 12  | 17 07  | 17 08  | 17 19  | 17 20  | 17 20  | 17 21  | 30 12  |   -    |   -    || Second range begins on first's begin and ends on the hour before the first range ends.
        /// | 32 | 17 08  | 17 20  | 17 07  | 17 19  || 01 12  | 17 06  | 17 07  | 17 07  | 17 08  | 17 19  | 17 20  | 17 20  | 17 21  | 30 12  || Second range begins hour before first begins and ends on the hour before the first range ends.
        /// | 33 | 17 08  | 17 20  | 17 06  | 17 19  || 01 12  | 17 05  | 17 06  | 17 07  | 17 08  | 17 19  | 17 20  | 17 20  | 17 21  | 30 12  || Second range begins hour before first begins and ends on the hour before the first range ends.
        /// | 34 | 17 08  | 17 20  | 17 18  | 17 18  || 01 12  | 17 07  | 17 08  | 17 17  | 17 18  | 17 18  | 17 19  | 17 20  | 17 21  | 30 12  || Second range begins before first ends with gap and ends before the first range ends with gap.
        /// | 35 | 17 08  | 17 20  | 17 10  | 17 18  || 01 12  | 17 07  | 17 08  | 17 09  | 17 10  | 17 18  | 17 19  | 17 20  | 17 21  | 30 12  || Second range begins after first ends with gap and ends before the first range ends with gap.
        /// | 36 | 17 08  | 17 20  | 17 09  | 17 18  || 01 12  | 17 07  | 17 08  | 17 08  | 17 09  | 17 18  | 17 19  | 17 20  | 17 21  | 30 12  || Second range begins hour after first begins with gap and ends before the first range ends with gap.
        /// | 37 | 17 08  | 17 20  | 17 08  | 17 18  || 01 12  | 17 07  | 17 08  | 17 18  | 17 19  | 17 20  | 17 21  | 30 12  |   -    |   -    || Second range begins on first's begin and ends before the first range ends with gap.
        /// | 38 | 17 08  | 17 20  | 17 07  | 17 18  || 01 12  | 17 06  | 17 07  | 17 07  | 17 08  | 17 18  | 17 19  | 17 20  | 17 21  | 30 12  || Second range begins hour before first begins and ends before the first range ends with gap.
        /// | 39 | 17 08  | 17 20  | 17 06  | 17 18  || 01 12  | 17 05  | 17 06  | 17 07  | 17 08  | 17 18  | 17 19  | 17 20  | 17 21  | 30 12  || Second range begins hour before first begins and ends before the first range ends with gap.
        /// | 40 | 17 08  | 17 20  | 17 10  | 17 10  || 01 12  | 17 07  | 17 08  | 17 09  | 17 10  | 17 10  | 17 11  | 17 20  | 17 21  | 30 12  || Second range begins after first ends with gap and ends after the first range begins with gap.
        /// | 41 | 17 08  | 17 20  | 17 09  | 17 10  || 01 12  | 17 07  | 17 08  | 17 08  | 17 09  | 17 10  | 17 11  | 17 20  | 17 21  | 30 12  || Second range begins hour after first begins with gap and ends after the first range begins with gap.
        /// | 42 | 17 08  | 17 20  | 17 08  | 17 10  || 01 12  | 17 07  | 17 08  | 17 10  | 17 11  | 17 20  | 17 21  | 30 12  |   -    |   -    || Second range begins on first's begin and ends after the first range begins with gap.
        /// | 43 | 17 08  | 17 20  | 17 07  | 17 10  || 01 12  | 17 06  | 17 07  | 17 07  | 17 08  | 17 10  | 17 11  | 17 20  | 17 21  | 30 12  || Second range begins hour before first begins and ends after the first range begins with gap.
        /// | 44 | 17 08  | 17 20  | 17 06  | 17 10  || 01 12  | 17 05  | 17 06  | 17 07  | 17 08  | 17 10  | 17 11  | 17 20  | 17 21  | 30 12  || Second range begins hour before first begins and ends after the first range begins with gap.
        /// | 45 | 17 08  | 17 20  | 17 09  | 17 09  || 01 12  | 17 07  | 17 08  | 17 08  | 17 09  | 17 09  | 17 10  | 17 20  | 17 21  | 30 12  || Second range begins hour after first begins with gap and ends after the first range begins with no gap.
        /// | 46 | 17 08  | 17 20  | 17 08  | 17 09  || 01 12  | 17 07  | 17 08  | 17 09  | 17 10  | 17 20  | 17 21  | 30 12  |   -    |   -    || Second range begins on first's begin and ends after the first range begins with no gap.
        /// | 47 | 17 08  | 17 20  | 17 07  | 17 09  || 01 12  | 17 06  | 17 07  | 17 07  | 17 08  | 17 09  | 17 10  | 17 20  | 17 21  | 30 12  || Second range begins hour before first begins and ends after the first range begins with no gap.
        /// | 48 | 17 08  | 17 20  | 17 06  | 17 09  || 01 12  | 17 05  | 17 06  | 17 07  | 17 08  | 17 09  | 17 10  | 17 20  | 17 21  | 30 12  || Second range begins hour before first begins and ends after the first range begins with no gap.
        /// | 49 | 17 08  | 17 20  | 17 08  | 17 08  || 01 12  | 17 07  | 17 08  | 17 08  | 17 09  | 17 20  | 17 21  | 30 12  |   -    |   -    || Second range begins on first's begin and ends on first range's begin.
        /// | 50 | 17 08  | 17 20  | 17 07  | 17 08  || 01 12  | 17 06  | 17 07  | 17 07  | 17 08  | 17 08  | 17 09  | 17 20  | 17 21  | 30 12  || Second range begins hour before first begins and ends on first range's begin.
        /// | 51 | 17 08  | 17 20  | 17 06  | 17 08  || 01 12  | 17 05  | 17 06  | 17 07  | 17 08  | 17 08  | 17 09  | 17 20  | 17 21  | 30 12  || Second range begins hour before first begins and ends on first range's begin.
        /// | 52 | 17 08  | 17 20  | 17 07  | 17 07  || 01 12  | 17 06  | 17 07  | 17 07  | 17 08  | 17 20  | 17 21  | 30 12  |   -    |   -    || Second range begins hour before first begins and ends before first range begins.
        /// | 53 | 17 08  | 17 20  | 17 06  | 17 07  || 01 12  | 17 05  | 17 06  | 17 07  | 17 08  | 17 20  | 17 21  | 30 12  |   -    |   -    || Second range begins hour before first begins and ends hour before first range begins.
        /// | 54 | 17 08  | 17 20  | 17 06  | 17 06  || 01 12  | 17 05  | 17 06  | 17 06  | 17 07  | 17 07  | 17 08  | 17 20  | 17 21  | 30 12  || Second range begins hour before first begins and ends before first range begins with gap.
        /// 
        /// </summary>
        [TestMethod]
        public void DistinctHourRangesWithCases()
        {
            /* Test Case Count */
            const int caseCount = 55;


            /* Helper Values */
            DateTime initialBegan = new DateTime(2020, 6, 1, 12, 0, 0);
            DateTime initialEnded = new DateTime(2020, 6, 30, 12, 0, 0);

            DateTime h0112 = new DateTime(2020, 6, 1, 12, 0, 0);
            DateTime h1705 = new DateTime(2020, 6, 17, 5, 0, 0);
            DateTime h1706 = new DateTime(2020, 6, 17, 6, 0, 0);
            DateTime h1707 = new DateTime(2020, 6, 17, 7, 0, 0);
            DateTime h1708 = new DateTime(2020, 6, 17, 8, 0, 0);
            DateTime h1709 = new DateTime(2020, 6, 17, 9, 0, 0);
            DateTime h1710 = new DateTime(2020, 6, 17, 10, 0, 0);
            DateTime h1711 = new DateTime(2020, 6, 17, 11, 0, 0);
            DateTime h1717 = new DateTime(2020, 6, 17, 17, 0, 0);
            DateTime h1718 = new DateTime(2020, 6, 17, 18, 0, 0);
            DateTime h1719 = new DateTime(2020, 6, 17, 19, 0, 0);
            DateTime h1720 = new DateTime(2020, 6, 17, 20, 0, 0);
            DateTime h1721 = new DateTime(2020, 6, 17, 21, 0, 0);
            DateTime h1722 = new DateTime(2020, 6, 17, 22, 0, 0);
            DateTime h1723 = new DateTime(2020, 6, 17, 23, 0, 0);
            DateTime h3012 = new DateTime(2020, 6, 30, 12, 0, 0);

            /* Input Values */
            DateTime?[] began1List = {
                                        h1708, h1708, h1708, h1708, h1708, h1708, h1708, h1708, h1708, h1708,
                                        h1708, h1708, h1708, h1708, h1708, h1708, h1708, h1708, h1708, h1708,
                                        h1708, h1708, h1708, h1708, h1708, h1708, h1708, h1708, h1708, h1708,
                                        h1708, h1708, h1708, h1708, h1708, h1708, h1708, h1708, h1708, h1708,
                                        h1708, h1708, h1708, h1708, h1708, h1708, h1708, h1708, h1708, h1708,
                                        h1708, h1708, h1708, h1708, h1708
                                     };
            DateTime?[] ended1List = {
                                        h1720, h1720, h1720, h1720, h1720, h1720, h1720, h1720, h1720, h1720,
                                        h1720, h1720, h1720, h1720, h1720, h1720, h1720, h1720, h1720, h1720,
                                        h1720, h1720, h1720, h1720, h1720, h1720, h1720, h1720, h1720, h1720,
                                        h1720, h1720, h1720, h1720, h1720, h1720, h1720, h1720, h1720, h1720,
                                        h1720, h1720, h1720, h1720, h1720, h1720, h1720, h1720, h1720, h1720,
                                        h1720, h1720, h1720, h1720, h1720
                                     };
            DateTime?[] began2List = {
                                        h1722, h1721, h1720, h1719, h1718, h1710, h1709, h1708, h1707, h1706,
                                        h1721, h1720, h1719, h1718, h1710, h1709, h1708, h1707, h1706, h1720,
                                        h1719, h1718, h1710, h1709, h1708, h1707, h1706, h1719, h1718, h1710,
                                        h1709, h1708, h1707, h1706, h1718, h1710, h1709, h1708, h1707, h1706,
                                        h1710, h1709, h1708, h1707, h1706, h1709, h1708, h1707, h1706, h1708,
                                        h1707, h1706, h1707, h1706, h1706
                                     };
            DateTime?[] ended2List = {
                                        h1722, h1722, h1722, h1722, h1722, h1722, h1722, h1722, h1722, h1722,
                                        h1721, h1721, h1721, h1721, h1721, h1721, h1721, h1721, h1721, h1720,
                                        h1720, h1720, h1720, h1720, h1720, h1720, h1720, h1719, h1719, h1719,
                                        h1719, h1719, h1719, h1719, h1718, h1718, h1718, h1718, h1718, h1718,
                                        h1710, h1710, h1710, h1710, h1710, h1709, h1709, h1709, h1709, h1708,
                                        h1708, h1708, h1707, h1707, h1706
                                     };

            /* Expected Values */
            DateTime?[,] expBeganLists = new DateTime?[5, caseCount]
            {
                {
                    h0112, h0112, h0112, h0112, h0112, h0112, h0112, h0112, h0112, h0112,
                    h0112, h0112, h0112, h0112, h0112, h0112, h0112, h0112, h0112, h0112,
                    h0112, h0112, h0112, h0112, h0112, h0112, h0112, h0112, h0112, h0112,
                    h0112, h0112, h0112, h0112, h0112, h0112, h0112, h0112, h0112, h0112,
                    h0112, h0112, h0112, h0112, h0112, h0112, h0112, h0112, h0112, h0112,
                    h0112, h0112, h0112, h0112, h0112
                },
                {
                    h1708, h1708, h1708, h1708, h1708, h1708, h1708, h1708, h1707, h1706,
                    h1708, h1708, h1708, h1708, h1708, h1708, h1708, h1707, h1706, h1708,
                    h1708, h1708, h1708, h1708, h1708, h1707, h1706, h1708, h1708, h1708,
                    h1708, h1708, h1707, h1706, h1708, h1708, h1708, h1708, h1707, h1706,
                    h1708, h1708, h1708, h1707, h1706, h1708, h1708, h1707, h1706, h1708,
                    h1707, h1706, h1707, h1706, h1706
                },
                {
                    h1721, h1721, h1720, h1719, h1718, h1710, h1709, h1721, h1708, h1708,
                    h1721, h1720, h1719, h1718, h1710, h1709, h1721, h1708, h1708, h1720,
                    h1719, h1718, h1710, h1709, h1721, h1708, h1708, h1719, h1718, h1710,
                    h1709, h1720, h1708, h1708, h1718, h1710, h1709, h1719, h1708, h1708,
                    h1710, h1709, h1711, h1708, h1708, h1709, h1710, h1708, h1708, h1709,
                    h1708, h1708, h1708, h1708, h1707
                },
                {
                    h1722, h1723, h1721, h1721, h1721, h1721, h1721, h1723, h1721, h1721,
                    h1722, h1721, h1721, h1721, h1721, h1721, h1722, h1721, h1721, h1721,
                    h1721, h1721, h1721, h1721, null, h1721, h1721, h1720, h1720, h1720,
                    h1720, h1721, h1720, h1720, h1719, h1719, h1719, h1721, h1719, h1719,
                    h1711, h1711, h1721, h1711, h1711, h1710, h1721, h1710, h1710, h1721,
                    h1709, h1709, h1721, h1721, h1708
                },
                {
                    h1723, null, h1723, h1723, h1723, h1723, h1723, null, h1723, h1723,
                    null, h1722, h1722, h1722, h1722, h1722, null, h1722, h1722, null,
                    null, null, null, null, null, null, null, h1721, h1721, h1721,
                    h1721, null, h1721, h1721, h1721, h1721, h1721, null, h1721, h1721,
                    h1721, h1721, null, h1721, h1721, h1721, null, h1721, h1721, null,
                    h1721, h1721, null, null, h1721
                }
            };
            DateTime?[,] expEndedLists = new DateTime?[5, caseCount]
            {
                {
                    h1707, h1707, h1707, h1707, h1707, h1707, h1707, h1707, h1706, h1705,
                    h1707, h1707, h1707, h1707, h1707, h1707, h1707, h1706, h1705, h1707,
                    h1707, h1707, h1707, h1707, h1707, h1706, h1705, h1707, h1707, h1707,
                    h1707, h1707, h1706, h1705, h1707, h1707, h1707, h1707, h1706, h1705,
                    h1707, h1707, h1707, h1706, h1705, h1707, h1707, h1706, h1705, h1707,
                    h1706, h1705, h1706, h1705, h1705
                },
                {
                    h1720, h1720, h1719, h1718, h1717, h1709, h1708, h1720, h1707, h1707,
                    h1720, h1719, h1718, h1717, h1709, h1708, h1720, h1707, h1707, h1719,
                    h1718, h1717, h1709, h1708, h1720, h1707, h1707, h1718, h1717, h1709,
                    h1708, h1719, h1707, h1707, h1717, h1709, h1708, h1718, h1707, h1707,
                    h1709, h1708, h1710, h1707, h1707, h1708, h1709, h1707, h1707, h1708,
                    h1707, h1707, h1707, h1707, h1706
                },
                {
                    h1721, h1722, h1720, h1720, h1720, h1720, h1720, h1722, h1720, h1720,
                    h1721, h1720, h1720, h1720, h1720, h1720, h1721, h1720, h1720, h1720,
                    h1720, h1720, h1720, h1720, h3012, h1720, h1720, h1719, h1719, h1719,
                    h1719, h1720, h1719, h1719, h1718, h1718, h1718, h1720, h1718, h1718,
                    h1710, h1710, h1720, h1710, h1710, h1709, h1720, h1709, h1709, h1720,
                    h1708, h1708, h1720, h1720, h1707
                },
                {
                    h1722, h3012, h1722, h1722, h1722, h1722, h1722, h3012, h1722, h1722,
                    h3012, h1721, h1721, h1721, h1721, h1721, h3012, h1721, h1721, h3012,
                    h3012, h3012, h3012, h3012, null, h3012, h3012, h1720, h1720, h1720,
                    h1720, h3012, h1720, h1720, h1720, h1720, h1720, h3012, h1720, h1720,
                    h1720, h1720, h3012, h1720, h1720, h1720, h3012, h1720, h1720, h3012,
                    h1720, h1720, h3012, h3012, h1720
                },
                {
                    h3012, null, h3012, h3012, h3012, h3012, h3012, null, h3012, h3012,
                    null, h3012, h3012, h3012, h3012, h3012, null, h3012, h3012, null,
                    null, null, null, null, null, null, null, h3012, h3012, h3012,
                    h3012, null, h3012, h3012, h3012, h3012, h3012, null, h3012, h3012,
                    h3012, h3012, null, h3012, h3012, h3012, null, h3012, h3012, null,
                    h3012, h3012, null, null, h3012
                }
            };


            /* Check array lengths */
            Assert.AreEqual(caseCount, began1List.Length, "began1List length");
            Assert.AreEqual(caseCount, ended1List.Length, "ended1List length");
            Assert.AreEqual(caseCount, began2List.Length, "began2List length");
            Assert.AreEqual(caseCount, began2List.Length, "began2List length");


            /* Case Variables */
            DistinctHourRanges distinctHourRanges;
            int rangeCount;

            /* Run Cases */
            for (int caseDex = 0; caseDex < caseCount; caseDex++)
            {
                // Create ranges object and update with new ranges.
                distinctHourRanges = new DistinctHourRanges(initialBegan, initialEnded);
                distinctHourRanges.Add(began1List[caseDex], ended1List[caseDex]);
                distinctHourRanges.Add(began2List[caseDex], ended2List[caseDex]);

                // Determine number of rxpected ranges for the case.
                rangeCount = 0;
                rangeCount += (expBeganLists[0, caseDex] != null) ? 1 : 0;
                rangeCount += (expBeganLists[1, caseDex] != null) ? 1 : 0;
                rangeCount += (expBeganLists[2, caseDex] != null) ? 1 : 0;
                rangeCount += (expBeganLists[3, caseDex] != null) ? 1 : 0;
                rangeCount += (expBeganLists[4, caseDex] != null) ? 1 : 0;

                // Check range count
                Assert.AreEqual(rangeCount, distinctHourRanges.RangeCount, $"Case {caseDex}: Range Count");

                // Check range bounds
                for (int rangeDex = 0; rangeDex < rangeCount; rangeDex++)
                {
                    Assert.AreEqual(expBeganLists[rangeDex, caseDex], distinctHourRanges[rangeDex].Began, $"Case {caseDex}, Range {rangeDex}: Began");
                    Assert.AreEqual(expEndedLists[rangeDex, caseDex], distinctHourRanges[rangeDex].Ended, $"Case {caseDex}, Range {rangeDex}: Ended");
                }
            }
        }


        [TestMethod]
        public void Earliest()
        {
            DateTime?[] list = new DateTime?[] { new DateTime(2016, 6, 18), null, new DateTime(2017, 7, 31), new DateTime(2016, 6, 17) };
            DateTime? result;

            /* Test for earliest where null is not deemed the earliest and null exists. */
            list = new DateTime?[] { new DateTime(2016, 6, 18), null, new DateTime(2017, 7, 31), new DateTime(2016, 6, 17) };
            result = cDateFunctions.Earliest(false, list);
            Assert.AreEqual(new DateTime(2016, 6, 17), result, "Test for earliest where null is not deemed the earliest and null exists.");

            /* Test for earliest where null is deemed the earliest and null exists. */
            list = new DateTime?[] { new DateTime(2016, 6, 18), null, new DateTime(2017, 7, 31), new DateTime(2016, 6, 17) };
            result = cDateFunctions.Earliest(true, list);
            Assert.AreEqual(null, result, "Test for earliest where null is deemed the earliest and null exists.");

            /* Test for earliest where null is not deemed the earliest and does not null exists. */
            list = new DateTime?[] { new DateTime(2016, 6, 18), new DateTime(2017, 7, 31), new DateTime(2016, 6, 17) };
            result = cDateFunctions.Earliest(false, list);
            Assert.AreEqual(new DateTime(2016, 6, 17), result, "Test for earliest where null is not deemed the earliest and does not null exists.");

            /* Test for earliest where null is deemed the earliest and nulld oes not exists. */
            list = new DateTime?[] { new DateTime(2016, 6, 18), new DateTime(2017, 7, 31), new DateTime(2016, 6, 17) };
            result = cDateFunctions.Earliest(true, list);
            Assert.AreEqual(new DateTime(2016, 6, 17), result, "Test for earliest where null is deemed the earliest and nulld oes not exists.");

            /* Test for earliest where nulls are ignored and a null exists. */
            list = new DateTime?[] { new DateTime(2016, 6, 18), null, new DateTime(2017, 7, 31), new DateTime(2016, 6, 17) };
            result = cDateFunctions.Earliest(list);
            Assert.AreEqual(new DateTime(2016, 6, 17), result, "Test for earliest where null is not deemed the earliest and null exists.");

            /* Test for earliest where nulls are ignored and a null does not exists. */
            list = new DateTime?[] { new DateTime(2016, 6, 18), new DateTime(2017, 7, 31), new DateTime(2016, 6, 17) };
            result = cDateFunctions.Earliest(list);
            Assert.AreEqual(new DateTime(2016, 6, 17), result, "Test for earliest where null is not deemed the earliest and does not null exists.");
        }

        [TestMethod]
        public void HourDifference()
        {
            DateTime referenceDate = new DateTime(2016, 6, 17, 12, 0, 0);
            int offset;

            offset = -365; Assert.AreEqual(offset, cDateFunctions.HourDifference(referenceDate, referenceDate.AddHours(offset)), string.Format("DateDifference test for reference {0} and test {1}", referenceDate, referenceDate.AddHours(offset)));
            offset = -37; Assert.AreEqual(offset, cDateFunctions.HourDifference(referenceDate, referenceDate.AddHours(offset)), string.Format("DateDifference test for reference {0} and test {1}", referenceDate, referenceDate.AddHours(offset)));
            offset = -13; Assert.AreEqual(offset, cDateFunctions.HourDifference(referenceDate, referenceDate.AddHours(offset)), string.Format("DateDifference test for reference {0} and test {1}", referenceDate, referenceDate.AddHours(offset)));
            offset = -12; Assert.AreEqual(offset, cDateFunctions.HourDifference(referenceDate, referenceDate.AddHours(offset)), string.Format("DateDifference test for reference {0} and test {1}", referenceDate, referenceDate.AddHours(offset)));
            offset = -1; Assert.AreEqual(offset, cDateFunctions.HourDifference(referenceDate, referenceDate.AddHours(offset)), string.Format("DateDifference test for reference {0} and test {1}", referenceDate, referenceDate.AddHours(offset)));
            offset = 0; Assert.AreEqual(offset, cDateFunctions.HourDifference(referenceDate, referenceDate.AddHours(offset)), string.Format("DateDifference test for reference {0} and test {1}", referenceDate, referenceDate.AddHours(offset)));
            offset = 1; Assert.AreEqual(offset, cDateFunctions.HourDifference(referenceDate, referenceDate.AddHours(offset)), string.Format("DateDifference test for reference {0} and test {1}", referenceDate, referenceDate.AddHours(offset)));
            offset = 11; Assert.AreEqual(offset, cDateFunctions.HourDifference(referenceDate, referenceDate.AddHours(offset)), string.Format("DateDifference test for reference {0} and test {1}", referenceDate, referenceDate.AddHours(offset)));
            offset = 12; Assert.AreEqual(offset, cDateFunctions.HourDifference(referenceDate, referenceDate.AddHours(offset)), string.Format("DateDifference test for reference {0} and test {1}", referenceDate, referenceDate.AddHours(offset)));
            offset = 36; Assert.AreEqual(offset, cDateFunctions.HourDifference(referenceDate, referenceDate.AddHours(offset)), string.Format("DateDifference test for reference {0} and test {1}", referenceDate, referenceDate.AddHours(offset)));
            offset = 365; Assert.AreEqual(offset, cDateFunctions.HourDifference(referenceDate, referenceDate.AddHours(offset)), string.Format("DateDifference test for reference {0} and test {1}", referenceDate, referenceDate.AddHours(offset)));
        }

    }
}
