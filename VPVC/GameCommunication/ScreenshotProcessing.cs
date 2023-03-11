using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using VPVC.BackendCommunication.Shared;
using Size = OpenCvSharp.Size;

#pragma warning disable CA1416

namespace VPVC.GameCommunication;

// see https://csharpexamples.com/fast-image-processing-c/

public static class ScreenshotProcessing {
    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    public static Tuple<int, Tuple<int, int>?>? ExtractGameStateAndRelativePlayerPosition(Bitmap inputBitmap) {
        BlockingCollection<Tuple<int, int>> mapPlayerMarkerPixelPositions = new BlockingCollection<Tuple<int, int>>();
        // BlockingCollection<Tuple<int, int>> possibleMapOutlinePixelPositions = new BlockingCollection<Tuple<int, int>>();
        
        Rectangle cropRect = new Rectangle(0, 0, (int) (inputBitmap.Width * 0.4), (inputBitmap.Height / 2));
        Bitmap croppedImageBitmap = inputBitmap.Clone(cropRect, inputBitmap.PixelFormat);

        unsafe {
            BitmapData bitmapData = croppedImageBitmap.LockBits(new Rectangle(0, 0, croppedImageBitmap.Width, croppedImageBitmap.Height), ImageLockMode.ReadWrite, croppedImageBitmap.PixelFormat);

            int bytesPerPixel = Image.GetPixelFormatSize(croppedImageBitmap.PixelFormat) / 8;
            int heightInPixels = bitmapData.Height;
            int width = bitmapData.Width;
            int widthInBytes = width * bytesPerPixel;
            byte* ptrFirstPixel = (byte*)bitmapData.Scan0;

            int minPixelsBetweenMapOutlinePixelCandidates = (int) (width * 0.01);
            
            int whitePixelCount = 0;
            int possibleLobbyBackgroundPixelCount = 0;
            
            int possibleAgentSelectBackgroundPixelCount = 0;
            int possibleAgentSelectTimerPixelCount = 0;
            
            var pr = Parallel.For(0, heightInPixels, yPosition => {
                byte* currentLine = ptrFirstPixel + (yPosition * bitmapData.Stride);

                int pixelsSinceLastBrightPixel = -1;
                int pixelsSinceLastPossibleMapOutlinePixel = -1;
                
                for (int xByteIndex = 0; xByteIndex < widthInBytes; xByteIndex += bytesPerPixel) {
                    int xPosition = (xByteIndex / bytesPerPixel);

                    int bluePixelValue = currentLine[xByteIndex];
                    int greenPixelValue = currentLine[xByteIndex + 1];
                    int redPixelValue = currentLine[xByteIndex + 2];
                    
                    int previousXByteIndex = xByteIndex - bytesPerPixel;

                    if (
                        (
                            (redPixelValue == 221 || redPixelValue == 222) &&
                            (greenPixelValue == 222 || greenPixelValue == 223) &&
                            (bluePixelValue == 142 || bluePixelValue == 143)
                        ) /* || (
                            (redPixelValue == 221) &&
                            (greenPixelValue == 222) &&
                            (bluePixelValue == 222)
                        ) */
                    ) {
                        // Probably the player's marker on the map
                        
                        mapPlayerMarkerPixelPositions.Add(new Tuple<int, int>(xPosition, yPosition));
                        
                        currentLine[previousXByteIndex] = 0;
                        currentLine[previousXByteIndex + 1] = 0;
                        currentLine[previousXByteIndex + 2] = 0;

                        continue;
                    }

                    if (xPosition < 1) {
                        continue;
                    }

                    if (redPixelValue == 255 && greenPixelValue == 255 && bluePixelValue == 255) {
                        whitePixelCount += 1;
                    } else if (
                        redPixelValue is >= 2 and <= 20 &&
                        greenPixelValue is >= 20 and <= 59 &&
                        bluePixelValue is >= 36 and <= 90
                    ) {
                        possibleLobbyBackgroundPixelCount += 1;
                    } else if (
                        redPixelValue is >= 55 and <= 134 &&
                        greenPixelValue is >= 178 and <= 210 &&
                        bluePixelValue is >= 237 and <= 242
                    ) {
                        possibleAgentSelectBackgroundPixelCount += 1;
                    } else if (
                        redPixelValue is >= 157 and <= 162 &&
                        greenPixelValue is >= 215 and <= 220 &&
                        bluePixelValue is >= 231 and <= 235
                    ) {
                        possibleAgentSelectTimerPixelCount += 1;
                    }

                    int previousBluePixelValue = currentLine[previousXByteIndex];
                    int previousGreenPixelValue = currentLine[previousXByteIndex + 1];
                    int previousRedPixelValue = currentLine[previousXByteIndex + 2];

                    currentLine[previousXByteIndex] = 0;
                    currentLine[previousXByteIndex + 1] = 0;
                    currentLine[previousXByteIndex + 2] = 0;

                    int redPixelDifference = redPixelValue - previousRedPixelValue;
                    int greenPixelDifference = greenPixelValue - previousGreenPixelValue;
                    int bluePixelDifference = bluePixelValue - previousBluePixelValue;

                    if (
                        pixelsSinceLastBrightPixel != -1 &&
                        pixelsSinceLastBrightPixel < 6 &&
                        (redPixelDifference <= -8) &&
                        (greenPixelDifference <= -8) &&
                        (bluePixelDifference <= -8)
                    ) {
                        if (pixelsSinceLastPossibleMapOutlinePixel == -1 || pixelsSinceLastPossibleMapOutlinePixel >= minPixelsBetweenMapOutlinePixelCandidates) {
                            /* possibleMapOutlinePixelPositions.Add(new Tuple<int, int>(
                                xPosition - 1,
                                yPosition
                            )); */
                            
                            currentLine[previousXByteIndex] = 255;
                            currentLine[previousXByteIndex + 1] = 255;
                            currentLine[previousXByteIndex + 2] = 255;
                        
                            pixelsSinceLastPossibleMapOutlinePixel = 0;

                            continue;
                        }
                        
                        pixelsSinceLastPossibleMapOutlinePixel = 0;
                    }
                    
                    if (
                        (redPixelDifference >= 8) &&
                        (greenPixelDifference >= 8) &&
                        (bluePixelDifference >= 8)
                    ) {
                        pixelsSinceLastBrightPixel = 0;
                    } else if (pixelsSinceLastBrightPixel == 6) {
                        pixelsSinceLastBrightPixel = -1;
                    } else if (pixelsSinceLastBrightPixel != -1) {
                        pixelsSinceLastBrightPixel++;
                    } else if (pixelsSinceLastPossibleMapOutlinePixel != -1) {
                        pixelsSinceLastPossibleMapOutlinePixel++;
                    }
                }
            });

            croppedImageBitmap.UnlockBits(bitmapData);
            
            Tuple<int, int>? mapPlayerMarkerPixelPosition;

            int allPixelCount = croppedImageBitmap.Width * croppedImageBitmap.Height;

            double whitePixelFractionOfAllPixels = ((double) whitePixelCount) / ((double) allPixelCount);
            double possibleLobbyBackgroundPixelFractionOfAllPixels = ((double) possibleLobbyBackgroundPixelCount) / ((double) allPixelCount);
            
            double possibleAgentSelectBackgroundPixelFractionOfAllPixels = ((double) possibleAgentSelectBackgroundPixelCount) / ((double) allPixelCount);
            double possibleAgentSelectTimerPixelFractionOfAllPixels = ((double) possibleAgentSelectTimerPixelCount) / ((double) allPixelCount);

            /* Logger.Log($"White pixel fraction of all pixels: {whitePixelFractionOfAllPixels}");
            Logger.Log($"Possible lobby background pixel fraction of all pixels: {possibleLobbyBackgroundPixelFractionOfAllPixels}");
            Logger.Log($"Possible agent select background pixel fraction of all pixels: {possibleAgentSelectBackgroundPixelFractionOfAllPixels}");
            Logger.Log($"Possible agent select timer pixel fraction of all pixels: {possibleAgentSelectTimerPixelFractionOfAllPixels}"); */

            DebuggingInformationHelper.lastScreenshotPossibleLobbyBackgroundPixelFraction = possibleLobbyBackgroundPixelFractionOfAllPixels;
            DebuggingInformationHelper.lastScreenshotPossibleAgentSelectBackgroundPixelFraction = possibleAgentSelectBackgroundPixelFractionOfAllPixels;
            DebuggingInformationHelper.lastScreenshotPossibleAgentSelectTimerPixelFraction = possibleAgentSelectTimerPixelFractionOfAllPixels;

            if (!mapPlayerMarkerPixelPositions.Any()) {
                if (whitePixelFractionOfAllPixels < 0.01) {
                    if (possibleLobbyBackgroundPixelFractionOfAllPixels >= 0.06) {
                        croppedImageBitmap.Dispose();
                        return new Tuple<int, Tuple<int, int>?>(GameStates.lobby, null);
                    }

                    if (possibleAgentSelectBackgroundPixelFractionOfAllPixels >= 0.06 && possibleAgentSelectTimerPixelFractionOfAllPixels >= 0.002) {
                        croppedImageBitmap.Dispose();
                        return new Tuple<int, Tuple<int, int>?>(GameStates.agentSelect, null);
                    }
                }

                croppedImageBitmap.Dispose();
                return null;
            } else if (mapPlayerMarkerPixelPositions.Count < 2) {
                mapPlayerMarkerPixelPosition = new Tuple<int, int>(
                    mapPlayerMarkerPixelPositions.First().Item1,
                    mapPlayerMarkerPixelPositions.First().Item2
                );
            } else {
                var sortedMapPlayerMarkerPixelPositionsX = mapPlayerMarkerPixelPositions
                    .OrderBy(position => position.Item1)
                    .Select(position => position.Item1)
                    .ToArray();
            
                var sortedMapPlayerMarkerPixelPositionsY = mapPlayerMarkerPixelPositions
                    .OrderBy(position => position.Item2)
                    .Select(position => position.Item2)
                    .ToArray();
                
                var mapPlayerMarkerWidth = sortedMapPlayerMarkerPixelPositionsX.First() - sortedMapPlayerMarkerPixelPositionsX.Last();
                var mapPlayerMarkerHeight = sortedMapPlayerMarkerPixelPositionsY.First() - sortedMapPlayerMarkerPixelPositionsY.Last();

                mapPlayerMarkerPixelPosition = new Tuple<int, int>(
                    sortedMapPlayerMarkerPixelPositionsX.Last() + (mapPlayerMarkerWidth / 2),
                    sortedMapPlayerMarkerPixelPositionsY.Last() + (mapPlayerMarkerHeight / 2)
                );
            }

            /* var possibleMapOutlinePixelPositionsArray = possibleMapOutlinePixelPositions.ToArray();
                
            Logger.LogVerbose("Player marker pixel position: " + mapPlayerMarkerPixelPosition);
            Logger.LogVerbose("Possible map outline pixel count: " + possibleMapOutlinePixelPositionsArray.Count());
            
            Logger.LogVerbose("Saving image...");

            croppedImageBitmap.Save(@"C:\Users\mrcl\Pictures\vpvc-processed.png");
            
            Logger.LogVerbose("Saved image."); */

            var detectedMapOutlineAndRelativePlayerPosition = DetectMapOutlineAndCalculateRelativePlayerPosition(croppedImageBitmap, mapPlayerMarkerPixelPosition);
            
            croppedImageBitmap.Dispose();
            
            return new Tuple<int, Tuple<int, int>?>(GameStates.inGame, detectedMapOutlineAndRelativePlayerPosition);
        }
    }

    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    private static Tuple<int, int>? DetectMapOutlineAndCalculateRelativePlayerPosition(Bitmap inputBitmap, Tuple<int, int> mapPlayerMarkerPixelPosition) {
        int imageScaleDownFactor = 2;
        
        using var sourceMat = inputBitmap.ToMat();
        using var resizedMat = sourceMat.Resize(new Size(inputBitmap.Width / imageScaleDownFactor, inputBitmap.Height / imageScaleDownFactor));
        using var edgeMap = resizedMat.Canny(
            1000,
            200,
            7
        );
        using var smoothedMap = edgeMap.GaussianBlur(new Size(3, 3), 3, 3);
        
        var circleSegments = smoothedMap.HoughCircles(
            HoughModes.Gradient,
            1,
            50,
            320,
            70,
            inputBitmap.Height / 2 / 5,
            inputBitmap.Height
        );

        if (circleSegments.Length < 1) {
            // Error
            return null;
        }

        var mapCircleSegment = circleSegments[0];

        var mapCircleStartX = (int) (mapCircleSegment.Center.X - mapCircleSegment.Radius);
        // var mapCircleEndX = (int) (mapCircleSegment.Center.X + mapCircleSegment.Radius);

        var mapCircleStartY = (int) (mapCircleSegment.Center.Y - mapCircleSegment.Radius);
        // var mapCircleEndY = (int) (mapCircleSegment.Center.Y + mapCircleSegment.Radius);

        int relativePlayerPositionX = (int) (((mapPlayerMarkerPixelPosition.Item1 / imageScaleDownFactor) - mapCircleStartX) / (mapCircleSegment.Radius * 2) * 100);
        int relativePlayerPositionY = (int) (((mapPlayerMarkerPixelPosition.Item2 / imageScaleDownFactor) - mapCircleStartY) / (mapCircleSegment.Radius * 2) * 100);

        if (relativePlayerPositionX < 0 || relativePlayerPositionX > 100 || relativePlayerPositionY < 0 || relativePlayerPositionY > 100) {
            // These values don't make sense, the image processing has likely done something wrong
            return null;
        }

        return new Tuple<int, int>(relativePlayerPositionX, relativePlayerPositionY);

        /* var outputMap = smoothedMap.CvtColor(ColorConversionCodes.GRAY2BGR); 

        outputMap.Circle(circleSegments[0].Center.ToPoint(), (int) circleSegments[0].Radius, Scalar.Red);
        
        outputMap.Rectangle(
            new Point(mapCircleStartX, mapCircleStartY),
            new Point(mapCircleEndX, mapCircleEndY),
            Scalar.Green
        );
        
        outputMap.Circle(new Point(mapPlayerMarkerPixelPosition.Item1 / imageScaleDownFactor, mapPlayerMarkerPixelPosition.Item2 / imageScaleDownFactor), 5, Scalar.Yellow, 2);

        Logger.LogVerbose("Circle segment count: " + circleSegments.Count());

        outputMap.ToBitmap().Save(@"C:\Users\mrcl\Pictures\vpvc-cv.png"); */
    }
}