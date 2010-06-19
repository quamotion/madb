using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Managed.Adb.Extensions;

namespace Managed.Adb {
	public sealed class Log {
		

		public interface ILogOutput {
			void Write ( LogLevel.LogLevelInfo logLevel, String tag, String message );
			void WriteAndPromptLog ( LogLevel.LogLevelInfo logLevel, String tag, String message );
		}

		private static LogLevel.LogLevelInfo mLevel = DdmPreferences.getLogLevel ( );

		private static ILogOutput sLogOutput;

		private const char[] mSpaceLine = new char[72];
		private const char[] mHexDigit = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
		/// <summary>
		/// Static Initializer for the <see cref="Log"/> class.
		/// </summary>
		static Log ( ) {
			/* prep for hex dump */
			int i = mSpaceLine.Length - 1;
			while ( i >= 0 )
				mSpaceLine[i--] = ' ';
			mSpaceLine[0] = mSpaceLine[1] = mSpaceLine[2] = mSpaceLine[3] = '0';
			mSpaceLine[4] = '-';
		}

		private Log ( ) { }

		static sealed class Config {
			public const bool LOGV = true;
			public const bool LOGD = true;
		};

		/**
     * Outputs a {@link LogLevel#VERBOSE} level message.
     * @param tag The tag associated with the message.
     * @param message The message to output.
     */
		public static void v ( String tag, String message ) {
			WriteLine ( LogLevel.Verbose, tag, message );
		}

		/**
		 * Outputs a {@link LogLevel#DEBUG} level message.
		 * @param tag The tag associated with the message.
		 * @param message The message to output.
		 */
		public static void d ( String tag, String message ) {
			WriteLine ( LogLevel.Debug, tag, message );
		}

		/**
		 * Outputs a {@link LogLevel#INFO} level message.
		 * @param tag The tag associated with the message.
		 * @param message The message to output.
		 */
		public static void i ( String tag, String message ) {
			WriteLine ( LogLevel.Info, tag, message );
		}

		/**
		 * Outputs a {@link LogLevel#WARN} level message.
		 * @param tag The tag associated with the message.
		 * @param message The message to output.
		 */
		public static void w ( String tag, String message ) {
			WriteLine ( LogLevel.Warn, tag, message );
		}

		/**
		 * Outputs a {@link LogLevel#ERROR} level message.
		 * @param tag The tag associated with the message.
		 * @param message The message to output.
		 */
		public static void e ( String tag, String message ) {
			WriteLine ( LogLevel.Error, tag, message );
		}

		/**
		 * Outputs a log message and attempts to display it in a dialog.
		 * @param tag The tag associated with the message.
		 * @param message The message to output.
		 */
		public static void LogAndDisplay ( LogLevel.LogLevelInfo logLevel, String tag, String message ) {
			if ( sLogOutput != null ) {
				sLogOutput.WriteAndPromptLog ( logLevel, tag, message );
			} else {
				WriteLine ( logLevel, tag, message );
			}
		}

		/**
		 * Outputs a {@link LogLevel#ERROR} level {@link Throwable} information.
		 * @param tag The tag associated with the message.
		 * @param throwable The {@link Throwable} to output.
		 */
		public static void e ( String tag, Exception exception ) {
			if ( exception != null ) {
				WriteLine ( LogLevel.Error, tag, exception.Message + '\n' + exception.StackTrace );
			}
		}

		static void SetLevel ( LogLevel.LogLevelInfo logLevel ) {
			mLevel = logLevel;
		}

		/**
		 * Sets the {@link ILogOutput} to use to print the logs. If not set, {@link System#out}
		 * will be used.
		 * @param logOutput The {@link ILogOutput} to use to print the log.
		 */
		public static void SetLogOutput ( ILogOutput logOutput ) {
			sLogOutput = logOutput;
		}

		/**
		 * Show hex dump.
		 * <p/>
		 * Local addition.  Output looks like:
		 * 1230- 00 11 22 33 44 55 66 77 88 99 aa bb cc dd ee ff  0123456789abcdef
		 * <p/>
		 * Uses no string concatenation; creates one String object per line.
		 */
		internal static void HexDump ( String tag, LogLevel.LogLevelInfo level, byte[] data, int offset, int length ) {

			int kHexOffset = 6;
			int kAscOffset = 55;
			char[] line = new char[mSpaceLine.Length];
			int addr, baseAddr, count;
			int i, ch;
			bool needErase = true;

			//Log.w(tag, "HEX DUMP: off=" + offset + ", length=" + length);

			baseAddr = 0;
			while ( length != 0 ) {
				if ( length > 16 ) {
					// full line
					count = 16;
				} else {
					// partial line; re-copy blanks to clear end
					count = length;
					needErase = true;
				}

				if ( needErase ) {
					//System.arraycopy(mSpaceLine, 0, line, 0, mSpaceLine.Length);
					mSpaceLine.CopyTo ( line, 0 );
					needErase = false;
				}

				// output the address (currently limited to 4 hex digits)
				addr = baseAddr;
				addr &= 0xffff;
				ch = 3;
				while ( addr != 0 ) {
					line[ch] = mHexDigit[addr & 0x0f];
					ch--;
					addr >>= 4;
				}

				// output hex digits and ASCII chars
				ch = kHexOffset;
				for ( i = 0; i < count; i++ ) {
					byte val = data[offset + i];

					line[ch++] = mHexDigit[( val >> 4 ) & 0x0f];
					line[ch++] = mHexDigit[val & 0x0f];
					ch++;

					if ( val >= 0x20 && val < 0x7f )
						line[kAscOffset + i] = (char)val;
					else
						line[kAscOffset + i] = '.';
				}

				WriteLine ( level, tag, new String ( line ) );

				// advance to next chunk of data
				length -= count;
				offset += count;
				baseAddr += count;
			}

		}

		/**
		 * Dump the entire contents of a byte array with DEBUG priority.
		 */
		internal static void HexDump ( byte[] data ) {
			HexDump ( "ddms", LogLevel.Debug, data, 0, data.Length );
		}

		/* currently prints to stdout; could write to a log window */
		private static void WriteLine ( LogLevel.LogLevelInfo logLevel, String tag, String message ) {
			if ( logLevel.Priority >= mLevel.Priority ) {
				if ( sLogOutput != null ) {
					sLogOutput.Write ( logLevel, tag, message );
				} else {
					Write ( logLevel, tag, message );
				}
			}
		}

		/**
		 * Prints a log message.
		 * @param logLevel
		 * @param tag
		 * @param message
		 */
		public static void Write ( LogLevel.LogLevelInfo logLevel, String tag, String message ) {
			Console.Write ( GetLogFormatString ( logLevel, tag, message ) );
		}

		/**
		 * Formats a log message.
		 * @param logLevel
		 * @param tag
		 * @param message
		 */
		public static String GetLogFormatString ( LogLevel.LogLevelInfo logLevel, String tag, String message ) {
			long msec;

			msec = DateTime.Now.ToUnixEpoch ( );
			return String.Format ( "{0:00}:{1:00} {2}/{3}: {4}\n", ( msec / 60000 ) % 60, ( msec / 1000 ) % 60,
							logLevel.Letter, tag, message );
		}
	}
}
