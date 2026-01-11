using App.Animations;
using LAYERS_OF_THE_ATMOSPHERE.Properties;
using NAudio.Wave;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WinForms;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Text;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LAYERS_OF_THE_ATMOSPHERE
{
    public partial class ScimulationLOTA : Form
    {
        //metadata
        PrivateFontCollection pfc = new PrivateFontCollection();
        //pfc.AddFontFile(@"C:\Users\user\Downloads\vcr-osd-mono\VCR_OSD_MONO_1.001[1].ttf");
        //Font defaultFont = new Font("IBM Plex Mono", 9, FontStyle.SemiBold | FontStyle.Italic);

        //first-order global parameters
        bool isActive = false;
        bool isActiveTool = false;
        bool isSimulating = false;
        bool isChanged = false;
        bool simState = true;
        private bool _isFullScreen;
        double[] simData = new double[5000];
        double[] simFrame = new double[5000];

        //double-order global parameters
        double altitudeKm = 0;

        //simulation params: Balloon
        Animator runningAnimation1a;
        Animator runningAnimation2a;
        Animator runningAnimation3a;
        Animator runningAnimation4a;
        Animator runningAnimation1b;
        Animator runningAnimation2b;
        Animator runningAnimation3b;
        Animator runningAnimation4b;
        bool balloonLimit = true;
        bool isPopped = false;
        bool isBoiled = false;
        double simAltitideKm;
        int simCounter = 0;
        double thick = 0.000172;
        double stretch = 30;
        double size = 0.29;
        double inner = 103000;

        //simulation params: Table


        //enums
        public enum AudioType
        {
            Wav,
            Mp3
        }

        // gg
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            UpdateFullScreenState();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateFullScreenState();
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            UpdateFullScreenState();
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            UpdateFullScreenState();
        }

        private void UpdateFullScreenState()
        {
            bool nowFullScreen = IsFullScreenNow();

            if (nowFullScreen == _isFullScreen)
                return;

            _isFullScreen = nowFullScreen;

            if (_isFullScreen)
                OnFullScreenEntered();
            else
                OnFullScreenExited();
        }

        private bool IsFullScreenNow()
        {
            // Case 1: Maximized (treat as fullscreen)
            if (WindowState == FormWindowState.Maximized)
                return true;

            // Case 2: Borderless fullscreen
            if (FormBorderStyle == FormBorderStyle.None)
            {
                Screen screen = Screen.FromControl(this);
                return Bounds.Width >= screen.Bounds.Width &&
                       Bounds.Height >= screen.Bounds.Height;
            }

            return false;
        }

        // on full screen event
        private void OnFullScreenEntered()
        {
            AtmosphereImage.Visible = false;
            CumulonimbusBtn.Visible = false;
            CumulusBtn.Visible = false;
            StratusBtn.Visible = false;
            AltocumulusBtn.Visible = false;
            CirrusBtn.Visible = false;
            WeatherBalloonBtn.Visible = false;
            MeteorBurnBtn.Visible = false;
            AuroraBtn.Visible = false;
            SateliteBtn.Visible = false;
            ExitBtn.Visible = false;
            ToolsBkg.Visible = false;
            ToolsBtn.Visible = false;
            ThermoBtn.Visible = false;
            ThermoInfo.Visible = false;
            ThermoUsableA.Visible = false;
            ThermoUsableB.Visible = false;
            BaroBtn.Visible = false;
            BaroInfo.Visible = false;
            BaroUsableA.Visible = false;
            BaroUsableB.Visible = false;
            GasAnaBtn.Visible = false;
            GasAnaInfo.Visible = false;
            GasAnaUsable.Visible = false;
            AnemoBtn.Visible = false;
            AnemoInfo.Visible = false;
            AnemoUsable.Visible = false;
            CumulonimbusInfo.Visible = false;
            CumulusInfo.Visible = false;
            CirrusInfo.Visible = false;
            AltocumulusInfo.Visible = false;
            StratusInfo.Visible = false;
            WeatherBalloonInfo.Visible = false;
            MeteorBurnInfo.Visible = false;
            AuroraInfo.Visible = false;
            SateliteBtn.Visible = false;
            SateliteInfo.Visible = false;
            EverestLine.Visible = false;
            OzoneLine.Visible = false;
            TropoLine.Visible = false;
            StratoLine.Visible = false;
            MesoLine.Visible = false;
            IonosLine.Visible = false;
            ThermoLine.Visible = false;
            ExosLine.Visible = false;
            EverestInfo.Visible = false;
            OzoneInfo.Visible = false;
            StratoInfo.Visible = false;
            MesoInfo.Visible = false;
            IonosInfo.Visible = false;
            ThermosInfo.Visible = false;
            ExosInfo.Visible = false;
            ErrorBoxScreen.Visible = true;
        }

        // on not full screen
        private void OnFullScreenExited()
        {
            AtmosphereImage.Visible = true;
            CumulonimbusBtn.Visible = true;
            CumulusBtn.Visible = true;
            StratusBtn.Visible = true;
            AltocumulusBtn.Visible = true;
            CirrusBtn.Visible = true;
            WeatherBalloonBtn.Visible = true;
            MeteorBurnBtn.Visible = true;
            AuroraBtn.Visible = true;
            SateliteBtn.Visible = true;
            ToolsBtn.Visible = true;
            ExitBtn.Visible = true;
            EverestLine.Visible = true;
            OzoneLine.Visible = true;
            TropoLine.Visible = true;
            StratoLine.Visible = true;
            MesoLine.Visible = true;
            IonosLine.Visible = true;
            ThermoLine.Visible = true;
            ExosLine.Visible = true;
            ErrorBoxScreen.Visible = false;
        }

        // allows looping sound
        public class LoopStream : WaveStream
        {
            private readonly WaveStream sourceStream;

            public LoopStream(WaveStream sourceStream)
            {
                this.sourceStream = sourceStream;
            }

            public override WaveFormat WaveFormat => sourceStream.WaveFormat;

            public override long Length => long.MaxValue;

            public override long Position
            {
                get => sourceStream.Position;
                set => sourceStream.Position = value;
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                int totalBytesRead = 0;

                while (totalBytesRead < count)
                {
                    int bytesRead = sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                    if (bytesRead == 0)
                    {
                        sourceStream.Position = 0; // restart
                    }
                    else
                    {
                        totalBytesRead += bytesRead;
                    }
                }

                return totalBytesRead;
            }
        }

        // function for playing sound
        public static class SoundPlayer
        {
            private static readonly List<IWavePlayer> activePlayers = new();

            public static void Play(Stream audioStream, AudioType type = AudioType.Wav)
            {
                Task.Run(() =>
                {
                    IWavePlayer output = new WaveOutEvent();
                    WaveStream reader = type switch
                    {
                        AudioType.Mp3 => new Mp3FileReader(audioStream),
                        _ => new WaveFileReader(audioStream), // default = WAV
                    };

                    output.Init(reader);
                    output.Play();

                    output.PlaybackStopped += (s, e) =>
                    {
                        output.Dispose();
                        reader.Dispose();
                    };
                });
            }

            public static IWavePlayer PlayLoop(string filePath)
            {
                var outputDevice = new WaveOutEvent();
                var audioFile = new AudioFileReader(filePath);
                var loop = new LoopStream(audioFile);

                outputDevice.Init(loop);
                outputDevice.Play();

                return outputDevice; // keep reference to stop later
            }
        }

        public static void CenterHorizontally(Control parent, Control child)
        {
            if (parent == null || child == null)
                return;

            child.Left = (parent.ClientSize.Width - child.Width) / 2;
        }

        public static class AnimationHelpers
        {
            // AnimationHelpers.AnimateMove(label1, 100, 110, 500, 510, 6000, 5, "Linear");
            public static Animator AnimateMove(Control control, int startX, int startY, int endX, int endY, int durationMs, string easingName = "Linear")
            {
                // Convert string ? EasingType
                if (!Enum.TryParse(easingName, true, out EasingType easing))
                {
                    easing = EasingType.Linear; // fallback
                }

                var startValues = new List<double> { startX, startY };
                var endValues = new List<double> { endX, endY };

                return new Animator()
                    .AddPath(easing, startValues, endValues, durationMs)
                    .SetFrameEvent(values =>
                    {
                        if (control.IsDisposed) return;

                        control.Invoke(() =>
                        {
                            control.Left = (int)values[0];
                            control.Top = (int)values[1];
                        });
                    })
                    .Start();
            }

            // AnimationHelpers.AnimateCustomMove(label1, 100, 110, 500, 510, 6000, v => Math.Sin(v));
            public static Animator AnimateCustomMove(Control control, int startX, int startY, int endX, int endY, int durationMs, Func<double, double> easingFunc)
            {
                var startValues = new List<double> { startX, startY };
                var endValues = new List<double> { endX, endY };

                return new Animator()
                    .AddPath(easingFunc, startValues, endValues, durationMs)
                    .SetFrameEvent(values =>
                    {
                        if (control.IsDisposed) return;

                        control.Invoke(() =>
                        {
                            control.Left = (int)values[0];
                            control.Top = (int)values[1];
                        });
                    })
                    .Start();
            }
        }

        // all of atmospheric data
        public static class AtmosphericGraphData
        {
            // Atmospheric data
            public static double Dinitrogen(double height)
            {
                double product = 0;
                height = 0.02 * height;

                if (height >= 0 && height < 0.026)
                {
                    product = Math.Abs(Math.Round(11.76 - (0.00271 * (height - 0.026)), 5));
                }
                else if (height >= 0.026 && height < 0.763)
                {
                    product = Math.Abs(Math.Round(11.76 - (0.00271 * (height - 0.026)), 5));
                }
                else if (height >= 0.763 && height < 1.506)
                {
                    product = Math.Abs(Math.Round(11.758 + (0.0162 * (height - 0.763)), 5));
                }
                else if (height >= 1.506 && height < 1.635)
                {
                    product = Math.Abs(Math.Round(11.77 + (0.0465 * (height - 1.506)), 5));
                }
                else if (height >= 1.635 && height < 1.752)
                {
                    product = Math.Abs(Math.Round(11.776 + (0.274 * (height - 1.635)), 5));
                }
                else if (height >= 1.752 && height < 1.856)
                {
                    product = Math.Abs(Math.Round(11.808 - (0.75 * (height - 1.752)), 5));
                }
                else if (height >= 1.856 && height < 1.957)
                {
                    product = Math.Abs(Math.Round(11.73 - (0.86 * (height - 1.856)), 5));
                }
                else if (height >= 2.174 && height < 2.92)
                {
                    product = Math.Abs(Math.Round(11.27 - (2.89 * (height - 2.174)), 5));
                }
                else if (height >= 1.957 && height < 2.174)
                {
                    product = Math.Abs(Math.Round(11.643 - (1.72 * (height - 1.957)), 5));
                }
                else if (height >= 2.92 && height < 3.444)
                {
                    product = Math.Abs(Math.Round(9.11 - (3.0763358 * (height - 2.92)), 5));
                }
                else if (height >= 3.444 && height < 3.92)
                {
                    product = Math.Abs(Math.Round(7.498 - (3.1345 * (height - 3.444)), 5));
                }
                else if (height >= 3.92 && height < 4.255)
                {
                    product = Math.Abs(Math.Round(6.006 - (3.03 * (height - 3.92)), 5));
                }
                else if (height >= 4.255 && height < 4.854)
                {
                    product = Math.Abs(Math.Round(4.99 - (2.67 * (height - 4.255)), 5));
                }
                else if (height >= 4.854 && height < 5.41)
                {
                    product = Math.Abs(Math.Round(3.39 - (2.045 * (height - 4.854)), 5));
                }
                else if (height >= 5.41 && height < 5.89)
                {
                    product = Math.Abs(Math.Round(2.253 - (1.53 * (height - 5.41)), 5));
                }
                else if (height >= 5.89 && height < 6.534)
                {
                    product = Math.Abs(Math.Round(1.52 - (1.048137 * (height - 5.89)), 5));
                }
                else if (height >= 6.534 && height < 7.124)
                {
                    product = Math.Abs(Math.Round(0.845 - (0.64 * (height - 6.534)), 5));
                }
                else if (height >= 7.124 && height < 7.95)
                {
                    product = Math.Abs(Math.Round(0.4674 - (0.36 * (height - 7.124)), 5));
                }
                else if (height >= 7.95 && height < 8.795)
                {
                    product = Math.Abs(Math.Round(0.16964 - (0.17 * (height - 7.95)), 5));
                }
                else if (height >= 8.795 && height < 9.5)
                {
                    product = Math.Abs(Math.Round(0.03 - (0.043 * (height - 8.795)), 5));
                }
                else
                {
                    product = 2147483647;
                }

                return Math.Round((100 / 15.07) * product, 2);
            }

            public static double Dioxygen(double height)
            {
                double product = 0;
                height = 0.02 * height;

                if (height >= 0 && height < 0.343)
                {
                    product = Math.Abs(Math.Round((-0.023 * height) + 3.125, 5));
                }
                else if (height >= 0.343 && height < 0.765)
                {
                    product = Math.Abs(Math.Round((0.019 * (height - 0.343)) + 3.117, 5));
                }
                else if (height >= 1.33 && height < 1.576)
                {
                    product = Math.Abs(Math.Round((-0.069 * (height - 1.33)) + 3.117, 5));
                }
                else if (height >= 1.576 && height < 1.75)
                {
                    product = Math.Abs(Math.Round((-0.316 * (height - 1.576)) + 3.1, 5));
                }
                else if (height >= 1.75 && height < 1.957)
                {
                    product = Math.Abs(Math.Round((-1.165 * (height - 1.75)) + 3.045, 5));
                }
                else if (height >= 1.957 && height < 2.38)
                {
                    product = Math.Abs(Math.Round((-2.80 * (height - 1.957)) + 2.804, 5));
                }
                else if (height >= 2.38 && height < 2.56)
                {
                    product = Math.Abs(Math.Round((-1.83 * (height - 2.38)) + 1.62, 5));
                }
                else if (height >= 2.56 && height < 2.843)
                {
                    product = Math.Abs(Math.Round((-1.00 * (height - 2.56)) + 1.29, 5));
                }
                else if (height >= 2.843 && height < 3.225)
                {
                    product = Math.Abs(Math.Round((-0.67 * (height - 2.843)) + 1.007, 5));
                }
                else if (height >= 3.225 && height < 3.722)
                {
                    product = Math.Abs(Math.Round((-0.51 * (height - 3.225)) + 0.75, 5));
                }
                else if (height >= 3.722 && height < 4.116)
                {
                    product = Math.Abs(Math.Round((-0.38325 * (height - 3.722)) + 0.496, 5));
                }
                else if (height >= 4.116 && height < 4.566)
                {
                    product = Math.Abs(Math.Round((-0.28445 * (height - 4.116)) + 0.345, 5));
                }
                else if (height >= 4.566 && height < 5.02)
                {
                    product = Math.Abs(Math.Round((-0.213657 * (height - 4.566)) + 0.217, 5));
                }
                else if (height >= 5.02 && height < 5.485)
                {
                    product = Math.Abs(Math.Round((-0.14194 * (height - 5.02)) + 0.12, 5));
                }
                else if (height >= 5.485 && height < 5.875)
                {
                    product = Math.Abs(Math.Round((-0.087 * (height - 5.485)) + 0.054, 5));
                }
                else if (height >= 5.875 && height < 6.3)
                {
                    product = Math.Abs(Math.Round((-0.047 * (height - 5.875)) + 0.02, 5));
                }
                else
                {
                    product = 2147483647;
                }

                return Math.Round((100 / 15.07) * product, 2);
            }

            public static double Argon(double height)
            {
                double product = 0;
                height = 0.02 * height;

                if (height >= 0 && height < 1.6)
                {
                    product = 0.1;
                }
                else if (height >= 1.6 && height < 1.75)
                {
                    product = Math.Abs(Math.Round((-0.027 * (height - 1.6)) + 0.1, 5));
                }
                else if (height >= 1.75 && height < 1.898)
                {
                    product = Math.Abs(Math.Round((-0.095 * (height - 1.75)) + 0.096, 5));
                }
                else if (height >= 1.898 && height < 2.03)
                {
                    product = Math.Abs(Math.Round((-0.189 * (height - 1.8977)) + 0.082, 5));
                }
                else if (height >= 2.03 && height < 2.164)
                {
                    product = Math.Abs(Math.Round((-0.1865325 * (height - 2.029975)) + 0.057, 5));
                }
                else if (height >= 2.164 && height < 2.304)
                {
                    product = Math.Abs(Math.Round((-0.136 * (height - 2.164)) + 0.032, 5));
                }
                else if (height >= 2.304 && height < 2.493)
                {
                    product = Math.Abs(Math.Round((-0.069 * (height - 2.30342)) + 0.013, 5));
                }

                else
                {
                    product = 2147483647;
                }

                return Math.Round((100 / 15.07) * product, 2);
            }

            public static double Oxygen(double height)
            {
                double product = 0;
                height = 0.02 * height;

                if (height >= 1.678 && height < 1.764)
                {
                    product = Math.Abs(Math.Round((0.349 * (height - 1.678)) + 0.002, 5));
                }
                else if (height >= 1.764 && height < 1.96)
                {
                    product = Math.Abs(Math.Round((1.969 * (height - 1.764)) + 0.032, 5));
                }
                else if (height >= 1.96 && height < 2.16)
                {
                    product = Math.Abs(Math.Round((4.74 * (height - 1.96)) + 0.418, 5));
                }
                else if (height >= 2.16 && height < 2.39)
                {
                    product = Math.Abs(Math.Round((5.6261 * (height - 2.16)) + 1.366, 5));
                }
                else if (height >= 2.39 && height < 2.476)
                {
                    product = Math.Abs(Math.Round((4.535 * (height - 2.39)) + 2.66, 5));
                }
                else if (height >= 2.476 && height < 2.67)
                {
                    product = Math.Abs(Math.Round((4.433 * (height - 2.476)) + 3.05, 5));
                }
                else if (height >= 2.67 && height < 2.93)
                {
                    product = Math.Abs(Math.Round((3.808 * (height - 2.67)) + 3.91, 5));
                }
                else if (height >= 2.93 && height < 3.2)
                {
                    product = Math.Abs(Math.Round((3.704 * (height - 2.93)) + 4.9, 5));
                }
                else if (height >= 3.2 && height < 3.467)
                {
                    product = Math.Abs(Math.Round((3.606 * (height - 3.2)) + 5.9, 5));
                }
                else if (height >= 3.467 && height < 3.738)
                {
                    product = Math.Abs(Math.Round((3.645 * (height - 3.467)) + 6.863, 5));
                }
                else if (height >= 3.738 && height < 3.96)
                {
                    product = Math.Abs(Math.Round((3.365 * (height - 3.738)) + 7.85, 5));
                }
                else if (height >= 3.96 && height < 4.233)
                {
                    product = Math.Abs(Math.Round((3.213 * (height - 3.96)) + 8.597, 5));
                }
                else if (height >= 4.233 && height < 4.47)
                {
                    product = Math.Abs(Math.Round((2.98 * (height - 4.233)) + 9.474, 5));
                }
                else if (height >= 4.47 && height < 4.757)
                {
                    product = Math.Abs(Math.Round((2.59 * (height - 4.47)) + 10.18, 5));
                }
                else if (height >= 4.757 && height < 5.028)
                {
                    product = Math.Abs(Math.Round((2.129 * (height - 4.757)) + 10.923, 5));
                }
                else if (height >= 5.028 && height < 5.36)
                {
                    product = Math.Abs(Math.Round((1.807 * (height - 5.028)) + 11.5, 5));
                }
                else if (height >= 5.36 && height < 5.738)
                {
                    product = Math.Abs(Math.Round((1.159 * (height - 5.36)) + 12.1, 5));
                }
                else if (height >= 5.738 && height < 6.09)
                {
                    product = Math.Abs(Math.Round((0.614 * (height - 5.738)) + 12.538, 5));
                }
                else if (height >= 6.09 && height < 6.333)
                {
                    product = Math.Abs(Math.Round((0.202 * (height - 6.09)) + 12.754, 5));
                }
                else if (height >= 6.333 && height < 6.566)
                {
                    product = Math.Abs(Math.Round((-0.249 * (height - 6.333)) + 12.803, 5));
                }
                else if (height >= 6.566 && height < 6.836)
                {
                    product = Math.Abs(Math.Round((-0.6 * (height - 6.566)) + 12.745, 5));
                }
                else if (height >= 6.836 && height < 7.116)
                {
                    product = Math.Abs(Math.Round((-1.021 * (height - 6.836)) + 12.583, 5));
                }
                else if (height >= 7.116 && height < 7.365)
                {
                    product = Math.Abs(Math.Round((-1.394 * (height - 7.116)) + 12.297, 5));
                }
                else if (height >= 7.365 && height < 7.65)
                {
                    product = Math.Abs(Math.Round((-1.895 * (height - 7.365)) + 11.95, 5));
                }
                else if (height >= 7.65 && height < 8)
                {
                    product = Math.Abs(Math.Round((-2.171 * (height - 7.65)) + 11.41, 5));
                }
                else if (height >= 8 && height < 8.2)
                {
                    product = Math.Abs(Math.Round((-2.6 * (height - 8)) + 10.65, 5));
                }
                else if (height >= 8.2 && height < 8.375)
                {
                    product = Math.Abs(Math.Round((-2.743 * (height - 8.2)) + 10.13, 5));
                }
                else if (height >= 8.375 && height < 8.52)
                {
                    product = Math.Abs(Math.Round((-3.007 * (height - 8.375)) + 9.65, 5));
                }
                else if (height >= 8.52 && height < 8.684)
                {
                    product = Math.Abs(Math.Round((-2.988 * (height - 8.52)) + 9.214, 5));
                }
                else if (height >= 8.684 && height < 8.84)
                {
                    product = Math.Abs(Math.Round((-3.167 * (height - 8.684)) + 8.724, 5));
                }
                else if (height >= 8.84 && height < 9.04)
                {
                    product = Math.Abs(Math.Round((-3.235 * (height - 8.84)) + 8.23, 5));
                }
                else if (height >= 9.04 && height < 9.533)
                {
                    product = Math.Abs(Math.Round((-3.133 * (height - 9.04)) + 7.583, 5));
                }
                else if (height >= 9.533 && height < 9.99)
                {
                    product = Math.Abs(Math.Round((-3.0065 * (height - 9.533)) + 6.04, 5));
                }
                else if (height >= 9.99 && height < 10.23)
                {
                    product = Math.Abs(Math.Round((-2.608 * (height - 9.99)) + 4.666, 5));
                }
                else if (height >= 10.23 && height < 10.424)
                {
                    product = Math.Abs(Math.Round((-2.474 * (height - 10.23)) + 4.04, 5));
                }
                else if (height >= 10.424 && height < 10.623)
                {
                    product = Math.Abs(Math.Round((-2.211 * (height - 10.424)) + 3.56, 5));
                }
                else if (height >= 10.623 && height < 10.877)
                {
                    product = Math.Abs(Math.Round((-2.008 * (height - 10.623)) + 3.12, 5));
                }
                else if (height >= 10.877 && height < 11.15)
                {
                    product = Math.Abs(Math.Round((-1.685 * (height - 10.877)) + 2.61, 5));
                }
                else if (height >= 11.15 && height < 11.48)
                {
                    product = Math.Abs(Math.Round((-1.412 * (height - 11.15)) + 2.15, 5));
                }
                else if (height >= 11.48 && height < 11.75)
                {
                    product = Math.Abs(Math.Round((-1.2 * (height - 11.48)) + 1.684, 5));
                }
                else if (height >= 11.75 && height < 11.997)
                {
                    product = Math.Abs(Math.Round((-0.959 * (height - 11.75)) + 1.36, 5));
                }
                else if (height >= 11.997 && height < 12.334)
                {
                    product = Math.Abs(Math.Round((-0.81 * (height - 11.997)) + 1.123, 5));
                }
                else if (height >= 12.334 && height < 12.774)
                {
                    product = Math.Abs(Math.Round((-0.591 * (height - 12.334)) + 0.85, 5));
                }
                else if (height >= 12.774 && height < 13.1)
                {
                    product = Math.Abs(Math.Round((-0.46 * (height - 12.774)) + 0.59, 5));
                }
                else if (height >= 13.1 && height < 13.477)
                {
                    product = Math.Abs(Math.Round((-0.329 * (height - 13.1)) + 0.44, 5));
                }
                else if (height >= 13.477 && height < 13.87)
                {
                    product = Math.Abs(Math.Round((-0.254 * (height - 13.477)) + 0.316, 5));
                }
                else if (height >= 13.87 && height < 14.39)
                {
                    product = Math.Abs(Math.Round((-0.179 * (height - 13.87)) + 0.216, 5));
                }
                else if (height >= 14.39 && height < 14.904)
                {
                    product = Math.Abs(Math.Round((-0.113 * (height - 14.39)) + 0.123, 5));
                }
                else if (height >= 14.904 && height < 15.443)
                {
                    product = Math.Abs(Math.Round((-0.083 * (height - 14.904)) + 0.065, 5));
                }
                else if (height >= 15.443 && height < 16.73)
                {
                    product = Math.Abs(Math.Round((-0.016 * (height - 15.443)) + 0.02, 5));
                }
                else
                {
                    product = 2147483647;
                }

                return Math.Round((100 / 15.07) * product, 2);
            }

            public static double Nitrogen(double height)
            {
                double product = 0;
                height = 0.02 * height;

                if (height >= 4 && height < 6.6)
                {
                    product = Math.Abs(Math.Round(0.00385 * (height - 4), 5));
                }
                else if (height >= 6.6 && height < 10.35)
                {
                    product = Math.Abs(Math.Round(-0.00267 * (height - 6.6) + 0.01, 5));
                }
                else
                {
                    product = 2147483647;
                }

                return Math.Round((100 / 15.07) * product, 2);
            }

            public static double Hydrogen(double height)
            {
                double product = 0;
                height = 0.02 * height;

                if (height >= 5.5 && height < 6.5)
                {
                    product = Math.Abs(Math.Round((0.01 * (height - 5.5)), 5));
                }
                else if (height >= 6.5 && height < 7.155)
                {
                    product = Math.Abs(Math.Round((0.038 * (height - 6.5)) + 0.01, 5));
                }
                else if (height >= 7.155 && height < 7.734)
                {
                    product = Math.Abs(Math.Round((0.123 * (height - 7.155)) + 0.035, 5));
                }
                else if (height >= 7.734 && height < 8.32)
                {
                    product = Math.Abs(Math.Round((0.195 * (height - 7.734)) + 0.106, 5));
                }
                else if (height >= 8.32 && height < 8.78)
                {
                    product = Math.Abs(Math.Round((0.254 * (height - 8.32)) + 0.22, 5));
                }
                else if (height >= 8.78 && height < 9.182)
                {
                    product = Math.Abs(Math.Round((0.291 * (height - 8.78)) + 0.337, 5));
                }
                else if (height >= 9.182 && height < 9.537)
                {
                    product = Math.Abs(Math.Round((0.327 * (height - 9.182)) + 0.454, 5));
                }
                else if (height >= 9.537 && height < 11.8)
                {
                    product = Math.Abs(Math.Round((0.377 * (height - 9.537)) + 0.57, 5));
                }
                else if (height >= 11.8 && height < 13.03)
                {
                    product = Math.Abs(Math.Round((0.381 * (height - 11.8)) + 1.422, 5));
                }
                else if (height >= 13.03 && height < 14.46)
                {
                    product = Math.Abs(Math.Round((0.412 * (height - 13.03)) + 1.89, 5));
                }
                else if (height >= 14.46 && height < 15.53)
                {
                    product = Math.Abs(Math.Round((0.467 * (height - 14.46)) + 2.48, 5));
                }
                else if (height >= 15.53 && height < 16.536)
                {
                    product = Math.Abs(Math.Round((0.504 * (height - 15.53)) + 2.98, 5));
                }
                else if (height >= 16.536 && height < 17.92)
                {
                    product = Math.Abs(Math.Round((0.558 * (height - 16.536)) + 3.487, 5));
                }
                else if (height >= 17.92 && height < 18.44)
                {
                    product = Math.Abs(Math.Round((0.583 * (height - 17.92)) + 4.26, 5));
                }
                else if (height >= 18.44 && height < 19.167)
                {
                    product = Math.Abs(Math.Round((0.629 * (height - 18.44)) + 4.563, 5));
                }
                else if (height >= 19.167 && height < 20)
                {
                    product = Math.Abs(Math.Round((0.588 * (height - 19.167)) + 5.02, 5));
                }
                else
                {
                    product = 2147483647;
                }

                return Math.Round((100 / 15.07) * product, 2);
            }

            public static double Helium(double height)
            {
                double product = 0;
                height = 0.02 * height;

                if (height >= 3 && height < 3.494)
                {
                    product = Math.Abs(Math.Round((0.02 * (height - 3)), 5));
                }
                else if (height >= 3.494 && height < 3.904)
                {
                    product = Math.Abs(Math.Round((0.024 * (height - 3.494)) + 0.01, 5));
                }
                else if (height >= 3.904 && height < 4.32)
                {
                    product = Math.Abs(Math.Round((0.106 * (height - 3.904)) + 0.02, 5));
                }
                else if (height >= 4.32 && height < 4.63)
                {
                    product = Math.Abs(Math.Round((0.194 * (height - 4.32)) + 0.064, 5));
                }
                else if (height >= 4.63 && height < 4.967)
                {
                    product = Math.Abs(Math.Round((0.279 * (height - 4.63)) + 0.124, 5));
                }
                else if (height >= 4.967 && height < 5.267)
                {
                    product = Math.Abs(Math.Round((0.357 * (height - 4.967)) + 0.218, 5));
                }
                else if (height >= 5.267 && height < 5.717)
                {
                    product = Math.Abs(Math.Round((0.509 * (height - 5.267)) + 0.325, 5));
                }
                else if (height >= 5.717 && height < 6.155)
                {
                    product = Math.Abs(Math.Round((0.726 * (height - 5.717)) + 0.554, 5));
                }
                else if (height >= 6.155 && height < 6.55)
                {
                    product = Math.Abs(Math.Round((0.978 * (height - 6.155)) + 0.872, 5));
                }
                else if (height >= 6.55 && height < 6.82)
                {
                    product = Math.Abs(Math.Round((1.315 * (height - 6.55)) + 1.258, 5));
                }
                else if (height >= 6.82 && height < 7.066)
                {
                    product = Math.Abs(Math.Round((1.48 * (height - 6.82)) + 1.613, 5));
                }
                else if (height >= 7.066 && height < 7.36)
                {
                    product = Math.Abs(Math.Round((1.78 * (height - 7.066)) + 1.977, 5));
                }
                else if (height >= 7.36 && height < 7.597)
                {
                    product = Math.Abs(Math.Round((1.995 * (height - 7.36)) + 2.5, 5));
                }
                else if (height >= 7.597 && height < 7.874)
                {
                    product = Math.Abs(Math.Round((2.249 * (height - 7.597)) + 2.973, 5));
                }
                else if (height >= 7.874 && height < 8.335)
                {
                    product = Math.Abs(Math.Round((2.612 * (height - 7.874)) + 3.596, 5));
                }
                else if (height >= 8.335 && height < 8.9)
                {
                    product = Math.Abs(Math.Round((2.92 * (height - 8.335)) + 4.8, 5));
                }
                else if (height >= 8.9 && height < 9.686)
                {
                    product = Math.Abs(Math.Round((2.907 * (height - 8.9)) + 6.45, 5));
                }
                else if (height >= 9.686 && height < 9.885)
                {
                    product = Math.Abs(Math.Round((2.708 * (height - 9.686)) + 8.734, 5));
                }
                else if (height >= 9.885 && height < 10.08)
                {
                    product = Math.Abs(Math.Round((2.395 * (height - 9.885)) + 9.273, 5));
                }
                else if (height >= 10.08 && height < 10.296)
                {
                    product = Math.Abs(Math.Round((2.296 * (height - 10.08)) + 9.74, 5));
                }
                else if (height >= 10.296 && height < 10.53)
                {
                    product = Math.Abs(Math.Round((1.983 * (height - 10.296)) + 10.236, 5));
                }
                else if (height >= 10.53 && height < 10.79)
                {
                    product = Math.Abs(Math.Round((1.654 * (height - 10.53)) + 10.7, 5));
                }
                else if (height >= 10.79 && height < 11.17)
                {
                    product = Math.Abs(Math.Round((1.403 * (height - 10.79)) + 11.13, 5));
                }
                else if (height >= 11.17 && height < 11.57)
                {
                    product = Math.Abs(Math.Round((1.018 * (height - 11.17)) + 11.663, 5));
                }
                else if (height >= 11.57 && height < 11.93)
                {
                    product = Math.Abs(Math.Round((0.736 * (height - 11.57)) + 12.07, 5));
                }
                else if (height >= 11.93 && height < 12.375)
                {
                    product = Math.Abs(Math.Round((0.416 * (height - 11.93)) + 12.335, 5));
                }
                else if (height >= 12.375 && height < 12.838)
                {
                    product = Math.Abs(Math.Round((0.194 * (height - 12.375)) + 12.52, 5));
                }
                else if (height >= 12.838 && height < 13.27)
                {
                    product = Math.Abs(Math.Round(12.61, 5));
                }
                else if (height >= 13.27 && height < 13.72)
                {
                    product = Math.Abs(Math.Round((-0.12 * (height - 13.27)) + 12.61, 5));
                }
                else if (height >= 13.72 && height < 14.223)
                {
                    product = Math.Abs(Math.Round((-0.231 * (height - 13.72)) + 12.556, 5));
                }
                else if (height >= 14.223 && height < 14.735)
                {
                    product = Math.Abs(Math.Round((-0.312 * (height - 14.223)) + 12.44, 5));
                }
                else if (height >= 14.735 && height < 15.29)
                {
                    product = Math.Abs(Math.Round((-0.36 * (height - 14.735)) + 12.28, 5));
                }
                else if (height >= 15.29 && height < 15.934)
                {
                    product = Math.Abs(Math.Round((-0.435 * (height - 15.29)) + 12.08, 5));
                }
                else if (height >= 15.934 && height < 17)
                {
                    product = Math.Abs(Math.Round((-0.497 * (height - 15.934)) + 11.8, 5));
                }
                else if (height >= 17 && height < 18.482)
                {
                    product = Math.Abs(Math.Round((-0.567 * (height - 17)) + 11.27, 5));
                }
                else if (height >= 18.482 && height < 20)
                {
                    product = Math.Abs(Math.Round((-0.646 * (height - 18.482)) + 10.43, 5));
                }
                else
                {
                    product = 2147483647;
                }

                return Math.Round((100 / 15.07) * product, 2);
            }

            // Pressure data
            public static double PressureAtHeight(double height)
            {
                if (height < 0)
                    return 0.0;

                double product = 0;
                height = height / 6d;

                if (height >= 0 && height < 0.0125)
                {
                    product = Math.Abs(Math.Round((-166.4 * height) + 20, 5));
                }
                else if (height >= 0.0125 && height < 0.025)
                {
                    product = Math.Abs(Math.Round((-193.6 * (height - 0.0125)) + 17.92, 5));
                }
                else if (height >= 0.025 && height < 0.05)
                {
                    product = Math.Abs(Math.Round((-80 * (height - 0.025)) + 15.5, 5));
                }
                else if (height >= 0.05 && height < 0.06)
                {
                    product = Math.Abs(Math.Round((-214 * (height - 0.05)) + 13.5, 5));
                }
                else if (height >= 0.06 && height < 0.08)
                {
                    product = Math.Abs(Math.Round((-124.5 * (height - 0.06)) + 11.36, 5));
                }
                else if (height >= 0.08 && height < 0.12)
                {
                    product = Math.Abs(Math.Round((-31.25 * (height - 0.08)) + 8.87, 5));
                }
                else if (height >= 0.12 && height < 0.2)
                {
                    product = Math.Abs(Math.Round((-15.25 * (height - 0.12)) + 7.62, 5));
                }
                else if (height >= 0.2 && height < 0.27)
                {
                    product = Math.Abs(Math.Round((-10.2 * (height - 0.2)) + 6.4, 5));
                }
                else if (height >= 0.27 && height < 0.42)
                {
                    product = Math.Abs(Math.Round((-4.84 * (height - 0.27)) + 5.686, 5));
                }
                else if (height >= 0.42 && height < 0.64)
                {
                    product = Math.Abs(Math.Round((-3.09 * (height - 0.42)) + 4.96, 5));
                }
                else if (height >= 0.64 && height < 0.97)
                {
                    product = Math.Abs(Math.Round((-1.73 * (height - 0.64)) + 4.28, 5));
                }
                else if (height >= 0.97 && height < 1.48)
                {
                    product = Math.Abs(Math.Round((-1.23 * (height - 0.97)) + 3.71, 5));
                }
                else if (height >= 1.48 && height < 2.12)
                {
                    product = Math.Abs(Math.Round((-0.8481 * (height - 1.4774)) + 3.085, 5));
                }
                else if (height >= 2.12 && height < 2.866)
                {
                    product = Math.Abs(Math.Round((-0.5362 * (height - 2.12)) + 2.54, 5));
                }
                else if (height >= 2.866 && height < 3.67)
                {
                    product = Math.Abs(Math.Round((-0.4 * (height - 2.866)) + 2.14, 5));
                }
                else if (height >= 3.67 && height < 4.52)
                {
                    product = Math.Abs(Math.Round((-0.2922 * (height - 3.66446)) + 1.82, 5));
                }
                else if (height >= 4.52 && height < 5.31)
                {
                    product = Math.Abs(Math.Round((-0.2848 * (height - 4.52)) + 1.57, 5));
                }
                else if (height >= 5.31 && height < 6.27)
                {
                    product = Math.Abs(Math.Round((-0.249 * (height - 5.31)) + 1.345, 5));
                }
                else if (height >= 6.27 && height < 7.09)
                {
                    product = Math.Abs(Math.Round((-0.239 * (height - 6.27)) + 1.106, 5));
                }
                else if (height >= 7.09 && height < 7.69)
                {
                    product = Math.Abs(Math.Round((-0.1935 * (height - 7.09)) + 0.91, 5));
                }
                else if (height >= 7.69 && height < 8.764)
                {
                    product = Math.Abs(Math.Round((-0.1807 * (height - 7.69)) + 0.794, 5));
                }
                else if (height >= 8.764 && height < 9.34)
                {
                    product = Math.Abs(Math.Round((-0.1459 * (height - 8.764)) + 0.6, 5));
                }
                else if (height >= 9.34 && height < 10.28)
                {
                    product = Math.Abs(Math.Round((-0.1723 * (height - 9.34)) + 0.516, 5));
                }
                else if (height >= 10.28 && height < 10.716)
                {
                    product = Math.Abs(Math.Round((-0.1009 * (height - 10.28)) + 0.354, 5));
                }
                else if (height >= 10.716 && height < 12.08)
                {
                    product = Math.Abs(Math.Round((-0.12463 * (height - 10.716)) + 0.31, 5));
                }
                else if (height >= 12.08 && height < 12.66)
                {
                    product = Math.Abs(Math.Round((-0.138 * (height - 12.08)) + 0.14, 5));
                }
                else if (height >= 12.66 && height <= 13.02)
                {
                    product = Math.Abs(Math.Round((-0.108 * (height - 12.66)) + 0.06, 5));
                }
                else
                {
                    product = 0;
                }

                //clamping because of decimal limitations
                if (product > 13.02)
                    product = 13.02;

                return Math.Round(product * (1200d / 15.42d), 2);

            }

            // Temperature data
            public static double TemperatureAtHeight(double height)
            {
                double product = 0;
                // scales the simulation height with graph height
                height = 1.905d * height;

                if (height >= 0 && height <= 21.61)
                {
                    product = Math.Round(3.235 * height - 15, 5);
                }
                else if (height > 21.61 && height <= 37.62)
                {
                    product = Math.Round(54.9, 5);
                }
                else if (height > 37.62 && height <= 59.67)
                {
                    product = Math.Round(-0.549 * (height - 37.62) + 54.9, 5);
                }
                else if (height > 59.67 && height <= 90.64)
                {
                    product = Math.Round(-1.352 * (height - 59.67) + 42.8, 5);
                }
                else if (height > 90.64 && height <= 97.6)
                {
                    product = Math.Round(0.9, 5);
                }
                else if (height > 97.6 && height <= 134.3)
                {
                    product = Math.Round(1.463 * (height - 97.6) + 0.9, 5);
                }
                else if (height > 134.3 && height <= 161.2)
                {
                    product = Math.Round(1.293 * (height - 134.3) + 54.5, 5);
                }
                else if (height > 161.2 && height <= 171.3)
                {
                    product = Math.Round(89.34, 5);
                }
                else if (height > 171.3 && height <= 189.8)
                {
                    product = Math.Round(-0.838 * (height - 171.3) + 89.34, 5);
                }
                else if (height > 189.8 && height <= 208.85)
                {
                    product = Math.Round(-2.395 * (height - 189.8) + 73.83, 5);
                }
                else if (height > 208.85 && height <= 228.6)
                {
                    product = Math.Round(-5.486 * (height - 208.85) + 28.2, 5);
                }
                else
                {
                    product = 10000;
                }

                if (product < 0)
                {
                    return Math.Abs(Math.Round(product, 2));
                }
                else
                {
                    return Math.Round(-1 * product, 2);
                }

            }

            // wind speed
            public static double WindSpeedAtHeight(double height)
            {
                double product;
                height = 20d - (height / 6d); // 39.6 / 6

                if (height < 0)
                    product = 10000;

                else if (height >= 0 && height < 0.423)
                    product = Math.Abs(Math.Round(0.0402 * height + 1.66, 5));

                else if (height < 0.8)
                    product = Math.Abs(Math.Round(0.855803 * height + 1.31516, 5));

                else if (height < 1.163)
                    product = Math.Abs(Math.Round(1.036 * height + 1.171, 5));

                else if (height < 1.42)
                    product = Math.Abs(Math.Round(1.6611 * height + 0.444, 5));

                else if (height < 1.74)
                    product = Math.Abs(Math.Round(2.15476 * height - 0.257, 5));

                else if (height < 2.12)
                    product = Math.Abs(Math.Round(2.58177 * height - 1.0, 5));

                else if (height < 2.41)
                    product = Math.Abs(Math.Round(2.98743 * height - 1.86, 5));

                else if (height < 2.76)
                    product = Math.Abs(Math.Round(2.7999 * height - 1.408, 5));

                else if (height < 2.927)
                    product = Math.Abs(Math.Round(1.73323 * height + 1.536, 5));

                else if (height < 3.155)
                    product = Math.Abs(Math.Round(0.97852 * height + 3.745, 5));

                else if (height < 3.377)
                    product = Math.Abs(Math.Round(0.3595 * height + 5.69916, 5));

                else if (height < 3.617)
                    product = Math.Abs(Math.Round(-0.400 * height + 8.26344, 5));

                else if (height < 3.823)
                    product = Math.Abs(Math.Round(-0.986 * height + 10.383, 5));

                else if (height < 3.977)
                    product = Math.Abs(Math.Round(-1.58553 * height + 12.675, 5));

                else if (height < 4.1)
                    product = Math.Abs(Math.Round(-2.1927715 * height + 15.090, 5));

                else if (height < 4.435)
                    product = Math.Abs(Math.Round(-3.826675 * height + 21.789, 5));

                else if (height < 4.6)
                    product = Math.Abs(Math.Round(-6.16287 * height + 32.150, 5));

                else if (height < 4.75)
                    product = Math.Abs(Math.Round(-17.18026 * height + 82.83, 5));

                else if (height < 4.835)
                    product = Math.Abs(Math.Round(-5.2369 * height + 26.0857, 5));

                else if (height < 4.953)
                    product = Math.Abs(Math.Round(-3.47688 * height + 17.5702, 5));

                else if (height < 5.126)
                    product = Math.Abs(Math.Round(-1.560 * height + 8.07589, 5));

                else if (height < 5.256)
                    product = Math.Abs(Math.Round(-0.569 * height + 2.996, 5));

                else if (height < 5.358)
                    product = Math.Abs(Math.Round(1.902 * height - 9.99157, 5));

                else if (height < 5.48)
                    product = Math.Abs(Math.Round(1.066 * height - 5.512282, 5));

                else if (height < 5.586)
                    product = Math.Abs(Math.Round(7.68 * height - 41.757, 5));

                else if (height < 5.984)
                    product = Math.Abs(Math.Round(5.3318 * height - 28.64, 5));

                else if (height < 6.42)
                    product = Math.Abs(Math.Round(6.9344 * height - 38.2, 5));

                else if (height < 6.71)
                    product = Math.Abs(Math.Round(7.13533333 * height - 39.4899, 5));

                else if (height < 6.91)
                    product = Math.Abs(Math.Round(5.29 * height - 27.10772, 5));

                else if (height < 7.12)
                    product = Math.Abs(Math.Round(4.571 * height - 22.08, 5));

                else if (height < 7.35)
                    product = Math.Abs(Math.Round(3.71004 * height - 15.95, 5));

                else if (height < 7.64)
                    product = Math.Abs(Math.Round(2.6188835 * height - 7.93, 5));

                else if (height < 7.94)
                    product = Math.Abs(Math.Round(1.43433 * height + 1.12142, 5));

                else if (height < 8.126)
                    product = Math.Abs(Math.Round(0.822580645 * height + 5.978709677, 5));

                else if (height <= 8.374)
                    product = Math.Abs(Math.Round(12.663, 5));

                else if (height <= 8.374)
                {
                    product = 0;
                }
                else if (height <= 8.977)
                {
                    product = Math.Round((-1.081562 * height) + 21.72, 5);
                }
                else if (height <= 11.36)
                {
                    product = Math.Round((-1.66082 * height) + 26.92, 5);
                }
                else if (height <= 11.96)
                {
                    product = Math.Round((-1.5358 * height) + 25.50, 5);
                }
                else if (height <= 13.14)
                {
                    product = Math.Round((-1.41039 * height) + 24.00, 5);
                }
                else if (height <= 14.365)
                {
                    product = Math.Round((-1.574 * height) + 26.15, 5);
                }
                else if (height <= 15.88)
                {
                    product = Math.Round((-1.5615 * height) + 25.93, 5);
                }
                else if (height <= 16.107)
                {
                    product = Math.Round((-1.665 * height) + 27.58, 5);
                }
                else if (height <= 16.286)
                {
                    product = Math.Round((-2.435 * height) + 39.9826, 5);
                }
                else if (height <= 16.46)
                {
                    product = Math.Round((-1.897 * height) + 31.225, 5);
                }
                else if (height <= 16.6)
                {
                    product = 0;
                }
                else if (height <= 16.8)
                {
                    product = Math.Round((1.7355422 * height) - 28.81, 5);
                }
                else if (height <= 17.03)
                {
                    product = Math.Round((3.983 * height) - 66.568, 5);
                }
                else if (height <= 17.3)
                {
                    product = Math.Round((4.209 * height) - 70.42, 5);
                }
                else if (height <= 17.54)
                {
                    product = Math.Round((2.8182 * height) - 46.36, 5);
                }
                else if (height <= 17.835)
                {
                    product = Math.Round((1.4915 * height) - 23.09, 5);
                }
                else if (height <= 18.217)
                {
                    product = Math.Round((0.51645 * height) - 5.70, 5);
                }
                else if (height <= 18.73)
                {
                    product = Math.Round((-0.493 * height) + 12.69, 5);
                }
                else if (height <= 19.083)
                {
                    product = Math.Round((-1.06214 * height) + 23.35, 5);
                }
                else if (height <= 19.264)
                {
                    product = Math.Round((-2.1663 * height) + 44.42, 5);
                }
                else if (height <= 20)
                {
                    product = Math.Round((-2.2416 * height) + 45.87, 5);
                }
                else
                {
                    product = 10000;
                }

                product = (70 / 13.13) * product;

                return Math.Round((product * 60 * 60) / (1000), 2);
            }

        }

        // Allows any elements to be dragged
        public static class DraggableHelper
        {
            private class DragState
            {
                public bool Enabled = true;
                public bool Dragging;
                public Point Offset;
                public MouseEventHandler MouseDown;
                public MouseEventHandler MouseMove;
                public MouseEventHandler MouseUp;
            }

            private static readonly Dictionary<Control, DragState> _states
                = new Dictionary<Control, DragState>();

            // Enable dragging on a control
            public static void Enable(Control ctrl)
            {
                if (_states.ContainsKey(ctrl))
                    return;

                var state = new DragState();

                state.MouseDown = (s, e) =>
                {
                    if (!state.Enabled || e.Button != MouseButtons.Left)
                        return;

                    state.Dragging = true;
                    state.Offset = e.Location;
                    ctrl.BringToFront();
                };

                state.MouseMove = (s, e) =>
                {
                    if (!state.Enabled || !state.Dragging)
                        return;

                    ctrl.Left += e.X - state.Offset.X;
                    ctrl.Top += e.Y - state.Offset.Y;
                };

                state.MouseUp = (s, e) =>
                {
                    state.Dragging = false;
                };

                ctrl.MouseDown += state.MouseDown;
                ctrl.MouseMove += state.MouseMove;
                ctrl.MouseUp += state.MouseUp;

                _states[ctrl] = state;
            }

            // Disable dragging but keep handlers attached
            public static void Disable(Control ctrl)
            {
                if (_states.TryGetValue(ctrl, out var state))
                {
                    state.Enabled = false;
                }
            }

            // Re-enable dragging
            public static void EnableDrag(Control ctrl)
            {
                if (_states.TryGetValue(ctrl, out var state))
                {
                    state.Enabled = true;
                }
            }

            // Completely remove dragging support
            public static void Remove(Control ctrl)
            {
                if (!_states.TryGetValue(ctrl, out var state))
                    return;

                ctrl.MouseDown -= state.MouseDown;
                ctrl.MouseMove -= state.MouseMove;
                ctrl.MouseUp -= state.MouseUp;

                _states.Remove(ctrl);
            }

            // Optional cleanup for disposed controls
            public static void Cleanup()
            {
                var toRemove = new List<Control>();

                foreach (var kv in _states)
                {
                    if (kv.Key.IsDisposed)
                        toRemove.Add(kv.Key);
                }

                foreach (var ctrl in toRemove)
                    _states.Remove(ctrl);
            }
        }

        // allows buttons to be moved

        public static class MouseWheelMover
        {
            // Store handlers so we can remove them later
            private static readonly Dictionary<Control, MouseEventHandler> _handlers = new();

            public static void Enable(
                Control control,
                Control toolcont1a = null,
                Control toolcont1b = null,
                Control toolcont2a = null,
                Control toolcont2b = null,
                Control toolcont3 = null,
                Control toolcont4 = null,
                int floorBottomY = 0,
                int ceilingTopY = 0,
                int speed = 30)
            {
                if (control == null)
                    throw new ArgumentNullException(nameof(control));

                if (ceilingTopY > floorBottomY)
                    throw new ArgumentException("ceilingTopY must be <= floorBottomY");

                // Prevent double-enable
                Disable(control);

                MouseEventHandler handler = (s, e) =>
                {
                    int delta = (e.Delta / 120) * speed;
                    int newTop = control.Top - delta;

                    // Ceiling
                    if (newTop < ceilingTopY)
                        newTop = ceilingTopY;

                    // Floor
                    int newBottom = newTop + control.Height;
                    if (newBottom > floorBottomY)
                        newTop = floorBottomY - control.Height;

                    control.Top = newTop;

                    if (control.Name == "AtmosphereImage" &&
                        toolcont1a != null && toolcont1b != null &&
                        toolcont2a != null && toolcont2b != null &&
                        toolcont3 != null && toolcont4 != null)
                    {
                        int absTop = Math.Abs(newTop);

                        toolcont1a.Top = absTop + 340;
                        toolcont1b.Top = absTop + 538;
                        toolcont2a.Top = absTop + 488;
                        toolcont2b.Top = absTop + 362;
                        toolcont3.Top = absTop + 246;
                        toolcont4.Top = absTop + 288;
                    }
                };

                _handlers[control] = handler;
                control.MouseWheel += handler;

                // Ensure mouse wheel works
                control.MouseEnter += (_, __) => control.Focus();
            }

            // Stops scrolling
            public static void Disable(Control control)
            {
                if (control == null)
                    return;

                if (_handlers.TryGetValue(control, out var handler))
                {
                    control.MouseWheel -= handler;
                    _handlers.Remove(control);
                }
            }
        }

        // easy access for doublebuffering
        public static void EnableDoubleBuffering(Control control)
        {
            typeof(Control)
                .GetProperty("DoubleBuffered",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance)
                ?.SetValue(control, true, null);
        }

        // mouse cursor and element catching
        public static Point GetDistanceFromParent(Control parent, Control element)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            // Convert element's location to screen coordinates
            Point elementOnScreen = element.PointToScreen(Point.Empty);

            // Convert parent's top-left to screen coordinates
            Point parentOnScreen = parent.PointToScreen(Point.Empty);

            // Distance relative to parent's top-left
            return new Point(
                elementOnScreen.X - parentOnScreen.X,
                elementOnScreen.Y - parentOnScreen.Y
            );
        }

        Point GetMouseRelativeToControl(Control control)
        {
            // Mouse position in screen coordinates
            Point mouseScreenPos = System.Windows.Forms.Cursor.Position;

            // Convert to control-relative coordinates
            Point mouseClientPos = control.PointToClient(mouseScreenPos);

            return mouseClientPos;
        }

        public static double ExponentialHeightSpeed(int height, double change = 4.91)
        {
            return 0.0865580448065d + 0.1182133269625d * (Math.Exp(change * ((3854 - Convert.ToDouble(height)) / 3516)) - 1) / (Math.Exp(change) - 1);
        }

        public static class LivePlotHelper
        {
            private static readonly Dictionary<FormsPlot, DataLogger> _loggers = new();

            /// <summary>
            /// Adds a point to the plot and automatically connects it.
            /// </summary>
            public static void AddPoint(
                FormsPlot formsPlot,
                double x,
                double y,
                ScottPlot.Color? color = null,    // optional color for this line
                bool refresh = true
            )
            {
                // Initialize DataLogger once
                if (!_loggers.ContainsKey(formsPlot))
                {
                    var logger = formsPlot.Plot.Add.DataLogger();

                    logger.LineWidth = 2;                // visible line
                    logger.MarkerSize = 5;               // visible marker
                    logger.MarkerShape = ScottPlot.MarkerShape.FilledCircle;

                    if (color.HasValue)
                        logger.Color = color.Value;
                    else
                        logger.Color = ScottPlot.Colors.Red;       // default color

                    _loggers[formsPlot] = logger;

                    // Optional axis labels
                    formsPlot.Plot.XLabel("X");
                    formsPlot.Plot.YLabel("Y");
                }

                // Add point
                _loggers[formsPlot].Add(x, y);

                if (refresh)
                    formsPlot.Refresh();
            }
        }


        // INITIALIZATION
        public ScimulationLOTA()
        {
            InitializeComponent();

        }

        private void ScimulationLOTA_Load(object sender, EventArgs e)
        {
            AtmosphereImage.SizeMode = PictureBoxSizeMode.Zoom;
            AuroraBtn.SizeMode = PictureBoxSizeMode.Zoom;

            EnableDoubleBuffering(ExitBtn);
            EnableDoubleBuffering(ToolsBtn);
            EnableDoubleBuffering(ToolsBkg);
            EnableDoubleBuffering(ThermoBtn);
            EnableDoubleBuffering(BaroBtn);
            EnableDoubleBuffering(GasAnaBtn);
            EnableDoubleBuffering(AtmosphereImage);
            EnableDoubleBuffering(CumulonimbusBtn);
            EnableDoubleBuffering(CumulusBtn);
            EnableDoubleBuffering(StratusBtn);
            EnableDoubleBuffering(AltocumulusBtn);
            EnableDoubleBuffering(CirrusBtn);
            EnableDoubleBuffering(MeteorBurnBtn);
            EnableDoubleBuffering(AuroraBtn);
            EnableDoubleBuffering(SimObjectImageA);

            // set parent to the background atmosphere
            CumulonimbusBtn.Parent = AtmosphereImage;
            CumulonimbusInfo.Parent = AtmosphereImage;
            CumulusBtn.Parent = AtmosphereImage;
            CumulusInfo.Parent = AtmosphereImage;
            AltocumulusBtn.Parent = AtmosphereImage;
            AltocumulusInfo.Parent = AtmosphereImage;
            CirrusBtn.Parent = AtmosphereImage;
            CirrusInfo.Parent = AtmosphereImage;
            StratusBtn.Parent = AtmosphereImage;
            StratusInfo.Parent = AtmosphereImage;
            MeteorBurnBtn.Parent = AtmosphereImage;
            MeteorBurnInfo.Parent = AtmosphereImage;
            AuroraBtn.Parent = AtmosphereImage;
            AuroraInfo.Parent = AtmosphereImage;
            WeatherBalloonBtn.Parent = AtmosphereImage;
            WeatherBalloonInfo.Parent = AtmosphereImage;
            ThermoUsableA.Parent = AtmosphereImage;
            ThermoUsableB.Parent = AtmosphereImage;
            BaroUsableA.Parent = AtmosphereImage;
            BaroUsableB.Parent = AtmosphereImage;
            GasAnaUsable.Parent = AtmosphereImage;
            AnemoUsable.Parent = AtmosphereImage;
            EverestLine.Parent = AtmosphereImage;
            EverestInfo.Parent = AtmosphereImage;
            SateliteBtn.Parent = AtmosphereImage;
            SateliteInfo.Parent = AtmosphereImage;
            TropoLine.Parent = AtmosphereImage;
            TropoInfo.Parent = AtmosphereImage;
            StratoLine.Parent = AtmosphereImage;
            MesoLine.Parent = AtmosphereImage;
            ThermoLine.Parent = AtmosphereImage;
            ExosLine.Parent = AtmosphereImage;
            OzoneLine.Parent = AtmosphereImage;
            OzoneInfo.Parent = AtmosphereImage;
            IonosLine.Parent = AtmosphereImage;
            StratoInfo.Parent = AtmosphereImage;
            MesoInfo.Parent = AtmosphereImage;
            ThermosInfo.Parent = AtmosphereImage;
            ExosInfo.Parent = AtmosphereImage;
            IonosInfo.Parent = AtmosphereImage;
            SimPlayBtn.Parent = AtmosphereImage;

            // set parent to the tools background
            ThermoBtn.Parent = ToolsBkg;
            BaroBtn.Parent = ToolsBkg;
            GasAnaBtn.Parent = ToolsBkg;
            AnemoBtn.Parent = ToolsBkg;

            // set parent to the sim box
            MagicBalloonBtn.Parent = SimBox;
            FloatingTableBtn.Parent = SimBox;

            // set parent to the sim background
            SimObjectImageA.Parent = SimImage;
            SimObjectImageB.Parent = SimImage;
            SimTimer1.Parent = SimImage;
            SimTimer2.Parent = SimImage;
            SimTimer3.Parent = SimImage;
            SimTimerGo.Parent = SimImage;

            //set parent to the altitude
            AltitudeInfoText.Parent = AltitudeInfo;

            //set parent to the tools
            ThermoInfoText.Parent = ThermoUsableA;
            BaroInfoText.Parent = BaroUsableA;
            GasAnaN2.Parent = GasAnaUsable;
            GasAnaN.Parent = GasAnaUsable;
            GasAnaO2.Parent = GasAnaUsable;
            GasAnaO.Parent = GasAnaUsable;
            GasAnaAr.Parent = GasAnaUsable;
            GasAnaHe.Parent = GasAnaUsable;
            GasAnaH.Parent = GasAnaUsable;
            AnemoInfoText.Parent = AnemoUsable;

            //set parent ot the magicballoonsimbox
            SimBalloonParam.Parent = SimBoxParams;

            //set parent ot the simresult
            FinalHeight.Parent = SimResult;

            //set parent to the simcalcalloon
            SimPascal.Parent = SimCalUniversal;
            SimThickness.Parent = SimCalUniversal;
            SimPressure.Parent = SimCalUniversal;
            SimSumBalloon.Parent = SimCalUniversal;
            SimPlot.Parent = SimResult;

            //set parent to the simmapbox
            SimMapArrow.Parent = SimMapBox;

            //centering texts
            CenterHorizontally(AtmosphereImage, EverestInfo);
            CenterHorizontally(AtmosphereImage, OzoneInfo);
            CenterHorizontally(AtmosphereImage, TropoInfo);
            CenterHorizontally(AtmosphereImage, StratoInfo);
            CenterHorizontally(AtmosphereImage, MesoInfo);
            CenterHorizontally(AtmosphereImage, IonosInfo);
            CenterHorizontally(AtmosphereImage, ThermosInfo);
            CenterHorizontally(AtmosphereImage, ExosInfo);
            CenterHorizontally(AtmosphereImage, AltitudeInfo);
            CenterHorizontally(AtmosphereImage, SpecialSimExitBtn);
            CenterHorizontally(AtmosphereImage, SimBoxParams);
            CenterHorizontally(AtmosphereImage, SimBalloonInfo);
            CenterHorizontally(AtmosphereImage, MagicBalloonPresetbox);
            CenterHorizontally(SimCalUniversal, SimPlot);
            CenterHorizontally(SimImage, SimTimer3);
            CenterHorizontally(SimImage, SimTimer2);
            CenterHorizontally(SimImage, SimTimer1);
            CenterHorizontally(SimImage, SimTimerGo);
            CenterHorizontally(SimImage, SimObjectImageA);
            CenterHorizontally(SimImage, SimObjectImageB);
            CenterHorizontally(SimImage, SpecialSimExitBtn);

            //Disabling anything to display
            ErrorBoxScreen.Visible = false;
            ToolsBkg.Visible = false;
            ThermoBtn.Visible = false;
            ThermoInfo.Visible = false;
            ThermoUsableA.Visible = false;
            ThermoUsableB.Visible = false;
            BaroBtn.Visible = false;
            BaroInfo.Visible = false;
            BaroUsableA.Visible = false;
            BaroUsableB.Visible = false;
            GasAnaBtn.Visible = false;
            GasAnaInfo.Visible = false;
            GasAnaUsable.Visible = false;
            AnemoBtn.Visible = false;
            AnemoInfo.Visible = false;
            AnemoUsable.Visible = false;
            MagicBalloonInfo.Visible = false;
            FloatingTableInfo.Visible = false;
            CumulonimbusInfo.Visible = false;
            StratusInfo.Visible = false;
            AltocumulusInfo.Visible = false;
            CirrusInfo.Visible = false;
            CumulusInfo.Visible = false;
            WeatherBalloonInfo.Visible = false;
            MeteorBurnInfo.Visible = false;
            AuroraInfo.Visible = false;
            EverestInfo.Visible = false;
            SateliteInfo.Visible = false;
            OzoneInfo.Visible = false;
            TropoInfo.Visible = false;
            StratoInfo.Visible = false;
            MesoInfo.Visible = false;
            ThermosInfo.Visible = false;
            ExosInfo.Visible = false;
            IonosInfo.Visible = false;
            SimBoxParams.Visible = false;
            MagicBalloonPresetbox.Visible = false;
            SimParam1.Visible = false;
            SimParam4.Visible = false;
            SimParam3.Visible = false;
            SimParam2.Visible = false;
            SimScroll1.Visible = false;
            SimScroll4.Visible = false;
            SimScroll3.Visible = false;
            SimScroll2.Visible = false;
            SimExitBtn.Visible = false;
            SimBox.Visible = false;
            SimPlayBtn.Visible = false;
            SimTimer3.Visible = false;
            SimTimer2.Visible = false;
            SimTimer1.Visible = false;
            SimTimerGo.Visible = false;
            SimObjectImageA.Visible = false;
            SimObjectImageB.Visible = false;
            SimMapBox.Visible = false;
            SimCalUniversal.Visible = false;
            SimBalloonInfo.Visible = false;
            SimResult.Visible = false;
            SimInfoBtn.Visible = false;
            SpecialSimExitBtn.Visible = false;
            FinalHeight.Visible = false;
            WORLD_POS.Visible = true;

            // Setting z-level to proper levels
            EverestLine.BringToFront();
            EverestInfo.BringToFront();
            TropoLine.BringToFront();
            StratoLine.BringToFront();
            MesoLine.BringToFront();
            ThermoLine.BringToFront();
            ThermoLine.BringToFront();
            ExosLine.BringToFront();
            OzoneLine.BringToFront();
            OzoneInfo.BringToFront();
            IonosLine.BringToFront();
            ThermoUsableA.BringToFront();
            ThermoUsableB.BringToFront();
            ThermoInfo.BringToFront();
            BaroUsableA.BringToFront();
            BaroUsableB.BringToFront();
            BaroInfo.BringToFront();
            GasAnaInfo.BringToFront();
            GasAnaUsable.BringToFront();
            AnemoUsable.BringToFront();
            MeteorBurnBtn.BringToFront();
            AuroraBtn.SendToBack();
            ThermosInfo.BringToFront();

            SimParam1.Text = "0.000172";
            SimParam4.Text = "75993.75";
            SimParam3.Text = "0.29";
            SimParam2.Text = "30";

            // allows the atmosphere to be moved
            MouseWheelMover.Enable(AtmosphereImage, ThermoUsableA, ThermoUsableB, BaroUsableA, BaroUsableB, GasAnaUsable, AnemoUsable, 3840, -3160);

            // sets preset to default
            MagicBalloonPresetbox.SelectedIndex = 0;
        }

        // ACTIONS

        //toolsbtn
        private void ToolsBtn_Click(object sender, EventArgs e)
        {
            SoundPlayer.Play(Properties.Audios.Click);

            if (!isActive)
            {
                ToolsBkg.Visible = true;
                ThermoBtn.Visible = true;
                BaroBtn.Visible = true;
                GasAnaBtn.Visible = true;
                AnemoBtn.Visible = true;

                SimBtn.Enabled = false;
                isActive = true;
            }
            else
            {
                ToolsBkg.Visible = false;
                ThermoBtn.Visible = false;
                BaroBtn.Visible = false;
                GasAnaBtn.Visible = false;
                AnemoBtn.Visible = false;

                SimBtn.Enabled = true;
                isActive = false;
            }
        }

        private void ToolsBtn_MouseDown(object sender, MouseEventArgs e)
        {
            Size preshiftSize = ToolsBtn.Size;
            Point preshiftLoc = ToolsBtn.Location;
            ToolsBtn.Location = new Point(preshiftLoc.X, preshiftLoc.Y + 6);
            ToolsBtn.Size = new Size(preshiftSize.Width, preshiftSize.Height - 6);
            ToolsBtn.Image = Properties.Resources._29;
        }

        private void ToolsBtn_MouseUp(object sender, MouseEventArgs e)
        {
            Size preshiftSize = ToolsBtn.Size;
            Point preshiftLoc = ToolsBtn.Location;
            ToolsBtn.Location = new Point(preshiftLoc.X, preshiftLoc.Y - 6);
            ToolsBtn.Size = new Size(preshiftSize.Width, preshiftSize.Height + 6);
            ToolsBtn.Image = Properties.Resources._26;
        }

        //simbtn
        private void SimBtn_Click(object sender, EventArgs e)
        {
            if (!isActive)
            {
                SoundPlayer.Play(Properties.Audios.Click);
                SimBox.Visible = true;
                MagicBalloonBtn.Visible = true;
                FloatingTableBtn.Visible = true;
                ToolsBtn.Enabled = false;
                isActive = true;
            }
            else
            {
                SoundPlayer.Play(Properties.Audios.Click);
                SimBox.Visible = false;
                SimBoxParams.Visible = false;
                FloatingTableBtn.Visible = false;
                ToolsBtn.Enabled = true;
                isActive = false;
            }
        }

        private void SimBtn_MouseDown(object sender, MouseEventArgs e)
        {
            Size preshiftSize = SimBtn.Size;
            Point preshiftLoc = SimBtn.Location;
            SimBtn.Location = new Point(preshiftLoc.X, preshiftLoc.Y + 6);
            SimBtn.Size = new Size(preshiftSize.Width, preshiftSize.Height - 6);
            SimBtn.Image = Properties.Resources._69;
        }

        private void SimBtn_MouseUp(object sender, MouseEventArgs e)
        {
            Size preshiftSize = SimBtn.Size;
            Point preshiftLoc = SimBtn.Location;
            SimBtn.Location = new Point(preshiftLoc.X, preshiftLoc.Y - 6);
            SimBtn.Size = new Size(preshiftSize.Width, preshiftSize.Height + 6);
            SimBtn.Image = Properties.Resources._68;
        }

        //simplaybtn-key

        private async void SimPlayBtn_Click(object sender, EventArgs e)
        {
            isSimulating = true;
            isPopped = false;
            isBoiled = false;
            SimImage.Location = new Point(0, -9682);

            AtmosphereImage.Visible = false;
            SimBox.Visible = false;
            SimBoxParams.Visible = false;
            MagicBalloonPresetbox.Visible = false;
            SimParam1.Visible = false;
            SimParam4.Visible = false;
            SimParam3.Visible = false;
            SimParam2.Visible = false;
            SimBalloonParam.Visible = false;
            SimBalloonInfo.Visible = false;
            SimInfoBtn.Visible = false;
            SimScroll1.Visible = false;
            SimScroll4.Visible = false;
            SimScroll3.Visible = false;
            SimScroll2.Visible = false;
            SimExitBtn.Visible = false;

            inner = Convert.ToDouble(SimParam4.Text);
            thick = Convert.ToDouble(SimParam1.Text);
            size = Convert.ToDouble(SimParam3.Text);
            stretch = Convert.ToDouble(SimParam2.Text);

            await Task.Delay(1000); // 3
            SimTimer3.BringToFront();
            SimTimer3.Visible = true;

            await Task.Delay(1000); // 2
            SimTimer3.Visible = false;

            SimTimer2.BringToFront();
            SimTimer2.Visible = true;

            await Task.Delay(1000); // 1
            SimTimer2.Visible = false;

            SimTimer1.BringToFront();
            SimTimer1.Visible = true;

            await Task.Delay(1000); // GO!
            SimTimer1.Visible = false;

            SimTimerGo.BringToFront();
            SimTimerGo.Visible = true;

            await Task.Delay(250);
            SimTimerGo.Visible = false;

            if (simState)
            {
                SimCalUniversal.Image = Properties.Resources._4S;
                SimObjectImageA.Image = Properties.Resources._6aS;
                SimObjectImageA.Size = new Size(110, 110);
            }
            else
            {
                SimCalUniversal.Image = Properties.Resources._18S;
                SimObjectImageA.Image = Properties.Resources._15aS;
                SimObjectImageA.Size = new Size(110, 140);
            }

            AltitudeInfo.BringToFront();
            AltitudeInfo.Visible = true;
            SpecialSimExitBtn.Visible = true;
            SimMapBox.Visible = true;
            SimCalUniversal.Visible = true;
            SimObjectImageA.BringToFront();
            SimObjectImageA.Visible = true;

            if (simState)
            {
                SimThickness.Text = Convert.ToString(thick);
                CenterHorizontally(SimCalUniversal, SimThickness);
            }
            else
            {
                SimThickness.Text = "1";
                CenterHorizontally(SimCalUniversal, SimThickness);
            }

            int alpha = 167; // adjusting for easingtype errors
            int beta = -9680; // adjusting starting point
            int delay1 = 22 * 1000;
            int delay2 = 15 * 1000;

            // runningAnimation1 = AnimationHelpers.AnimateCustomMove(SimImage, -10, beta, -10, 0 + alpha, 22000, v => Math.Pow(v, 5));
            // runningAnimation4 = AnimationHelpers.AnimateCustomMove(SimMapArrow, 1, 620, 1, -5, 22000, v => Math.Pow(v, 5));

            if (simState)
            {
                if (!balloonLimit)
                {
                    Point originalBalloonPos = SimObjectImageA.Location;
                    runningAnimation1a = AnimationHelpers.AnimateMove(SimImage, -10, beta, -10, 0 + alpha, delay1, "SinusoidalEaseInOut");
                    runningAnimation2a = AnimationHelpers.AnimateMove(SimObjectImageA, originalBalloonPos.X, originalBalloonPos.Y, originalBalloonPos.X, originalBalloonPos.Y + (beta - alpha), delay1, "SinusoidalEaseInOut");
                    runningAnimation3a = AnimationHelpers.AnimateMove(SimObjectImageB, originalBalloonPos.X, originalBalloonPos.Y, originalBalloonPos.X, originalBalloonPos.Y + (beta - alpha), delay1, "SinusoidalEaseInOut");
                    runningAnimation4a = AnimationHelpers.AnimateMove(SimMapArrow, 1, 620, 1, -5, delay1, "SinusoidalEaseInOut");
                    await Task.Delay(delay1);
                    if (!isPopped)
                    {
                        AltitudeInfoText.Text = "1,000,000M";
                        simAltitideKm = 1000;

                        await Task.Delay(2000);
                        FinalHeight.Text = AltitudeInfoText.Text;
                        SimObjectImageA.Visible = false;
                        SimObjectImageB.Visible = false;
                        FinalHeight.Visible = true;
                        SimResult.Image = Properties.Resources._13bS;

                        SimResult.Visible = true;
                        SimPlot.Plot.Title("Altitude vs Stretching Force");
                        SimPlot.Plot.XLabel("(m)");
                        SimPlot.Plot.YLabel("(MPa)");
                        SimPlot.BringToFront();
                        CenterHorizontally(SimResult, SimPlot);
                        SimPlot.Plot.Add.Scatter(simFrame, simData);
                        SimPlot.Refresh();
                    }
                }
                else
                {
                    double parentDeltaY = (-8320 + alpha) - beta;
                    runningAnimation1a = AnimationHelpers.AnimateMove(SimImage, -10, beta, -10, -8320 + alpha, 15000, "SinusoidalEaseInOut"); // 1, 620, 1, (int)(620 - parentDeltaY)
                    runningAnimation2a = AnimationHelpers.AnimateMove(SimObjectImageA, SimObjectImageA.Left, SimObjectImageA.Top, SimObjectImageA.Left, (int)(SimObjectImageA.Top - parentDeltaY), delay2, "SinusoidalEaseInOut");
                    runningAnimation3a = AnimationHelpers.AnimateMove(SimObjectImageB, SimObjectImageA.Left, SimObjectImageA.Top, SimObjectImageA.Left, (int)(SimObjectImageA.Top - parentDeltaY), delay2, "SinusoidalEaseInOut");
                    runningAnimation4a = AnimationHelpers.AnimateMove(SimMapArrow, 1, 620, 1, 518, 15000, "SinusoidalEaseInOut");
                    await Task.Delay(delay2);
                    if (!isPopped)
                    {
                        AltitudeInfoText.Text = "53,700M";
                        simAltitideKm = 53.7;

                        await Task.Delay(2000);
                        FinalHeight.Text = AltitudeInfoText.Text;
                        SimObjectImageA.Visible = false;
                        SimObjectImageB.Visible = false;
                        SimResult.Visible = true;
                        FinalHeight.Visible = true;
                        SimResult.Image = Properties.Resources._13bS;

                        SimResult.Visible = true;
                        SimPlot.Plot.Title("Altitude vs Stretching Force");
                        SimPlot.Plot.XLabel("(m)");
                        SimPlot.Plot.YLabel("(MPa)");
                        SimPlot.BringToFront();
                        CenterHorizontally(SimResult, SimPlot);
                        SimPlot.Plot.Add.Scatter(simFrame, simData);
                        SimPlot.Refresh();
                    }
                }
            }
            else
            {
                Point originalBalloonPos = SimObjectImageA.Location;
                runningAnimation1b = AnimationHelpers.AnimateMove(SimImage, -10, beta, -10, 0 + alpha, delay1, "SinusoidalEaseInOut");
                runningAnimation2b = AnimationHelpers.AnimateMove(SimObjectImageA, originalBalloonPos.X, originalBalloonPos.Y, originalBalloonPos.X, originalBalloonPos.Y + (beta - alpha), delay1, "SinusoidalEaseInOut");
                runningAnimation3b = AnimationHelpers.AnimateMove(SimObjectImageB, originalBalloonPos.X, originalBalloonPos.Y, originalBalloonPos.X, originalBalloonPos.Y + (beta - alpha), delay1, "SinusoidalEaseInOut");
                runningAnimation4b = AnimationHelpers.AnimateMove(SimMapArrow, 1, 620, 1, -5, delay1, "SinusoidalEaseInOut");
                await Task.Delay(delay1);
                if (!isPopped)
                {
                    AltitudeInfoText.Text = "1,000,000M";
                    simAltitideKm = 1000;

                    await Task.Delay(2000);
                    FinalHeight.Text = AltitudeInfoText.Text;
                    SimObjectImageA.Visible = false;
                    SimObjectImageB.Visible = false;
                    FinalHeight.Visible = true;
                    SimResult.Image = Properties.Resources._13bS;

                    SimResult.Visible = true;
                    SimPlot.Plot.Title("Altitude vs Stretching Force");
                    SimPlot.Plot.XLabel("(m)");
                    SimPlot.Plot.YLabel("(MPa)");
                    SimPlot.BringToFront();
                    CenterHorizontally(SimResult, SimPlot);
                    SimPlot.Plot.Add.Scatter(simFrame, simData);
                    SimPlot.Refresh();
                }
            }
            
        }

        //simbtn
        private void SimBtn_Click(object sender, EventArgs e)
        {
            if (!isActive)
            {
                SoundPlayer.Play(Properties.Audios.Click);
                SimBox.Visible = true;
                MagicBalloonBtn.Visible = true;
                FloatingTableBtn.Visible = true;
                SimPlayBtn.Visible = true;
                isActive = true;
            }
            else
            {
                SoundPlayer.Play(Properties.Audios.Click);
                SimBox.Visible = false;
                MagicBalloonSimBox.Visible = false;
                FloatingTableBtn.Visible = false;
                SimPlayBtn.Visible = false;
                isActive = false;
            }
        }

        private void SimBtn_MouseDown(object sender, MouseEventArgs e)
        {
            Size preshiftSize = SimBtn.Size;
            Point preshiftLoc = SimBtn.Location;
            SimBtn.Location = new Point(preshiftLoc.X, preshiftLoc.Y + 6);
            SimBtn.Size = new Size(preshiftSize.Width, preshiftSize.Height - 6);
            SimBtn.Image = Properties.Resources._69;
        }

        private void SimBtn_MouseUp(object sender, MouseEventArgs e)
        {
            Size preshiftSize = SimBtn.Size;
            Point preshiftLoc = SimBtn.Location;
            SimBtn.Location = new Point(preshiftLoc.X, preshiftLoc.Y - 6);
            SimBtn.Size = new Size(preshiftSize.Width, preshiftSize.Height + 6);
            SimBtn.Image = Properties.Resources._68;
        }

        //simplaybtn


        // cumulonimbusbtn
        private void CumulonimbusBtn_MouseHover(object sender, EventArgs e)
        {
            CumulonimbusBtn.Image = Properties.Resources._44;
            CumulonimbusInfo.Visible = true;

            AltitudeInfoText.Text = ">2km - 18km";
            CenterHorizontally(AltitudeInfo, AltitudeInfoText);
        }

        private void CumulonimbusBtn_MouseLeave(object sender, EventArgs e)
        {
            CumulonimbusBtn.Image = Properties.Resources._43;
            CumulonimbusInfo.Visible = false;
        }

        private void CumulonimbusBtn_MouseMove(object sender, MouseEventArgs e)
        {
            CumulonimbusInfo.Focus();
            CumulonimbusInfo.BringToFront();
            CumulonimbusInfo.Left = e.X + 765;
            CumulonimbusInfo.Top = e.Y + 3540;
        }

        // aurorabtn
        private void AuroraBtn_MouseHover(object sender, EventArgs e)
        {
            AuroraBtn.Image = Properties.Resources._58;
            AuroraInfo.Visible = true;

            AltitudeInfoText.Text = "100km - 200km";
            CenterHorizontally(AltitudeInfo, AltitudeInfoText);

        }

        private void AuroraBtn_MouseLeave(object sender, EventArgs e)
        {
            AuroraBtn.Image = Properties.Resources._57;
            AuroraInfo.Visible = false;
        }

        private void AuroraBtn_MouseMove(object sender, MouseEventArgs e)
        {
            AuroraInfo.Focus();
            AuroraInfo.BringToFront();
            AuroraInfo.Left = e.X + 438;
            AuroraInfo.Top = e.Y + 1445;
        }

        // meteorbtn
        private void MeteorBurnBtn_MouseHover(object sender, EventArgs e)
        {
            MeteorBurnBtn.Image = Properties.Resources._56;
            MeteorBurnInfo.Visible = true;

            AltitudeInfoText.Text = "80km - 120km";
            CenterHorizontally(AltitudeInfo, AltitudeInfoText);
        }

        private void MeteorBurnBtn_MouseLeave(object sender, EventArgs e)
        {
            MeteorBurnBtn.Image = Properties.Resources._55;
            MeteorBurnInfo.Visible = false;
        }

        private void MeteorBurnBtn_MouseMove(object sender, MouseEventArgs e)
        {
            MeteorBurnInfo.Focus();
            MeteorBurnInfo.BringToFront();
            MeteorBurnInfo.Left = e.X + 570;
            MeteorBurnInfo.Top = e.Y + 2462;
        }

        //stratus
        private void StratusBtn_MouseHover(object sender, EventArgs e)
        {
            StratusBtn.Image = Properties.Resources._49;
            StratusInfo.Visible = true;

            AltitudeInfoText.Text = ">2km";
            CenterHorizontally(AltitudeInfo, AltitudeInfoText);
        }

        private void StratusBtn_MouseLeave(object sender, EventArgs e)
        {
            StratusBtn.Image = Properties.Resources._50;
            StratusInfo.Visible = false;
        }

        private void StratusBtn_MouseMove(object sender, MouseEventArgs e)
        {
            StratusInfo.Focus();
            StratusInfo.BringToFront();
            StratusInfo.Left = e.X + 565;
            StratusInfo.Top = e.Y + 3677;
        }

        // cumulusbtn
        private void CumulusBtn_MouseHover(object sender, EventArgs e)
        {
            CumulusBtn.Image = Properties.Resources._52;
            CumulusInfo.Visible = true;

            AltitudeInfoText.Text = ">2km";
            CenterHorizontally(AltitudeInfo, AltitudeInfoText);
        }

        private void CumulusBtn_MouseLeave(object sender, EventArgs e)
        {
            CumulusBtn.Image = Properties.Resources._51;
            CumulusInfo.Visible = false;
        }

        private void CumulusBtn_MouseMove(object sender, MouseEventArgs e)
        {
            CumulusInfo.Focus();
            CumulusInfo.BringToFront();
            CumulusInfo.Left = e.X + 811;
            CumulusInfo.Top = e.Y + 3673;
        }

        //altocumulusbtn
        private void AltocumulusBtn_MouseHover(object sender, EventArgs e)
        {
            AltocumulusBtn.Image = Properties.Resources._48;
            AltocumulusInfo.Visible = true;

            AltitudeInfoText.Text = ">2km - 8km";
            CenterHorizontally(AltitudeInfo, AltitudeInfoText);
        }

        private void AltocumulusBtn_MouseLeave(object sender, EventArgs e)
        {
            AltocumulusBtn.Image = Properties.Resources._47;
            AltocumulusInfo.Visible = false;
        }

        private void AltocumulusBtn_MouseMove(object sender, MouseEventArgs e)
        {
            AltocumulusInfo.Focus();
            AltocumulusInfo.BringToFront();
            AltocumulusInfo.Left = e.X + 337;
            AltocumulusInfo.Top = e.Y + 3635;
        }

        //cirrusbtn
        private void CirrusBtn_MouseHover(object sender, EventArgs e)
        {
            CirrusBtn.Image = Properties.Resources._46;
            CirrusInfo.Visible = true;

            AltitudeInfoText.Text = ">6km - 18km";
            CenterHorizontally(AltitudeInfo, AltitudeInfoText);
        }

        private void CirrusBtn_MouseLeave(object sender, EventArgs e)
        {
            CirrusBtn.Image = Properties.Resources._45;
            CirrusInfo.Visible = false;
        }

        private void CirrusBtn_MouseMove(object sender, MouseEventArgs e)
        {
            CirrusInfo.Focus();
            CirrusInfo.BringToFront();
            CirrusInfo.Left = e.X + 58;
            CirrusInfo.Top = e.Y + 3581;
        }

        //weatherballoonbtn
        private void WeatherBalloonBtn_MouseHover(object sender, EventArgs e)
        {
            WeatherBalloonBtn.Image = Properties.Resources._54;
            WeatherBalloonInfo.Visible = true;

            AltitudeInfoText.Text = "16km - 35km";
            CenterHorizontally(AltitudeInfo, AltitudeInfoText);

        }

        private void WeatherBalloonBtn_MouseLeave(object sender, EventArgs e)
        {
            WeatherBalloonBtn.Image = Properties.Resources._53;
            WeatherBalloonInfo.Visible = false;
        }

        private void WeatherBalloonBtn_MouseMove(object sender, MouseEventArgs e)
        {
            WeatherBalloonInfo.Focus();
            WeatherBalloonInfo.BringToFront();
            WeatherBalloonInfo.Left = e.X + 240;
            WeatherBalloonInfo.Top = e.Y + 3433;
        }

        // thermobtn
        private void ThermoBtn_MouseHover(object sender, EventArgs e)
        {
            ThermoBtn.Image = Properties.Resources._14;
            ThermoInfo.Visible = true;
        }

        private void ThermoBtn_MouseLeave(object sender, EventArgs e)
        {
            ThermoBtn.Image = Properties.Resources._13;
            ThermoInfo.Visible = false;
        }

        private void ThermoBtn_Click(object sender, EventArgs e)
        {
            SoundPlayer.Play(Properties.Audios.Click);

            if (!isActiveTool)
            {
                ThermoUsableA.Visible = true;
                ThermoUsableB.Visible = true;

                BaroUsableA.Visible = false;
                BaroUsableB.Visible = false;
                GasAnaUsable.Visible = false;
                AnemoUsable.Visible = false;

                BaroBtn.Enabled = false;
                GasAnaBtn.Enabled = false;
                AnemoBtn.Enabled = false;

                BaroBtn.Image = Properties.Resources._16D;
                GasAnaBtn.Image = Properties.Resources._18D;
                AnemoBtn.Image = Properties.Resources._63D;

                ToolsBkg.Visible = false;
                ThermoBtn.Visible = false;
                BaroBtn.Visible = false;
                GasAnaBtn.Visible = false;
                AnemoBtn.Visible = false;

                isActive = false;
                isActiveTool = true;
            }
            else
            {
                ThermoUsableA.Visible = false;
                ThermoUsableB.Visible = false;

                BaroBtn.Enabled = true;
                GasAnaBtn.Enabled = true;
                AnemoBtn.Enabled = true;

                BaroBtn.Image = Properties.Resources._16;
                GasAnaBtn.Image = Properties.Resources._18;
                AnemoBtn.Image = Properties.Resources._63;

                isActiveTool = false;
            }
        }

        // barobtn
        private void BarometerBtn_MouseHover(object sender, EventArgs e)
        {
            BaroBtn.Image = Properties.Resources._17;
            BaroInfo.Visible = true;
        }

        private void BarometerBtn_MouseLeave(object sender, EventArgs e)
        {
            BaroBtn.Image = Properties.Resources._16;
            BaroInfo.Visible = false;
        }

        private void BaroBtn_Click(object sender, EventArgs e)
        {
            SoundPlayer.Play(Properties.Audios.Click);

            if (!isActiveTool)
            {
                BaroUsableA.Visible = true;
                BaroUsableB.Visible = true;

                ThermoUsableA.Visible = false;
                ThermoUsableB.Visible = false;
                GasAnaUsable.Visible = false;
                AnemoUsable.Visible = false;

                ThermoBtn.Enabled = false;
                GasAnaBtn.Enabled = false;
                AnemoBtn.Enabled = false;

                ThermoBtn.Image = Properties.Resources._13D;
                GasAnaBtn.Image = Properties.Resources._18D;
                AnemoBtn.Image = Properties.Resources._63D;

                ToolsBkg.Visible = false;
                ThermoBtn.Visible = false;
                BaroBtn.Visible = false;
                GasAnaBtn.Visible = false;
                AnemoBtn.Visible = false;

                isActive = false;
                isActiveTool = true;
            }
            else
            {
                BaroUsableA.Visible = false;
                BaroUsableB.Visible = false;

                ThermoBtn.Enabled = true;
                GasAnaBtn.Enabled = true;
                AnemoBtn.Enabled = true;

                ThermoBtn.Image = Properties.Resources._13;
                GasAnaBtn.Image = Properties.Resources._18;
                AnemoBtn.Image = Properties.Resources._63;

                isActiveTool = false;
            }
        }

        //gasanabtn
        private void GasAnaBtn_MouseHover(object sender, EventArgs e)
        {
            GasAnaBtn.Image = Properties.Resources._20;
            GasAnaInfo.Visible = true;
        }

        private void GasAnaBtn_MouseLeave(object sender, EventArgs e)
        {
            GasAnaBtn.Image = Properties.Resources._21;
            GasAnaInfo.Visible = false;
        }

        private void GasAnaBtn_Click(object sender, EventArgs e)
        {
            SoundPlayer.Play(Properties.Audios.Click);

            if (!isActiveTool)
            {
                GasAnaUsable.Visible = true;

                ThermoUsableA.Visible = false;
                ThermoUsableB.Visible = false;
                BaroUsableA.Visible = false;
                BaroUsableB.Visible = false;
                AnemoUsable.Visible = false;

                ThermoBtn.Enabled = false;
                BaroBtn.Enabled = false;
                AnemoBtn.Enabled = false;

                ThermoBtn.Image = Properties.Resources._13D;
                BaroBtn.Image = Properties.Resources._16D;
                AnemoBtn.Image = Properties.Resources._63D;

                ToolsBkg.Visible = false;
                ThermoBtn.Visible = false;
                BaroBtn.Visible = false;
                GasAnaBtn.Visible = false;
                AnemoBtn.Visible = false;

                isActive = false;
                isActiveTool = true;
            }
            else
            {
                GasAnaUsable.Visible = false;

                ThermoBtn.Enabled = true;
                BaroBtn.Enabled = true;
                AnemoBtn.Enabled = true;

                ThermoBtn.Image = Properties.Resources._13;
                BaroBtn.Image = Properties.Resources._16;
                AnemoBtn.Image = Properties.Resources._63;

                isActiveTool = false;
            }

        }

        //anemobtn
        private void AnemoBtn_MouseHover(object sender, EventArgs e)
        {
            AnemoBtn.Image = Properties.Resources._64;
            AnemoInfo.Visible = true;
        }

        private void AnemoBtn_MouseLeave(object sender, EventArgs e)
        {
            AnemoBtn.Image = Properties.Resources._63;
            AnemoInfo.Visible = false;
        }

        private void AnemoBtn_Click(object sender, EventArgs e)
        {
            SoundPlayer.Play(Properties.Audios.Click);

            if (!isActiveTool)
            {
                AnemoUsable.Visible = true;

                ThermoUsableA.Visible = false;
                ThermoUsableB.Visible = false;
                BaroUsableA.Visible = false;
                BaroUsableB.Visible = false;
                GasAnaUsable.Visible = false;

                ThermoBtn.Enabled = false;
                BaroBtn.Enabled = false;
                GasAnaBtn.Enabled = false;

                ThermoBtn.Image = Properties.Resources._13D;
                BaroBtn.Image = Properties.Resources._16D;
                GasAnaBtn.Image = Properties.Resources._18D;

                ToolsBkg.Visible = false;
                ThermoBtn.Visible = false;
                BaroBtn.Visible = false;
                GasAnaBtn.Visible = false;
                AnemoBtn.Visible = false;

                isActive = false;
                isActiveTool = true;
            }
            else
            {
                AnemoUsable.Visible = false;

                ThermoBtn.Enabled = true;
                BaroBtn.Enabled = true;
                GasAnaBtn.Enabled = true;

                ThermoBtn.Image = Properties.Resources._13;
                BaroBtn.Image = Properties.Resources._16;
                GasAnaBtn.Image = Properties.Resources._18;

                isActiveTool = false;
            }
        }

        //MagicBalloonbtn-key
        private void MagicBalloonBtn_Click(object sender, EventArgs e)
        {
            SoundPlayer.Play(Properties.Audios.Click);

            SimBoxParams.Visible = true;
            MagicBalloonPresetbox.Visible = true;
            SimParam1.Visible = true;
            SimParam4.Visible = true;
            SimParam3.Visible = true;
            SimParam2.Visible = true;
            SimScroll1.Visible = true;
            SimScroll4.Visible = true;
            SimScroll3.Visible = true;
            SimScroll2.Visible = true;
            SimExitBtn.Visible = true;
            SimPlayBtn.Visible = true;
            SimInfoBtn.Visible = true;
            SimBalloonParam.Visible = true;

            SimBox.Visible = false;
            ToolsBtn.Visible = false;
            ExitBtn.Visible = false;
            SimBtn.Visible = false;
            EverestLine.Visible = false;
            OzoneLine.Visible = false;
            TropoLine.Visible = false;
            StratoLine.Visible = false;
            MesoLine.Visible = false;
            IonosInfo.Visible = false;
            ThermoLine.Visible = false;
            ExosLine.Visible = false;
            CumulonimbusBtn.Visible = false;
            AltocumulusBtn.Visible = false;
            CumulusBtn.Visible = false;
            StratusBtn.Visible = false;
            CirrusBtn.Visible = false;
            WeatherBalloonBtn.Visible = false;
            MeteorBurnBtn.Visible = false;
            AuroraBtn.Visible = false;
            SateliteBtn.Visible = false;
            ThermoUsableA.Visible = false;
            ThermoUsableB.Visible = false;
            BaroUsableA.Visible = false;
            BaroUsableB.Visible = false;
            GasAnaUsable.Visible = false;
            AnemoUsable.Visible = false;
            AltitudeInfo.Visible = false;

            AtmosphereImage.Location = new Point(0, -3154);
            SimParam1.PlaceholderText = "Wall Thickness (m)";
            SimParam2.PlaceholderText = "Tensile Strength (MPa)";
            SimParam3.PlaceholderText = "Diameter (m)";
            SimParam4.PlaceholderText = "Inner Pressure (Pa)";

            simState = true;
        }

        private void MagicBalloonBtn_MouseHover(object sender, EventArgs e)
        {
            MagicBalloonBtn.Image = Properties.Resources._77;
            MagicBalloonInfo.Visible = true;
        }

        private void MagicBalloonBtn_MouseLeave(object sender, EventArgs e)
        {
            MagicBalloonBtn.Image = Properties.Resources._76;
            MagicBalloonInfo.Visible = false;
        }

        //FloatingTableBtn-key
        private void FloatingTableBtn_Click(object sender, EventArgs e)
        {
            SimBoxParams.Visible = true;
            FloatingTablePresetBox.Visible = true;
            SimParam1.Visible = true;
            SimParam4.Visible = true;
            SimParam3.Visible = true;
            SimParam2.Visible = true;
            SimScroll1.Visible = true;
            SimScroll4.Visible = true;
            SimScroll3.Visible = true;
            SimScroll2.Visible = true;
            SimExitBtn.Visible = true;
            SimPlayBtn.Visible = true;
            SimInfoBtn.Visible = true;
            SimBalloonParam.Visible = true;

            SimBox.Visible = false;
            ToolsBtn.Visible = false;
            ExitBtn.Visible = false;
            SimBtn.Visible = false;
            EverestLine.Visible = false;
            OzoneLine.Visible = false;
            TropoLine.Visible = false;
            StratoLine.Visible = false;
            MesoLine.Visible = false;
            IonosInfo.Visible = false;
            ThermoLine.Visible = false;
            ExosLine.Visible = false;
            CumulonimbusBtn.Visible = false;
            AltocumulusBtn.Visible = false;
            CumulusBtn.Visible = false;
            StratusBtn.Visible = false;
            CirrusBtn.Visible = false;
            WeatherBalloonBtn.Visible = false;
            MeteorBurnBtn.Visible = false;
            AuroraBtn.Visible = false;
            SateliteBtn.Visible = false;
            ThermoUsableA.Visible = false;
            ThermoUsableB.Visible = false;
            BaroUsableA.Visible = false;
            BaroUsableB.Visible = false;
            GasAnaUsable.Visible = false;
            AnemoUsable.Visible = false;
            AltitudeInfo.Visible = false;

            AtmosphereImage.Location = new Point(0, -3154);
            SimParam1.PlaceholderText = "Height (m)";
            SimParam2.PlaceholderText = "Temperature (K)";
            SimParam3.PlaceholderText = "Enthalpy of Vaporization (J/Mol)";
            SimParam4.PlaceholderText = "Pressure (Pa)";

            simState = false;
        }
        private void FloatingTableBtn_MouseHover(object sender, EventArgs e)
        {
            FloatingTableBtn.Image = Properties.Resources._79;
            FloatingTableInfo.Visible = true;
        }

        private void FloatingTableBtn_MouseLeave(object sender, EventArgs e)
        {
            FloatingTableBtn.Image = Properties.Resources._78;
            FloatingTableInfo.Visible = false;
        }

        //siminfobtn-key
        private void SimInfoBtn_Click(object sender, EventArgs e)
        {

            if (!isActive)
            {
                SimBoxParams.Visible = false;
                SimBalloonInfo.Visible = true;

                isActive = true;
            }
            else
            {
                SimBoxParams.Visible = true;
                SimBalloonInfo.Visible = false;

                isActive = false;
            }
        }

        //SimExitBtn-key
        private void SimExitBtn_Click(object sender, EventArgs e)
        {
            SoundPlayer.Play(Properties.Audios.Click);

            SimBoxParams.Visible = false;
            MagicBalloonPresetbox.Visible = false;
            SimParam1.Visible = false;
            SimParam4.Visible = false;
            SimParam3.Visible = false;
            SimParam2.Visible = false;
            SimScroll1.Visible = false;
            SimScroll4.Visible = false;
            SimScroll3.Visible = false;
            SimScroll2.Visible = false;
            SimExitBtn.Visible = false;
            SimPlayBtn.Visible = false;
            SimInfoBtn.Visible = false;
            SimBalloonInfo.Visible = false;
            SimBalloonParam.Visible = false;

            SimBox.Visible = true;
            ToolsBtn.Visible = true;
            ExitBtn.Visible = true;
            SimBtn.Visible = true;
            EverestLine.Visible = true;
            OzoneLine.Visible = true;
            TropoLine.Visible = true;
            StratoLine.Visible = true;
            MesoLine.Visible = true;
            IonosInfo.Visible = true;
            ThermoLine.Visible = true;
            ExosLine.Visible = true;
            CumulonimbusBtn.Visible = true;
            AltocumulusBtn.Visible = true;
            CumulusBtn.Visible = true;
            StratusBtn.Visible = true;
            CirrusBtn.Visible = true;
            WeatherBalloonBtn.Visible = true;
            MeteorBurnBtn.Visible = true;
            AuroraBtn.Visible = true;
            SateliteBtn.Visible = true;
            AltitudeInfo.Visible = true;

            isActive = false;
        }

        private void SimExitBtn_MouseDown(object sender, MouseEventArgs e)
        {
            Size preshiftSize = SimExitBtn.Size;
            Point preshiftLoc = SimExitBtn.Location;
            SimExitBtn.Location = new Point(preshiftLoc.X, preshiftLoc.Y + 6);
            SimExitBtn.Size = new Size(preshiftSize.Width, preshiftSize.Height - 6);
            SimExitBtn.Image = Properties.Resources._27;
        }

        private void SimExitBtn_MouseUp(object sender, MouseEventArgs e)
        {
            Size preshiftSize = SimExitBtn.Size;
            Point preshiftLoc = SimExitBtn.Location;
            SimExitBtn.Location = new Point(preshiftLoc.X, preshiftLoc.Y - 6);
            SimExitBtn.Size = new Size(preshiftSize.Width, preshiftSize.Height + 6);
            SimExitBtn.Image = Properties.Resources._24;
        }

        //SpecialSimExitBtn-key
        private void SpecialSimExitBtn_Click(object sender, EventArgs e)
        {
            SoundPlayer.Play(Properties.Audios.Click);

            if (!isSimulating)
            {
                SimBoxParams.Visible = false;
                MagicBalloonPresetbox.Visible = false;
                SimParam1.Visible = false;
                SimParam4.Visible = false;
                SimParam3.Visible = false;
                SimParam2.Visible = false;
                SimScroll1.Visible = false;
                SimScroll4.Visible = false;
                SimScroll3.Visible = false;
                SimScroll2.Visible = false;
                SimExitBtn.Visible = false;
                SimPlayBtn.Visible = false;
                SimResult.Visible = false;

                SimBox.Visible = true;
                ToolsBtn.Visible = true;
                ExitBtn.Visible = true;
                SimBtn.Visible = true;
                EverestLine.Visible = true;
                OzoneLine.Visible = true;
                TropoLine.Visible = true;
                StratoLine.Visible = true;
                MesoLine.Visible = true;
                IonosInfo.Visible = true;
                ThermoLine.Visible = true;
                ExosLine.Visible = true;
                CumulonimbusBtn.Visible = true;
                AltocumulusBtn.Visible = true;
                CumulusBtn.Visible = true;
                StratusBtn.Visible = true;
                CirrusBtn.Visible = true;
                WeatherBalloonBtn.Visible = true;
                MeteorBurnBtn.Visible = true;
                AuroraBtn.Visible = true;
                SateliteBtn.Visible = true;
                AltitudeInfo.Visible = true;

                Array.Clear(simData, 0, simData.Length);
                Array.Clear(simFrame, 0, simFrame.Length);
                isPopped = false;
                simCounter = 0;
                thick = 0.000172;
                stretch = 30;
                size = 0.29;
                inner = 103000;

            }
            else
            {
                runningAnimation1a.Stop();
                runningAnimation2a.Stop();
                runningAnimation3a.Stop();
                runningAnimation4a.Stop();
                SimImage.Location = new Point(1400, -9682);
                SpecialSimExitBtn.Visible = false;
                SimMapBox.Visible = false;
                SimCalUniversal.Visible = false;
                SimResult.Visible = false;

                AtmosphereImage.Visible = true;
                SimBox.Visible = true;
                SimBoxParams.Visible = true;
                MagicBalloonPresetbox.Visible = true;
                SimParam1.Visible = true;
                SimParam4.Visible = true;
                SimParam3.Visible = true;
                SimParam2.Visible = true;
                SimBtn.Visible = true;
                ToolsBtn.Visible = true;
                SimScroll1.Visible = true;
                SimScroll4.Visible = true;
                SimScroll3.Visible = true;
                SimScroll2.Visible = true;

                isSimulating = false;
                Array.Clear(simData, 0, simData.Length);
                Array.Clear(simFrame, 0, simFrame.Length);
                isPopped = false;
                simCounter = 0;
                thick = 0.000172;
                stretch = 30;
                size = 0.29;
                inner = 103000;

            }
        }

        //satelitebtn
        private void SateliteBtn_MouseHover(object sender, EventArgs e)
        {
            SateliteBtn.Image = Properties.Resources._60;
            SateliteInfo.Visible = true;

            AltitudeInfoText.Text = "160km - 1,500km";
            CenterHorizontally(AltitudeInfo, AltitudeInfoText);
        }

        private void SateliteBtn_MouseLeave(object sender, EventArgs e)
        {
            SateliteBtn.Image = Properties.Resources._59;
            SateliteInfo.Visible = false;
        }

        private void SateliteBtn_MouseMove(object sender, MouseEventArgs e)
        {
            SateliteInfo.Focus();
            SateliteInfo.BringToFront();
            SateliteInfo.Left = e.X + 420;
            SateliteInfo.Top = e.Y + 640;
        }

        //atmosphereimage-key
        private void AtmosphereImage_MouseMove(object sender, MouseEventArgs e)
        {
            Point mouseCoords = GetMouseRelativeToControl(AtmosphereImage);
            AltitudeInfoText.ForeColor = System.Drawing.Color.Black;
            double temperatureHeight = 0;
            double pressureHeight = 0;
            double anemoHeight = 0;

            double Dinitrogen = 0;
            double Dioxygen = 0;
            double Oxygen = 0;
            double Argon = 0;
            double Helium = 0;
            double Nitrogen = 0;
            double Hydrogen = 0;

            if (mouseCoords.Y > 2854)
            {
                // changes of coordinates
                AltitudeInfoText.Text = $"{Math.Round(((3840 - Convert.ToDouble(mouseCoords.Y)) * 0.0865580448065d) * 1000):n0}m";
                altitudeKm = Math.Round((3840 - Convert.ToDouble(mouseCoords.Y)) * 0.0865580448065d, 2);

                // data for tools
                temperatureHeight = AtmosphericGraphData.TemperatureAtHeight(altitudeKm);
                pressureHeight = AtmosphericGraphData.PressureAtHeight(altitudeKm);
                anemoHeight = AtmosphericGraphData.WindSpeedAtHeight(altitudeKm);

                Dinitrogen = Math.Round(AtmosphericGraphData.Dinitrogen(altitudeKm), 3);
                Dioxygen = Math.Round(AtmosphericGraphData.Dioxygen(altitudeKm), 3);
                Oxygen = Math.Round(AtmosphericGraphData.Oxygen(altitudeKm), 3);
                Argon = Math.Round(AtmosphericGraphData.Argon(altitudeKm), 3);
                Helium = Math.Round(AtmosphericGraphData.Helium(altitudeKm), 3);
                Nitrogen = Math.Round(AtmosphericGraphData.Nitrogen(altitudeKm), 3);
                Hydrogen = Math.Round(AtmosphericGraphData.Hydrogen(altitudeKm), 3);

                //temperature
                if (temperatureHeight == -10000)
                {
                    ThermoInfoText.Text = "No Data";
                }
                else
                {
                    ThermoInfoText.Text = Convert.ToString(temperatureHeight) + "°C";
                }

                //pressure
                BaroInfoText.Text = Convert.ToString(pressureHeight) + " mb";

                //anemometer
                if (anemoHeight > 243.04)
                {
                    AnemoInfoText.Text = "No Data";
                }
                else
                {
                    AnemoInfoText.Text = Convert.ToString(anemoHeight) + "kph";
                }

                //compositions
                if (Dinitrogen == 14250057378.9)
                {
                    GasAnaN2.Text = "N2: N/A";
                }
                else
                {
                    GasAnaN2.Text = "N2: " + Convert.ToString(Dinitrogen) + "%";
                }
                if (Dioxygen == 14250057378.9)
                {
                    GasAnaO2.Text = "O2: N/A";
                }
                else
                {
                    GasAnaO2.Text = "O2: " + Convert.ToString(Dioxygen) + "%";
                }
                if (Oxygen == 14250057378.9)
                {
                    GasAnaO.Text = "O2: N/A";
                }
                else
                {
                    GasAnaO.Text = "O: " + Convert.ToString(Oxygen) + "%";
                }
                if (Argon == 14250057378.9)
                {
                    GasAnaAr.Text = "Ar: N/A";
                }
                else
                {
                    GasAnaAr.Text = "Ar: " + Convert.ToString(Argon) + "%";
                }
                if (Helium == 14250057378.9)
                {
                    GasAnaHe.Text = "He: N/A";
                }
                else
                {
                    GasAnaHe.Text = "He: " + Convert.ToString(Helium) + "%";
                }
                if (Nitrogen == 14250057378.9)
                {
                    GasAnaN.Text = "N: N/A";
                }
                else
                {
                    GasAnaN.Text = "N: " + Convert.ToString(Nitrogen) + "%";
                }
                if (Hydrogen == 14250057378.9)
                {
                    GasAnaH.Text = "H: N/A";
                }
                else
                {
                    GasAnaH.Text = "H: " + Convert.ToString(Hydrogen) + "%";
                }

                CenterHorizontally(ThermoUsableA, ThermoInfoText);
                CenterHorizontally(BaroUsableA, BaroInfoText);
                CenterHorizontally(GasAnaUsable, GasAnaH);
                CenterHorizontally(GasAnaUsable, GasAnaHe);
                CenterHorizontally(GasAnaUsable, GasAnaAr);
                CenterHorizontally(GasAnaUsable, GasAnaN);
                CenterHorizontally(GasAnaUsable, GasAnaN2);
                CenterHorizontally(GasAnaUsable, GasAnaO2);
                CenterHorizontally(GasAnaUsable, GasAnaO);
                CenterHorizontally(AnemoUsable, AnemoInfoText);
                CenterHorizontally(AltitudeInfo, AltitudeInfoText);

                WORLD_POS.Text = "WORLD_POS: " + Convert.ToString(altitudeKm);
            }
            else if (mouseCoords.Y > 40)
            {
                AltitudeInfoText.Text = $"{Math.Round(((3840 - Convert.ToDouble(mouseCoords.Y)) * ExponentialHeightSpeed(mouseCoords.Y)) * 1000):n0}m";
                altitudeKm = Math.Round((3840 - Convert.ToDouble(mouseCoords.Y)) * ExponentialHeightSpeed(mouseCoords.Y), 2);

                temperatureHeight = AtmosphericGraphData.TemperatureAtHeight(altitudeKm);
                anemoHeight = AtmosphericGraphData.WindSpeedAtHeight(altitudeKm);

                Dinitrogen = Math.Round(AtmosphericGraphData.Dinitrogen(altitudeKm), 3);
                Dioxygen = Math.Round(AtmosphericGraphData.Dioxygen(altitudeKm), 3);
                Oxygen = Math.Round(AtmosphericGraphData.Oxygen(altitudeKm), 3);
                Argon = Math.Round(AtmosphericGraphData.Argon(altitudeKm), 3);
                Helium = Math.Round(AtmosphericGraphData.Helium(altitudeKm), 3);
                Nitrogen = Math.Round(AtmosphericGraphData.Nitrogen(altitudeKm), 3);
                Hydrogen = Math.Round(AtmosphericGraphData.Hydrogen(altitudeKm), 3);

                //temperature
                if (temperatureHeight == -10000)
                {
                    ThermoInfoText.Text = "No Data";
                }
                else
                {
                    ThermoInfoText.Text = Convert.ToString(temperatureHeight) + "°C";
                }

                //anemometer
                if (anemoHeight > 243.04)
                {
                    AnemoInfoText.Text = "No Data";
                }
                else
                {
                    AnemoInfoText.Text = Convert.ToString(anemoHeight) + "kph";
                }

                //compositions
                if (Dinitrogen == 14250057378.9)
                {
                    GasAnaN2.Text = "N2: N/A";
                }
                else
                {
                    GasAnaN2.Text = "N2: " + Convert.ToString(Dinitrogen) + "%";
                }
                if (Dioxygen == 14250057378.9)
                {
                    GasAnaO2.Text = "O2: N/A";
                }
                else
                {
                    GasAnaO2.Text = "O2: " + Convert.ToString(Dioxygen) + "%";
                }
                if (Oxygen == 14250057378.9)
                {
                    GasAnaO.Text = "O2: N/A";
                }
                else
                {
                    GasAnaO.Text = "O: " + Convert.ToString(Oxygen) + "%";
                }
                if (Argon == 14250057378.9)
                {
                    GasAnaAr.Text = "Ar: N/A";
                }
                else
                {
                    GasAnaAr.Text = "Ar: " + Convert.ToString(Argon) + "%";
                }
                if (Helium == 14250057378.9)
                {
                    GasAnaHe.Text = "He: N/A";
                }
                else
                {
                    GasAnaHe.Text = "He: " + Convert.ToString(Helium) + "%";
                }
                if (Nitrogen == 14250057378.9)
                {
                    GasAnaN.Text = "N: N/A";
                }
                else
                {
                    GasAnaN.Text = "N: " + Convert.ToString(Nitrogen) + "%";
                }
                if (Hydrogen == 14250057378.9)
                {
                    GasAnaH.Text = "H: N/A";
                }
                else
                {
                    GasAnaH.Text = "H: " + Convert.ToString(Hydrogen) + "%";
                }

                CenterHorizontally(ThermoUsableA, ThermoInfoText);
                CenterHorizontally(BaroUsableA, BaroInfoText);
                CenterHorizontally(GasAnaUsable, GasAnaH);
                CenterHorizontally(GasAnaUsable, GasAnaHe);
                CenterHorizontally(GasAnaUsable, GasAnaAr);
                CenterHorizontally(GasAnaUsable, GasAnaN);
                CenterHorizontally(GasAnaUsable, GasAnaN2);
                CenterHorizontally(GasAnaUsable, GasAnaO2);
                CenterHorizontally(GasAnaUsable, GasAnaO);
                CenterHorizontally(AnemoUsable, AnemoInfoText);
                CenterHorizontally(AltitudeInfo, AltitudeInfoText);
                WORLD_POS.Text = "WORLD_POS: " + Convert.ToString(altitudeKm);
            }
            else
            {
                AltitudeInfoText.Location = new Point(35, 20);
                AltitudeInfoText.Text = "60km - 10,000KM";

                CenterHorizontally(ThermoUsableA, ThermoInfoText);
                CenterHorizontally(BaroUsableA, BaroInfoText);
                CenterHorizontally(GasAnaUsable, GasAnaH);
                CenterHorizontally(GasAnaUsable, GasAnaHe);
                CenterHorizontally(GasAnaUsable, GasAnaAr);
                CenterHorizontally(GasAnaUsable, GasAnaN);
                CenterHorizontally(GasAnaUsable, GasAnaN2);
                CenterHorizontally(GasAnaUsable, GasAnaO2);
                CenterHorizontally(GasAnaUsable, GasAnaO);
                CenterHorizontally(AnemoUsable, AnemoInfoText);
                CenterHorizontally(AltitudeInfo, AltitudeInfoText);
                WORLD_POS.Text = "10000";
            }
        }

        //exitbtn
        private void ExitBtn_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ExitBtn_MouseDown(object sender, MouseEventArgs e)
        {
            Size preshiftSize = ExitBtn.Size;
            Point preshiftLoc = ExitBtn.Location;
            ExitBtn.Location = new Point(preshiftLoc.X, preshiftLoc.Y + 6);
            ExitBtn.Size = new Size(preshiftSize.Width, preshiftSize.Height - 6);
            ExitBtn.Image = System.Drawing.Image.FromFile(@"C:\Users\user\Downloads\LOTA\25.png");
        }

        private void ExitBtn_MouseUp(object sender, MouseEventArgs e)
        {
            Size preshiftSize = ExitBtn.Size;
            Point preshiftLoc = ExitBtn.Location;
            ExitBtn.Location = new Point(preshiftLoc.X, preshiftLoc.Y - 6);
            ExitBtn.Size = new Size(preshiftSize.Width, preshiftSize.Height + 6);
            ExitBtn.Image = Properties.Resources._22;
        }

        // everestline 
        private void EverestLine_MouseHover(object sender, EventArgs e)
        {
            EverestLine.Image = Properties.Resources._72;
            EverestInfo.Visible = true;

            AltitudeInfoText.Text = "8,849M";
            altitudeKm = 9;
        }

        private void EverestLine_MouseLeave(object sender, EventArgs e)
        {
            EverestLine.Image = Properties.Resources._73;
            EverestInfo.Visible = false;
        }

        // thermoline
        private void ThermoLine_MouseHover(object sender, EventArgs e)
        {
            ThermoLine.Image = null;
            ThermoLine.BackColor = System.Drawing.Color.White;
            ThermosInfo.Visible = true;

            AltitudeInfoText.Text = "600,000M";
            altitudeKm = 600;
        }

        private void ThermoLine_MouseLeave(object sender, EventArgs e)
        {
            ThermoLine.Image = Properties.Resources._11;
            ThermoLine.BackColor = System.Drawing.Color.Transparent;
            ThermosInfo.Visible = false;
        }

        // exosline
        private void ExosLine_MouseHover(object sender, EventArgs e)
        {
            ExosLine.Image = Properties.Resources._75;
            ExosInfo.Visible = true;

            AltitudeInfoText.Text = "60 - 10,000KM";
            CenterHorizontally(AltitudeInfo, AltitudeInfoText);
            altitudeKm = 10000;
        }

        private void ExosLine_MouseLeave(object sender, EventArgs e)
        {
            ExosLine.Image = Properties.Resources._74;
            ExosInfo.Visible = false;
        }

        // ionosline
        private void IonosLine_MouseHover(object sender, EventArgs e)
        {
            IonosLine.Image = Properties.Resources._72;
            IonosInfo.Visible = true;

            AltitudeInfoText.Text = "60,000M";
            altitudeKm = 60;
        }

        private void IonosLine_MouseLeave(object sender, EventArgs e)
        {
            IonosLine.Image = Properties.Resources._73;
            IonosInfo.Visible = false;
        }

        // stratoline
        private void StratoLine_MouseHover(object sender, EventArgs e)
        {
            StratoLine.Image = null;
            StratoLine.BackColor = System.Drawing.Color.White;
            StratoInfo.Visible = true;

            AltitudeInfoText.Text = "50,000M";
            altitudeKm = 50;
        }

        private void StratoLine_MouseLeave(object sender, EventArgs e)
        {
            StratoLine.Image = Properties.Resources._11;
            StratoLine.BackColor = System.Drawing.Color.Transparent;
            StratoInfo.Visible = false;
        }

        // mesoline
        private void MesoLine_MouseHover(object sender, EventArgs e)
        {
            MesoLine.Image = null;
            MesoLine.BackColor = System.Drawing.Color.White;
            MesoInfo.Visible = true;

            AltitudeInfoText.Text = "85,000M";
            altitudeKm = 85;
        }

        private void MesoLine_MouseLeave(object sender, EventArgs e)
        {
            MesoLine.Image = Properties.Resources._11;
            MesoLine.BackColor = System.Drawing.Color.Transparent;
            MesoInfo.Visible = false;
        }

        // ozoneline
        private void OzoneLine_MouseHover(object sender, EventArgs e)
        {
            OzoneLine.Image = Properties.Resources._72;
            OzoneInfo.Visible = true;

            AltitudeInfoText.Text = "10,000M";
            altitudeKm = 10;
        }

        private void OzoneLine_MouseLeave(object sender, EventArgs e)
        {
            OzoneLine.Image = Properties.Resources._73;
            OzoneInfo.Visible = false;
        }

        // tropoline
        private void TropoLine_MouseHover(object sender, EventArgs e)
        {
            TropoLine.Image = null;
            TropoLine.BackColor = System.Drawing.Color.White;
            TropoInfo.Visible = true;

            AltitudeInfoText.Text = "20,000M";
            altitudeKm = 20;
        }

        private void TropoLine_MouseLeave(object sender, EventArgs e)
        {
            TropoLine.Image = Properties.Resources._11;
            TropoLine.BackColor = System.Drawing.Color.Transparent;
            TropoInfo.Visible = false;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void GasAnaO2_Click(object sender, EventArgs e)
        {

        }

        private void GasAnaAr_Click(object sender, EventArgs e)
        {

        }
        private void ThickScroll_ValueChanged(object sender, EventArgs e)
        {
            if (simState)
            {
                SimParam1.Text = Convert.ToString(Math.Round((SimScroll1.Value / 100d) * 0.001d, 5));
                thick = Math.Round((SimScroll1.Value / 100d) * 0.001d, 5);
            }
            else
            {

            }
        }

        private void StretchinessScroll_Scroll(object sender, ScrollEventArgs e)
        {
            if (simState)
            {
                MagicBalloonPresetbox.SelectedIndex = 2;
                SimParam2.Text = Convert.ToString(Math.Round((SimScroll2.Value / 100d) * 999d, 2));
                stretch = Math.Round((SimScroll2.Value / 100d) * 999d, 2);
            }
            else
            {

            }
        }

        private void SizeScroll_Scroll(object sender, ScrollEventArgs e)
        {
            if (simState)
            {
                MagicBalloonPresetbox.SelectedIndex = 2;
                SimParam3.Text = Convert.ToString(Math.Round((SimScroll3.Value / 100d) * 999d, 2));
                size = Math.Round((SimScroll3.Value / 100d) * 999d, 2);
            }
            else
            {

            }
        }

        private void InnerScroll_Scroll(object sender, ScrollEventArgs e)
        {
            if (simState)
            {
                MagicBalloonPresetbox.SelectedIndex = 2;
                SimParam4.Text = Convert.ToString(Math.Round(101325d + 9077.51 * (SimScroll4.Value - 1), 2));
                inner = Math.Round(101325d + 9077.51 * (SimScroll4.Value - 1), 2);
            }
            else
            {

            }
        }

        private void MagicBalloonPresetbox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string preset = MagicBalloonPresetbox.SelectedItem.ToString();
            isChanged = true;

            if (preset == "Balloon")
            {
                SimParam1.Text = "0.000172";
                SimParam4.Text = "103000";
                SimParam3.Text = "0.29";
                SimParam2.Text = "30";
                thick = 0.000172;
                inner = 103000;
                size = 0.29;
                stretch = 30;

            }
            else if (preset == "Weather Balloon")
            {
                SimParam1.Text = "0.0001";
                SimParam4.Text = "101525";
                SimParam3.Text = "2.44";
                SimParam2.Text = "1.5";
                thick = 0.0001;
                inner = 101525;
                size = 2.44;
                stretch = 1.5;

            }
            else if (preset == "Custom")
            {
                SimParam1.Text = "0";
                SimParam4.Text = "0";
                SimParam3.Text = "0";
                SimParam2.Text = "0";
                thick = 0;
                inner = 0;
                size = 0;
                stretch = 0;
            }
        }

        private void FloatingTablePresetBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string preset = MagicBalloonPresetbox.SelectedItem.ToString();
            isChanged = true;

            if (preset == "Water")
            {
                SimParam1.Text = "0";
                SimParam4.Text = "373.13";
                SimParam3.Text = "43900";
                SimParam2.Text = "101325";
                thick = 0;
                inner = 373.13;
                size = 43900;
                stretch = 101325;

            }
            else if (preset == "Mercury")
            {
                SimParam1.Text = "0";
                SimParam4.Text = "629.88";
                SimParam3.Text = "59110";
                SimParam2.Text = "101325";
                thick = 0;
                inner = 629.88;
                size = 59110;
                stretch = 101325;

            }
            else if (preset == "Custom")
            {
                SimParam1.Text = "0";
                SimParam4.Text = "0";
                SimParam3.Text = "0";
                SimParam2.Text = "0";
                thick = 0;
                inner = 0;
                size = 0;
                stretch = 0;
            }
        }

        private void MagicBalloonTextboxThick_TextChanged(object sender, EventArgs e)
        {
            if (SimParam1.Text.Length == 0)
            {

            }
            else
            {
                thick = Convert.ToDouble(SimParam1.Text);
            }
        }

        private void MagicBalloonTextboxStretch_TextChanged(object sender, EventArgs e)
        {
            if (simState)
            {
                if (!isChanged)
                {
                    MagicBalloonPresetbox.SelectedIndex = 2;
                }
                else
                {
                    isChanged = true;
                }

                if (SimParam2.Text.Length == 0)
                {

                }
                else
                {
                    stretch = Convert.ToDouble(SimParam2.Text);
                }
            }
            else
            {
                if (!isChanged)
                {
                    FloatingTablePresetBox.SelectedIndex = 2;
                }
                else
                {
                    isChanged = true;
                }

                if (SimParam2.Text.Length == 0)
                {

                }
                else
                {
                    stretch = Convert.ToDouble(SimParam2.Text);
                }
            }
        }

        private void MagicBalloonTextboxSize_TextChanged(object sender, EventArgs e)
        {
            if (simState)
            {
                if (!isChanged)
                {
                    MagicBalloonPresetbox.SelectedIndex = 2;
                }
                else
                {
                    isChanged = true;
                }

                if (SimParam3.Text.Length == 0)
                {

                }
                else
                {
                    size = Convert.ToDouble(SimParam3.Text);
                }
            }
            else
            {
                if (!isChanged)
                {
                    FloatingTablePresetBox.SelectedIndex = 2;
                }
                else
                {
                    isChanged = true;
                }

                if (SimParam3.Text.Length == 0)
                {

                }
                else
                {
                    size = Convert.ToDouble(SimParam3.Text);
                }
            }
        }

        private void MagicBalloonTextboxInner_TextChanged(object sender, EventArgs e)
        {
            if (simState)
            {
                if (!isChanged)
                {
                    FloatingTablePresetBox.SelectedIndex = 2;
                }
                else
                {
                    isChanged = true;
                }

                if (SimParam4.Text.Length == 0)
                {

                }
                else
                {
                    inner = Convert.ToDouble(SimParam4.Text);
                }
            }
            else
            {

            }
        }

        private void SimMapArrow_LocationChanged(object sender, EventArgs e)
        {

        }

        private void AltitudeInfoText_TextChanged(object sender, EventArgs e)
        {
            CenterHorizontally(AltitudeInfo, AltitudeInfoText);
        }

        private void SimThickness_Click(object sender, EventArgs e)
        {

        }

        private void SimCalBalloon_Click(object sender, EventArgs e)
        {

        }

        private void SimPressure_Click(object sender, EventArgs e)
        {


        }

        private void AtmosphereImage_Click(object sender, EventArgs e)
        {

        }

        private async void SimImage_LocationChanged(object sender, EventArgs e)
        {
            // calculating global position to world position
            SIM_POS.Text = $"{Convert.ToDouble(simAltitideKm)}";
            double universalFinalResult = 0;
            double preCheckedPos = Math.Round((9862.0 - Math.Abs(SimImage.Location.Y - 182.42)) / 9861.0 * 1000000.0);

            if (preCheckedPos < 0)
            {
                AltitudeInfoText.Text = $"0M";
                simAltitideKm = 0;
            }
            else
            {
                if (Math.Abs(SimImage.Location.Y) > 7262)
                {
                    AltitudeInfoText.Text = $"{Math.Round(((9682d - Convert.ToDouble(Math.Abs(SimImage.Location.Y))) / 1529d) * 53700d, 2):n0}m";
                    simAltitideKm = Math.Round((((9682d - Convert.ToDouble(Math.Abs(SimImage.Location.Y))) / 1529d) * 53700d) / 1000d, 2);
                }
                else
                {
                    double y = Math.Abs(SimImage.Location.Y);
                    double sharpness = 4.193d; // <-- adjust this

                    double t = (8153d - y) / 8153d;
                    double expT = (Math.Exp(sharpness * t) - 1) / (Math.Exp(sharpness) - 1);

                    double value = 80000d + (1000000d - 80000d) * expT;

                    AltitudeInfoText.Text = $"{Math.Round(value, 2):n0}m";
                    simAltitideKm = Math.Round(value / 1000d, 2);
                }
            }

            // transformations
            if (simState)
            {
                double postResult = AtmosphericGraphData.PressureAtHeight(simAltitideKm);
                double finalResult = Math.Round((((inner - (postResult * 100)) * size) / (4 * thick)) / (1000000d), 2);
                universalFinalResult = finalResult;
                label1.Text = (postResult * 100).ToString();

                SimPascal.Text = $"({Math.Round(inner, 2):n0}Pa" + " - " + $"{Math.Round(postResult * 100, 2):n0}" + "Pa) × " + $"{size}M";
                SimPressure.Text = $"{finalResult:n0}MPa";
                SimSumBalloon.Text = $"{Math.Round(postResult * 100, 2):n0}Pa";

                CenterHorizontally(SimCalUniversal, SimPascal);
                CenterHorizontally(SimCalUniversal, SimPressure);
                CenterHorizontally(SimCalUniversal, SimSumBalloon);

                if (finalResult > stretch && simCounter > 1)
                {
                    isPopped = true;
                    runningAnimation1a.Stop();
                    runningAnimation2a.Stop();
                    runningAnimation3a.Stop();
                    runningAnimation4a.Stop();
                    SimObjectImageA.Visible = false; // balloon popsQ
                    SimObjectImageB.Visible = true;
                    await Task.Delay(2000);
                    SimObjectImageA.Visible = false;
                    SimObjectImageB.Visible = false;
                    SimResult.Visible = true; // results
                    FinalHeight.Visible = true;
                    FinalHeight.Text = AltitudeInfoText.Text;
                    CenterHorizontally(SimResult, FinalHeight);

                    SimPlot.Visible = true;
                    SimResult.Image = Properties.Resources._13aS;
                    SimPlot.Plot.Title("Altitude vs Stretching Force");
                    SimPlot.Plot.XLabel("(m)");
                    SimPlot.Plot.YLabel("(MPa)");
                    SimPlot.BringToFront();
                    CenterHorizontally(SimResult, SimPlot);
                    SimPlot.Plot.Add.Scatter(simFrame, simData);
                    SimPlot.Refresh();
                }
            }
            else
            {
                double postResultP = AtmosphericGraphData.PressureAtHeight(simAltitideKm);
                double postResultT = AtmosphericGraphData.TemperatureAtHeight(simAltitideKm);
                double finalResult = 1.0 / ((1.0 / inner) - (8.314 / size) * Math.Log(stretch / postResultP));
                universalFinalResult = finalResult;

                SimPascal.Text = $"(1 / {Math.Round(inner, 2):n0}K) - (8.314J⋅K⁻¹⋅mol⁻¹ / {Math.Round(size):n0}J/mol) × ln({Math.Round(stretch, 2):n0} / {Math.Round(postResultP, 2):n0})";
                SimPressure.Text = $"{finalResult:n0}K";
                SimSumBalloon.Text = $"{Math.Round(postResultP + 273.15, 2):n0}K";

                CenterHorizontally(SimCalUniversal, SimPascal);
                CenterHorizontally(SimCalUniversal, SimPressure);
                CenterHorizontally(SimCalUniversal, SimSumBalloon);

                if (finalResult < AtmosphericGraphData.TemperatureAtHeight(simAltitideKm) && simCounter > 1)
                {
                    isBoiled = true;
                    runningAnimation1b.Stop();
                    runningAnimation2b.Stop();
                    runningAnimation3b.Stop();
                    runningAnimation4b.Stop();
                    SimObjectImageA.Visible = false; // substance boils
                    SimObjectImageB.Visible = true;
                    await Task.Delay(2000);
                    SimObjectImageA.Visible = false;
                    SimObjectImageB.Visible = false;
                    SimResult.Visible = true; // results
                    FinalHeight.Visible = true;
                    FinalHeight.Text = AltitudeInfoText.Text;
                    CenterHorizontally(SimResult, FinalHeight);

                    SimPlot.Visible = true;
                    SimResult.Image = Properties.Resources._13aS;
                    SimPlot.Plot.Title("Altitude vs Boiling Point");
                    SimPlot.Plot.XLabel("(m)");
                    SimPlot.Plot.YLabel("(K)");
                    SimPlot.BringToFront();
                    CenterHorizontally(SimResult, SimPlot);
                    SimPlot.Plot.Add.Scatter(simFrame, simData);
                    SimPlot.Refresh();
                }
            }
            
            if (simCounter > 0)
            {
                simData[simCounter] = universalFinalResult;
                simFrame[simCounter] = simAltitideKm * 1000d;
            }

            simCounter++;
        }

        private void SimResult_Click(object sender, EventArgs e)
        {

        }

        private void SimBalloonInfo_Click(object sender, EventArgs e)
        {

        }

        private void SimBalloonParam_Click(object sender, EventArgs e)
        {
            if (balloonLimit)
            {
                SimBalloonParam.BackColor = System.Drawing.Color.Olive;
                SimBalloonParam.ForeColor = System.Drawing.Color.Black;
                balloonLimit = false;
            }
            else
            {
                SimBalloonParam.BackColor = System.Drawing.Color.Gold;
                SimBalloonParam.ForeColor = System.Drawing.Color.Black;
                balloonLimit = true;
            }
        }
    }
}
