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
    private RangeSlider rangeSlider;
    private MediaElement mediaElementRef;
    private DispatcherTimer mediaTimer;
    private Rectangle topShadowRef;
    private Rectangle bottomShadowRef;
    private Label mediaTitleRef;
    public Slider playSliderRef;
    public Thumb thumbStart;
    public Thumb thumbCurrent;
    public Thumb thumbEnd;
    public Label startTSLabel;
    public Label headTSLabel;
    public Label endTSLabel;
    private Button prevButtonRef;
    private Button nextButtonRef;
    private double mediaMsTotal;
    public bool isPaused = false;
    public bool isOver = false;
    public bool mediaControllingSlider = false;
    public int mediaStartPos = 0;
    public int mediaEndPos = 10000000;
    private string[] videoFiles;
    private int currentVideoIndex;

    public MainWindow(string[] args){
        string initialVideoPath;
        if(args.Length > 0){
            initialVideoPath = args[0];
        }else{
            string? filePath = GetVideoFromFileDialog();
            if(filePath == null){return;}
            initialVideoPath = filePath;
        }

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
        thumbStart = (FindName("ThumbStart") as Thumb)!;
        thumbEnd = (FindName("ThumbEnd") as Thumb)!;
        startTSLabel = (FindName("startTS") as Label)!;
        headTSLabel = (FindName("headTS") as Label)!;
        endTSLabel = (FindName("endTS") as Label)!;
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
        mediaStartPos = 0;
        mediaEndPos = 100000000;
        mediaElementRef.Source = new Uri(videoPath);
        mediaTitleRef.Content = System.IO.Path.GetFileName(videoPath);
        Dispatcher.BeginInvoke(new Action(() =>{
            playSliderRef = (FindName("playSlider") as Slider)!;
            Thumb thumb = (playSliderRef.Template.FindName("PART_Track", playSliderRef) as Track)!.Thumb;
            Panel.SetZIndex(thumb, 5);
            thumb.DragStarted += SliderDragStarted;
            thumb.DragCompleted += SliderDragCompleted;
            rangeSlider = new RangeSlider(this);   // for some reason, rangeSlider isnt init here yet? Just make a new one each media switch
        }), DispatcherPriority.Background);
        PlayMedia();
    }

    private void InitializeTimer()
    {
        mediaTimer = new DispatcherTimer();
        mediaTimer.Interval = TimeSpan.FromMilliseconds(100); // Update every 100ms
        mediaTimer.Tick += MediaTimerTick;
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

    public void RestartMedia(){
        if(isOver){
            isOver = false;
            double mediaMs = Math.Max(0, mediaStartPos / rangeSlider.sliderMax * mediaMsTotal);
            mediaElementRef.Position = TimeSpan.FromMilliseconds(mediaMs);
            PlayMedia();
        }
    }

    private void MediaTimerTick(object? sender, EventArgs e){
        double mediaMs = mediaElementRef.Position.TotalMilliseconds;
        int mediaPos = (int)Math.Min((int)(mediaMs / mediaMsTotal * rangeSlider.sliderMax), rangeSlider.sliderMax); // media progress from 0-1000
        headTSLabel.Content = TimeSpan.FromMilliseconds(mediaMs).ToString(@"hh\:mm\:ss\.fff");

        mediaControllingSlider = true;
        rangeSlider.PlaySliderValueChanged(mediaPos);
        mediaControllingSlider = false;

        Title = $"{mediaPos} {mediaEndPos}";
        if(mediaPos >= mediaEndPos){
            MediaEnded(mediaElementRef, new RoutedEventArgs());
        }
    }

    private void MediaOpened(object sender, EventArgs e){
        Dispatcher.BeginInvoke(new Action(() =>{
            mediaMsTotal = mediaElementRef.NaturalDuration.TimeSpan.TotalMilliseconds;
        }), DispatcherPriority.Background);
    }

    private void MediaEnded(object sender, EventArgs e){
        PauseMedia();
        isOver = true;
        topShadowRef.Visibility = Visibility.Visible;
        bottomShadowRef.Visibility = Visibility.Visible;
    }

    private void playSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e){
        isOver = false;                              // any drag or change will reset isOver. Media over checks are done AFTER playSlider is updated
        int newValue = (int)playSliderRef.Value;
        rangeSlider.PlaySliderValueChanged(newValue);
        headTSLabel.Content = TimeSpan.FromMilliseconds(mediaElementRef.Position.TotalMilliseconds).ToString(@"hh\:mm\:ss\.fff");
    }

    private void SliderDragStarted(object? sender, DragStartedEventArgs e){
        rangeSlider.DragStart();
    }

    private void SliderDragCompleted(object? sender, DragCompletedEventArgs e){
        rangeSlider.DragEnd();
    }

    private void ThumbDragStarted(object? sender, DragStartedEventArgs e){
        Thumb senderThumb = (sender as Thumb)!;
        if(senderThumb == thumbEnd){
            rangeSlider.EndThumbDragStart();
        }else{
            rangeSlider.DragStart();
        }
    }

    private void ThumbDragCompleted(object sender, DragCompletedEventArgs e){
        Thumb senderThumb = (sender as Thumb)!;
        if(senderThumb == thumbEnd){
            rangeSlider.EndThumbDragEnd();
        }else{
            rangeSlider.DragEnd();
        }
    }

    private void ThumbStart_DragDelta(object sender, DragDeltaEventArgs e){
        rangeSlider.StartDragDelta(e);
        double ms = Math.Max(0, rangeSlider.startPos / rangeSlider.sliderMax * mediaMsTotal);
        startTSLabel.Content = TimeSpan.FromMilliseconds(ms).ToString(@"hh\:mm\:ss\.fff");
    }

    private void ThumbEnd_DragDelta(object sender, DragDeltaEventArgs e){
        rangeSlider.EndDragDelta(e);
        double ms = Math.Max(0, rangeSlider.endPos / rangeSlider.sliderMax * mediaMsTotal);
        endTSLabel.Content = TimeSpan.FromMilliseconds(ms).ToString(@"hh\:mm\:ss\.fff");
    }

    public void UpdateMediaPosition(int position){  // take a value from 0 to 1000
        double mediaMs = Math.Max(0, position / rangeSlider.sliderMax * mediaMsTotal);
        mediaElementRef.Position = TimeSpan.FromMilliseconds(mediaMs);
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

        TimeSpan start = TimeSpan.FromMilliseconds(Math.Max(0, mediaStartPos / 1000.0 * mediaMsTotal));
        TimeSpan end = TimeSpan.FromMilliseconds(Math.Max(0, mediaEndPos / 1000.0 * mediaMsTotal));
        TimeSpan duration = end - start;

        string ffmpegPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg", "ffmpeg.exe");
        string arguments = $"-i \"{inputPath}\" -ss {start} -t {duration} -c:v copy -c:a copy \"{fullPath}\"";

        ProcessStartInfo processStartInfo = new ProcessStartInfo{
            FileName = ffmpegPath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try{
            using (Process process = Process.Start(processStartInfo)!){}
            MessageBox.Show($"Successfully saved trim to {fullPath}", "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
        }catch (Exception ex){
            MessageBox.Show($"Error running FFmpeg: {ex.Message}", "FFmpeg Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Console.WriteLine("Error running FFmpeg: " + ex.Message);
        }
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