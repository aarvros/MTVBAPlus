using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;

public partial class RangeSlider
{
    private Thumb startThumb;
    private Thumb currThumb;
    private Thumb endThumb;
    private Rectangle trimRange;
    public int startPos;
    public int currPos;
    public int endPos;
    private int maxCanvasPosition;
    public RangeSlider(Thumb startThumb, Thumb currThumb, Thumb endThumb, Rectangle trimRange, int maxCanvasPosition){
        this.startThumb = startThumb;
        this.currThumb = currThumb;
        this.endThumb = endThumb;
        this.trimRange = trimRange;
        this.maxCanvasPosition = maxCanvasPosition;     // resizing the window will cause issues

        startThumb.Margin = new Thickness(0,0,0,0);
        currThumb.Margin = new Thickness(0,0,0,0);
        endThumb.Margin = new Thickness(maxCanvasPosition,0,0,0);
        UpdateTrimRange();

        startPos = MapThumbToPosition(startThumb);
        currPos = MapThumbToPosition(currThumb);
        endPos = MapThumbToPosition(endThumb);
    }

    private int MapThumbToPosition(Thumb thumb){
        double position = Math.Floor(thumb.Margin.Left);
        return (int)(position / maxCanvasPosition * 1000);
    }

    public string StartDragDelta(DragDeltaEventArgs e){
        double newLeft = Math.Max(0, startThumb.Margin.Left + e.HorizontalChange);
        newLeft = Math.Min(newLeft, endThumb.Margin.Left);
        startThumb.Margin = new Thickness(newLeft,0,0,0);
        startPos = MapThumbToPosition(startThumb);
        SnapCurrThumbToBounds();
        UpdateTrimRange();
        return $"{newLeft} {startPos}";
    }

    public string CurrDragDelta(DragDeltaEventArgs e){
        double newLeft = Math.Min(currThumb.Margin.Left + e.HorizontalChange, maxCanvasPosition);
        newLeft = Math.Min(newLeft, endThumb.Margin.Left);
        newLeft = Math.Max(startThumb.Margin.Left, newLeft);
        currThumb.Margin = new Thickness(newLeft,0,0,0);
        currPos = MapThumbToPosition(currThumb);
        return $"{newLeft} {currPos}";
    }

    public string EndDragDelta(DragDeltaEventArgs e){
        double newLeft = Math.Min(endThumb.Margin.Left + e.HorizontalChange, maxCanvasPosition);
        newLeft = Math.Max(startThumb.Margin.Left, newLeft);
        endThumb.Margin = new Thickness(newLeft,0,0,0);
        endPos = MapThumbToPosition(endThumb);
        SnapCurrThumbToBounds();
        UpdateTrimRange();
        return $"{newLeft} {endPos}";
    }

    public void SnapCurrThumbToBounds(){
        double currThumbPos = currThumb.Margin.Left;
        double newLeft = Math.Min(currThumbPos, endThumb.Margin.Left);
        newLeft = Math.Max(startThumb.Margin.Left, newLeft);
        currThumb.Margin = new Thickness(newLeft,0,0,0);
        currPos = MapThumbToPosition(currThumb);
        // Call to media player to update both time and start time
    }

    private void UpdateTrimRange(){
        trimRange.Margin = new Thickness(startThumb.Margin.Left, 0, 0, 0);
        trimRange.Width = endThumb.Margin.Left - startThumb.Margin.Left;
    }
}