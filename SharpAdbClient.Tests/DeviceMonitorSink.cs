using System;
using System.Collections.ObjectModel;
using System.Threading;

namespace SharpAdbClient.Tests
{
    internal class DeviceMonitorSink
    {
        public DeviceMonitorSink(DeviceMonitor monitor)
        {
            if (monitor == null)
            {
                throw new ArgumentNullException(nameof(monitor));
            }

            this.Monitor = monitor;
            this.Monitor.DeviceChanged += OnDeviceChanged;
            this.Monitor.DeviceConnected += OnDeviceConnected;
            this.Monitor.DeviceDisconnected += OnDeviceDisconnected;

            this.ChangedEvents = new Collection<DeviceDataEventArgs>();
            this.DisconnectedEvents = new Collection<DeviceDataEventArgs>();
            this.ConnectedEvents = new Collection<DeviceDataEventArgs>();
        }

        public Collection<DeviceDataEventArgs> DisconnectedEvents
        {
            get;
            private set;
        }

        public Collection<DeviceDataEventArgs> ConnectedEvents
        {
            get;
            private set;
        }

        public Collection<DeviceDataEventArgs> ChangedEvents
        {
            get;
            private set;
        }

        public DeviceMonitor Monitor
        {
            get;
            private set;
        }

        public ManualResetEvent CreateEventSignal()
        {
            ManualResetEvent signal = new ManualResetEvent(false);
            this.Monitor.DeviceChanged += (sender, e) => signal.Set();
            this.Monitor.DeviceConnected += (sender, e) => signal.Set();
            this.Monitor.DeviceDisconnected += (sender, e) => signal.Set();
            return signal;
        }

        protected virtual void OnDeviceDisconnected(object sender, DeviceDataEventArgs e)
        {
            this.DisconnectedEvents.Add(e);
        }

        protected virtual void OnDeviceConnected(object sender, DeviceDataEventArgs e)
        {
            this.ConnectedEvents.Add(e);
        }

        protected virtual void OnDeviceChanged(object sender, DeviceDataEventArgs e)
        {
            this.ChangedEvents.Add(e);
        }
    }
}
