using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Windows.System.RemoteSystems;

namespace Aurora.Music.Core.Tools
{
    public class ProjectRome : IDisposable
    {
        private RemoteSystemWatcher watcher;
        private readonly List<RemoteSystem> m_deviceList = new List<RemoteSystem>();
        private readonly int waitTimeout = 5000;

        private AutoResetEvent waitHandle;

        private static ProjectRome current;
        public static ProjectRome Current
        {
            get
            {
                if (current == null)
                {
                    return new ProjectRome();
                }
                else
                {
                    return current;
                }
            }
        }

        private static List<IRemoteSystemFilter> MakeFilterList()
        {
            // construct an empty list
            var localListOfFilters = new List<IRemoteSystemFilter>();

            // construct a discovery type filter that only allows "proximal" connections:
            var discoveryFilter = new RemoteSystemDiscoveryTypeFilter(RemoteSystemDiscoveryType.Any);


            // construct a device type filter that only allows desktop and mobile devices:
            // For this kind of filter, we must first create an IIterable of strings representing the device types to allow.
            // These strings are stored as static read-only properties of the RemoteSystemKinds class.
            var listOfTypes = new List<string>
            {
                RemoteSystemKinds.Desktop,
                RemoteSystemKinds.Holographic,
                RemoteSystemKinds.Hub,
                RemoteSystemKinds.Laptop,
                RemoteSystemKinds.Tablet,
                RemoteSystemKinds.Xbox
            };

            // Put the list of device types into the constructor of the filter
            var kindFilter = new RemoteSystemKindFilter(listOfTypes);


            // construct an availibility status filter that only allows devices marked as available:
            var statusFilter = new RemoteSystemStatusTypeFilter(RemoteSystemStatusType.Available);

            // add the 3 filters to the list
            localListOfFilters.Add(discoveryFilter);
            localListOfFilters.Add(kindFilter);
            localListOfFilters.Add(statusFilter);

            // return the list
            return localListOfFilters;
        }

        public async Task<bool> WaitForFirstDeviceAsync()
        {
            return await Task.Run(() =>
            {
                if (m_deviceList.Count > 0)
                {
                    return true;
                }
                else
                {
                    waitHandle.Reset();
                    Task.Run(async () =>
                    {
                        await Task.Delay(waitTimeout);
                        waitHandle.Set();
                    });
                    waitHandle.WaitOne();
                    return m_deviceList.Count > 0;
                }
            });
        }


        // This method returns an open connection to a particular app service on a remote system.
        // param "remotesys" is a RemoteSystem object representing the device to connect to.
        public async Task<ValueSet> RequestRemoteResponseAsync(ValueSet querys, bool rollOver = false)
        {
            if (m_deviceList.Count == 0)
            {
                return null;
            }
            // Set up a new app service connection. The app service name and package family name that
            // are used here correspond to the AppServices UWP sample.
            var connection = new AppServiceConnection
            {
                AppServiceName = Consts.ProjectRomeService,
                PackageFamilyName = Consts.PackageFamilyName
            };

            foreach (var remotesys in m_deviceList)
            {
                // a valid RemoteSystem object is needed before going any further
                if (remotesys == null)
                {
                    continue;
                }

                // Create a remote system connection request for the given remote device
                var connectionRequest = new RemoteSystemConnectionRequest(remotesys);

                // "open" the AppServiceConnection using the remote request
                var status = await connection.OpenRemoteAsync(connectionRequest);

                // only continue if the connection opened successfully
                if (status != AppServiceConnectionStatus.Success)
                {
                    continue;
                }

                // send input and receive output in a variable
                var response = await connection.SendMessageAsync(querys);

                // check that the service successfully received and processed the message
                if (response.Status == AppServiceResponseStatus.Success)
                {
                    if (!rollOver)
                        // Get the data that the service returned:
                        return response.Message;
                    else
                    {
                        continue;
                    }
                }
            }
            return null;
        }

        public ProjectRome(int timeout = 5000)
        {
            if (current != null)
            {
                current.Dispose();
                current = null;
            }
            waitTimeout = timeout;
            waitHandle = new AutoResetEvent(false);
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            BuildDeviceListAsync();
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            current = this;
        }

        private async Task BuildDeviceListAsync()
        {
            var accessStatus = await RemoteSystem.RequestAccessAsync();

            if (accessStatus == RemoteSystemAccessStatus.Allowed)
            {
                watcher = GetWatcher();

                // Subscribing to the event raised when a new remote system is found by the watcher.
                watcher.RemoteSystemAdded += RemoteSystemWatcher_RemoteSystemAdded;

                // Subscribing to the event raised when a previously found remote system is no longer available.
                watcher.RemoteSystemRemoved += RemoteSystemWatcher_RemoteSystemRemoved;

                watcher.Start();
            }
            else
            {
                //throw new UnauthorizedAccessException("Remote system not allowed " + accessStatus);
            }
        }

        private void RemoteSystemWatcher_RemoteSystemRemoved(RemoteSystemWatcher sender, RemoteSystemRemovedEventArgs args)
        {
            var device = m_deviceList.Find(a => a.Id == args.RemoteSystemId);
            if (device != null)
                m_deviceList.Remove(device);
        }

        private void RemoteSystemWatcher_RemoteSystemAdded(RemoteSystemWatcher sender, RemoteSystemAddedEventArgs args)
        {
            waitHandle.Set();
            m_deviceList.Add(args.RemoteSystem);
        }

        private static RemoteSystemWatcher GetWatcher() => RemoteSystem.CreateWatcher(MakeFilterList());

        public void Dispose()
        {
            waitHandle.Dispose();
        }
    }
}
