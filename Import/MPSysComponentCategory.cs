using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

using ECMPS.Checks.CheckEngine;
using ECMPS.Checks.Parameters;

namespace ECMPS.Checks.MonitorPlanImport
{
    class cMPSysComponentCategory : cCategory
    {
        cMonitorPlanImportProcess _MPProcess = null;
        string _PrimaryKey = null;

        #region Constructors

        public cMPSysComponentCategory( cCheckEngine CheckEngine, cMonitorPlanImportProcess mpProcess, string MSC_PK )
            : base( CheckEngine, (cProcess)mpProcess, "MPSYSCP" )
        {
            _MPProcess = mpProcess;
            _PrimaryKey = "MSC_PK";

            TableName = "MP_MonitoringSysComponent";
            CurrentRowId = MSC_PK;

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
            string sFilter = string.Format( "{0}={1}", _PrimaryKey, CurrentRowId );

            DataRowView currentRecord = new DataView( _MPProcess.SourceData.Tables["WS_MonitoringSysComponent"], sFilter, "", DataViewRowState.CurrentRows )[0];
            SetCheckParameter( "Current_Workspace_System_Component", currentRecord, eParameterDataType.DataRowView );

            DataView view = new DataView( _MPProcess.SourceData.Tables["WS_Component"], "", "", DataViewRowState.CurrentRows );
            SetCheckParameter( "Workspace_Component_Records", view, eParameterDataType.DataView );
        }

        protected override bool SetErrorSuppressValues()
        {
          ErrorSuppressValues = null;
          return true;
        }

        protected override void SetRecordIdentifier()
        {
            DataRowView CurrentRecord = (DataRowView)GetCheckParameter( "Current_Workspace_System_Component" ).ParameterValue;
            RecordIdentifier = string.Format( "Component ID {0}", CurrentRecord["COMPONENT_IDENTIFIER"].ToString() );
        }

        #endregion    
    }
}
