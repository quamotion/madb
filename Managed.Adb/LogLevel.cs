using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	/// <summary>
	/// 
	/// </summary>
	public static class LogLevel {

		/// <summary>
		/// Initializes the <see cref="LogLevel"/> class.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the verbose log level.
		/// </summary>
		/// <value>The verbose log level.</value>
		public static LogLevelInfo Verbose { get; private set; }
		/// <summary>
		/// Gets or sets the debug log level.
		/// </summary>
		/// <value>The debug log level.</value>
		public static LogLevelInfo Debug { get; private set; }
		/// <summary>
		/// Gets or sets the info log level.
		/// </summary>
		/// <value>The info log level.</value>
		public static LogLevelInfo Info { get; private set; }
		/// <summary>
		/// Gets or sets the warn log level.
		/// </summary>
		/// <value>The warn log level.</value>
		public static LogLevelInfo Warn { get; private set; }
		/// <summary>
		/// Gets or sets the error log level.
		/// </summary>
		/// <value>The error log level.</value>
		public static LogLevelInfo Error { get; private set; }
		/// <summary>
		/// Gets or sets the assert log level.
		/// </summary>
		/// <value>The assert log level.</value>
		public static LogLevelInfo Assert { get; private set; }

		/// <summary>
		/// Gets or sets the levels.
		/// </summary>
		/// <value>The levels.</value>
		public static Dictionary<String, LogLevelInfo> Levels { get; private set; }

		public static LogLevelInfo GetByString ( String value ) {

			foreach ( LogLevelInfo item in Values ) {
				if ( String.Compare ( item.Value, value, true ) == 0 ) {
					return item;
				}
			}
			return null;
		}

		/// <summary>
		/// Gets the by letter.
		/// </summary>
		/// <param name="letter">The letter.</param>
		/// <returns></returns>
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

		/// <summary>
		/// 
		/// </summary>
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

			/// <summary>
			/// Gets or sets the priority.
			/// </summary>
			/// <value>The priority.</value>
			public int Priority { get; set; }
			/// <summary>
			/// Gets or sets the value.
			/// </summary>
			/// <value>The value.</value>
			public String Value { get; set; }
			/// <summary>
			/// Gets or sets the letter.
			/// </summary>
			/// <value>The letter.</value>
			public Char Letter { get; set; }
		}
	}
}
