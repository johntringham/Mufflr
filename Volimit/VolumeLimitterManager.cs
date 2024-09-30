﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volimit.Logic;

namespace Volimit
{
    class VolumeLimitterManager
    {
        public bool IsRunning { get; set; } = true;
        public float WasapiVolume { get; set; }
        public float SystemVolume { get; set; }

        public float ScaledWasapiIfNoCapping { get; set; }
        public float ScaledWasapi { get; set; }

        public float IntendedVolume { get; set; }

        public DesktopVolumeReader VolumeReader { get; set; }
        public SystemVolumeSettings VolumeSettings { get; set; }

        public float VolumeCap { get; set; } = 0.2f;

        public float BounceBackRate { get; set; } = 5f;

        public Task MainLoopTask;

        private DateTime lastTickTime = DateTime.Now;
        private DateTime lastUpdateTime = DateTime.Now;

        public TimeSpan UserInputWaitTime { get; set; } = TimeSpan.FromSeconds(2);
        public TimeSpan ReportBackTimeSpan { get; set; } = TimeSpan.FromSeconds(1f / 60f); // Update 60fps

        private Settings userSettings;
        private readonly CancellationToken cts;

        public event EventHandler<EventArgs> StuffChangedEvent;
        private void RaiseStuffChanged()
        {
            this.StuffChangedEvent?.Invoke(this, new EventArgs());
        }

        public VolumeLimitterManager(CancellationToken cts)
        {
            this.userSettings = UserSettingsManager.GetSettings();

            this.VolumeCap = userSettings.VolumeCap;

            this.VolumeSettings = new SystemVolumeSettings();

            this.VolumeSettings.DefaultDeviceChanged += OnDefaultDeviceChanged;

            StartVolumeReader();

            this.VolumeSettings.VolumeSetByUser += this.OnUserSetVolume;
            
            this.cts = cts;
        }

        private void StartVolumeReader()
        {
            this.VolumeReader = new DesktopVolumeReader();
            this.VolumeReader.VolumeChanged += OnDesktopAudioHeard;
            this.IntendedVolume = this.VolumeSettings.CurrentSystemVolume ?? 0.3f;
        }

        private void OnDefaultDeviceChanged(object? sender, EventArgs e)
        {
            this.VolumeReader.VolumeChanged -= OnDesktopAudioHeard;
            StartVolumeReader();
        }

        private void OnUserSetVolume(object? sender, EventArgs e)
        {
            this.IntendedVolume = VolumeSettings.UserSetVolume;
            RaiseStuffChanged();
        }

        public void StartMainLoop()
        {
            try
            {
                MainLoop();
            }
            catch (TaskCanceledException e)
            {
                // Task cancelled - exiting gracefully
            }
        }

        private void MainLoop()
        {
            while (true)
            {
                cts.ThrowIfCancellationRequested();

                if (IsRunning)
                {
                    Tick();

                    //Thread.Sleep(TimeSpan.Zero); // Needs to be low latency
                    Thread.Sleep(TimeSpan.FromMilliseconds(0.1)); // Needs to be low latency
                }
                else
                {
                    // Not running, can be slower
                    Thread.Sleep(TimeSpan.FromSeconds(0.1));

                    // It would be better to just end this task and restart it when the on/off switch is turned back
                    // on, but this lazy hack is easier.
                }
            }
        }

        private void Tick()
        {
            this.SystemVolume = VolumeSettings.PollCurrentSystemVolume() ?? 0;
            this.ScaledWasapi = this.WasapiVolume * this.SystemVolume;
            this.ScaledWasapiIfNoCapping = this.WasapiVolume * this.IntendedVolume;

            if (!InUserInputCooldown())
            {
                if (ScaledWasapi > VolumeCap)
                {
                    var newScaledSystemVolume = VolumeCap / WasapiVolume;
                    VolumeSettings.SetSystemVolume(newScaledSystemVolume);
                }
                else if (!this.SystemVolume.IsCloseTo(this.IntendedVolume))
                {
                    var dt = (float)(DateTime.Now - lastTickTime).TotalSeconds; // todo: is double better?
                    var lerpedVolume = Utils.Damp(this.SystemVolume, this.IntendedVolume, this.BounceBackRate, dt);

                    VolumeSettings.SetSystemVolume(lerpedVolume);
                }
            }

            lastTickTime = DateTime.Now;
            if (lastTickTime - lastUpdateTime > this.ReportBackTimeSpan)
            {
                this.RaiseStuffChanged();
                this.lastUpdateTime = lastTickTime;
            }
        }

        private bool InUserInputCooldown()
        {
            return VolumeSettings.UserSetTime != null && (DateTime.Now - VolumeSettings.UserSetTime.Value) < UserInputWaitTime;
        }

        private void OnDesktopAudioHeard(object? sender, System.EventArgs e)
        {
            this.WasapiVolume = VolumeReader.CurrentWasapiVolume;
        }

        internal void SaveUserSettings()
        {
            this.userSettings.VolumeCap = VolumeCap;
            UserSettingsManager.SaveSettings(this.userSettings);
        }
    }
}
