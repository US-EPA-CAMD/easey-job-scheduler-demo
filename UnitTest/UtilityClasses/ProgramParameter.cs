using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitTest.UtilityClasses
{

    /// <summary>
    /// Contains the program and parameter codes for a Program Parameter record.
    /// </summary>
    public class ProgramParameter
    {

        /// <summary>
        /// Creates a ProgramParameer instance.
        /// </summary>
        /// <param name="prgCd">The program code for the program parameter record.</param>
        /// <param name="parameterCd">The parameter code for the program parameter record.</param>
        public ProgramParameter(string prgCd, string parameterCd)
        {
            PrgCd = prgCd;
            ParameterCd = parameterCd;
        }


        /// <summary>
        /// The program code for the program parameter record.
        /// </summary>
        public string PrgCd { get; private set; }


        /// <summary>
        /// The parameter code for the program parameter record.
        /// </summary>
        public string ParameterCd { get; private set; }

    }

}
