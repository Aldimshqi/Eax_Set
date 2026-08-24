using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

public class SpriteButton : Control
{
    private Image spriteSheet;
    private Image processedSprite;
    private int frameCount = 1;
    private bool isHover = false;
    private bool isPressed = false;
    private Color? transparentColorKey = null;

    public event EventHandler Clicked;

    public Image SpriteSheet
    {
        get { return spriteSheet; }
        set
        {
            spriteSheet = value;
            ApplyTransparency();
            Invalidate();
        }
    }

    public int FrameCount
    {
        get { return frameCount; }
        set { frameCount = value; Invalidate(); }
    }

    public Color? TransparentColorKey
    {
        get { return transparentColorKey; }
        set
        {
            transparentColorKey = value;
            ApplyTransparency();
            Invalidate();
        }
    }

    public int TransparencyTolerance { get; set; } = 25;
    public bool IsButtonEnabled { get; set; } = true;

    public SpriteButton()
    {
        this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                      ControlStyles.UserPaint |
                      ControlStyles.OptimizedDoubleBuffer |
                      ControlStyles.SupportsTransparentBackColor, true);
        this.BackColor = Color.Transparent;
        this.Cursor = Cursors.Hand;
    }

    private void ApplyTransparency()
    {
        if (spriteSheet == null)
        {
            processedSprite = null;
            return;
        }

        if (transparentColorKey == null)
        {
            processedSprite = spriteSheet;
            return;
        }

        Bitmap source = new Bitmap(spriteSheet);
        Bitmap bmp = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
        Color key = transparentColorKey.Value;

        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                Color px = source.GetPixel(x, y);
                int diff = Math.Abs(px.R - key.R) + Math.Abs(px.G - key.G) + Math.Abs(px.B - key.B);
                bmp.SetPixel(x, y, diff <= TransparencyTolerance ? Color.Transparent : px);
            }
        }
        processedSprite = bmp;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (processedSprite == null) return;

        int frameWidth = processedSprite.Width / frameCount;
        int frameIndex;

        if (!IsButtonEnabled) frameIndex = Math.Min(3, frameCount - 1);
        else if (isPressed) frameIndex = Math.Min(2, frameCount - 1);
        else if (isHover) frameIndex = Math.Min(1, frameCount - 1);
        else frameIndex = 0;

        Rectangle srcRect = new Rectangle(frameIndex * frameWidth, 0, frameWidth, processedSprite.Height);
        e.Graphics.DrawImage(processedSprite, this.ClientRectangle, srcRect, GraphicsUnit.Pixel);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        if (!IsButtonEnabled) return;
        isHover = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        isHover = false;
        isPressed = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (!IsButtonEnabled) return;
        isPressed = true;
        Invalidate();
        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (!IsButtonEnabled) return;
        isPressed = false;
        Invalidate();
        Clicked?.Invoke(this, EventArgs.Empty);
        base.OnMouseUp(e);
    }
}