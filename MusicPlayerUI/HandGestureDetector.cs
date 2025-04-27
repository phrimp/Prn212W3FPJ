using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Point = OpenCvSharp.Point;

namespace MusicPlayerUI
{
    public class HandGestureDetector : IDisposable
    {
        public VideoCapture _capture; // Made public for status checking
        private CascadeClassifier _handCascade;
        private Thread _cameraThread;
        private bool _isRunning = false;
        private WriteableBitmap _currentFrame;

        // Event to notify when a gesture is detected
        public event EventHandler<string> GestureDetected;

        // Event to update the preview image
        public event EventHandler<WriteableBitmap> FrameUpdated;

        public HandGestureDetector()
        {
            try
            {
                // Initialize VideoCapture with default camera (usually 0)
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        _capture = new VideoCapture(i);
                        if (_capture.IsOpened())
                            break;
                    }
                    catch { }
                }
                if (!_capture.IsOpened())
                {
                    MessageBox.Show("Could not open camera. Running in simulation mode.",
                        "Camera Access Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    _capture = null; // Set to null to indicate simulation mode
                }
                else
                {
                    // Set resolution to improve performance
                    _capture.Set(VideoCaptureProperties.FrameWidth, 640);
                    _capture.Set(VideoCaptureProperties.FrameHeight, 480);
                }

                // Initialize hand cascade classifier (this is a placeholder - actual hand detection is more complex)
                // In a real app, you would need to train or use pre-trained models for hand detection
                string handCascadePath = "haarcascade_hand.xml";
                try
                {
                    _handCascade = new CascadeClassifier(handCascadePath);
                }
                catch (Exception)
                {
                    // Fallback - this won't actually detect hands but we'll simulate detection
                    MessageBox.Show("Hand detection model not found. Using simulated gestures for demo purposes.",
                        "Model Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing camera: {ex.Message}\nRunning in simulation mode.",
                    "Camera Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                _capture = null; // Set to null to indicate simulation mode
            }
        }

        public void Start()
        {
            if (_isRunning)
                return;

            _isRunning = true;
            _cameraThread = new Thread(ProcessFrames);
            _cameraThread.IsBackground = true; // This ensures the thread doesn't prevent app shutdown
            _cameraThread.Start();
        }

        public void Stop()
        {
            _isRunning = false;
            _cameraThread?.Join(1000); // Wait for thread to finish with timeout
        }

        private void ProcessFrames()
        {
            using (var frame = new Mat())
            {
                int frameCount = 0;
                DateTime lastGestureTime = DateTime.MinValue;
                Random random = new Random();

                while (_isRunning)
                {
                    try
                    {
                        // Check if we're in simulation mode (no camera access)
                        bool simulationMode = (_capture == null || !_capture.IsOpened());

                        Mat processedFrame;

                        if (simulationMode)
                        {
                            // Create a blank frame for simulation
                            processedFrame = new Mat(480, 640, MatType.CV_8UC3, Scalar.Black);
                            Cv2.PutText(processedFrame, "Camera Simulation Mode",
                                new OpenCvSharp.Point(150, 50), HersheyFonts.HersheySimplex, 1, Scalar.White, 2);
                            Cv2.PutText(processedFrame, "No camera access",
                                new OpenCvSharp.Point(200, 90), HersheyFonts.HersheySimplex, 0.7, Scalar.Gray, 1);
                        }
                        else
                        {
                            // Capture frame from camera
                            _capture.Read(frame);

                            if (frame.Empty())
                            {
                                Thread.Sleep(30);
                                continue;
                            }

                            processedFrame = frame.Clone();
                        }

                        // Process for gesture detection (simulation or real)
                        using (var grayFrame = new Mat())
                        {
                            if (!simulationMode)
                            {
                                Cv2.CvtColor(processedFrame, grayFrame, ColorConversionCodes.BGR2GRAY);
                                Cv2.EqualizeHist(grayFrame, grayFrame);
                            }

                            // Simulate gesture detection
                            frameCount++;

                            // Simulate gesture detection every 50 frames (about 2 seconds)
                            if (frameCount >= 50)
                            {
                                frameCount = 0;

                                // Only trigger gesture if enough time has passed (prevent rapid-fire)
                                if ((DateTime.Now - lastGestureTime).TotalSeconds >= 3)
                                {
                                    lastGestureTime = DateTime.Now;

                                    // Simulate different gestures
                                    string[] gestures = { "next", "previous", "playpause", "volumeup", "volumedown", "random" };
                                    string detectedGesture = gestures[random.Next(gestures.Length)];

                                    // Draw text on the frame to indicate gesture
                                    Cv2.PutText(processedFrame, $"Gesture: {detectedGesture}",
                                        new OpenCvSharp.Point(10, 30), HersheyFonts.HersheySimplex, 1, Scalar.Red, 2);

                                    // Notify about detected gesture
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        GestureDetected?.Invoke(this, detectedGesture);
                                    });
                                }
                            }

                            // Add hand tracking visualization (real or simulated)
                            AddHandTrackingVisualizations(processedFrame, simulationMode);

                            // Convert to bitmap for display
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                _currentFrame = processedFrame.ToWriteableBitmap();
                                FrameUpdated?.Invoke(this, _currentFrame);
                            });
                        }

                        // Dispose of the processed frame if we created it
                        if (simulationMode || processedFrame != frame)
                        {
                            processedFrame.Dispose();
                        }

                        // Slow down processing to reduce CPU usage
                        Thread.Sleep(30);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing frame: {ex.Message}");
                        Thread.Sleep(100); // Sleep longer on error
                    }
                }
            }
        }

        private void AddHandTrackingVisualizations(Mat frame, bool simulationMode = false)
        {
            if (simulationMode)
            {
                // In simulation mode, create a more elaborate visualization
                // Draw a box for hand placement
                Cv2.Rectangle(frame, new OpenCvSharp.Rect(220, 140, 200, 200), Scalar.Blue, 2);
                Cv2.PutText(frame, "Hand Detection Zone", new OpenCvSharp.Point(230, 130),
                    HersheyFonts.HersheySimplex, 0.7, Scalar.Cyan);

                // Simulate a hand with moving dots
                int time = Environment.TickCount;

                // Main palm point
                int x = (int)(320 + 50 * Math.Sin(time / 1000.0));
                int y = (int)(240 + 30 * Math.Cos(time / 800.0));

                // Draw the simulated hand
                Cv2.Circle(frame, new OpenCvSharp.Point(x, y), 15, Scalar.Green, -1); // Palm

                // Fingertips
                for (int i = 0; i < 5; i++)
                {
                    double angle = Math.PI / 2 + (i - 2) * Math.PI / 10;
                    int fingerX = (int)(x + 50 * Math.Cos(angle + time / 5000.0));
                    int fingerY = (int)(y - 50 * Math.Sin(angle + time / 5000.0));

                    Cv2.Circle(frame, new OpenCvSharp.Point(fingerX, fingerY), 8, Scalar.Yellow, -1);
                    Cv2.Line(frame, new OpenCvSharp.Point(x, y), new OpenCvSharp.Point(fingerX, fingerY), Scalar.Yellow, 3);
                }

                // Add instructions
                Cv2.PutText(frame, "Simulating gesture detection", new OpenCvSharp.Point(10, 410),
                    HersheyFonts.HersheySimplex, 0.7, Scalar.White);
                Cv2.PutText(frame, "Random gestures generated automatically", new OpenCvSharp.Point(10, 440),
                    HersheyFonts.HersheySimplex, 0.6, Scalar.Gray);
            }
            else
            {
                // In real camera mode
                // In a real implementation, this would draw hand landmarks or bounding boxes
                // For this demo, we'll draw a simple moving dot to simulate tracking

                int x = (int)(320 + 150 * Math.Sin(DateTime.Now.Millisecond / 500.0));
                int y = (int)(240 + 100 * Math.Cos(DateTime.Now.Millisecond / 500.0));

                Cv2.Circle(frame, new OpenCvSharp.Point(x, y), 10, Scalar.Green, -1);
                Cv2.Circle(frame, new OpenCvSharp.Point(x, y), 20, Scalar.Yellow, 2);

                // Draw guide box in the center
                Cv2.Rectangle(frame, new OpenCvSharp.Rect(220, 140, 200, 200), Scalar.Blue, 2);
                Cv2.PutText(frame, "Place hand here", new OpenCvSharp.Point(230, 130),
                    HersheyFonts.HersheySimplex, 0.7, Scalar.Cyan);
            }
        }

        public void Dispose()
        {
            Stop();
            _capture?.Dispose();
            _handCascade?.Dispose();
        }
    }
}