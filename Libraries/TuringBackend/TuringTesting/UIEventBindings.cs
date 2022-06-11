using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TuringBackend.Networking;

namespace TuringBackend
{
    public delegate void ReceivedDataCallback(Packet Data);

    public static class UIEventBindings
    {
        public static EventHandler ClientSuccessConnecting;
        public static EventHandler ClientFailedConnecting;
        public static EventHandler RecievedErrorNotification;

        public static EventHandler UpdateFileBrowser;

        public static Dictionary<int, List<ReceivedDataCallback>> DataSubscribers = new Dictionary<int, List<ReceivedDataCallback>>();
    }
}
