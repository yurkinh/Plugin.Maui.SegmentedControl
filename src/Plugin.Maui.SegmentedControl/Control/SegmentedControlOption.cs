using System.ComponentModel;
using System.Diagnostics;

namespace Plugin.Maui.SegmentedControl;

public class SegmentedControlOption : View, ISegmentedControlOption
{
    SegmentedControl _parent;

    int _instanceId;

    public SegmentedControlOption()
    {
        _instanceId = Random.Shared.Next();
        Debug.WriteLine($"Created SegmentedControlOption {_instanceId}");
    }

    public void SetParent(SegmentedControl parent)
    {
        _parent = parent;
    }


    public static readonly BindableProperty TextProperty
    = BindableProperty.Create(nameof(Text), typeof(string), 
                typeof(SegmentedControlOption), string.Empty);
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    protected override void OnPropertyChanged(string propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(Text)
            || propertyName == nameof(IsEnabled))
        {
            Debug.WriteLine($"OnPropertyChanged SegmentedControlOption {_instanceId} Property {propertyName}");
            _parent?.NotifySegmentChanged(this);
        }
    }

}

