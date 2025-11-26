namespace Plugin.Maui.SegmentedControl.Control
{
    public class ElementChildrenChanging(IList<SegmentedControlOption> oldValues, IList<SegmentedControlOption> newValues) : EventArgs
    {
        public IList<SegmentedControlOption> OldValues { get; } = oldValues;
        public IList<SegmentedControlOption> NewValues { get; } = newValues;
    }
}
