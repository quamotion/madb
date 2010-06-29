using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Managed.Adb {
	public class InstallReceiver : MultiLineReceiver {
		/// <summary>
		/// 
		/// </summary>
		private const String SUCCESS_OUTPUT = "Success";
		/// <summary>
		/// 
		/// </summary>
		private const String FAILURE_PATTERN = @"Failure(?:\s+\[(.*)\])?";


		private const String UNKNOWN_ERROR = "An unknown error occured.";
		/// <summary>
		/// Processes the new lines.
		/// </summary>
		/// <param name="lines">The lines.</param>
		protected override void ProcessNewLines ( string[] lines ) {
			foreach ( String line in lines ) {
				if ( line.Length > 0 ) {
					Console.WriteLine ( line );
					if ( line.StartsWith ( SUCCESS_OUTPUT ) ) {
						ErrorMessage = null;
					} else {
						Match m = Regex.Match ( line, FAILURE_PATTERN );
						if ( m.Success ) {
							string msg = m.Groups[1].Value;
							ErrorMessage = String.IsNullOrEmpty ( msg ) ? UNKNOWN_ERROR : msg;
						}
					}
				}
			}
		}

		/// <summary>
		/// Gets the error message if the install was unsuccessful.
		/// </summary>
		/// <value>The error message.</value>
		public String ErrorMessage { get; private set; }
	}
}
