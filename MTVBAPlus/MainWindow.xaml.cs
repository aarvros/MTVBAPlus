using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;

namespace MTVBAPlus;

public partial class MainWindow : Window
{
    #pragma warning disable CS8618
    private MediaElement mediaElementRef;
    private DispatcherTimer mediaTimer;
    private Rectangle topShadowRef;
    private Rectangle bottomShadowRef;
    private Label mediaTitleRef;
    public Slider playSliderRef;
    public Label startTSLabel;
    public Label headTSLabel;
    public Label endTSLabel;
    public Line startMarker;
    public Line endMarker;
    private Button prevButtonRef;
    private Button nextButtonRef;
    private double mediaMsTotal;
    public bool isDragging;
    public bool unpauseAfterDragging;
    public bool isPaused = false;
    public bool isOver = false;
    public double mediaStartMs = 0;
    public double mediaEndMs = 10000000;
    private string[] videoFiles;
    private int currentVideoIndex;
    private string ffmpegPath;

    public MainWindow(string[] args){   // https://github.com/skeskinen/smartcut    take a look at this instead of using ffmpeg
        string initialVideoPath;
        if(args.Length > 0){
            initialVideoPath = args[0];
        }else{
            string? filePath = GetVideoFromFileDialog();
            if(filePath == null){Close();}
            initialVideoPath = filePath!;
        }

        ffmpegPath = FFmpegHelper.ExtractFfmpeg();
        InitializeComponent();
        InitializeRefs();
        InitializeVideoFolder(initialVideoPath);
        InitializeTimer();                  // do this once
        InitializeMedia(initialVideoPath);
    }

    private string? GetVideoFromFileDialog(){
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Title = "Select mp4";
        openFileDialog.Filter = "MP4 files (*.mp4)|*.mp4|All files (*.*)|*.*";
        openFileDialog.DefaultExt = "mp4";
        bool? result = openFileDialog.ShowDialog();
        if(result != true){
            return null;
        }else{
            return openFileDialog.FileName;
        }
    }

    private void InitializeRefs(){
        mediaElementRef = (FindName("mediaElement") as MediaElement)!;
        topShadowRef = (FindName("topShadow") as Rectangle)!;
        bottomShadowRef = (FindName("bottomShadow") as Rectangle)!;
        mediaTitleRef = (FindName("mediaTitle") as Label)!;
        //playSliderRef = (FindName("playSlider") as Slider)!;
        startTSLabel = (FindName("startTS") as Label)!;
        headTSLabel = (FindName("headTS") as Label)!;
        endTSLabel = (FindName("endTS") as Label)!;
        startMarker = (FindName("StartMarker") as Line)!;
        EndMarker = (FindName("EndMarker") as Line)!;
        prevButtonRef = (FindName("PrevButton") as Button)!;
        nextButtonRef = (FindName("NextButton") as Button)!;
    }

    private void InitializeVideoFolder(string initialVideoPath){
        string videoDirectory = System.IO.Path.GetDirectoryName(initialVideoPath)!;
        videoFiles = Directory.GetFiles(videoDirectory, "*.mp4").OrderBy(f => System.IO.Path.GetFileName(f)).ToArray();
        currentVideoIndex = Array.IndexOf(videoFiles, initialVideoPath);
        if(currentVideoIndex <= 0){prevButtonRef.IsEnabled = false;}
        if(currentVideoIndex >= videoFiles.Length-1){nextButtonRef.IsEnabled = false;}
    }

    private void LoadPrevVideo(object sender, EventArgs e){
        currentVideoIndex--;
        if(currentVideoIndex <= 0){
            prevButtonRef.IsEnabled = false;
        }
        nextButtonRef.IsEnabled = true;
        InitializeMedia(videoFiles[currentVideoIndex]);
    }

    private void LoadNextVideo(object sender, EventArgs e){
        currentVideoIndex++;
        if(currentVideoIndex >= videoFiles.Length-1){
            nextButtonRef.IsEnabled = false;
        }
        prevButtonRef.IsEnabled = true;
        InitializeMedia(videoFiles[currentVideoIndex]);
    }
    
    private void InitializeMedia(string videoPath){
        PauseMedia();
        isOver = false;
        mediaElementRef.Source = new Uri(videoPath);
        mediaTitleRef.Content = System.IO.Path.GetFileName(videoPath);
        Dispatcher.BeginInvoke(new Action(() =>{
            playSliderRef = (FindName("playSlider") as Slider)!;
            Thumb thumb = (playSliderRef.Template.FindName("PART_Track", playSliderRef) as Track)!.Thumb;
            Panel.SetZIndex(thumb, 5);
            thumb.DragStarted += SliderDragStarted;
            thumb.DragCompleted += SliderDragCompleted;
            //rangeSlider = new RangeSlider(this);   // for some reason, rangeSlider isnt init here yet? Just make a new one each media switch
        }), DispatcherPriority.Background);
        PlayMedia();
    }

    private void MediaOpened(object sender, EventArgs e){
        Dispatcher.BeginInvoke(new Action(() =>{
            mediaMsTotal = mediaElementRef.NaturalDuration.TimeSpan.TotalMilliseconds;
            mediaStartMs = 0;
            mediaEndMs = mediaMsTotal;
            playSliderRef.Maximum = mediaMsTotal;
        }), DispatcherPriority.Background);
        mediaTimer.Start();
    }

    private void InitializeTimer(){
        mediaTimer = new DispatcherTimer();
        mediaTimer.Interval = TimeSpan.FromMilliseconds(1); // Update every 100ms
        mediaTimer.Tick += MediaTimerTick;
    }

    private void MediaTimerTick(object? sender, EventArgs e){
        double mediaMs = mediaElementRef.Position.TotalMilliseconds;
        headTSLabel.Content = TimeSpan.FromMilliseconds(mediaMs).ToString(@"hh\:mm\:ss\.fff");

        if(!isDragging){
            SetPlaySliderValue(mediaMs);
            headTSLabel.Content = TimeSpan.FromMilliseconds(mediaElementRef.Position.TotalMilliseconds).ToString(@"hh\:mm\:ss\.fff");
        }else{
            UpdateMediaPosition(playSliderRef.Value);
        }

        if(!isDragging && mediaMs >= mediaEndMs){
            PauseMedia();
            isOver = true;
            topShadowRef.Visibility = Visibility.Visible;
            bottomShadowRef.Visibility = Visibility.Visible;
        }
    }

    public void SetPlaySliderValue(double newValue){
        newValue = Math.Max(mediaStartMs, newValue);    // clamp min value to start position
        newValue = Math.Min(newValue, mediaEndMs);      // clamp max value to end position
        playSlider.Value = newValue;
        if(isDragging){
            UpdateMediaPosition(newValue);
        }
    }

    public void UpdateMediaPosition(double newValue){
        newValue = Math.Max(mediaStartMs, newValue);
        newValue = Math.Min(newValue, mediaEndMs);
        mediaElementRef.Position = TimeSpan.FromMilliseconds(newValue);
    }

    public void TogglePlayback(object sender, RoutedEventArgs e){
        TogglePlayback();
    }
        
    public void TogglePlayback(object sender, MouseButtonEventArgs args){
        TogglePlayback();
    }

    public void TogglePlayback(){
        if(isOver){
            RestartMedia();
        }else if(isPaused){
            PlayMedia();
        }else{
            PauseMedia();
        }
    }
    
    public void PauseMedia(){
        mediaElementRef.Pause();
        isPaused = true;
    }

    public void PlayMedia(){
        isPaused = false;
        mediaElementRef.Play();
    }

    public void RestartMedia(){
        if(isOver){
            isOver = false;
            double mediaMs = mediaStartMs;
            mediaElementRef.Position = TimeSpan.FromMilliseconds(mediaMs);
            PlayMedia();
        }
    }

    private void SliderDragStarted(object? sender, DragStartedEventArgs e){
        isDragging = true;
        isOver = false;
        unpauseAfterDragging = !isPaused;
        PauseMedia();
    }

    private void SliderDragCompleted(object? sender, DragCompletedEventArgs e){
        if(unpauseAfterDragging){
            PlayMedia();
        }
        isDragging = false;
    }

    public void SetStartPos(object sender, RoutedEventArgs e){
        double value = playSliderRef.Value;
        mediaStartMs = value;
        UpdateMarker(StartMarker, value);
        startTSLabel.Content = TimeSpan.FromMilliseconds(value).ToString(@"hh\:mm\:ss\.fff");
    }

    public void SetEndPos(object sender, RoutedEventArgs e){
        double value = playSliderRef.Value;
        mediaEndMs = value;
        UpdateMarker(EndMarker, value);
        endTSLabel.Content = TimeSpan.FromMilliseconds(value).ToString(@"hh\:mm\:ss\.fff");
    }

    public void UnsetStartPos(object sender, RoutedEventArgs e){
        mediaStartMs = 0;
        UpdateMarker(StartMarker, -5000);
        startTSLabel.Content = TimeSpan.FromMilliseconds(0).ToString(@"hh\:mm\:ss\.fff");
    }

    public void UnsetEndPos(object sender, RoutedEventArgs e){
        mediaEndMs = mediaMsTotal;
        UpdateMarker(EndMarker, 10000000000000000);
        endTSLabel.Content = TimeSpan.FromMilliseconds(mediaMsTotal).ToString(@"hh\:mm\:ss\.fff");
        isOver = false;     // Lets you continue playback without restarting
    }

    private void UpdateMarker(Line marker, double timeMs){
        double max = playSliderRef.Maximum;
        double width = playSliderRef.ActualWidth;

        if (max <= 0 || width <= 0)
            return;

        double percent = timeMs / max;
        double pos = percent * width;

        Canvas.SetLeft(marker, pos + 10);
    }

    private void TrimVideo(object sender, EventArgs e){
        if(!isPaused){
            PauseMedia();
        }

        string inputPath = videoFiles[currentVideoIndex];
        
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.Title = "Save As";
        saveFileDialog.Filter = "MP4 files (*.mp4)|*.mp4|All files (*.*)|*.*";
        saveFileDialog.DefaultExt = "mp4";
        saveFileDialog.DefaultDirectory = System.IO.Path.GetDirectoryName(inputPath)!;
        bool? result = saveFileDialog.ShowDialog();

        if (result != true){return;}
        string fullPath = saveFileDialog.FileName;

        if(File.Exists(fullPath)){
            MessageBox.Show($"File {fullPath} already exists!", "File Already Exists", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            return;
        }

        StartTrimWithProgress(inputPath, fullPath, mediaStartMs, mediaEndMs);
    }

    private void StartTrimWithProgress(string inputPath, string outputPath, double startMs, double endMs)
    {
        var loadingDialog = new LoadingDialog();
        loadingDialog.Owner = this;

        Task.Run(() =>
        {
            bool success = FFmpegHelper.TrimVideo(inputPath, outputPath, startMs, endMs);

            Dispatcher.Invoke(() =>{
                loadingDialog.Close();
            });
        });

        loadingDialog.ShowDialog();
    }


    private void WindowMouseEnter(object sender, MouseEventArgs e){
        topShadowRef.Visibility = Visibility.Visible;
        bottomShadowRef.Visibility = Visibility.Visible;
    }

    private void WindowMouseExit(object sender, MouseEventArgs e){
        topShadowRef.Visibility = Visibility.Hidden;
        bottomShadowRef.Visibility = Visibility.Hidden;
    }
}