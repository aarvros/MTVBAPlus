using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using MTVBAPlus;

public partial class RangeSlider
{
    private MainWindow mainWindow;
    public double sliderMax = 10000.0;
    public Slider playSlider;
    private Thumb startThumb;
    private Thumb endThumb;
    public int startPos = 0;
    public int endPos;
    private double sliderWidth;
    private bool unpauseAfterDragging = true;
    private int prevPosition = 0;
    public RangeSlider(MainWindow mainWindow){
        this.mainWindow = mainWindow;
        playSlider = mainWindow.playSliderRef;
        startThumb = mainWindow.thumbStart;
        endThumb = mainWindow.thumbEnd;
        sliderWidth = playSlider.ActualWidth-12;   // correct for thumb width

        ResetThumbs();
    }

    public void ResetThumbs(){
        startThumb.Margin = new Thickness(0,0,0,0);
        endThumb.Margin = new Thickness(sliderWidth,0,0,0);

        startPos = 0;
        endPos = (int)sliderMax;
        mainWindow.mediaEndPos = endPos;
        
        SetStartMarginFromPosition(startPos);
        SetEndMarginFromPosition(endPos);
    }

    private int GetStartThumbPositionFromMargin(double margin){
        return (int)((margin + startThumb.ActualWidth/2) / sliderWidth * sliderMax);      // position based on the middle of the thumb
    }

    private int GetEndThumbPositionFromMargin(double margin){
        return (int)((margin - endThumb.ActualWidth/2) / sliderWidth * sliderMax);
    }

    private void SetStartMarginFromPosition(int position){
        double margin = position / sliderMax * sliderWidth - (startThumb.ActualWidth/2); // set margin left by 6, so middle of thumb is equal to position 0 on track
        startThumb.Margin = new Thickness(margin,0,0,0);
    }

    private void SetEndMarginFromPosition(int position){
        double margin = position / sliderMax * sliderWidth + (endThumb.ActualWidth/2); // set margin right by 6, so middle of thumb is equal to position 0 on track
        endThumb.Margin = new Thickness(margin,0,0,0);
    }

    public void DragStart(){
        unpauseAfterDragging = !mainWindow.isPaused;
        mainWindow.PauseMedia();
    }

    public void DragEnd(){
        if(unpauseAfterDragging){
            mainWindow.PlayMedia();
        }
    }

    public void EndThumbDragStart(){
        prevPosition = (int)playSlider.Value;   // save play head pos on drag start
        DragStart();                            // continue with normal drag start
    }

    public void EndThumbDragEnd(){
        mainWindow.UpdateMediaPosition(prevPosition);   // change media position back to prev position
        DragEnd();                                      // continue with normal drag end
    }

    public void PlaySliderValueChanged(int newValue){
        newValue = Math.Max(startPos, newValue);    // clamp min value to start position
        newValue = Math.Min(newValue, endPos);      // clamp max value to end position
        playSlider.Value = newValue;
        if(!mainWindow.mediaControllingSlider){
            mainWindow.UpdateMediaPosition(newValue);
        }
    }

    public void StartDragDelta(DragDeltaEventArgs e){
        double newMargin = startThumb.Margin.Left + e.HorizontalChange;
        startPos = GetStartThumbPositionFromMargin(newMargin);
        startPos = Math.Max(0, startPos);
        startPos = Math.Min(startPos, endPos);
        SetStartMarginFromPosition(startPos);
        mainWindow.mediaStartPos = startPos;
        PlaySliderValueChanged(startPos);       // drags the slider thumb with it. Does not do this on end thumb intentionally
        mainWindow.Title = $"{sliderWidth} {newMargin} {sliderMax} {startPos}";
    }


    public void EndDragDelta(DragDeltaEventArgs e){     // dont snap play head to end thumb, but still snap media pos for scrubbing
        double newMargin = endThumb.Margin.Left + e.HorizontalChange;
        endPos = GetEndThumbPositionFromMargin(newMargin);
        endPos = Math.Max(startPos, endPos);
        endPos = Math.Min(endPos, (int)sliderMax);
        SetEndMarginFromPosition(endPos);
        mainWindow.mediaEndPos = endPos;
        mainWindow.UpdateMediaPosition(endPos);
        mainWindow.Title = $"{sliderWidth} {newMargin} {sliderMax} {endPos}";


        //double newLeft = Math.Min(endThumb.Margin.Left + e.HorizontalChange, sliderWidth);
        //newLeft = Math.Max(startThumb.Margin.Left, newLeft);
        //endThumb.Margin = new Thickness(newLeft,0,0,0);
        //endPos = GetEndThumbPositionFromMargin();
        //mainWindow.Title = $"{sliderWidth} {newLeft} {endPos}";
        //mainWindow.mediaEndPos = endPos;
        //mainWindow.UpdateMediaPosition(endPos);
        //mainWindow.isOver = false;
        //SnapCurrThumbToBounds();
    }

    public bool UpdateCurrThumb(int position){  // incoming position is the mediaPlayer.Time mapped from time to position 0-1000
        //bool stopMedia = position >= endPos;    // stops the media if it runs over the ending position. PlayMedia is responsible for setting start time to startpos
        //double margin = MapPositionToMargin(position);
        //currThumb.Margin = new Thickness(margin,0,0,0);
        //currPos = position;
        //SnapCurrThumbToBounds(mediaSnapSource: true);
        //return stopMedia;
        return false;
    }
}