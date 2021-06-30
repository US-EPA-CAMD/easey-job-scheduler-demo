using System;
using System.Data;

using ECMPS.Checks.CheckEngine;
using ECMPS.Checks.Parameters;
using ECMPS.Checks.TypeUtilities;

namespace ECMPS.Checks.QAScreenEvaluation
{
  public class cFFACCTTRow : cCategory
  {

    #region Private Fields

    private cQAScreenMain mQAScreenMain;
    private DataTable mAccuracyTable;
    private string mComponentID;
    private string mMonitorLocationID;

    #endregion

    #region Constructors

    public cFFACCTTRow(cCheckEngine ACheckEngine, cQAScreenMain QAScreenMain, string MonitorLocationId, string TestSumId, DataTable AccuracyTable)
      : base(ACheckEngine, (cProcess)QAScreenMain, "SCRFFTT", AccuracyTable)
    {
      InitializeCurrent(MonitorLocationId, TestSumId);

      mQAScreenMain = QAScreenMain;
      mAccuracyTable = AccuracyTable;
      mMonitorLocationID = MonitorLocationId;

      FilterData();

      SetRecordIdentifier();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Sets the checkbands for this category to the passed check bands and then executes
    /// those checks.
    /// </summary>
    /// <param name="ACheckBands">The check bands to process.</param>
    /// <returns>True if the processing of check executed normally.</returns>
    public bool ProcessChecks(cCheckParameterBands ACheckBands)
    {
      this.SetCheckBands(ACheckBands);
      return base.ProcessChecks();
    }

    #endregion

    #region Base Class Overrides

    protected override void FilterData()
    {
      SetCheckParameter("Current_Transmitter_Transducer_Test", new DataView(mAccuracyTable,
        "", "", DataViewRowState.CurrentRows)[0], eParameterDataType.DataRowView);

      SetCheckParameter("Current_Test", new DataView(mAccuracyTable,
        "", "", DataViewRowState.CurrentRows)[0], eParameterDataType.DataRowView);

      mComponentID = cDBConvert.ToString(((DataRowView)GetCheckParameter("Current_Transmitter_Transducer_Test").ParameterValue)["component_id"]);

      if (mComponentID == "")
        SetCheckParameter("Current_Component", null, eParameterDataType.DataRowView);
      else
        SetCheckParameter("Current_Component", new DataView(mQAScreenMain.SourceData.Tables["QAComponent"],
            "component_id = '" + mComponentID + "'", "", DataViewRowState.CurrentRows)[0], eParameterDataType.DataRowView);

      SetCheckParameter("Location_Test_Records", new DataView(mQAScreenMain.SourceData.Tables["TestSummary"],
        "Mon_loc_id = '" + mMonitorLocationID + "'", "", DataViewRowState.CurrentRows), eParameterDataType.DataView);

      SetCheckParameter("Transmitter_Transducer_Test_Records", new DataView(mQAScreenMain.SourceData.Tables["TestSummary"],
          "test_type_cd = 'FFACC' and component_id = '" + mComponentID + "'", "", DataViewRowState.CurrentRows), eParameterDataType.DataView);

      SetCheckParameter("QA_Supplemental_Data_Records", new DataView(mQAScreenMain.SourceData.Tables["QASuppData"],
        "", "", DataViewRowState.CurrentRows), eParameterDataType.DataView);

      SetCheckParameter("Test_Reason_Code_Lookup_Table", new DataView(mQAScreenMain.SourceData.Tables["TestReasonCode"],
        "", "", DataViewRowState.CurrentRows), eParameterDataType.DataView);

      SetCheckParameter("Test_Result_Code_Lookup_Table", new DataView(mQAScreenMain.SourceData.Tables["TestResultCode"],
        "", "", DataViewRowState.CurrentRows), eParameterDataType.DataView);

      SetCheckParameter("Accuracy_Specification_Code_Lookup_Table", new DataView(mQAScreenMain.SourceData.Tables["AccuracySpecCode"],
        "", "", DataViewRowState.CurrentRows), eParameterDataType.DataView);
    }
    protected override void SetRecordIdentifier()
    {

      RecordIdentifier = "this record";

    }

    protected override bool SetErrorSuppressValues()
    {
        ErrorSuppressValues = null;
        return true;
    }

    #endregion

  }
}
