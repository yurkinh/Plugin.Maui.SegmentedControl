using Plugin.Maui.SegmentedControl.Control;

namespace Plugin.Maui.SegmentedControl;

public interface ISegmentedControl: IView
{
    /// <summary>
    /// Tint color of selected segment
    /// </summary>
    Color TintColor { get; set; }
    
    /// <summary>
    /// Text color of unselected segment
    /// </summary>
    Color TextColor { get; set; }
    
    /// <summary>
    /// Text color of selected segment
    /// </summary>
    Color SelectedTextColor { get; set; }

    /// <summary>
    /// Background color of disabled unselected segment
    /// </summary>
    Color DisabledBackgroundColor { get; set; }

    /// <summary>
    /// Text color of disabled segment (selected or not)
    /// </summary>
    Color DisabledTextColor { get; set; }


    /// <summary>
    /// Tint color of disabled selected segment
    /// </summary>
    Color DisabledTintColor { get; set; }

    /// <summary>
    /// Font size for segment text
    /// </summary>
    double FontSize { get; set; }

    /// <summary>
    /// Padding for segment content
    /// </summary>
    Thickness Padding { get; set; }

    int SelectedSegment { get; set; }

    GroupToggleBehavior GroupToggleBehavior { get; set; }
}

public interface ISegmentedControlOption : IView
{
    string Text { get; set; }
}

