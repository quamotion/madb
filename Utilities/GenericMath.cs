#if DOTNET35
namespace Managed.Adb.Utilities
{
    /// <summary>
    /// Generic math equivalents of System.Math.
    /// (Calling this just Math makes far too much mess.)
    /// </summary>
    public static class GenericMath
    {
        /// <summary>
        /// Returns the absolute value of a specified number.
        /// </summary>
        /// <typeparam name="T">Type to calculate with</typeparam>
        /// <param name="input">Input to return the absolute value of.</param>
        /// <returns>The input value if it is greater than or equal to the default value of T,
        /// or the negated input value otherwise</returns>
        public static T Abs<T>(T input)
        {
            return Operator<T>.GreaterThanOrEqual(input, default(T)) 
                ? input 
                : Operator<T>.Negate(input);
        }

        /// <summary>
        /// Returns whether or not two inputs are "close" to each other with respect to a given delta.
        /// </summary>
        /// <remarks>
        /// This implementation currently does no overflow checking - if (input1-input2) overflows, it
        /// could yield the wrong result.
        /// </remarks>
        /// <typeparam name="T">Type to calculate with</typeparam>
        /// <param name="input1">First input value</param>
        /// <param name="input2">Second input value</param>
        /// <param name="delta">Permitted range (exclusive)</param>
        /// <returns>True if Abs(input1-input2) is less than or equal to delta; false otherwise.</returns>
        public static bool WithinDelta<T>(T input1, T input2, T delta)
        {
            return Operator<T>.LessThanOrEqual(Abs(Operator<T>.Subtract(input1, input2)), delta);
        }
    }
}
#endif
