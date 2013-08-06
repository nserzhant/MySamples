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
    public class DownloaderSlider : Slider
    {
        #region Properties and fields

        private Rectangle outerRectangle = null;
        private Rectangle downloadedPositionRectangle = null;

        public static readonly DependencyProperty ColorLineProperty = 
            DependencyProperty.Register("ColorLine", typeof(Brush), typeof(DownloaderSlider), null);
        public static readonly DependencyProperty PositionDownloadedProperty = 
            DependencyProperty.Register("PositionDownloaded", typeof(double), typeof(DownloaderSlider), null);
        public static readonly DependencyProperty ColorDownloadedProperty = 
            DependencyProperty.Register("ColorDownloaded", typeof(Brush), typeof(DownloaderSlider), null);

        public double PositionDownloaded
        {
            get { return (double)GetValue(PositionDownloadedProperty); }
            set { SetValue(PositionDownloadedProperty, value); }
        }

        public Brush ColorLine
        {
            get { return (Brush)GetValue(ColorLineProperty); }
            set { SetValue(ColorLineProperty, value); }
        }

        public Brush ColorDownloaded
        {
            get { return (Brush)GetValue(ColorDownloadedProperty); }
            set { SetValue(ColorDownloadedProperty, value); }
        }

        private double downloadingProgress = 0;

        public double DownloadingProgress
        {
            get { return downloadingProgress; }
            set
            {
                downloadingProgress = value;
                this.resetDownloadedRectanglePosition();
            }
        }

        #endregion

        public DownloaderSlider()
        {
            this.DefaultStyleKey = typeof(DownloaderSlider);
        }


        #region Methods

        public override void OnApplyTemplate()
        {
            outerRectangle = this.GetTemplateChild("OuterRectangle") as Rectangle;
            downloadedPositionRectangle = this.GetTemplateChild("DownloadedPositionRectangle") as Rectangle;
            if (outerRectangle != null)
            {
                outerRectangle.SizeChanged += (s, a) =>
                {
                    this.resetDownloadedRectanglePosition();
                };
            }
            base.OnApplyTemplate();
        }

        private void resetDownloadedRectanglePosition()
        {
            if (outerRectangle == null || downloadedPositionRectangle == null)
            {
                return;
            }
            double outerSize = outerRectangle.ActualWidth;
            downloadedPositionRectangle.Width = outerSize * downloadingProgress;
        }

        #endregion

    }
}
