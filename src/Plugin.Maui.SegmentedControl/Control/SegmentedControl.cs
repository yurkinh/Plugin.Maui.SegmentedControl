#nullable enable

using Plugin.Maui.SegmentedControl.Control;
using System.ComponentModel;

namespace Plugin.Maui.SegmentedControl;

public class SegmentedControl : View, IViewContainer<SegmentedControlOption>, ISegmentedControl
{


    public SegmentedControl()
    {
        Children = new List<SegmentedControlOption>();
    }


    #region Children

    public void NotifySegmentChanged(SegmentedControlOption segment)
    {
        //just redraw all
        OnPropertyChanged(nameof(Children));
    }

    public event EventHandler<ElementChildrenChanging>? OnElementChildrenChanging;

    public static readonly BindableProperty ChildrenProperty
        = BindableProperty.Create(
            nameof(Children),
            typeof(IList<SegmentedControlOption>),
            typeof(SegmentedControl),
            default(IList<SegmentedControlOption>),
            BindingMode.OneWay,

            propertyChanging: OnChildrenChanging);
    static void OnChildrenChanging(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SegmentedControl segmentedControl
            && newValue is IList<SegmentedControlOption> newItemsList
            && segmentedControl.Children != null)
        {
            segmentedControl.OnElementChildrenChanging?.Invoke(segmentedControl, new ElementChildrenChanging((IList<SegmentedControlOption>)oldValue, newItemsList));

            foreach (var newSegment in newItemsList)
            {
                newSegment.BindingContext = segmentedControl.BindingContext;
                //works for programmatically setting Children
                newSegment.SetParent(segmentedControl);
            }
            segmentedControl.Children = newItemsList;
        }

    }

    public IList<SegmentedControlOption> Children
    {
        get => (IList<SegmentedControlOption>)GetValue(ChildrenProperty);
        set => SetValue(ChildrenProperty, value);
    }


    protected override void OnChildAdded(Element child)
    {
        base.OnChildAdded(child);

        if (child is SegmentedControlOption ctr)
        {
            ctr.SetParent(this);
        }
    }

    #endregion



    public static readonly BindableProperty TintColorProperty = BindableProperty.Create(nameof(TintColor), typeof(Color), typeof(SegmentedControl), Colors.Blue);
    public Color TintColor
    {
        get { return (Color)GetValue(TintColorProperty); }
        set { SetValue(TintColorProperty, value); }
    }


    public static readonly BindableProperty DisabledBackgroundColorProperty = BindableProperty.Create(nameof(DisabledBackgroundColor), typeof(Color), typeof(SegmentedControl), Colors.Gray);
    public Color DisabledBackgroundColor
    {
        get { return (Color)GetValue(DisabledBackgroundColorProperty); }
        set { SetValue(DisabledBackgroundColorProperty, value); }
    }

    public static readonly BindableProperty DisabledTintColorProperty = BindableProperty.Create(nameof(DisabledTintColor), typeof(Color), typeof(SegmentedControl), Colors.Gray);
    public Color DisabledTintColor
    {
        get { return (Color)GetValue(DisabledTintColorProperty); }
        set { SetValue(DisabledTintColorProperty, value); }
    }

    public static readonly BindableProperty DisabledTextColorProperty = BindableProperty.Create(nameof(DisabledTextColor), typeof(Color), typeof(SegmentedControl), Colors.Gray);
    public Color DisabledTextColor
    {
        get { return (Color)GetValue(DisabledTextColorProperty); }
        set { SetValue(DisabledTextColorProperty, value); }
    }

    public static readonly BindableProperty SelectedTextColorProperty = BindableProperty.Create(nameof(SelectedTextColor), typeof(Color), typeof(SegmentedControl), Colors.Black);
    public Color SelectedTextColor
    {
        get { return (Color)GetValue(SelectedTextColorProperty); }
        set { SetValue(SelectedTextColorProperty, value); }
    }

    public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(SegmentedControl), Colors.Black);
    public Color TextColor
    {
        get { return (Color)GetValue(TextColorProperty); }
        set { SetValue(TextColorProperty, value); }
    }

    public static readonly BindableProperty SelectedSegmentProperty = BindableProperty.Create(nameof(SelectedSegment), typeof(int), typeof(SegmentedControl), -1);

    public int SelectedSegment
    {
        get
        {
            return (int)GetValue(SelectedSegmentProperty);
        }
        set
        {
            SetValue(SelectedSegmentProperty, value);
        }
    }

    public static readonly BindableProperty GroupToggleBehaviorProperty
        = BindableProperty.Create(
            nameof(GroupToggleBehavior),
            typeof(GroupToggleBehavior),
            typeof(SegmentedControl),
            GroupToggleBehavior.Radio);

    public GroupToggleBehavior GroupToggleBehavior
    {
        get
        {
            return (GroupToggleBehavior)GetValue(GroupToggleBehaviorProperty);
        }
        set
        {
            SetValue(GroupToggleBehaviorProperty, value);
        }
    }

    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(double), typeof(SegmentedControl), 14.0);
    public double FontSize
    {
        get { return (double)GetValue(FontSizeProperty); }
        set { SetValue(FontSizeProperty, value); }
    }

    public static readonly BindableProperty PaddingProperty = BindableProperty.Create(nameof(Padding), typeof(Thickness), typeof(SegmentedControl), new Thickness(0));
    public Thickness Padding
    {
        get { return (Thickness)GetValue(PaddingProperty); }
        set { SetValue(PaddingProperty, value); }
    }



    public event EventHandler<ValueChangedEventArgs>? ValueChanged;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SendValueChanged()
    {
        ValueChanged?.Invoke(this, new ValueChangedEventArgs { NewValue = this.SelectedSegment });
    }

    public event EventHandler<SegmentTappedEventArgs>? SegmentTapped;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SendSegmentTapped(int segment)
    {
        if (IsEnabled
            && segment >= 0
            && segment < Children.Count
            && Children[segment].IsEnabled)
        {
            SegmentTapped?.Invoke(this, new SegmentTappedEventArgs { Index = segment });
        }
    }


    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (Children is not null)
        {
            foreach (var segment in Children)
            {
                segment.BindingContext = BindingContext;
                segment.SetParent(this);
            }
        }
    }
}
