using System;
using System.Collections.Generic;
using System.Text;

namespace ECMPS.Common
{
    /// <summary>
    /// Static class to help ensure our database integrity
    /// </summary>
    public static class cDBIntegrity
    {
        /// <summary>
        /// Get the checksum for checks - a string of hex numbers
        /// </summary>
        public static string Checksum_Checks
        {
            get { return "29539C40F4F00BBB353C483962BD8288E582BFC7"; }
        }
    }
}
