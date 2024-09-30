using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Volimit.Logic;

namespace Volimit.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public bool IsRunning { get; set; } = true;

    public bool RunAtStartupEnabled
    {
        get => UserSettingsManager.IsAutoStartup();
        set
        {
            UserSettingsManager.SetAutoStartup(value);
        }
    }
    public float WasapiVolume => this.Manager?.WasapiVolume ?? 0f;
    public float SystemVolume => this.Manager?.SystemVolume ?? 0f;

    public float ScaledWasapiIfNoCapping => this.Manager?.ScaledWasapiIfNoCapping ?? 0f;
    public float ScaledWasapi => this.Manager?.ScaledWasapi ?? 0f;

    private CancellationTokenSource cancelationTokenSource = new CancellationTokenSource();

    public float IntendedVolume
    {
        get => this.Manager?.IntendedVolume ?? 0.1f;
        set
        {
            this.Manager.IntendedVolume = value;
        }
    }

    public float VolumeCap
    {
        get => this.Manager?.VolumeCap ?? 0.05f;
        set
        {
            this.Manager.VolumeCap = value;
        }
    }

    public float BounceBackRate { get; set; } = 5f;
    internal VolumeLimitterManager Manager { get; set; }

    private Settings settings;

    private float intendedVolume;
    private float volumeCap = 0.2f;
    private bool runAtStartupEnabled = false;

    public MainViewModel()
    {
        var threadStart = new ThreadStart(RunManager);
        var thread = new Thread(threadStart);
        thread.Start();
    }

    private void RunManager()
    {
        this.Manager = new VolumeLimitterManager(this.cancelationTokenSource.Token);
        this.OnPropertyChanged(nameof(VolumeCap));
        this.OnPropertyChanged(nameof(IntendedVolume));

        this.Manager.StuffChangedEvent += StuffChanged;

        // Note: this method runs until the program is closed. Anything in this method after this line will not run.
        this.Manager.StartMainLoop();
    }

    private void StuffChanged(object? sender, EventArgs e)
    {
        this.OnPropertyChanged(nameof(IntendedVolume));
        this.OnPropertyChanged(nameof(VolumeCap));
        this.OnPropertyChanged(nameof(WasapiVolume));
        this.OnPropertyChanged(nameof(SystemVolume));
        this.OnPropertyChanged(nameof(ScaledWasapiIfNoCapping));
        this.OnPropertyChanged(nameof(ScaledWasapi));
    }

    internal void OnExit()
    {
        this.Manager.SaveUserSettings();

        this.cancelationTokenSource?.Cancel();
    }
}
