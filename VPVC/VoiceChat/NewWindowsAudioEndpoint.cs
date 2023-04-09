using System;
using System.Linq;
using System.Threading;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
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
    private VolumeSampleProvider? volumeSampleProvider;

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

    private readonly int minimumMicrophoneBufferAverage = 30;

    public NewWindowsAudioEndpoint(bool isSourceEnabled = true, bool isSinkEnabled = true) {
        this.isSourceEnabled = isSourceEnabled;
        this.isSinkEnabled = isSinkEnabled;

        waveFormat = new WaveFormat(audioSampleRate, audioChannelCount);

        opusEncoder = new OpusEncoder(Application.VoIP, audioSampleRate, audioChannelCount) {
            VBR = true
        };
        
        opusDecoder = new OpusDecoder(audioSampleRate, audioChannelCount);

        if (isSinkEnabled) {
            InitializePlaybackDevice();
        }

        if (isSourceEnabled) {
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
        if (volumeSampleProvider != null) {
            var volumeDifference = volumeSampleProvider.Volume - volumeFraction;
            
            App.RunInBackground(() => {
                for (var i = 0; i < transitionTimeInMilliseconds; i++) {
                    volumeSampleProvider.Volume = Math.Clamp(volumeSampleProvider.Volume + (volumeDifference / transitionTimeInMilliseconds), 0f, 2f);
                    Thread.Sleep(1);
                }

                volumeSampleProvider.Volume = volumeFraction;
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
        waveProvider = new BufferedWaveProvider(waveFormat) {
            DiscardOnBufferOverflow = true
        };
        volumeSampleProvider = new VolumeSampleProvider(waveProvider.ToSampleProvider());
        waveOutEvent?.Init(volumeSampleProvider);
    }

    private void HandleLocalAudioSampleAvailable(object? sender, WaveInEventArgs args) {
        int average = args.Buffer.Take(args.BytesRecorded).Where((_, i) => i % 2 == 0).Select((_, i) => BitConverter.ToInt16(args.Buffer, i * 2)).Sum(s => Math.Abs(s)) / args.BytesRecorded;

        if (average < minimumMicrophoneBufferAverage) {
            return;
        }

        var encodedBytes = new byte[byteCountPerFrame];
        var encodedLength = opusEncoder.Encode(args.Buffer, args.BytesRecorded, encodedBytes, encodedBytes.Length);
        
        Array.Resize(ref encodedBytes, encodedLength);
        
        hasNewSamples?.Invoke(encodedBytes);
    }
}