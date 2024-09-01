using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volimit.Logic;

public class CustomWasapiLoopbackCapture : WasapiCapture
{
    //
    // Summary:
    //     Initialises a new instance of the WASAPI capture class
    public CustomWasapiLoopbackCapture(int bufferTimeMs)
        : this(GetDefaultLoopbackCaptureDevice(), bufferTimeMs)
    {
    }

    //
    // Summary:
    //     Initialises a new instance of the WASAPI capture class
    //
    // Parameters:
    //   captureDevice:
    //     Capture device to use
    public CustomWasapiLoopbackCapture(MMDevice captureDevice, int bufferTimeMs)
        : base(captureDevice, false, bufferTimeMs)
    {
    }

    //
    // Summary:
    //     Gets the default audio loopback capture device
    //
    // Returns:
    //     The default audio loopback capture device
    public static MMDevice GetDefaultLoopbackCaptureDevice()
    {
        return new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
    }

    //
    // Summary:
    //     Specify loopback
    protected override AudioClientStreamFlags GetAudioClientStreamFlags()
    {
        return AudioClientStreamFlags.Loopback | base.GetAudioClientStreamFlags();
    }
}
    

