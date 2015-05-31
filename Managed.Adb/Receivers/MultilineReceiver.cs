using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	/// <summary>
	/// 
	/// </summary>
	public abstract class MultiLineReceiver : IShellOutputReceiver {

		/// <summary>
		/// 
		/// </summary>
		protected const String NEWLINE = "\r\n";
		/// <summary>
		/// 
		/// </summary>
		protected const String ENCODING = "ISO-8859-1";

		/// <summary>
		/// Gets or sets a value indicating whether [trim lines].
		/// </summary>
		/// <value><c>true</c> if [trim lines]; otherwise, <c>false</c>.</value>
		public bool TrimLines { get; set; }
		/// <summary>
		/// Gets or sets the unfinished line.
		/// </summary>
		/// <value>The unfinished line.</value>
		protected String UnfinishedLine { get; set; }
		/// <summary>
		/// Gets or sets the lines.
		/// </summary>
		/// <value>The lines.</value>
		protected ICollection<String> Lines { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="MultiLineReceiver"/> class.
		/// </summary>
		public MultiLineReceiver( ) {
			Lines = new List<String> ( );
		}

		/// <summary>
		/// Adds the output.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="offset">The offset.</param>
		/// <param name="length">The length.</param>
		public void AddOutput( byte[] data, int offset, int length ) {
			if ( !IsCancelled ) {
				String s = null;
				try {
					s = Encoding.GetEncoding ( ENCODING ).GetString ( data, offset, length ); //$NON-NLS-1$
				} catch ( DecoderFallbackException ) {
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
					//Lines.Clear ( );
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
				}
			}
		}

		/// <summary>
		/// Flushes the output.
		/// </summary>
		public void Flush( ) {
			if (!IsCancelled && Lines.Count > 0 ) {
				// at this point we've split all the lines.
				// make the array
				String[] lines = Lines.ToArray ( );

				// send it for final processing
				ProcessNewLines ( lines );
				Lines.Clear ( );
			}

			if ( !IsCancelled && !String.IsNullOrEmpty ( UnfinishedLine ) ) {
				ProcessNewLines ( new String[] { UnfinishedLine } );
			}

			Done ( );
		}

		/// <summary>
		/// Finishes the receiver
		/// </summary>
		protected virtual void Done( ) {
			// Do nothing
		}

		/// <summary>
		/// Gets a value indicating whether this instance is canceled.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is canceled; otherwise, <c>false</c>.
		/// </value>
		public virtual bool IsCancelled { get; protected set; }

		/// <summary>
		/// Processes the new lines.
		/// </summary>
		/// <param name="lines">The lines.</param>
		protected abstract void ProcessNewLines( String[] lines );
	}

}
