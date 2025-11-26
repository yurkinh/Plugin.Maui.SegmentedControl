using System.Diagnostics;

namespace Plugin.Maui.SegmentedControl;

public class SegmentedControlOption : View, ISegmentedControlOption
{
    private SegmentedControl parent;

    private readonly int instanceId;

    public SegmentedControlOption()
    {
        instanceId = Random.Shared.Next();
        Trace.WriteLine($"Created SegmentedControlOption {instanceId}");
    }

    public void SetParent(SegmentedControl parent)
    {
        this.parent = parent;
    }


    public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(SegmentedControlOption), string.Empty);
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    protected override void OnPropertyChanged(string propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(Text) || propertyName == nameof(IsEnabled))
        {
            Trace.WriteLine($"OnPropertyChanged SegmentedControlOption {instanceId} Property {propertyName}");
            parent?.NotifySegmentChanged(this);
        }
    }

}

