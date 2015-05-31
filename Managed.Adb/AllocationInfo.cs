using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Managed.Adb {

	/// <summary>
	///  Holds Allocation information.
	/// </summary>
	public class AllocationInfo : IComparable<AllocationInfo> {

		/// <summary>
		/// Initializes a new instance of the <see cref="AllocationInfo"/> class.
		/// </summary>
		/// <param name="allocatedClass">The allocated class.</param>
		/// <param name="allocationSize">Size of the allocation.</param>
		/// <param name="threadId">The thread id.</param>
		/// <param name="stackTrace">The stack trace.</param>
		public AllocationInfo ( String allocatedClass, int allocationSize, short threadId, StackTrace stackTrace ) {
			AllocatedClass = allocatedClass;
			AllocationSize = allocationSize;
			ThreadId = threadId;
			StackTrace = stackTrace;
		}

		/// <summary>
		/// Gets the allocated class.
		/// </summary>
		/// <value>The allocated class.</value>
		public String AllocatedClass { get; private set; }
		/// <summary>
		/// Gets the size of the allocation.
		/// </summary>
		/// <value>The size of the allocation.</value>
		public int AllocationSize { get; private set; }
		/// <summary>
		/// Gets the thread id.
		/// </summary>
		/// <value>The thread id.</value>
		public short ThreadId { get; private set; }
		/// <summary>
		/// Gets the stack trace.
		/// </summary>
		/// <value>The stack trace.</value>
		public StackTrace StackTrace { get; private set; }


		#region IComparable<AllocationInfo> Members

		/// <summary>
		/// Compares the current object with another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>.
		/// </returns>
		public int CompareTo ( AllocationInfo other ) {
			return other.AllocationSize - AllocationSize;
		}

		#endregion
	}
}
