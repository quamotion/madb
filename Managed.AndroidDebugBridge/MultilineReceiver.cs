using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	public abstract class MultiLineReceiver : IShellOutputReceiver {

		protected const String NEWLINE = "\r\n";
		protected const String ENCODING = "ISO-8859-1";

		public bool TrimLines { get; set; }
		protected String UnfinishedLine { get; set; }
		protected List<String> Lines { get; set; }

		public MultiLineReceiver ( ) {
			Lines = new List<string> ( );
		}

		public void AddOutput ( byte[] data, int offset, int length ) {
			if ( !IsCancelled ) {
				String s = null;
				try {
					s = Encoding.GetEncoding ( ENCODING ).GetString ( data, offset, length ); //$NON-NLS-1$
				} catch ( DecoderFallbackException e ) {
					// normal encoding didn't work, try the default one
					s = Encoding.Default.GetString ( data, offset, length );
				}

				// ok we've got a string
				if ( !String.IsNullOrEmpty ( s ) ) {
					// if we had an unfinished line we add it.
					if ( !String.IsNullOrEmpty ( UnfinishedLine ) ) {
						s = UnfinishedLine + s;
						UnfinishedLine = null;
					}

					// now we split the lines
					Lines.Clear ( );
					int start = 0;
					do {
						int index = s.IndexOf ( NEWLINE, start ); //$NON-NLS-1$

						// if \r\n was not found, this is an unfinished line
						// and we store it to be processed for the next packet
						if ( index == -1 ) {
							UnfinishedLine = s.Substring ( start );
							break;
						}

						// so we found a \r\n;
						// extract the line
						String line = s.Substring ( start, index - start );
						if ( TrimLines ) {
							line = line.Trim ( );
						}
						Lines.Add ( line );

						// move start to after the \r\n we found
						start = index + 2;
					} while ( true );

					if ( Lines.Count > 0 ) {
						// at this point we've split all the lines.
						// make the array
						String[] lines = Lines.ToArray ( );

						// send it for final processing
						ProcessNewLines ( lines );
					}
				}
			}
		}

		public void Flush ( ) {
			if ( !String.IsNullOrEmpty ( UnfinishedLine ) ) {
				ProcessNewLines ( new String[] { UnfinishedLine } );
			}

			Done ( );
		}

		protected virtual void Done ( ) {
			// Do nothing
		}

		public bool IsCancelled { get; private set; }

		public abstract void ProcessNewLines ( String[] lines );
	}

}
