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

namespace MTVBAPlus;

public partial class MainWindow : Window
{
    #pragma warning disable CS8618
    private RangeSlider rangeSlider;
    private MediaElement mediaElementRef;
    private Rectangle topShadowRef;
    private Rectangle bottomShadowRef;
    private Grid threeThumbSlider;
    private Rectangle trimRange;
    private Thumb thumbStart;
    private Thumb thumbCurrent;
    private Thumb thumbEnd;
    private bool isPaused = false;
    private bool isOver = false;

    public MainWindow(){
        InitializeComponent();
        InitializeRefs();
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

        
    }
        
    private void TogglePlayback(object sender, MouseButtonEventArgs args){
        if(isOver){
            RestartMedia();
        }else if(isPaused){
            PlayMedia();
        }else{
            PauseMedia();
        }
    }
    
    private void PauseMedia(){
        mediaElementRef.Pause();
        isPaused = true;
        // logic for overlayed pause icon
    }

    private void PlayMedia(){
        //logic for removing pause icon
        isPaused = false;
        mediaElementRef.Play();
    }

    private void RestartMedia(){    // called from TogglePlayback
        if(isOver){
            isOver = false;
            mediaElementRef.Position = TimeSpan.Zero;
            PlayMedia();
        }
    }

    private void MediaOpened(object sender, EventArgs e){
        rangeSlider = new RangeSlider(thumbStart, thumbCurrent, thumbEnd, (int)threeThumbSlider.ActualWidth-12);   // Initialize Range Slider. 12 is the width of the selector
       //timelineSlider.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
    }

    private void MediaEnded(object sender, EventArgs e){
        mediaElementRef.Pause();
        isPaused = true;
        isOver = true;
        topShadowRef.Visibility = Visibility.Visible;
        bottomShadowRef.Visibility = Visibility.Visible;
    }

    // Jump to different parts of the media (seek to).
    private void SeekToMediaPosition(object sender, RoutedPropertyChangedEventArgs<double> args){
        //int SliderValue = (int)timelineSlider.Value;
        //TimeSpan ts = new TimeSpan(0, 0, 0, 0, SliderValue);
        //mediaElementRef.Position = ts;
    }

    private void InitializePropertyValues(){
        // Set the media's starting Volume and SpeedRatio to the current value of the
        // their respective slider controls.
        //mediaElement.Volume = (double)volumeSlider.Value;
        //mediaElement.SpeedRatio = (double)speedRatioSlider.Value;
    }

    private double MapPositionToRange(double position, double maxPosition){
        return position / maxPosition * 1000;
    }

    private void ThumbStart_DragDelta(object sender, DragDeltaEventArgs e){
        Title = rangeSlider.StartDragDelta(e).ToString();
        //UpdateTrimRange();
    }

    private void ThumbEnd_DragDelta(object sender, DragDeltaEventArgs e){
        Title = rangeSlider.EndDragDelta(e).ToString();
        //UpdateTrimRange();
    }

    private void ThumbCurrent_DragDelta(object sender, DragDeltaEventArgs e){
        Title = rangeSlider.CurrDragDelta(e).ToString();
        // TODO: Update media playback position based on newLeft
    }

    private void UpdateTrimRange(){
        double start = thumbStart.Margin.Left;
        double end = thumbEnd.Margin.Left;

        trimRange.Margin = new Thickness(start, 0, 0, 0);
        trimRange.Width = end - start;
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