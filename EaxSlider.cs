using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

public class EaxSlider : Control
{
    private int minimum = 0;
    private int maximum = 100;
    private int value = 0;
    private bool dragging = false;
    private bool hover = false;

    private Image rawSpriteSheet;
    private Image processedSpriteSheet;

    public int ThumbRegionX { get; set; } = 144;
    public int ThumbRegionWidth { get; set; } = 144;
    public int ThumbFrameCount { get; set; } = 4;

    public int Minimum { get => minimum; set { minimum = value; Invalidate(); } }
    public int Maximum { get => maximum; set { maximum = value; Invalidate(); } }

    public int Value
    {
        get => value;
        set
        {
            int v = Math.Max(minimum, Math.Min(maximum, value));
            if (v != this.value)
            {
                this.value = v;
                Invalidate();
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public event EventHandler ValueChanged;

    public Image ThumbSpriteSheet
    {
        get => rawSpriteSheet;
        set
        {
            rawSpriteSheet = value;
            ApplyMagentaTransparency();
            Invalidate();
        }
    }

    public EaxSlider()
    {
        this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                      ControlStyles.UserPaint |
                      ControlStyles.OptimizedDoubleBuffer |
                      ControlStyles.SupportsTransparentBackColor, true);
        this.BackColor = Color.Transparent;
        this.Cursor = Cursors.Hand;
        this.Height = 20;
    }

    private void ApplyMagentaTransparency()
    {
        if (rawSpriteSheet == null) { processedSpriteSheet = null; return; }

        Bitmap source = new Bitmap(rawSpriteSheet);
        Bitmap bmp = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
        Color magenta = Color.FromArgb(252, 0, 252);
        int tolerance = 60;

        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                Color px = source.GetPixel(x, y);
                int diff = Math.Abs(px.R - magenta.R) + Math.Abs(px.G - magenta.G) + Math.Abs(px.B - magenta.B);
                bmp.SetPixel(x, y, diff <= tolerance ? Color.Transparent : px);
            }
        }
        processedSpriteSheet = bmp;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        int trackHeight = 6;
        int trackY = (this.Height - trackHeight) / 2;
        int thumbWidth = 20;
        int thumbHeight = this.Height;

        float fraction = (maximum > minimum) ? (float)(value - minimum) / (maximum - minimum) : 0;
        int usableWidth = this.Width - thumbWidth;
        int thumbX = (int)(fraction * usableWidth);

        Rectangle trackRect = new Rectangle(thumbWidth / 2, trackY, this.Width - thumbWidth, trackHeight);
        using (SolidBrush emptyBrush = new SolidBrush(Color.FromArgb(210, 210, 206)))
        {
            e.Graphics.FillRectangle(emptyBrush, trackRect);
        }

        Rectangle filledRect = new Rectangle(thumbWidth / 2, trackY, thumbX, trackHeight);
        using (SolidBrush filledBrush = new SolidBrush(Color.FromArgb(120, 120, 125)))
        {
            e.Graphics.FillRectangle(filledBrush, filledRect);
        }

        using (Pen borderPen = new Pen(Color.FromArgb(160, 160, 160)))
        {
            e.Graphics.DrawRectangle(borderPen, trackRect);
        }

        if (processedSpriteSheet != null)
        {
            int frameWidth = ThumbRegionWidth / ThumbFrameCount;
            int frameIndex = dragging ? 2 : (hover ? 1 : 0);

            Rectangle srcRect = new Rectangle(ThumbRegionX + frameIndex * frameWidth, 0, frameWidth, processedSpriteSheet.Height);
            Rectangle dstRect = new Rectangle(thumbX, 0, thumbWidth, thumbHeight);
            e.Graphics.DrawImage(processedSpriteSheet, dstRect, srcRect, GraphicsUnit.Pixel);
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        dragging = true;
        UpdateValueFromMouseX(e.X);
        Invalidate();
        base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        hover = true;
        if (dragging) UpdateValueFromMouseX(e.X);
        Invalidate();
        base.OnMouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        dragging = false;
        Invalidate();
        base.OnMouseUp(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        hover = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    private void UpdateValueFromMouseX(int mouseX)
    {
        int thumbWidth = 20;
        int usableWidth = this.Width - thumbWidth;
        float fraction = (float)(mouseX - thumbWidth / 2) / usableWidth;
        fraction = Math.Max(0, Math.Min(1, fraction));
        Value = minimum + (int)Math.Round(fraction * (maximum - minimum));
    }
}