using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

public partial class RangeSlider
{
    private Thumb startThumb;
    private Thumb currThumb;
    private Thumb endThumb;
    private int startPos;
    private int currPos;
    private int endPos;
    private int maxCanvasPosition;
    public RangeSlider(Thumb startThumb, Thumb currThumb, Thumb endThumb, int maxCanvasPosition){
        this.startThumb = startThumb;
        this.currThumb = currThumb;
        this.endThumb = endThumb;
        this.maxCanvasPosition = maxCanvasPosition;     // resizing the window will cause issues

        startThumb.Margin = new Thickness(0,0,0,0);
        currThumb.Margin = new Thickness(0,0,0,0);
        endThumb.Margin = new Thickness(maxCanvasPosition,0,0,0);

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
}