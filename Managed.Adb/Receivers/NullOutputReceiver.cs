using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class NullOutputReceiver : IShellOutputReceiver
    {

        /// <summary>
        /// Prevents a default instance of the <see cref="NullOutputReceiver" /> class from being created.
        /// </summary>
        private NullOutputReceiver()
        {
            this.IsCancelled = false;
        }

        private static NullOutputReceiver _instance = null;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static IShellOutputReceiver Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new NullOutputReceiver();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Adds the output.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        public void AddOutput(byte[] data, int offset, int length)
        {
            // do nothing
        }

        /// <summary>
        /// Flushes the output.
        /// </summary>
        /// <remarks>
        /// This should always be called at the end of the "process" in order to indicate that the data is ready to be processed further if needed.
        /// </remarks>
        public void Flush()
        {
            // do nothing
        }

        /// <summary>
        /// Gets a value indicating whether this instance is cancelled.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance is cancelled; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsCancelled { get; private set; }

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
        public bool ParsesErrors 
        { 
            get
            {
                return false;
            }
        }
    }
}
