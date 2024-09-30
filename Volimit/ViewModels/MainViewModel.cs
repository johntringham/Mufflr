using System;
using System.Security.Principal;
using System.Threading.Tasks;
using Volimit.Logic;

namespace Volimit.ViewModels;

public partial class MainViewModel : ViewModelBase
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

    public TimeSpan UserInputWaitTime { get; set; } = TimeSpan.FromSeconds(2);

    private Settings settings;


    public MainViewModel()
    {
        this.settings = SettingsManager.GetSettings();

        this.VolumeCap = settings.VolumeCap;

        this.VolumeSettings = new SystemVolumeSettings();

        this.VolumeSettings.DefaultDeviceChanged += OnDefaultDeviceChanged;

        StartVolumeReader();

        this.MainLoopTask = RunMainLoop();
        this.VolumeSettings.VolumeSetByUser += this.OnUserSetVolume;
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
        this.OnPropertyChanged(nameof(this.IntendedVolume));
    }

    private async Task RunMainLoop()
    {
        while (true)
        {
            if (IsRunning)
            {
                Tick();

                // todo: is await the right thing to do here. should this just be another thread and have a thread.sleep
                await Task.Delay(1); // Needs to be low latency
            }
            else
            {
                // Not running, can be slower
                await Task.Delay(100); 

                // It would be better to just end this task and restart it when the on/off switch is turned back
                // on, but this lazy hack is easier.
            }
        }
    }

    private void Tick()
    {
        this.SystemVolume = VolumeSettings.PollCurrentSystemVolume() ?? 0;
        this.OnPropertyChanged(nameof(SystemVolume));

        this.ScaledWasapi = this.WasapiVolume * this.SystemVolume;
        this.ScaledWasapiIfNoCapping = this.WasapiVolume * this.IntendedVolume;
        this.OnPropertyChanged(nameof(this.ScaledWasapi));
        this.OnPropertyChanged(nameof(this.ScaledWasapiIfNoCapping));

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
    }

    private bool InUserInputCooldown()
    {
        return VolumeSettings.UserSetTime != null && (DateTime.Now - VolumeSettings.UserSetTime.Value) < UserInputWaitTime;
    }

    private void OnDesktopAudioHeard(object? sender, System.EventArgs e)
    {
        this.WasapiVolume = VolumeReader.CurrentWasapiVolume;
        this.OnPropertyChanged(nameof(WasapiVolume));
    }

    //TODO: this doesn't work now that we're systray based
    internal void OnUnloaded()
    {
        this.settings.VolumeCap = VolumeCap;
        SettingsManager.SaveSettings(this.settings);
    }
}
