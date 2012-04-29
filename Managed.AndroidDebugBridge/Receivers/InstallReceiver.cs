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
						Success = true;
					} else {
						Match m = Regex.Match ( line, FAILURE_PATTERN );
						ErrorMessage = UNKNOWN_ERROR;
						if ( m.Success ) {
							string msg = m.Groups[1].Value;
							ErrorMessage = String.IsNullOrEmpty ( msg ) ? UNKNOWN_ERROR : msg;
						}
						Success = false;
					}
				}
			}
		}

		/// <summary>
		/// Gets the error message if the install was unsuccessful.
		/// </summary>
		/// <value>The error message.</value>
		public String ErrorMessage { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the install was a success.
		/// </summary>
		/// <value>
		///   <c>true</c> if success; otherwise, <c>false</c>.
		/// </value>
		public bool Success { get; private set; }
	}
}
