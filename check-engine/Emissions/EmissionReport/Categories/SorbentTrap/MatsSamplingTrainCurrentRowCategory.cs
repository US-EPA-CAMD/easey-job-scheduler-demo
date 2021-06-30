﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ECMPS.Checks.CheckEngine;
using ECMPS.Checks.Em.Parameters;
using ECMPS.Checks.Parameters;
using ECMPS.Checks.TypeUtilities;

using ECMPS.Definitions.Extensions;
using ECMPS.ErrorSuppression;


namespace ECMPS.Checks.EmissionsReport
{

  /// <summary>
  /// Category class used for categories whose checks are driven by the MatsHourlyGfmRecord check parameter.
  /// </summary>
  public class MatsSamplingTrainCurrentRowCategory : cCategoryHourly
  {

    #region Constructors

    /// <summary>
    /// Creates a category object to represent the category indicated by the passed code, 
    /// and links that object to a parent category object.
    /// </summary>
    /// <param name="parentCategory">The parent category.</param>
    /// <param name="categoryCd">The category code of the category the object will represent.</param>
    public MatsSamplingTrainCurrentRowCategory(cCategory parentCategory, string categoryCd)
      : base(parentCategory, categoryCd)
    {
    }

    #endregion


    #region Base Class Overrides

    protected override void FilterData()
    {
    }

    protected override int[] GetDataBorderModcList()
    {
      return null;
    }

    protected override int[] GetQualityAssuranceHoursModcList()
    {
      return null;
    }

    protected override void SetRecordIdentifier()
    {
      if (EmParameters.MatsSamplingTrainRecord != null)
        RecordIdentifier = EmParameters.MatsSamplingTrainRecord.Description;
      else
        RecordIdentifier = null;
    }

    /// <summary>
    /// Populates the values used in matching category data to error supressions.
    /// </summary>
    /// <returns></returns>
    protected override bool SetErrorSuppressValues()
    {
      if (EmParameters.MatsSamplingTrainRecord != null)
      {
        long facId = CheckEngine.FacilityID;
        string locationName = EmParameters.MatsSamplingTrainRecord.LocationName;
        string matchDataValue = EmParameters.MatsSamplingTrainRecord.ComponentIdentifier;
        DateTime? matchTimeValue = EmParameters.MatsSamplingTrainRecord.BeginDatehour;

        ErrorSuppressValues = new cErrorSuppressValues(facId, locationName, "PARAM", matchDataValue, "HOUR", matchTimeValue);
        return true;
      }
      else
        return false;
    }

    #endregion

  }

}
