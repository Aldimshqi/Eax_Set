using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Eax_Set
{
    public partial class Form1 : Form
    {
        private SpriteButton btnClose;
        private SpriteButton btnMinimize;
        private EaxSlider effectsSlider;
        private Label effectsAmountLabel;
        private PictureBox backgroundBox;

        private bool formBuilt = false;

        private const int ORIGINAL_IMG_WIDTH = 1552;
        private const int ORIGINAL_IMG_HEIGHT = 1080;
        private const double SCALE = 0.315;
        private const int VOICEMETER_STRIP_INDEX = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (formBuilt) return;
            formBuilt = true;

            this.FormBorderStyle = FormBorderStyle.None;

            int formWidth = (int)(ORIGINAL_IMG_WIDTH * SCALE);
            int formHeight = (int)(ORIGINAL_IMG_HEIGHT * SCALE);
            this.ClientSize = new Size(formWidth, formHeight);

            BuildBackground();
            BuildButtons();
            BuildSlider();
            ApplyRoundedCorners();
            AddSubtleBorder();

            bool connected = VoicemeeterRemote.Connect();
            if (!connected)
            {
                MessageBox.Show(
                    "Could not connect to Voicemeeter.\n\n" +
                    "Path found: " + VoicemeeterRemote.FoundVoicemeeterPath + "\n" +
                    "Result code: " + VoicemeeterRemote.LastLoginResult + "\n" +
                    "Error: " + VoicemeeterRemote.LastError + "\n\n" +
                    "Make sure the project is set to x86.",
                    "Voicemeeter Diagnostics", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (VoicemeeterRemote.LastLoginResult == 1)
            {
                MessageBox.Show(
                    "Connected (found at: " + VoicemeeterRemote.FoundVoicemeeterPath + ")\n" +
                    "But Voicemeeter itself is not running.\nOpen it first, then try the slider.",
                    "Voicemeeter Diagnostics", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private int ScaleX(int originalX) => (int)(originalX * SCALE);
        private int ScaleY(int originalY) => (int)(originalY * SCALE);

        private void BuildBackground()
        {
            backgroundBox = new PictureBox();
            backgroundBox.Image = Image.FromFile(Application.StartupPath + @"\Resources\EffectsBackground.png");
            backgroundBox.SizeMode = PictureBoxSizeMode.StretchImage;
            backgroundBox.Dock = DockStyle.Fill;
            this.Controls.Add(backgroundBox);
            backgroundBox.MouseDown += BackgroundBox_MouseDown;
        }

        private void BuildButtons()
        {
            btnMinimize = new SpriteButton();
            btnMinimize.Size = new Size(24, 18);
            btnMinimize.SpriteSheet = Image.FromFile(Application.StartupPath + @"\Resources\Share_Bttn_GradMnu32_Minimise.bmp");
            btnMinimize.FrameCount = 4;
            btnMinimize.TransparentColorKey = Color.White;
            btnMinimize.TransparencyTolerance = 40;
            btnMinimize.Location = new Point(ScaleX(1400), ScaleY(15));
            btnMinimize.Clicked += (s, e) => this.WindowState = FormWindowState.Minimized;
            this.Controls.Add(btnMinimize);
            btnMinimize.BringToFront();

            btnClose = new SpriteButton();
            btnClose.Size = new Size(24, 18);
            btnClose.SpriteSheet = Image.FromFile(Application.StartupPath + @"\Resources\Share_Bttn_GradMnu32_Close.bmp");
            btnClose.FrameCount = 4;
            btnClose.TransparentColorKey = Color.White;
            btnClose.TransparencyTolerance = 40;
            btnClose.Location = new Point(ScaleX(1460), ScaleY(15));
            btnClose.Clicked += (s, e) => this.Close();
            this.Controls.Add(btnClose);
            btnClose.BringToFront();
        }

        private void BuildSlider()
        {
            effectsAmountLabel = new Label();
            effectsAmountLabel.Text = "Effects Amount : 0";
            effectsAmountLabel.Font = new Font("Segoe UI", 9);
            effectsAmountLabel.ForeColor = Color.FromArgb(60, 60, 60);
            effectsAmountLabel.BackColor = Color.Transparent;
            effectsAmountLabel.AutoSize = true;
            effectsAmountLabel.Location = new Point(ScaleX(1093), ScaleY(480));
            this.Controls.Add(effectsAmountLabel);
            effectsAmountLabel.BringToFront();

            effectsSlider = new EaxSlider();
            effectsSlider.Minimum = 0;
            effectsSlider.Maximum = 100;
            effectsSlider.Value = 0;
            effectsSlider.ThumbSpriteSheet = Image.FromFile(Application.StartupPath + @"\Resources\EAX_Sldr_Horizontal14.jpg");
            effectsSlider.ThumbRegionX = 144;
            effectsSlider.ThumbRegionWidth = 144;
            effectsSlider.ThumbFrameCount = 4;
            effectsSlider.Location = new Point(ScaleX(1130), ScaleY(540));
            effectsSlider.Size = new Size(ScaleX(300), 20);
            effectsSlider.ValueChanged += (s, e) =>
            {
                effectsAmountLabel.Text = "Effects Amount : " + effectsSlider.Value;
                OnEffectsAmountChanged(effectsSlider.Value);
            };
            this.Controls.Add(effectsSlider);
            effectsSlider.BringToFront();
        }

        private void OnEffectsAmountChanged(int sliderValue)
        {
            float reverbValue = sliderValue / 10.0f;
            VoicemeeterRemote.SetReverb(VOICEMETER_STRIP_INDEX, reverbValue);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            VoicemeeterRemote.Disconnect();
            base.OnFormClosed(e);
        }

        private void ApplyRoundedCorners()
        {
            int radius = 6;
            GraphicsPath path = new GraphicsPath();
            Rectangle bounds = new Rectangle(0, 0, this.Width, this.Height);
            path.AddArc(bounds.X, bounds.Y, radius, radius, 180, 90);
            path.AddArc(bounds.Right - radius, bounds.Y, radius, radius, 270, 90);
            path.AddArc(bounds.Right - radius, bounds.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - radius, radius, radius, 90, 90);
            path.CloseAllFigures();
            this.Region = new Region(path);
        }

        private void AddSubtleBorder()
        {
            backgroundBox.Paint += (s, e) =>
            {
                Rectangle rect = new Rectangle(0, 0, backgroundBox.Width - 1, backgroundBox.Height - 1);
                using (Pen borderPen = new Pen(Color.FromArgb(140, 150, 165)))
                {
                    e.Graphics.DrawRectangle(borderPen, rect);
                }
            };
            backgroundBox.Invalidate();
        }

        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private static extern void ReleaseCapture();

        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private static extern void SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        private void BackgroundBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.Y < ScaleY(90))
            {
                ReleaseCapture();
                SendMessage(this.Handle, 0x112, 0xf012, 0);
            }
        }
    }
}