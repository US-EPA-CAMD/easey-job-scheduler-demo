using System;
using System.Collections;
using System.Collections.Generic;


namespace ECMPS.Checks.TypeUtilities
{

    /// <summary>
    /// This object is used to capture distinct date ranges from added individual date ranges.
    /// 
    /// The initial date range is based on the Lower and Upper Bound 
    /// </summary>
    public class DistinctHourRanges : IEnumerable<DistinctHourRange>
    {

        /// <summary>
        /// Initialized a distinct ranges object including setting the lower and upper bounds for ranges to include.
        /// </summary>
        /// <param name="lowerBound">The lower bound for the distinct ranges.  This hour will become the begin date for ranges that stratle the date.</param>
        /// <param name="upperBound">The upper bound for the distinct ranges.  This hour will become the end date for ranges that stratle the date.</param>
        public DistinctHourRanges(DateTime? lowerBound, DateTime? upperBound)
        {
            MinHour = lowerBound.HasValue ? lowerBound.Value.Date.AddHours(lowerBound.Value.Hour) : DateTime.MinValue.Date;
            MaxHour = upperBound.HasValue ? upperBound.Value.Date.AddHours(upperBound.Value.Hour) : DateTime.MaxValue.Date.AddHours(23);

            LowerBound = MinHour;
            UpperBound = MaxHour;

            HourRangeList = new List<DistinctHourRange>();
            HourRangeList.Add(new DistinctHourRange(LowerBound, UpperBound));
        }


        #region Core Private Properties and Fields

        List<DistinctHourRange> HourRangeList;

        #endregion


        #region Public Properties

        /// <summary>
        /// Returns the range associated with the index.
        /// </summary>
        /// <param name="index">The index of the range to end.</param>
        /// <returns>The range at the index.</returns>
        public DistinctHourRange this[int index] {  get { return HourRangeList[index]; } }


        /// <summary>
        /// The lower bound for the distinct ranges.
        /// </summary>
        public DateTime LowerBound { get; private set; }


        /// <summary>
        /// The maximum hour with date for Distinct Hour Ranges.
        /// </summary>
        public static DateTime MaxHour { get; private set; }


        /// <summary>
        /// The minimum hour with date for Distinct Hour Ranges.
        /// </summary>
        public static DateTime MinHour { get; private set; }


        /// <summary>
        /// Returns the number of ranges contained in the object.
        /// </summary>
        public int RangeCount { get { return HourRangeList.Count; } }


        /// <summary>
        /// The upper bound for the distinct ranges.
        /// </summary>
        public DateTime UpperBound { get; private set; }

        #endregion


        #region Public Methods

        /// <summary>
        /// Add a new range to the existing ranges, spawning new ranges if overlaps occur to create separate ranges for overlaps.
        /// </summary>
        /// <param name="lowerBound">The lower bound of the new range.</param>
        /// <param name="upperBound">The upper bound of the new range.</param>
        public void Add(DateTime? lowerBound, DateTime? upperBound)
        {
            DistinctHourRange hourRange;
            DateTime lowerBoundTemp = lowerBound.HasValue ? lowerBound.Value.Date.AddHours(lowerBound.Value.Hour) : MinHour;
            DateTime upperBoundTemp = upperBound.HasValue ? upperBound.Value.Date.AddHours(upperBound.Value.Hour) : MaxHour;

            if (lowerBoundTemp < MinHour) lowerBoundTemp = MinHour;
            if (upperBoundTemp > MaxHour) upperBoundTemp = MaxHour;

            // Skip processing if lower and upper bounds are not consistent or not within the min/max range.
            if ((lowerBoundTemp <= upperBoundTemp) && (lowerBoundTemp <= MaxHour) && (upperBoundTemp >= MinHour))
            {
                bool done = false;

                for (int dex = HourRangeList.Count - 1; dex >= 0 && !done; dex--)
                {
                    hourRange = HourRangeList[dex];

                    // Same Range.
                    if ((lowerBoundTemp == hourRange.Began) && (upperBoundTemp == hourRange.Ended))
                    {
                        done = true;
                    }
                    // New begins after the existing range.
                    else if (lowerBoundTemp > hourRange.Ended)
                    {
                        HourRangeList.Add(new DistinctHourRange(lowerBoundTemp, upperBoundTemp));
                        done = true;
                    }
                    // New begins on end of existing range.
                    else if (lowerBoundTemp == hourRange.Ended)
                    {
                        // Note that hourRange.Began, hourRange.Ended, lowerBound and upperBound cannot all have the same value because of the check above for the same range.
                        if ((hourRange.Began < hourRange.Ended) && (lowerBoundTemp < upperBoundTemp))
                        {
                            HourRangeList.Add(new DistinctHourRange(lowerBoundTemp, lowerBoundTemp));
                            HourRangeList.Add(new DistinctHourRange(lowerBoundTemp.AddHours(1), upperBoundTemp));
                            hourRange.Ended = lowerBoundTemp.AddHours(-1);
                        }
                        else if ((hourRange.Began < hourRange.Ended) && (lowerBoundTemp == upperBoundTemp))
                        {
                            HourRangeList.Add(new DistinctHourRange(lowerBoundTemp, upperBoundTemp));
                            hourRange.Ended = lowerBoundTemp.AddHours(-1);
                        }
                        else if ((hourRange.Began == hourRange.Ended) && (lowerBoundTemp < upperBoundTemp))
                        {
                            HourRangeList.Add(new DistinctHourRange(lowerBoundTemp.AddHours(1), upperBoundTemp));
                        }

                        done = true;
                    }
                    // New begins between begin and end of existing range.
                    else if ((lowerBoundTemp > hourRange.Began) && (lowerBoundTemp < hourRange.Ended))
                    {
                        if (upperBoundTemp > hourRange.Ended)
                        {
                            HourRangeList.Add(new DistinctHourRange(lowerBoundTemp, hourRange.Ended));
                            HourRangeList.Add(new DistinctHourRange(hourRange.Ended.AddHours(1), upperBoundTemp));
                            hourRange.Ended = lowerBoundTemp.AddHours(-1);
                        }
                        else if (upperBoundTemp == hourRange.Ended)
                        {
                            HourRangeList.Add(new DistinctHourRange(lowerBoundTemp, upperBoundTemp));
                            hourRange.Ended = lowerBoundTemp.AddHours(-1);
                        }
                        else
                        {
                            HourRangeList.Add(new DistinctHourRange(lowerBoundTemp, upperBoundTemp));
                            HourRangeList.Add(new DistinctHourRange(upperBoundTemp.AddHours(1), hourRange.Ended));
                            hourRange.Ended = lowerBoundTemp.AddHours(-1);
                        }

                        done = true;
                    }
                    // New begins on begin of existing range.
                    else if (lowerBoundTemp == hourRange.Began)
                    {
                        // upperBoundTemp and hourRange.Ended cannot be equal because of previous same range check.
                        if (upperBoundTemp > hourRange.Ended)
                        {
                            HourRangeList.Add(new DistinctHourRange(hourRange.Ended.AddHours(1), upperBoundTemp));
                        }
                        else
                        {
                            HourRangeList.Add(new DistinctHourRange(upperBoundTemp.AddHours(1), hourRange.Ended));
                            hourRange.Ended = upperBoundTemp;
                        }

                        done = true;
                    }
                    else // lowerBoundTemp < hourRange.Began
                    {
                        if (upperBoundTemp > hourRange.Ended)
                        {
                            HourRangeList.Add(new DistinctHourRange(hourRange.Ended.AddHours(1), upperBoundTemp));
                            upperBoundTemp = hourRange.Began.AddHours(-1);
                        }
                        else if (upperBoundTemp == hourRange.Ended)
                        {
                            upperBoundTemp = hourRange.Began.AddHours(-1);
                        }
                        else if ((upperBoundTemp >= hourRange.Began) && (upperBoundTemp < hourRange.Ended))
                        {
                            HourRangeList.Add(new DistinctHourRange(upperBoundTemp.AddHours(1), hourRange.Ended));
                            hourRange.Ended = upperBoundTemp;
                            upperBoundTemp = hourRange.Began.AddHours(-1);
                        }

                        done = false; // Check the remaining range with the next (previous) existing range.
                    }
                }

                HourRangeList.Sort();
            }
        }

        #endregion


        #region IEnumerator Implementation

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>Returns the enumerator for the hour range list.</returns>
        IEnumerator<DistinctHourRange> IEnumerable<DistinctHourRange>.GetEnumerator()
        {
            return this.HourRangeList.GetEnumerator();
        }


        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>Returns the enumerator for the hour range list.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.HourRangeList.GetEnumerator();
        }

        #endregion

    }


    /// <summary>
    /// 
    /// </summary>
    public class DistinctHourRange: IEquatable<DistinctHourRange>, IComparable<DistinctHourRange>
    {

        /// <summary>
        /// Created a distinct hour range instance.
        /// </summary>
        /// <param name="began">The begin date of the hour range.</param>
        /// <param name="ended">The end date of the hour range.</param>
        public DistinctHourRange(DateTime began, DateTime ended)
        {
            began = began.Date.AddHours(began.Hour);
            ended = ended.Date.AddHours(ended.Hour);

            if (ended < began) throw new ArgumentOutOfRangeException("Ended must be greater than or equal to Began.");

            Began = began;
            Ended = ended;
        }


        #region Public Parameters

        /// <summary>
        /// The begin date for a date range.
        /// </summary>
        public DateTime Began { get; internal set; }

        /// <summary>
        /// The end date for a date range.
        /// </summary>
        public DateTime Ended { get; internal set; }

        #endregion


        #region Public Methods

        /// <summary>
        /// Compares the current instance to another instance.
        /// </summary>
        /// <param name="other">The other instance in the comparison.</param>
        /// <returns>-1 for less than, 1 for greater than, and 0 for the same.</returns>
        public int CompareTo(DistinctHourRange other)
        {
            int result;

            // A null value means that this object is greater.
            if (other == null)
            {
                result = 1;
            }
            else
            {
                if (this.Began == null && other.Began == null)
                    result = 0;
                else if (this.Began == null)
                    result = -1;
                else if (other.Began == null)
                    result = 1;
                else
                    result = this.Began.CompareTo(other.Began);

                if (result == 0)
                {
                    if (this.Ended == null && other.Ended == null)
                        result = 0;
                    else if (this.Ended == null)
                        result = 1;
                    else if (other.Ended == null)
                        result = -1;
                    else
                        result = this.Ended.CompareTo(other.Ended);
                }
            }

            return result;
        }


        /// <summary>
        /// Determine whether the current instance equals another instance.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(DistinctHourRange other)
        {
            if (other == null)
                return false;
            else
                return (this.Began == other.Began) && (this.Ended == other.Ended);
        }


        /// <summary>
        /// Determine whether the current instance equals another instance.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            DistinctHourRange objAsPart = obj as DistinctHourRange;

            if (objAsPart == null)
                return false;
            else
                return Equals(objAsPart);
        }


        /// <summary>
        /// Formats and hour in date form to a string.
        /// </summary>
        /// <param name="dateHour">The date/hour.</param>
        /// <returns>The formatted date/hour.</returns>
        public string FormatHour(DateTime dateHour)
        {
            return $"{dateHour.Year}-{dateHour.Month.ToString().PadLeft(2, '0')}-{dateHour.Day.ToString().PadLeft(2, '0')} {dateHour.Hour.ToString().PadLeft(2, '0')}";
        }


        /// <summary>
        /// Returns the has code for the current intance.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + Began.GetHashCode();
            hash = hash * 23 + Ended.GetHashCode();
            return hash;
        }


        /// <summary>
        /// Returns the string version of the object values.
        /// </summary>
        /// <returns>The string version of the object.</returns>
        public override string ToString()
        {
            string beganText = (Began != null) ? FormatHour(Began) : "min";
            string endedText = (Ended != null) ? FormatHour(Ended) : "max";

            return $"[{beganText}]..[{endedText}]";
        }

        #endregion

    }
}
