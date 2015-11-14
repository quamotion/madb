using System;
using System.Collections.Generic;

namespace SharpAdbClient.Tests
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

        Queue<byte[]> SyncDataReceived
        { get; }

        Queue<byte[]> SyncDataSent
        { get; }

        List<Tuple<SyncCommand, string>> SyncRequests
        { get; }
    }
}
