using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MedEasy.Mobile.Core.Device
{
    public class DeviceService
    {
        public Task OpenUri(Uri uri, CancellationToken ct = default)
        {
            Xamarin.Forms.Device.OpenUri(uri);

            return Task.CompletedTask;
        }
    }
}
