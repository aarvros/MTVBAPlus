using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MTVBAPlus;

public partial class MainWindow : Window
{
    #pragma warning disable CS8618
    private RangeSlider rangeSlider;
    private MediaElement mediaElementRef;
    private DispatcherTimer mediaTimer;
    private Rectangle topShadowRef;
    private Rectangle bottomShadowRef;
    private Grid threeThumbSlider;
    private Rectangle trimRange;
    private Thumb thumbStart;
    private Thumb thumbCurrent;
    private Thumb thumbEnd;
    private double mediaMsTotal;
    public bool isPaused = false;
    private bool isOver = false;
    public int mediaStartPos = 0;
    public int mediaEndPos = 10000000;

    public MainWindow(){
        InitializeComponent();
        InitializeRefs();
        InitializeTimer();
        PlayMedia();
    }

    private void InitializeRefs(){
        mediaElementRef = (FindName("mediaElement") as MediaElement)!;
        topShadowRef = (FindName("topShadow") as Rectangle)!;
        bottomShadowRef = (FindName("bottomShadow") as Rectangle)!;
        threeThumbSlider = (FindName("ThreeThumbSlider") as Grid)!;
        trimRange = (FindName("TrimRange") as Rectangle)!;
        thumbStart = (FindName("ThumbStart") as Thumb)!;
        thumbCurrent = (FindName("ThumbCurrent") as Thumb)!;
        thumbEnd = (FindName("ThumbEnd") as Thumb)!;
        Dispatcher.BeginInvoke(new Action(() =>{
            rangeSlider = new RangeSlider(this, thumbStart, thumbCurrent, thumbEnd, trimRange, threeThumbSlider.ActualWidth-12);
        }), System.Windows.Threading.DispatcherPriority.Background);
    }

    private void InitializeTimer()
    {
        mediaTimer = new DispatcherTimer();
        mediaTimer.Interval = TimeSpan.FromMilliseconds(100); // Update every 100ms
        mediaTimer.Tick += MediaTimerTick;
    }
        
    public void TogglePlayback(object sender, MouseButtonEventArgs args){
        if(isOver){
            RestartMedia();
        }else if(isPaused){
            PlayMedia();
        }else{
            PauseMedia();
        }
    }
    
    public void PauseMedia(){
        mediaTimer.Stop();
        mediaElementRef.Pause();
        isPaused = true;
        // logic for overlayed pause icon
    }

    public void PlayMedia(){
        //logic for removing pause icon
        isPaused = false;
        mediaElementRef.Play();
        mediaTimer.Start();
    }

    public void RestartMedia(){    // called from TogglePlayback
        if(isOver){
            isOver = false;
            mediaElementRef.Position = TimeSpan.FromMilliseconds(mediaStartPos);
            PlayMedia();
        }
    }

    private void MediaTimerTick(object? sender, EventArgs e){
        double mediaMs = mediaElementRef.Position.TotalMilliseconds;
        int mediaPos = Math.Min((int)(mediaMs / mediaMsTotal * 1000), 1000);    // media progress from 0-1000
        rangeSlider.UpdateCurrThumb(mediaPos);

        if(mediaPos >= mediaEndPos){
            MediaEnded(mediaElementRef, new RoutedEventArgs());
        }
    }

    private void MediaOpened(object sender, EventArgs e){
        Dispatcher.BeginInvoke(new Action(() =>{
            mediaMsTotal = mediaElementRef.NaturalDuration.TimeSpan.TotalMilliseconds;
        }), System.Windows.Threading.DispatcherPriority.Background);
    }

    private void MediaEnded(object sender, EventArgs e){
        PauseMedia();
        mediaTimer.Stop();
        isPaused = true;
        isOver = true;
        topShadowRef.Visibility = Visibility.Visible;
        bottomShadowRef.Visibility = Visibility.Visible;
    }

    private void DragStarted(object sender, DragStartedEventArgs e){
        rangeSlider.DragStart();
    }

    private void DragCompleted(object sender, DragCompletedEventArgs e){
        rangeSlider.DragEnd();
    }

    private void ThumbStart_DragDelta(object sender, DragDeltaEventArgs e){
        rangeSlider.StartDragDelta(e);
    }

    private void ThumbEnd_DragDelta(object sender, DragDeltaEventArgs e){
        rangeSlider.EndDragDelta(e);
    }

    private void ThumbCurrent_DragDelta(object sender, DragDeltaEventArgs e){
        rangeSlider.CurrDragDelta(e);
    }

    public void UpateMediaPosition(int position){
        double mediaMs = Math.Max(0, position / 1000.0 * mediaMsTotal);
        Title=$"{position} {mediaMs} {mediaMsTotal} {position / 1000.0 * mediaMsTotal}";
        mediaElementRef.Position = TimeSpan.FromMilliseconds(mediaMs);
    }

    private void WindowMouseEnter(object sender, MouseEventArgs e){
        topShadowRef.Visibility = Visibility.Visible;
        bottomShadowRef.Visibility = Visibility.Visible;
    }

    private void WindowMouseExit(object sender, MouseEventArgs e){
        topShadowRef.Visibility = Visibility.Hidden;
        bottomShadowRef.Visibility = Visibility.Hidden;
    }

    private void WindowSizeChanged(object sender, SizeChangedEventArgs e){
        double newWidth = e.NewSize.Width;
        double newHeight = e.NewSize.Height;

        Title = $"{newWidth} x {newHeight}";
    }
}