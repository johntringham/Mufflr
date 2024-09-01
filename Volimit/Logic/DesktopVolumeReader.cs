using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Volimit.Logic;
public class DesktopVolumeReader
{
    public float CurrentWasapiVolume { get; private set; }

    public event EventHandler VolumeChanged;

    public DesktopVolumeReader()
    {
        var capture = new CustomWasapiLoopbackCapture(1);
        capture.WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(48000, 1);
        //capture.WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(48000 / 32, 1);
        Monitor(capture);
    }

    private void Monitor(CustomWasapiLoopbackCapture capture)
    {
        capture.DataAvailable += CaptureDataAvailable;
        capture.StartRecording();
    }

    private void CaptureDataAvailable(object? sender, WaveInEventArgs args)
    {
        float max = 0;
        var buffer = new WaveBuffer(args.Buffer);
        // interpret as 32 bit floating point audio
        for (int index = 0; index < args.BytesRecorded / 4; index++)
        {
            var sample = buffer.FloatBuffer[index];

            // absolute value 
            if (sample < 0) sample = -sample;
            // is this the max value?
            if (sample > max) max = sample;
        }

        var volChanged = max != this.CurrentWasapiVolume;
        this.CurrentWasapiVolume = max;
        //Debug.WriteLine($"{max}");

        if(volChanged)
        {
            this.VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}