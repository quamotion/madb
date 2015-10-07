using System;

namespace Managed.Adb
{
    public interface IAdbSocket : IDisposable
    {
        void SendAdbRequest(string request);

        AdbResponse ReadAdbResponse(bool readDiagString);

        void Read(byte[] data);

        string ReadString();
    }
}
