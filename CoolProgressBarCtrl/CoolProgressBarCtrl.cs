/* ************** Code File Header ******************
 * File: 		CoolProgressBarCtrl.cs
 * Author: 		Nazario D'Apote
 * Email: 		nazario.dapote@gmail.com    
 * Date: 		27/20/2013
 * Version:		1.0.0.0
 * License:		LGPL
 * Description: Cool progress control that like AVAST antivirus 2014
 * **************************************************/

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CoolProgressBar
{
    public partial class CoolProgressBarCtrl : UserControl
    {
        #region SmoothDrawing class

        private class SmoothDrawing : IDisposable
        {
            private bool disposed = false;
            private SmoothingMode _originalMode;
            private Graphics _graphics;

            public SmoothDrawing(Graphics graphics)
            {
                _graphics = graphics;
                _originalMode = graphics.SmoothingMode;
                _graphics.SmoothingMode = SmoothingMode.AntiAlias;
            }

            //Implement IDisposable.
            public void Dispose()
            {
                // If this function is being called the user wants to release the
                // resources. lets call the Dispose which will do this for us.
                Dispose(true);

                // Now since we have done the cleanup already there is nothing left
                // for the Finalizer to do. So lets tell the GC not to call it later.
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if ( !disposed )
                {
                    if ( disposing )
                    {
                        // Free other state (managed objects).
                        //someone want the deterministic release of all resources
                        //Let us release all the managed resources
                        _graphics.SmoothingMode = _originalMode;
                    }
                    else
                    {
                        // Do nothing, no one asked a dispose, the object went out of
                        // scope and finalized is called so lets next round of GC 
                        // release these resources
                    }
                    // Free your own state (unmanaged objects).
                    // Set large fields to null.
                    disposed = true;
                }
            }

            // Use C# destructor syntax for finalization code.
            ~SmoothDrawing()
            {
                // Simply call Dispose(false).
                Dispose(false);
            }
        } 

        #endregion

        private const int DEFAULT_MAXIMUM = 100;
        private const int DEFAULT_MINIMUM = 0;
        private const int DEFAULT_VALUE = 0;
        private const int DEFAULT_STEP = 1;
        private const int DEFAULT_MARQUEE_ANIMATION_SPEED = 100;
        private Color DEFAULT_PROGRESS_COLOR = Color.CadetBlue;
        private const bool DEFAULT_LOCKRATIO = false;
        private int _marqueeValue;
        private bool _marqueeValueGrows;
        private float _currentRatio = 1.0f;

        public CoolProgressBarCtrl()
        {
            InitializeComponent();

            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetDefaults();
        }

        [Browsable(false)]
        private void SetDefaults()
        {
            ProgressColor = DEFAULT_PROGRESS_COLOR;
            Maximum = DEFAULT_MAXIMUM;
            Minimum = DEFAULT_MINIMUM;
            Value = DEFAULT_VALUE;
            Step = DEFAULT_STEP;
            MarqueeAnimationSpeed = DEFAULT_MARQUEE_ANIMATION_SPEED;
            Style = ProgressBarStyle.Continuous;
            _marqueeValue = Minimum;
            _marqueeValueGrows = true;
        }

        #region Properties

        private bool _lookRatio = DEFAULT_LOCKRATIO;

        [System.ComponentModel.Category("Behavior")]
        [System.ComponentModel.Description("When true the aspect ratio of the control is locked")]
        [System.ComponentModel.DefaultValue(DEFAULT_LOCKRATIO)]
        public bool LockRatio
        {
            get { return _lookRatio; }
            set { _lookRatio = value; this.Invalidate(); }
        }

        [Browsable(false)]
        private int _maximum;

        [System.ComponentModel.Category("Behavior")]
        [System.ComponentModel.Description("The upper bound of the range this ProgressBar is working with")]
        [System.ComponentModel.DefaultValue(DEFAULT_MAXIMUM)]
        public int Maximum
        {
            get { return _maximum; }
            set { _maximum = value; this.Invalidate(); }
        }

        [Browsable(false)]
        private int _minimum;

        [System.ComponentModel.Category("Behavior")]
        [System.ComponentModel.Description("The lower bound of the range this ProgressBar is working with")]
        [System.ComponentModel.DefaultValue(DEFAULT_MINIMUM)]
        public int Minimum
        {
            get { return _minimum; }
            set { _marqueeValue = _minimum = value; this.Invalidate(); }
        }

        [Browsable(false)]
        private int _step;

        [System.ComponentModel.Category("Behavior")]
        [System.ComponentModel.Description("The amount to increment the current value by when the PerformStep() method is called")]
        [System.ComponentModel.DefaultValue(DEFAULT_STEP)]
        public int Step
        {
            get { return _step; }
            set { _step = value; this.Invalidate(); }
        }

        [Browsable(false)]
        private int _value;

        [System.ComponentModel.Category("Behavior")]
        [System.ComponentModel.Description("The current value for the ProgressBar, in the range specified in the minimum and the maximum properties")]
        [System.ComponentModel.DefaultValue(DEFAULT_VALUE)]
        public int Value
        {
            get { return _value; }
            set 
            {
                _value = value; 

                if ( value > Maximum )
                    _value = Maximum;
                if ( value < Minimum )
                    _value = Minimum;

                this.Invalidate(); 
            }
        }

        [Browsable(false)]
        private Color _progressColor;

        [System.ComponentModel.Category("Appearance")]
        [System.ComponentModel.Description("The progress arc color for the ProgressBar")]
        [System.ComponentModel.DefaultValue(typeof(Color), "CadetBlue")]
        public Color ProgressColor
        {
            get { return _progressColor; }
            set { _progressColor = value; this.Invalidate(); }
        }

        [System.ComponentModel.Category("Behavior")]
        [System.ComponentModel.Description("The speed of the animation marquee in milliseconds")]
        [System.ComponentModel.DefaultValue(DEFAULT_MARQUEE_ANIMATION_SPEED)]
        public int MarqueeAnimationSpeed
        {
            get;
            set;
        }

        [Browsable(false)]
        private ProgressBarStyle _style;

        [System.ComponentModel.Category("Behavior")]
        [System.ComponentModel.Description("The property allows the user to set the style of the ProgressBar")]
        [System.ComponentModel.DefaultValue(typeof(ProgressBarStyle), "Continuous")]
        public ProgressBarStyle Style
        {
            get { return _style; }
            set 
            { 
                _style = value;

                if ( _style == ProgressBarStyle.Marquee )
                    StartMarquee();
                else
                    StopMarquee();

                this.Invalidate(); 
            }
        } 

        #endregion

        public void Increment(int delta)
        {
            Value += delta;
        }

        public void Decrement(int delta)
        {
            Value -= delta; 
        }

        public void PerformStep()
        {
            Increment(Step);
        }

        [Browsable(false)]
        private System.Windows.Forms.Timer _timer;

        [Browsable(false)]
        private void StartMarquee()
        {
            _marqueeValue = Minimum;

            _timer = new Timer(components);
            _timer.Interval = this.MarqueeAnimationSpeed;
            _timer.Tick += new EventHandler(OnAnimationTick);
            _timer.Start();
        }

        [Browsable(false)]
        private void StopMarquee()
        {
            if ( _timer != null )
                _timer.Dispose();
        }

        [Browsable(false)]
        private void OnAnimationTick(object sender, EventArgs e)
        {
            if ( _marqueeValueGrows )
            {
                if ( _marqueeValue < Maximum )
                    _marqueeValue++;
                else
                    _marqueeValueGrows = false;
            }
            else
            {
                if ( _marqueeValue > Minimum )
                    _marqueeValue--;
                else
                    _marqueeValueGrows = true;
            }

            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            PaintControl(e.Graphics, e.ClipRectangle);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if ( !LockRatio )
                this._currentRatio = (float) this.Height / (float) this.Width;
            else
                this.Height = (int) (this.Width * this._currentRatio);

            base.OnSizeChanged(e);
        }

        [Browsable(false)]
        private void PaintProgress(Graphics g, Brush brush, RectangleF start, RectangleF end)
        {
            float v = 0;
            if ( Style == ProgressBarStyle.Marquee )
                v = CalcScaledValue(_marqueeValue);
            else
                v = CalcScaledValue(this.Value);

            float width = Math.Abs(start.Width - end.Width) * v;
            float height = Math.Abs(start.Height - end.Height) * v;

            RectangleF innerEllipseRing = RectangleF.Inflate(start, width / 2, height / 2);
            g.FillEllipse(brush, innerEllipseRing);
        }

        [Browsable(false)]
        private float ScaledValue
        {
            get { return CalcScaledValue(this.Value); }
        }

        [Browsable(false)]
        private float CalcScaledValue(int value)
        {
            return ((float) value - Minimum) / (Maximum - Minimum);
        }

        [Browsable(false)]
        private void PaintControl(Graphics g, Rectangle clipRectangle)
        {
            if ( g == null ) return;
            if ( clipRectangle == null || clipRectangle.IsEmpty ) return;

            float distRing1 = -(this.Width * 5) / 100;          // 5%
            float distRing2 = -(this.Width * 10) / 100;         // 10%
            float distArcRing = -(this.Width * 4) / 100;        // 4%
            float distInnerCircle = -(this.Width * 30) / 100;   // 30%

            RectangleF externalRing1 = RectangleF.Inflate(this.ClientRectangle, distRing1, distRing1);
            RectangleF externalRing2 = RectangleF.Inflate(this.ClientRectangle, distRing2, distRing2);
            RectangleF innerEllipseRing = RectangleF.Inflate(this.ClientRectangle, distInnerCircle, distInnerCircle);
            RectangleF progressArcRing = RectangleF.Inflate(this.ClientRectangle, distArcRing, distArcRing);

            using ( var smoothDrawing = new SmoothDrawing(g) )
            using ( var progressCircleBrush = new SolidBrush(ProgressColor) )
            {
                // Set smooth drawing
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Paint inner circular progress
                PaintProgress(g, progressCircleBrush, innerEllipseRing, externalRing2);

                // Draw progress arc
                using ( var penProgressArc = new Pen(ProgressColor, 3.0f) )
                {
                    float progressAngle = 0;
                    if ( Style == ProgressBarStyle.Marquee )
                        progressAngle = 360 * CalcScaledValue(_marqueeValue);
                    else
                        progressAngle = 360 * ScaledValue;

                    try
                    {
                        g.DrawArc(penProgressArc, progressArcRing, 270, progressAngle);
                    }
                    catch { }
                }

                // Draw the circular extenal ring
                using ( GraphicsPath p1 = new GraphicsPath() )
                {
                    p1.AddEllipse(externalRing1);

                    using ( Region r1 = new Region(p1) )
                    using ( GraphicsPath p2 = new GraphicsPath() )
                    {

                        p2.AddEllipse(externalRing2);
                        r1.Exclude(p2);
                        g.FillRegion(Brushes.WhiteSmoke, r1);
                    }
                }

                // Draw the ring border
                g.DrawEllipse(Pens.White, externalRing1);
                g.DrawEllipse(Pens.White, externalRing2);

                // Draw the inner Ellipse
                g.FillEllipse(Brushes.WhiteSmoke, innerEllipseRing);
                g.DrawEllipse(Pens.White, innerEllipseRing);

                if ( Style != ProgressBarStyle.Marquee )
                {
                    // Draw the the progress percent
                    using ( System.Drawing.StringFormat drawFormat = new System.Drawing.StringFormat() )
                    {
                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                        drawFormat.Alignment = StringAlignment.Center;
                        drawFormat.LineAlignment = StringAlignment.Center;
                        string percentString = this.Value.ToString() + "%";
                        g.DrawString(percentString, this.Font, progressCircleBrush, this.ClientRectangle, drawFormat);
                    }
                }

            }

        }
    }
}
