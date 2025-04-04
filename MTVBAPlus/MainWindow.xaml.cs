using System.Text;
using System.Windows;
using System.Windows.Controls;
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

    private void OnMouseDownPlayMedia(object sender, MouseButtonEventArgs args){
        mediaElementRef.Play();
        InitializePropertyValues();
    }

    // Pause the media.
    private void OnMouseDownPauseMedia(object sender, MouseButtonEventArgs args){
        mediaElementRef.Pause();
    }

    // Stop the media.
    private void OnMouseDownStopMedia(object sender, MouseButtonEventArgs args){
        mediaElementRef.Stop();
    }

    // When the media opens, initialize the "Seek To" slider maximum value
    // to the total number of miliseconds in the length of the media clip.
    private void Element_MediaOpened(object sender, EventArgs e){
       //timelineSlider.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
    }

    // When the media playback is finished. Stop() the media to seek to media start.
    private void Element_MediaEnded(object sender, EventArgs e){
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