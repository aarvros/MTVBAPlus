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
    private MediaElement mediaElementRef;
    private bool isPaused = false;
    private bool isOver = false;

    public MainWindow(){
        InitializeComponent();
        mediaElementRef = (FindName("mediaElement") as MediaElement)!;
        mediaElementRef.Play();
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

    private void ThumbStart_DragDelta(object sender, DragDeltaEventArgs e)
    {
        double newLeft = Math.Max(0, ThumbStart.Margin.Left + e.HorizontalChange);
        double endLeft = ThumbEnd.Margin.Left;

        if (newLeft > endLeft) newLeft = endLeft;

        ThumbStart.Margin = new Thickness(newLeft, 0, 0, 0);
        UpdateTrimRange();
    }

    private void ThumbEnd_DragDelta(object sender, DragDeltaEventArgs e)
    {
        double newLeft = Math.Min(ThreeThumbSlider.ActualWidth - ThumbEnd.Width, ThumbEnd.Margin.Left + e.HorizontalChange);
        double startLeft = ThumbStart.Margin.Left;

        if (newLeft < startLeft) newLeft = startLeft;

        ThumbEnd.Margin = new Thickness(newLeft, 0, 0, 0);
        this.Title = $"{newLeft}";
        UpdateTrimRange();
    }

    private void ThumbCurrent_DragDelta(object sender, DragDeltaEventArgs e)
    {
        double newLeft = ThumbCurrent.Margin.Left + e.HorizontalChange;
        double min = ThumbStart.Margin.Left;
        double max = ThumbEnd.Margin.Left;

        newLeft = Math.Max(min, Math.Min(max, newLeft)); // Clamp to trim range

        ThumbCurrent.Margin = new Thickness(newLeft, 0, 0, 0);
        // TODO: Update media playback position based on newLeft
    }

    private void UpdateTrimRange()
    {
        double start = ThumbStart.Margin.Left;
        double end = ThumbEnd.Margin.Left;

        TrimRange.Margin = new Thickness(start, 0, 0, 0);
        TrimRange.Width = end - start;
    }




    private void MediaOpened(object sender, EventArgs e){
       //timelineSlider.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
    }

    private void MediaEnded(object sender, EventArgs e){
        mediaElementRef.Pause();
        isPaused = true;
        isOver = true;
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

    private void WindowSizeChanged(object sender, SizeChangedEventArgs e){
        double newWidth = e.NewSize.Width;
        double newHeight = e.NewSize.Height;

        this.Title = $"{newWidth} x {newHeight}";
    }
}