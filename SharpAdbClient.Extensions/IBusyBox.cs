//-----------------------------------------------------------------------
// <copyright file="IBusyBox.cs" company="Quamotion">
//     Copyright (c) 2015 Quamotion. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SharpAdbClient
{
    using System;
    using System.Collections.Generic;

    public interface IBusyBox
    {
        /// <include file='.\BusyBox.xml' path='/BusyBox/Available/*'/>
        bool Available { get; }

        /// <include file='.\BusyBox.xml' path='/BusyBox/Commands/*'/>
        List<string> Commands { get; }

        /// <include file='.\BusyBox.xml' path='/BusyBox/Device/*'/>
        IDevice Device { get; set; }

        /// <include file='.\BusyBox.xml' path='/BusyBox/Version/*'/>
        Version Version { get; }

        /// <include file='.\BusyBox.xml' path='/BusyBox/ExecuteRootShellCommand/*'/>
        void ExecuteRootShellCommand(string command, IShellOutputReceiver receiver, params object[] commandArgs);

        /// <include file='.\BusyBox.xml' path='/BusyBox/ExecuteShellCommand/*'/>
        void ExecuteShellCommand(string command, IShellOutputReceiver receiver, params object[] commandArgs);

        /// <include file='.\BusyBox.xml' path='/BusyBox/Install/*'/>
        bool Install(string busybox);

        /// <include file='.\BusyBox.xml' path='/BusyBox/Supports/*'/>
        bool Supports(string command);
    }
}