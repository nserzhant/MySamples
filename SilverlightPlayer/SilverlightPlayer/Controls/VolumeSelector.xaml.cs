using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SilverlightPlayer.Controls
{
    public partial class VolumeSelector : UserControl
    {
        #region Fields and properties

        private bool inSelectMode = false;
        private bool entered = false;
        private double value = 0;

        public double Value
        {
            get { return value; }
            set
            {
                if (value > 100.0)
                {
                    this.value = 100.0;
                }
                else
                {
                    this.value = value;
                }

                if (ValueChanged != null)
                {
                    ValueChanged(this, null);
                }
                updateLayout(this.value);
            }
        }

        #endregion

        #region Constructor

        public VolumeSelector()
        {
            InitializeComponent();
        }

        #endregion

        #region Events

        public event EventHandler ValueChanged;
        public event EventHandler StartHideVolume;
        public event EventHandler StartShowVolume;

        #endregion

        #region Methods

        private void Selector_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            inSelectMode = true;
        }

        private void Selector_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            inSelectMode = false;
        }

        private void updateLayout(double val)
        {
            valTxt.Text = ((int)val).ToString();
            LinearGradientBrush brush = Selector.OpacityMask as LinearGradientBrush;
            brush.GradientStops[0].Offset = val * 1.0 / 100;
            brush.GradientStops[1].Offset = val * 1.0 / 100;
        }

        private void container_MouseMove(object sender, MouseEventArgs e)
        {
            if (inSelectMode)
            {
                double x_pos = e.GetPosition(Selector).X;
                double relative = x_pos / Selector.Width * 100.0;
                if (relative < 0)
                {
                    relative = 0;
                }
                Value = relative;
            }
        }

        private void container_MouseEnter(object sender, MouseEventArgs e)
        {
            entered = true;
            if (this.StartShowVolume != null)
            {
                this.StartShowVolume(this, null);
            }
            showTimeSelector.Begin();
        }

        private void container_MouseLeave(object sender, MouseEventArgs e)
        {
            entered = false;
            if (!inSelectMode)
            {
                if (this.StartHideVolume != null)
                {
                    this.StartHideVolume(this, null);
                }
                hideTimeSelector.Begin();
            }
        }

        #endregion    
   
    }
}
