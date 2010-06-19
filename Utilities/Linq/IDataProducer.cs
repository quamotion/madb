using System;

namespace Managed.Adb.Utilities.Linq
{
    /// <summary>
    /// Interface to be implemented by sequences of data which have a "push"
    /// nature rather than "pull" - instead of the IEnumerable model of
    /// the client pulling data from the sequence, here the client registers
    /// an interest in the data being produced, and in the sequence reaching
    /// an end. The data producer than produces data whenever it wishes, and the
    /// clients can react. This allows other actions to occur between items being
    /// pulled, as well as multiple clients for the same sequence of data.
    /// </summary>
    public interface IDataProducer<T>
    {
        /// <summary>
        /// Event which is raised when an item of data is produced.
        /// This will not be raised after EndOfData has been raised.
        /// The parameter for the event is the 
        /// </summary>
        event Action<T> DataProduced;
        /// <summary>
        /// Event which is raised when the sequence has finished being
        /// produced. This will be raised exactly once, and after all
        /// DataProduced events (if any) have been raised.
        /// </summary>
        event Action EndOfData;
    }
}
