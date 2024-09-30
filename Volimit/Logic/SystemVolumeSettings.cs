using Avalonia.Animation;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Volimit.Utils;

namespace Volimit.Logic
{
    public class SystemVolumeSettings
    {
        public MMDevice? DefaultDevice { get; private set; }

        public float? CurrentSystemVolume => PollCurrentSystemVolume();

        private float? lastSetVolume = null;

        public event EventHandler<EventArgs> VolumeSetByUser;
        public event EventHandler<EventArgs> DefaultDeviceChanged;

        public float UserSetVolume { get; private set; }
        public DateTime? UserSetTime { get; private set; }

        public SystemVolumeSettings()
        {
            var devEnum = new MMDeviceEnumerator();
            this.DefaultDevice = devEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            var enumerator = new MMDeviceEnumerator();
            foreach (var wasapi in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                Debug.WriteLine($"{wasapi.DataFlow} {wasapi.FriendlyName} {wasapi.DeviceFriendlyName} {wasapi.State}");
            }

            if (DefaultDevice != null)
            {
                Debug.WriteLine($"{DefaultDevice.DataFlow} {DefaultDevice.FriendlyName} {DefaultDevice.DeviceFriendlyName} {DefaultDevice.State}");
            }
        }

        public float? PollCurrentSystemVolume()
        {
            var devEnum = new MMDeviceEnumerator();
            var prevDevice = this.DefaultDevice?.ID ?? "none";
            this.DefaultDevice = devEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            if (prevDevice != this.DefaultDevice.ID)
            {
                DefaultOutputDeviceChanged();
            }

            if (this.DefaultDevice != null)
            {
                var volume = this.DefaultDevice.AudioEndpointVolume;
                float leftVolumePercent = volume.Channels[0].VolumeLevelScalar;
                float rightVolumePercent = volume.Channels[1].VolumeLevelScalar;
                float masterVolumePercent = volume.MasterVolumeLevelScalar;

                if (lastSetVolume == null)
                {
                    UserSetVolume = masterVolumePercent;
                    lastSetVolume = masterVolumePercent;

                    RaiseUserVolumeSetByUser();
                }

                if (lastSetVolume != null && !lastSetVolume.Value.IsCloseTo(masterVolumePercent))
                {
                    this.UserSetTime = DateTime.Now;

                    this.UserSetVolume = masterVolumePercent;
                    this.lastSetVolume = UserSetVolume;

                    RaiseUserVolumeSetByUser();
                }

                return masterVolumePercent;
            }

            return null;
        }

        private void DefaultOutputDeviceChanged()
        {
            this.DefaultDeviceChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseUserVolumeSetByUser()
        {
            this.VolumeSetByUser?.Invoke(this, new EventArgs());
        }

        public void SetSystemVolume(float volume)
        {
            var devEnum = new MMDeviceEnumerator();
            this.DefaultDevice = devEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            if (this.DefaultDevice != null)
            {
                this.DefaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar = volume;
                this.lastSetVolume = volume;
            }
        }
    }
}
