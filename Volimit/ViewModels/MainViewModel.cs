using System;
using System.Threading.Tasks;
using Volimit.Logic;

namespace Volimit.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public string Greeting => "Welcome to Avalonia!";

    public float WasapiVolume { get; set; }
    public float SystemVolume { get; set; }
    public float ScaledWasapi { get; set; }

    public float IntendedVolume { get; set; }

    public DesktopVolumeReader VolumeReader { get; set; }
    public SystemVolumeSettings VolumeSettings { get; set; }

    public float VolumeCap { get; set; } = 0.1f;

    public float BounceBackRate { get; set; } = 0.01f;

    private Task MainLoopTask;

    public TimeSpan UserInputWaitTime { get; set; } = TimeSpan.FromSeconds(2);

    public MainViewModel()
    {
        this.VolumeReader = new DesktopVolumeReader();
        this.VolumeSettings = new SystemVolumeSettings();
        this.VolumeReader.VolumeChanged += OnDesktopAudioHeard;

        this.MainLoopTask = RunMainLoop();
    }

    private async Task? RunMainLoop()
    {
        while (true)
        {
            Tick();

            await Task.Delay(1);
        }
    }

    private void Tick()
    {
        this.SystemVolume = VolumeSettings.GetCurrentSystemVolume() ?? 0;
        this.OnPropertyChanged(nameof(SystemVolume));

        this.ScaledWasapi = this.WasapiVolume * this.SystemVolume;
        this.OnPropertyChanged(nameof(this.ScaledWasapi));

        if (!InUserInputCooldown())
        {
            if (ScaledWasapi > VolumeCap)
            {
                var newScaledSystemVolume = VolumeCap / WasapiVolume;
                VolumeSettings.SetSystemVolume(newScaledSystemVolume);
            }
            else if (this.SystemVolume < this.IntendedVolume)
            {
                var diff = this.IntendedVolume - this.SystemVolume;
                var increase = diff * this.BounceBackRate;

                VolumeSettings.SetSystemVolume(this.SystemVolume + increase);
            }
        }

        this.IntendedVolume = VolumeSettings.UserSetVolume;
        this.OnPropertyChanged(nameof(this.IntendedVolume));
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
