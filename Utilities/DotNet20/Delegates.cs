#if !DOTNET35
namespace System
{
    /// <summary>
    /// Delegate taking no parameters and returning no value.
    /// </summary>
    public delegate void Action();
    /// <summary>
    /// Generic delegate taking two parameters and returning no value.
    /// </summary>
    public delegate void Action<T1, T2>(T1 arg1, T2 arg2);
    /// <summary>
    /// Generic delegate taking three parameters and returning no value.
    /// </summary>
    public delegate void Action<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);
    /// <summary>
    /// Generic delegate taking four parameters and returning no value.
    /// </summary>
    public delegate void Action<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    /// <summary>
    /// Generic delegate taking no parameters and returning a value of the specified type.
    /// </summary>
    public delegate TResult Func<TResult>();
    /// <summary>
    /// Generic delegate taking one parameter and returning a value of the specified type.
    /// </summary>
    public delegate TResult Func<T, TResult>(T arg);
    /// <summary>
    /// Generic delegate taking two parameters and returning a value of the specified type.
    /// </summary>
    public delegate TResult Func<T1, T2, TResult>(T1 arg1, T2 arg2);
    /// <summary>
    /// Generic delegate taking three parameters and returning a value of the specified type.
    /// </summary>
    public delegate TResult Func<T1, T2, T3, TResult>(T1 arg1, T2 arg2, T3 arg3);
    /// <summary>
    /// Generic delegate taking four parameters and returning a value of the specified type.
    /// </summary>
    public delegate TResult Func<T1, T2, T3, T4, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
}
#endif