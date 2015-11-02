using System;
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

        Queue<SyncCommand> SyncResponses
        { get; }

        Queue<byte[]> SyncData
        { get; }

        List<Tuple<SyncCommand, string>> SyncRequests
        { get; }
    }
}
