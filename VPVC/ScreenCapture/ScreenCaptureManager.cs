using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using SharpGen.Runtime;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using VPVC.GameCommunication;

namespace VPVC.ScreenCapture; 

public class ScreenCaptureManager {
    private static ScreenCaptureManager? _instance = null;

    public static ScreenCaptureManager instance {
        get { return _instance ??= new ScreenCaptureManager(); }
    }

    private readonly IDXGIFactory1 dxgiFactory;
    private IDXGIAdapter? selectedAdapter;
    private IDXGIOutput? selectedOutput;
    private ID3D11Device? directDevice;
    private Texture2DDescription? textureDescription;
    private IDXGIOutputDuplication? outputDuplication;

    private bool hasInitialized = false;
    private bool isInitializing = false;
    
    private static readonly FeatureLevel[] adapterFeatureLevels = new[] {
        FeatureLevel.Level_11_0,
        FeatureLevel.Level_10_1,
        FeatureLevel.Level_10_0,
        FeatureLevel.Level_9_3,
        FeatureLevel.Level_9_2,
        FeatureLevel.Level_9_1,
    };

    private ScreenCaptureManager() {
        dxgiFactory = DXGI.CreateDXGIFactory1<IDXGIFactory1>();
    }
    
    public void SelectScreenWithDeviceId(string deviceId) {
        var adapterAndOutput = GetAdapterAndOutputForDeviceId(deviceId);

        selectedAdapter = adapterAndOutput!.Item1;
        selectedOutput = adapterAndOutput!.Item2;
    }

    public Bitmap? CaptureScreen() {
        var acquiredTexture = AcquireNextFrameTexture();

        if (acquiredTexture == null || directDevice == null) {
            return null;
        }

        var mappedSubresource = directDevice.ImmediateContext.Map(acquiredTexture, 0);

        var frameWidth = acquiredTexture.Description.Width;
        var frameHeight = acquiredTexture.Description.Height;

        var frameBitmap = new Bitmap(
            frameWidth,
            frameHeight,
            PixelFormat.Format32bppRgb
        );

        var frameBitmapData = frameBitmap.LockBits(
            new Rectangle(0, 0, frameWidth, frameHeight),
            ImageLockMode.WriteOnly,
            frameBitmap.PixelFormat
        );
        
        var sourcePointer = IntPtr.Add(mappedSubresource.DataPointer, 0);
        
        MemoryHelpers.CopyMemory(frameBitmapData.Scan0, sourcePointer, frameWidth * 4 /* bytes */ * frameHeight);
        
        frameBitmap.UnlockBits(frameBitmapData);
        
        directDevice.ImmediateContext.Unmap(acquiredTexture, 0);
        
        acquiredTexture.Dispose();

        return frameBitmap;
    }

    
    private ID3D11Texture2D? AcquireNextFrameTexture() {
        if (!hasInitialized) {
            Initialize();
        }

        var localTextureDescription = textureDescription;

        if (directDevice == null || outputDuplication == null || localTextureDescription == null) {
            return null;
        }

        var destinationTexture = directDevice.CreateTexture2D((Texture2DDescription) localTextureDescription);

        var frameAcquisitionResult = outputDuplication.AcquireNextFrame(250, out _, out var desktopResource);

        if (frameAcquisitionResult != Result.Ok) {
            return null;
        }

        var tempTexture = desktopResource.QueryInterface<ID3D11Texture2D>();
        
        directDevice.ImmediateContext.CopyResource(destinationTexture, tempTexture);
        
        tempTexture.Dispose();
        desktopResource.Dispose();

        outputDuplication.ReleaseFrame();

        return destinationTexture;
    }

    private void Initialize() {
        if (hasInitialized || isInitializing || selectedOutput == null) {
            return;
        }

        isInitializing = true;
        
        var outputDesktopCoordinates = selectedOutput.Description.DesktopCoordinates;
        var outputWidth = outputDesktopCoordinates.Right - outputDesktopCoordinates.Left;
        var outputHeight = outputDesktopCoordinates.Bottom - outputDesktopCoordinates.Top;
        
        textureDescription = new Texture2DDescription {
            CPUAccessFlags = CpuAccessFlags.Read,
            BindFlags = BindFlags.None,
            Format = Format.B8G8R8A8_UNorm,
            Width = outputWidth,
            Height = outputHeight,
            MiscFlags = ResourceOptionFlags.None,
            MipLevels = 1,
            ArraySize = 1,
            SampleDescription = { Count = 1, Quality = 0 },
            Usage = ResourceUsage.Staging
        };
        
        D3D11.D3D11CreateDevice(
            selectedAdapter,
            DriverType.Unknown,
            DeviceCreationFlags.None,
            adapterFeatureLevels,
            out directDevice
        );

        using var output1 = selectedOutput.QueryInterface<IDXGIOutput1>();

        outputDuplication = output1.DuplicateOutput(directDevice);

        hasInitialized = true;
        isInitializing = false;
    }

    private Tuple<IDXGIAdapter, IDXGIOutput>? GetAdapterAndOutputForDeviceId(string deviceId) {
        for (int adapterIndex = 0; dxgiFactory.EnumAdapters(adapterIndex, out var currentAdapter) == Result.Ok; adapterIndex++) {
            for (int outputIndex = 0; currentAdapter.EnumOutputs(outputIndex, out var currentOutput) == Result.Ok; outputIndex++) {
                if (!currentOutput.Description.AttachedToDesktop) {
                    continue;
                }

                if (currentOutput.Description.DeviceName == deviceId) {
                    return new Tuple<IDXGIAdapter, IDXGIOutput>(currentAdapter, currentOutput);
                }
            }
        }

        return null;
    }

    public List<ScreenInfo> GetScreens() {
        var allScreens = new List<ScreenInfo>();
        
        for (int adapterIndex = 0; dxgiFactory.EnumAdapters(adapterIndex, out var currentAdapter) == Result.Ok; adapterIndex++) {
            for (int outputIndex = 0; currentAdapter.EnumOutputs(outputIndex, out var currentOutput) == Result.Ok; outputIndex++) {
                if (!currentOutput.Description.AttachedToDesktop) {
                    continue;
                }
                
                var outputDesktopCoordinates = currentOutput.Description.DesktopCoordinates;
                var outputWidth = outputDesktopCoordinates.Right - outputDesktopCoordinates.Left;
                var outputHeight = outputDesktopCoordinates.Bottom - outputDesktopCoordinates.Top;

                allScreens.Add(new ScreenInfo(
                    currentOutput.Description.DeviceName,
                    $"Screen {currentOutput.Description.DeviceName.Replace("\\\\.\\DISPLAY", "")} ({outputWidth}x{outputHeight})"
                ));
            }
        }

        return allScreens;
    }
}