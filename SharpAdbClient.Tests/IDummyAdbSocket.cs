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

        /// <summary>
        /// Gets a value indicating whether the socket reconnected.
        /// </summary>
        bool DidReconnect
        { get; }

        /// <summary>
        /// If <see cref="false"/>, the socket will disconnect as soon as all data has been read. If <see cref="true"/>,
        /// the socket will wait for new messages to appear in the queue.
        /// </summary>
        bool WaitForNewData
        { get; set; }
    }
}
