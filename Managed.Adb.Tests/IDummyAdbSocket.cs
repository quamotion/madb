using System.Collections.Generic;

namespace Managed.Adb.Tests
{
    public interface IDummyAdbSocket : IAdbSocket
    {
        Queue<AdbResponse> Responses
        { get; }

        Queue<string> ResponseMessages
        { get; }

        List<string> Requests
        { get; }
    }
}
