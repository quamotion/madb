using System;
using System.Collections.Generic;
using System.IO;

namespace SharpAdbClient.Tests
{
    public interface IDummyAdbSocket : IAdbSocket
    {
        Stream ShellStream
        { get; set; }

        Queue<AdbResponse> Responses
        { get; }

        Queue<string> ResponseMessages
        { get; }

        List<string> Requests
        { get; }

        Queue<SyncCommand> SyncResponses
        { get; }

        Queue<byte[]> SyncDataReceived
        { get; }

        Queue<byte[]> SyncDataSent
        { get; }

        List<Tuple<SyncCommand, string>> SyncRequests
        { get; }
    }
}
