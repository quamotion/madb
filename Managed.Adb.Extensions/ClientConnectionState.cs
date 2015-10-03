// <copyright file="ClientConnectionState.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace Managed.Adb
{
    /// <summary>
    /// Determines the state of the connection.
    /// </summary>
    public enum ClientConnectionState
    {
        /// <summary>
        /// The connection is initializing.
        /// </summary>
        Init = 1,

        /// <summary>
        /// The response received from the client was not in the
        /// JDWP (Java Debug Wire Protocol) format, and the connection
        /// has been closed.
        /// </summary>
        NotJDWP = 2,

        /// <summary>
        /// A handshake has been sent to the client, and we are awaiting
        /// a response to the handshake.
        /// </summary>
        AwaitShake = 10,

        /// <summary>
        /// A DDM (Debug Monitor Server) packet has been sent to the client,
        /// and we are waiting for a response from the client.
        /// </summary>
        NeedDDMPacket = 11,

        /// <summary>
        /// The client is not DDM (Debug Monitor Server) aware, and 
        /// the connection has been closed.
        /// </summary>
        NotDDM = 12,

        /// <summary>
        /// The client is ready.
        /// </summary>
        Ready = 13,

        /// <summary>
        /// An I/O error has occurred during the handshake.
        /// </summary>
        Error = 20,

        /// <summary>
        /// The client has disconnected.
        /// </summary>
        Disconnected = 21,
    }
}
