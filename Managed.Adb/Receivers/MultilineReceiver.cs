using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class MultiLineReceiver : IShellOutputReceiver
    {

        /// <summary>
        /// 
        /// </summary>
        public const string NEWLINE = "\r\n";
        /// <summary>
        /// 
        /// </summary>
        public const string ENCODING = "ISO-8859-1";

        /// <summary>
        /// Gets or sets a value indicating whether [trim lines].
        /// </summary>
        /// <value><see langword="true"/> if [trim lines]; otherwise, <see langword="false"/>.</value>
        public bool TrimLines { get; set; }

        /// <summary>
        /// Gets a value indicating whether the receiver parses error messages.
        /// </summary>
        /// <value>
        ///     <see langword="true"/> if this receiver parsers error messages; otherwise <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// The default value is <see langword="false"/>. If set to <see langword="false"/>, the <see cref="AdbHelper"/>
        /// will detect common error messages and throw an exception.
        /// </remarks>
        public virtual bool ParsesErrors { get; protected set; }

        /// <summary>
        /// Gets or sets the unfinished line.
        /// </summary>
        /// <value>The unfinished line.</value>
        protected string UnfinishedLine { get; set; }
        /// <summary>
        /// Gets or sets the lines.
        /// </summary>
        /// <value>The lines.</value>
        protected ICollection<string> Lines { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiLineReceiver"/> class.
        /// </summary>
        public MultiLineReceiver()
        {
            this.Lines = new List<string>();
        }

        /// <summary>
        /// Adds the output.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        public void AddOutput(byte[] data, int offset, int length )
        {
            if (!this.IsCancelled )
            {
                string s = null;
                try
                {
                    s = Encoding.GetEncoding(ENCODING ).GetString(data, offset, length ); //$NON-NLS-1$
                }
                catch (DecoderFallbackException )
                {
                    // normal encoding didn't work, try the default one
                    s = Encoding.Default.GetString(data, offset, length );
                }

                // ok we've got a string
                if (!string.IsNullOrEmpty(s ) )
                {
                    // if we had an unfinished line we add it.
                    if (!string.IsNullOrEmpty(this.UnfinishedLine ) )
                    {
                        s = this.UnfinishedLine + s;
                        this.UnfinishedLine = null;
                    }

                    // now we split the lines
                    //Lines.Clear ( );
                    int start = 0;
                    do
                    {
                        int index = s.IndexOf(NEWLINE, start ); //$NON-NLS-1$

                        // if \r\n was not found, this is an unfinished line
                        // and we store it to be processed for the next packet
                        if (index == -1 )
                        {
                            this.UnfinishedLine = s.Substring(start );
                            break;
                        }

                        // so we found a \r\n;
                        // extract the line
                        string line = s.Substring(start, index - start );
                        if (this.TrimLines )
                        {
                            line = line.Trim();
                        }
                        this.Lines.Add(line );

                        // move start to after the \r\n we found
                        start = index + 2;
                    }
                    while (true );
                }
            }
        }

        /// <summary>
        /// Flushes the output.
        /// </summary>
        public void Flush()
        {
            if (!this.IsCancelled && this.Lines.Count > 0 )
            {
                // at this point we've split all the lines.
                // make the array
                string[] lines = this.Lines.ToArray();

                // send it for final processing
                this.ProcessNewLines(lines );
                this.Lines.Clear();
            }

            if (!this.IsCancelled && !string.IsNullOrEmpty(this.UnfinishedLine ) )
            {
                this.ProcessNewLines(new string[] { this.UnfinishedLine } );
            }

            this.Done();
        }

        /// <summary>
        /// Finishes the receiver
        /// </summary>
        protected virtual void Done()
        {
            // Do nothing
        }

        /// <summary>
        /// Gets a value indicating whether this instance is canceled.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if this instance is canceled; otherwise, <see langword="false"/>.
        /// </value>
        public virtual bool IsCancelled { get; protected set; }

        /// <summary>
        /// Processes the new lines.
        /// </summary>
        /// <param name="lines">The lines.</param>
        protected abstract void ProcessNewLines(string[] lines );
    }

}
