using NAudio.Wave;
using SIPSorcery.Media;
using SIPSorceryMedia.Abstractions;

namespace VPVC.VoiceChat;

public delegate void NewWindowsAudioEndpointHasNewSamples(byte[] samples);

public class NewWindowsAudioEndpoint {
    public event NewWindowsAudioEndpointHasNewSamples? hasNewSamples;

    private int audioEncodingBitCount = 17;
    private int audioSampleRate = 44100;
    private int audioChannelCount = 1;

    private WaveInEvent? waveInEvent;
    private WaveOutEvent? waveOutEvent;

    private WaveFormat? waveFormat;

    private BufferedWaveProvider? waveProvider;

    private int audioInputDeviceIndex = -1;
    private int audioOutputDeviceIndex = -1;

    private bool isPaused = false;
    private bool isStarted = false;
    private bool isClosed = false;
    
    private readonly bool isSourceEnabled;
    private readonly bool isSinkEnabled;

    public NewWindowsAudioEndpoint(bool isSourceEnabled = true, bool isSinkEnabled = true) {
        this.isSourceEnabled = isSourceEnabled;
        this.isSinkEnabled = isSinkEnabled;

        waveFormat = new WaveFormat();

        // audioFormatManager.SetSelectedFormat(audioEncoder.SupportedFormats.MaxBy(x => x.ClockRate));

        // audioSampleRate = audioFormatManager.SelectedFormat.ClockRate;

        if (isSinkEnabled) {
            InitializePlaybackDevice();
        }

        if (isSourceEnabled) {
            // waveSourceFormat = new Mp3WaveFormat(audioSampleRate, audioEncodingBitCount, audioChannelCount);
            
            waveInEvent = new WaveInEvent();
            waveInEvent.WaveFormat = waveFormat;
            waveInEvent.BufferMilliseconds = 20;
            waveInEvent.NumberOfBuffers = 2;
            waveInEvent.DeviceNumber = audioInputDeviceIndex;
            waveInEvent.DataAvailable += HandleLocalAudioSampleAvailable;
        }
    }
    
    public void PauseAudio() {
        if (!isPaused) {
            isPaused = true;
            waveInEvent?.StopRecording();
        }
    }

    public void ResumeAudio() {
        if (isPaused) {
            isPaused = false;
            waveInEvent?.StartRecording();
        }
    }

    public void StartAudio() {
        if (!isStarted) {
            isStarted = true;
            waveOutEvent?.Play();
            waveInEvent?.StartRecording();
        }
    }

    public void CloseAudio() {
        if (!isClosed) {
            isClosed = true;
            waveOutEvent?.Stop();

            if (waveInEvent != null) {
                waveInEvent.DataAvailable -= HandleLocalAudioSampleAvailable;
                waveInEvent.StopRecording();
            }
        }
    }

    public void SetOutputVolume(float volumeFraction) {
        if (waveOutEvent != null) {
            waveOutEvent.Volume = volumeFraction;
        }
    }

    public void GotAudioRtp(byte[] payload) {
        if (waveProvider == null) {
            return;
        }
        
        waveProvider.AddSamples(payload, 0, payload.Length);
    }

    public AudioFormat GetAudioFormat() {
        if (waveFormat == null) {
            return AudioFormat.Empty;
        }
        
        return new AudioFormat(
            74,
            "VPVC-DAF",
            waveFormat.SampleRate,
            waveFormat.SampleRate,
            waveFormat.Channels,
            null
        );
    }

    private void InitializePlaybackDevice() {
        waveOutEvent?.Stop();
        waveOutEvent = new WaveOutEvent();
        waveOutEvent.DeviceNumber = audioOutputDeviceIndex;
        waveProvider = new BufferedWaveProvider(waveFormat);
        waveProvider.DiscardOnBufferOverflow = true;
        waveOutEvent?.Init(waveProvider);
    }

    // private long lastAudioSampleTimestamp = -1;

    private void HandleLocalAudioSampleAvailable(object? sender, WaveInEventArgs args) {
        /* var diff = (DateTime.Now.ToFileTime() - lastAudioSampleTimestamp) / 10000;
        lastAudioSampleTimestamp = DateTime.Now.ToFileTime();
        
        Logger.Log($"Milliseconds since last audio sample: {diff}"); */
        
        hasNewSamples?.Invoke(args.Buffer);
    }
}