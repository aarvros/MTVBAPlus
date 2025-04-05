using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using MTVBAPlus;

public partial class RangeSlider
{
    private MainWindow mainWindow;
    private Thumb startThumb;
    private Thumb currThumb;
    private Thumb endThumb;
    private Rectangle trimRange;
    public int startPos;
    public int currPos;
    public int endPos;
    private double maxCanvasPosition;
    private bool unpauseAfterDragging;
    public RangeSlider(MainWindow mainWindow, Thumb startThumb, Thumb currThumb, Thumb endThumb, Rectangle trimRange, double maxCanvasPosition){
        this.mainWindow = mainWindow;
        this.startThumb = startThumb;
        this.currThumb = currThumb;
        this.endThumb = endThumb;
        this.trimRange = trimRange;
        this.maxCanvasPosition = maxCanvasPosition;     // resizing the window will cause issues
        unpauseAfterDragging = true;

        startThumb.Margin = new Thickness(0,0,0,0);
        currThumb.Margin = new Thickness(0,0,0,0);
        endThumb.Margin = new Thickness(maxCanvasPosition,0,0,0);
        UpdateTrimRange();

        startPos = MapMarginToPosition(startThumb);
        currPos = MapMarginToPosition(currThumb);
        endPos = MapMarginToPosition(endThumb);
    }

    private int MapMarginToPosition(Thumb thumb){
        return (int)(Math.Floor(thumb.Margin.Left) / maxCanvasPosition * 1000);
    }

    private double MapPositionToMargin(int position){
        return position / 1000.0 * maxCanvasPosition;
    }

    public bool UpdateCurrThumb(int position){  // incoming position is the mediaPlayer.Time mapped from time to position 0-1000
        bool stopMedia = position >= endPos;    // stops the media if it runs over the ending position. PlayMedia is responsible for setting start time to startpos
        double margin = MapPositionToMargin(position);
        currThumb.Margin = new Thickness(margin,0,0,0);
        SnapCurrThumbToBounds();
        return stopMedia;
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

    public void StartDragDelta(DragDeltaEventArgs e){
        double newLeft = Math.Max(0, startThumb.Margin.Left + e.HorizontalChange);
        newLeft = Math.Min(newLeft, endThumb.Margin.Left);
        startThumb.Margin = new Thickness(newLeft,0,0,0);
        startPos = MapMarginToPosition(startThumb);
        currThumb.Margin = new Thickness(startThumb.Margin.Left,0,0,0);
        currPos = MapMarginToPosition(currThumb);
        mainWindow.mediaStartPos = startPos;
        mainWindow.UpateMediaPosition(currPos);
        SnapCurrThumbToBounds();
        UpdateTrimRange();
    }

    public void CurrDragDelta(DragDeltaEventArgs e){
        double newLeft = Math.Min(currThumb.Margin.Left + e.HorizontalChange, maxCanvasPosition);
        newLeft = Math.Min(newLeft, endThumb.Margin.Left);
        newLeft = Math.Max(startThumb.Margin.Left, newLeft);
        currThumb.Margin = new Thickness(newLeft,0,0,0);
        currPos = MapMarginToPosition(currThumb);
        mainWindow.UpateMediaPosition(currPos);
    }

    public void EndDragDelta(DragDeltaEventArgs e){
        double newLeft = Math.Min(endThumb.Margin.Left + e.HorizontalChange, maxCanvasPosition);
        newLeft = Math.Max(startThumb.Margin.Left, newLeft);
        endThumb.Margin = new Thickness(newLeft,0,0,0);
        endPos = MapMarginToPosition(endThumb);
        currThumb.Margin = new Thickness(endThumb.Margin.Left,0,0,0);
        currPos = MapMarginToPosition(currThumb);
        mainWindow.mediaEndPos = endPos;
        mainWindow.UpateMediaPosition(currPos);
        SnapCurrThumbToBounds();
        UpdateTrimRange();
    }

    public void SnapCurrThumbToBounds(){
        double currThumbPos = currThumb.Margin.Left;
        double newLeft = Math.Min(currThumbPos, endThumb.Margin.Left);
        newLeft = Math.Max(startThumb.Margin.Left, newLeft);

        if(currThumbPos != newLeft){
            currThumb.Margin = new Thickness(newLeft,0,0,0);
            currPos = MapMarginToPosition(currThumb);   
            mainWindow.UpateMediaPosition(currPos);
        }
    }

    private void UpdateTrimRange(){
        trimRange.Margin = new Thickness(startThumb.Margin.Left, 0, 0, 0);
        trimRange.Width = endThumb.Margin.Left - startThumb.Margin.Left;
    }
}