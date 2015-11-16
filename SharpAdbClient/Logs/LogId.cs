namespace SharpAdbClient.Logs
{
    // https://android.googlesource.com/platform/system/core/+/master/include/log/log.h#596
    public enum LogId
    {
        Main = 0,
        Radio = 1,
        Events = 2,
        System = 3,
        Crash = 4,
        Kernel = 5
    }
}
