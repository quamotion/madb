using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Managed.Adb
{
    /// <summary>
    /// 
    /// </summary>
    public class InstallReceiver : MultiLineReceiver
    {
        /// <summary>
        /// 
        /// </summary>
        private const string SUCCESS_OUTPUT = "Success";
        /// <summary>
        /// 
        /// </summary>
        private const string FAILURE_PATTERN = @"Failure(?:\s+\[(.*)\])?";


        private const string UNKNOWN_ERROR = "An unknown error occurred.";
        /// <summary>
        /// Processes the new lines.
        /// </summary>
        /// <param name="lines">The lines.</param>
        protected override void ProcessNewLines ( string[] lines )
        {
            foreach (string line in lines )
            {
                if ( line.Length > 0 )
                {
                    if ( line.StartsWith ( SUCCESS_OUTPUT ) )
                    {
                        this.ErrorMessage = null;
                        this.Success = true;
                    }
                    else
                    {
                        var m = line.Match ( FAILURE_PATTERN, RegexOptions.Compiled | RegexOptions.IgnoreCase );
                        this.ErrorMessage = UNKNOWN_ERROR;
                        if ( m.Success )
                        {
                            string msg = m.Groups[1].Value;
                            this.ErrorMessage = string.IsNullOrEmpty ( msg ) || msg.IsNullOrWhiteSpace() ? UNKNOWN_ERROR : msg;
                        }
                        this.Success = false;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the error message if the install was unsuccessful.
        /// </summary>
        /// <value>The error message.</value>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the install was a success.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if success; otherwise, <see langword="false"/>.
        /// </value>
        public bool Success { get; private set; }
    }
}
