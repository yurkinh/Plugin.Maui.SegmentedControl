namespace Plugin.Maui.SegmentedControl;

public interface ISegmentedControl: IView
{
    public Color TintColor { get; set; }
    public Color DisabledColor { get; set; }
    public Color SelectedTextColor { get; set; }
    public int SelectedSegment { get; set; }
}

