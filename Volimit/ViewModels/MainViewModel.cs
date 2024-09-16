using System;
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

    private Task MainLoopTask;

    private DateTime lastTickTime = DateTime.Now;

    public TimeSpan UserInputWaitTime { get; set; } = TimeSpan.FromSeconds(2);

    public MainViewModel()
    {
        this.VolumeReader = new DesktopVolumeReader();
        this.VolumeSettings = new SystemVolumeSettings();
        this.VolumeReader.VolumeChanged += OnDesktopAudioHeard;

        this.MainLoopTask = RunMainLoop();
        this.VolumeSettings.VolumeSetByUser += this.OnUserSetVolume;

        this.IntendedVolume = this.VolumeSettings.CurrentSystemVolume ?? 0.3f;
    }

    private void OnUserSetVolume(object? sender, EventArgs e)
    {
        this.IntendedVolume = VolumeSettings.UserSetVolume;
        this.OnPropertyChanged(nameof(this.IntendedVolume));
    }

    private async Task? RunMainLoop()
    {
        while (true)
        {
            if (IsRunning)
            {
                Tick();
                await Task.Delay(1);
            }
            else
            {
                await Task.Delay(100);
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
}
