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
using System.Windows.Browser;
using System.Windows.Threading;
using System.ComponentModel;

namespace SilverlightPlayer.Controls
{
    [ScriptableType]
    public partial class CustomSilverlightPlayer : UserControl
    {
        #region Fields

        public DispatcherTimer timer = new DispatcherTimer();
        private bool isPausedBySlider = true;
        private string UrI = null;
        private IEnumerable<HtmlElement> hrefs = null;
        private HtmlDocument doc;

        #endregion

        #region Constructor
        public CustomSilverlightPlayer()
        {
            InitializeComponent();
            this.vol.StartShowVolume += (sender_, arg) => { this.hideExpandWndw.Begin(); };
            this.vol.StartHideVolume += (sender_, args) => { this.showExpandWnd.Begin(); };
            this.pathCont.MouseEnter += (sender_, args) => { this.pathIncreaseBrightness.Begin(); };
            this.pathCont.MouseLeave += (sender_, args) => { this.pathDecreseBrightness.Begin(); };
            this.pathCont.MouseLeftButtonUp += new MouseButtonEventHandler(pathCont_MouseLeftButtonUp);
            Application.Current.Host.Content.FullScreenChanged += new EventHandler(Content_FullScreenChanged);
            this.sliderPosition.AddHandler(Control.MouseLeftButtonDownEvent, new MouseButtonEventHandler((sender_, args) => { stopPlay(true); }), true);
            this.sliderPosition.AddHandler(Control.MouseLeftButtonUpEvent, new MouseButtonEventHandler((sender_, args) => { startPlay(true); }), true);
            this.sliderPosition.ValueChanged += new RoutedPropertyChangedEventHandler<double>(sliderPosition_ValueChanged);
            this.vol.ValueChanged += (sender_, args) => { if (media != null) { media.Volume = vol.Value / 100.0; } };
            this.media.MediaOpened += new RoutedEventHandler(media_MediaOpened);
            this.media.DownloadProgressChanged += new RoutedEventHandler(media_DownloadProgressChanged);
        }

        void sliderPosition_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (media.CurrentState == MediaElementState.Paused)
            {
                media.Position = TimeSpan.FromSeconds(sliderPosition.Value);
                this.PositionText.Content = media.Position.ToString().Substring(3, 5) + "/" + media.NaturalDuration.ToString().Substring(3, 5);
            }
        }

        #endregion

        #region Methods

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            timer.Interval = TimeSpan.FromSeconds(0.1);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
            HtmlPage.RegisterScriptableObject("bridge", this);
            UpdateTriggers();
        }

        void pathCont_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!Application.Current.Host.Content.IsFullScreen)
            {
                Application.Current.Host.Content.IsFullScreen = true;
            }
            else
            {
                Application.Current.Host.Content.IsFullScreen = false;
            }
        }

        void Content_FullScreenChanged(object sender, EventArgs e)
        {
            if (Application.Current.Host.Content.IsFullScreen)
            {
                path.Style = this.Resources["DecreasePath"] as Style;
            }
            else
            {
                path.Style = this.Resources["IncreasePath"] as Style;
            }
        }

        private void PlayClick(object Sender, EventArgs e)
        {
            if (media.CurrentState == MediaElementState.Playing)
            {
                stopPlay();
            }
            else
            {
                startPlay();
            }
        }

        //Calls when specified html-link clicked
        public void OnHtmlLinkClick(object sender, HtmlEventArgs args)
        {
            string link = (sender as HtmlElement).GetProperty("href").ToString();
            link = link.Substring(link.IndexOf('#') + 1);
            link = String.Format("http://{0}:{1}/{2}", doc.DocumentUri.Host, doc.DocumentUri.Port.ToString(), link);
            playFile(link);
        }

        //updates links refreshed by AJAX
        [ScriptableMember]
        public void UpdateTriggers()
        {
            doc = HtmlPage.Document;
            if (hrefs != null)
            {
                foreach (var href in hrefs)
                {
                    href.DetachEvent("onclick", OnHtmlLinkClick);
                }
            }
            hrefs = from c in doc.GetElementsByTagName("a").Cast<HtmlElement>()
                    where c.GetAttribute("class") != null
                    where c.GetAttribute("class").Contains("silverlight")
                    select c;
            foreach (HtmlElement el in hrefs)
            {
                el.AttachEvent("onclick", OnHtmlLinkClick);
            }
        }

        void media_DownloadProgressChanged(object sender, RoutedEventArgs e)
        {
            if (this.media.CurrentState != MediaElementState.Buffering)
            {
                this.sliderPosition.DownloadingProgress = media.DownloadProgress;
            }
        }

        void media_MediaOpened(object sender, RoutedEventArgs e)
        {
            this.sliderPosition.Maximum = media.NaturalDuration.TimeSpan.TotalSeconds;
        }

        private void media_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            if (UrI != null)
            {
                playFile(UrI);
            }
        }

        private void startPlay(bool isSourceSlider = false)
        {
            if (isSourceSlider && !isPausedBySlider)
            {
                return;
            }
            this.playPath.Opacity = 0;
            this.stopPath.Opacity = 1;

            if (media.CurrentState == MediaElementState.Closed)
            {
                media.CurrentStateChanged += media_CurrentStateChanged;
            }

            media.Play();
        }

        void media_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            media.Play();
            if (media.CurrentState != MediaElementState.Opening)
            {
                media.CurrentStateChanged -= media_CurrentStateChanged;
            }
        }

        private void stopPlay(bool isSourceSlider = false)
        {
            this.playPath.Opacity = 1;
            this.stopPath.Opacity = 0;
            if (media.CurrentState == MediaElementState.Playing
                && isSourceSlider)
            {
                isPausedBySlider = true;
            }
            else
            {
                isPausedBySlider = false;
            }

            media.Pause();
        }

        //change source of playing file
        private void playFile(string source)
        {
            UrI = source;
            this.media.Stop();
            this.media.Source = new Uri(this.UrI, UriKind.Absolute);
            this.media.Position = new TimeSpan(0);
            this.sliderPosition.Value = 0;
            startPlay();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (media.CurrentState != MediaElementState.Closed)
            {
                this.PositionText.Content = string.Format("{0}/{1}",
                    media.Position.ToString().Substring(3, 5),
                    media.NaturalDuration.ToString().Substring(3, 5));
                if (media.CurrentState == MediaElementState.Playing)
                {
                    this.sliderPosition.Value = media.Position.TotalSeconds;
                }
                if (media.Position == media.NaturalDuration && 
                    media.CurrentState != MediaElementState.Opening)
                {
                    stopPlay();
                }
            }
        }
        #endregion
    }
}
