using Plugin.Maui.SegmentedControl.Control;

namespace Plugin.Maui.SegmentedControl;

public interface ISegmentedControl: IView
{
    /// <summary>
    /// Tint color of selected segment
    /// </summary>
    public Color TintColor { get; set; }
    
    /// <summary>
    /// Text color of unselected segment
    /// </summary>
    public Color TextColor { get; set; }
    
    /// <summary>
    /// Text color of selected segment
    /// </summary>
    public Color SelectedTextColor { get; set; }

    /// <summary>
    /// Background color of disabled unselected segment
    /// </summary>
    public Color DisabledBackgroundColor { get; set; }

    /// <summary>
    /// Text color of disabled segment (selected or not)
    /// </summary>
    public Color DisabledTextColor { get; set; }


    /// <summary>
    /// Tint color of disabled selected segment
    /// </summary>
    public Color DisabledTintColor { get; set; }

    /// <summary>
    /// Font size for segment text
    /// </summary>
    public double FontSize { get; set; }

    /// <summary>
    /// Padding for segment content
    /// </summary>
    public Thickness Padding { get; set; }

    public int SelectedSegment { get; set; }

    public GroupToggleBehavior GroupToggleBehavior { get; set; }
}

public interface ISegmentedControlOption : IView
{
    public string Text { get; set; }
}

