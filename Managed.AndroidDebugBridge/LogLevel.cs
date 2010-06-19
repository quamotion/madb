using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	public static class LogLevel {

		static LogLevel ( ) {
			Levels = new Dictionary<string, LogLevelInfo> ( );
			Verbose = new LogLevelInfo { Priority = 2, Value = "verbose", Letter = 'V' };
			Debug = new LogLevelInfo { Priority = 3, Value = "debug", Letter = 'D' };
			Info = new LogLevelInfo { Priority = 4, Value = "info", Letter = 'I' };
			Warn = new LogLevelInfo { Priority = 5, Value = "warn", Letter = 'W' };
			Error = new LogLevelInfo { Priority = 6, Value = "error", Letter = 'E' };
			Assert = new LogLevelInfo { Priority = 7, Value = "assert", Letter = 'A' };

			Levels.Add ( Verbose.Value, Verbose );
			Levels.Add ( Debug.Value, Debug );
			Levels.Add ( Info.Value, Info );
			Levels.Add ( Warn.Value, Warn );
			Levels.Add ( Error.Value, Error );
			Levels.Add ( Assert.Value, Assert );

		}

		public static LogLevelInfo Verbose { get; private set; }
		public static LogLevelInfo Debug { get; private set; }
		public static LogLevelInfo Info { get; private set; }
		public static LogLevelInfo Warn { get; private set; }
		public static LogLevelInfo Error { get; private set; }
		public static LogLevelInfo Assert { get; private set; }

		public static Dictionary<String, LogLevelInfo> Levels { get; private set; }

		public static LogLevelInfo GetByString ( String value ) {

			foreach ( LogLevelInfo item in Values ) {
				if ( String.Compare ( item.Value, value, true ) == 0 ) {
					return item;
				}
			}
			return null;
		}

		public static LogLevelInfo GetByLetter ( String letter ) {
			return GetByLetter ( letter[0] );
		}

		/// <summary>
		/// Gets the by letter.
		/// </summary>
		/// <param name="letter">The letter.</param>
		/// <returns></returns>
		public static LogLevelInfo GetByLetter ( char letter ) {
			foreach ( LogLevelInfo item in Values ) {
				if ( item.Letter == letter ) {
					return item;
				}
			}

			return null;
		}

		/// <summary>
		/// Gets the values.
		/// </summary>
		/// <value>The values.</value>
		public static Dictionary<string, LogLevelInfo>.ValueCollection Values {
			get {
				return Levels.Values;
			}
		}

		/// <summary>
		/// Gets the keys.
		/// </summary>
		/// <value>The keys.</value>
		public static Dictionary<string, LogLevelInfo>.KeyCollection Keys {
			get {
				return Levels.Keys;
			}
		}

		public class LogLevelInfo {
			/// <summary>
			/// Initializes a new instance of the <see cref="LogLevelInfo"/> class.
			/// </summary>
			public LogLevelInfo ( ) {

			}

			/// <summary>
			/// Initializes a new instance of the <see cref="LogLevelInfo"/> class.
			/// </summary>
			/// <param name="priority">The priority.</param>
			/// <param name="value">The value.</param>
			/// <param name="letter">The letter.</param>
			public LogLevelInfo ( int priority, String value, Char letter ) {
				Priority = priority;
				Value = value;
				Letter = letter;
			}

			public int Priority { get; set; }
			public String Value { get; set; }
			public Char Letter { get; set; }
		}
	}
}
