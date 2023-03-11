using System;
using System.Threading;
using NAudio.Wave;
using OpusDotNet;

namespace VPVC.VoiceChat;

public delegate void NewWindowsAudioEndpointHasNewSamples(byte[] samples);

public class NewWindowsAudioEndpoint {
    public event NewWindowsAudioEndpointHasNewSamples? hasNewSamples;
    
    private int audioSampleRate = 48000;
    private int audioChannelCount = 2;

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

    private OpusEncoder opusEncoder;
    private OpusDecoder opusDecoder;

    private readonly int byteCountPerFrame = 160;

    public NewWindowsAudioEndpoint(bool isSourceEnabled = true, bool isSinkEnabled = true) {
        this.isSourceEnabled = isSourceEnabled;
        this.isSinkEnabled = isSinkEnabled;

        waveFormat = new WaveFormat(audioSampleRate, audioChannelCount);

        opusEncoder = new OpusEncoder(Application.VoIP, audioSampleRate, audioChannelCount) {
            VBR = true
        };
        
        opusDecoder = new OpusDecoder(audioSampleRate, audioChannelCount);

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

    public void SetOutputVolume(float volumeFraction, int transitionTimeInMilliseconds = 150) {
        if (waveOutEvent != null) {
            var volumeDifference = waveOutEvent.Volume - volumeFraction;
            
            App.RunInBackground(() => {
                for (var i = 0; i < transitionTimeInMilliseconds; i++) {
                    waveOutEvent.Volume += volumeDifference / (float) transitionTimeInMilliseconds;
                    Thread.Sleep(1);
                }

                waveOutEvent.Volume = volumeFraction;
            });
        }
    }

    public void GotAudioRtp(byte[] payload) {
        if (waveProvider == null) {
            return;
        }

        var decodedBytes = new byte[6000];
        var decodedLength = opusDecoder.Decode(payload, payload.Length, decodedBytes, decodedBytes.Length);
        
        Array.Resize(ref decodedBytes, decodedLength);
        
        waveProvider.AddSamples(decodedBytes, 0, decodedLength);
    }

    private void InitializePlaybackDevice() {
        waveOutEvent?.Stop();
        waveOutEvent = new WaveOutEvent();
        waveOutEvent.DesiredLatency = 60;
        waveOutEvent.DeviceNumber = audioOutputDeviceIndex;
        waveProvider = new BufferedWaveProvider(waveFormat);
        waveProvider.DiscardOnBufferOverflow = true;
        waveOutEvent?.Init(waveProvider);
    }

    private void HandleLocalAudioSampleAvailable(object? sender, WaveInEventArgs args) {
        var encodedBytes = new byte[byteCountPerFrame];
        var encodedLength = opusEncoder.Encode(args.Buffer, args.BytesRecorded, encodedBytes, encodedBytes.Length);
        
        Array.Resize(ref encodedBytes, encodedLength);
        
        hasNewSamples?.Invoke(encodedBytes);
    }
}