using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Managed.Adb {
	public class InstallReceiver : MultiLineReceiver {
		private const String SUCCESS_OUTPUT = "Success";
		private const String FAILURE_PATTERN = @"Failure\s+\[(.*)\]";

		protected override void ProcessNewLines ( string[] lines ) {
			foreach ( String line in lines ) {
				if ( line.Length > 0 ) {
					Console.WriteLine ( line );
					if ( line.StartsWith ( SUCCESS_OUTPUT ) ) {
						ErrorMessage = null;
					} else {
						Match m = Regex.Match ( line, FAILURE_PATTERN );
						if ( m.Success ) {
							ErrorMessage = m.Groups[1].Value;
						}
					}
				}
			}
		}

		/// <summary>
		/// Gets the error message if the install was unsuccessfull.
		/// </summary>
		public String ErrorMessage { get; private set; }
	}
}
