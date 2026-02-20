// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Extracted from https://github.com/CommunityToolkit/Windows (MIT License)
#if WINDOWS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinSize = Windows.Foundation.Size;
using WinRect = Windows.Foundation.Rect;

namespace Plugin.Maui.SegmentedControl.Windows;

/// <summary>
/// A panel that arranges its children in equal columns.
/// </summary>
public partial class EqualPanel : Panel
{
    double _maxItemWidth = 0;
    double _maxItemHeight = 0;
    int _visibleItemsCount = 0;

    /// <summary>
    /// Identifies the Spacing dependency property.
    /// </summary>
    public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(
        nameof(Spacing),
        typeof(double),
        typeof(EqualPanel),
        new PropertyMetadata(default(double), OnEqualPanelPropertyChanged));

    /// <summary>
    /// Backing <see cref="DependencyProperty"/> for the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
        nameof(Orientation),
        typeof(Microsoft.UI.Xaml.Controls.Orientation),
        typeof(EqualPanel),
        new PropertyMetadata(default(Microsoft.UI.Xaml.Controls.Orientation), OnEqualPanelPropertyChanged));

    /// <summary>
    /// Gets or sets the spacing between items.
    /// </summary>
    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the panel orientation.
    /// </summary>
    public Microsoft.UI.Xaml.Controls.Orientation Orientation
    {
        get => (Microsoft.UI.Xaml.Controls.Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="EqualPanel"/> class.
    /// </summary>
    public EqualPanel()
    {
        RegisterPropertyChangedCallback(HorizontalAlignmentProperty, OnAlignmentChanged);
    }

    /// <inheritdoc/>
    protected override WinSize MeasureOverride(WinSize availableSize)
    {
        _maxItemWidth = 0;
        _maxItemHeight = 0;

        var elements = Children.Where(e => e.Visibility == Microsoft.UI.Xaml.Visibility.Visible);
        _visibleItemsCount = elements.Count();

        foreach (var child in elements)
        {
            child.Measure(availableSize);
            _maxItemWidth = Math.Max(_maxItemWidth, child.DesiredSize.Width);
            _maxItemHeight = Math.Max(_maxItemHeight, child.DesiredSize.Height);
        }

        if (_visibleItemsCount <= 0)
        {
            return new WinSize(0, 0);
        }

        bool stretch = Orientation switch
        {
            Microsoft.UI.Xaml.Controls.Orientation.Horizontal => HorizontalAlignment is Microsoft.UI.Xaml.HorizontalAlignment.Stretch && !double.IsInfinity(availableSize.Width),
            _ => VerticalAlignment is Microsoft.UI.Xaml.VerticalAlignment.Stretch && !double.IsInfinity(availableSize.Height),
        };

        double uvU, uvV;
        double maxU = Orientation is Microsoft.UI.Xaml.Controls.Orientation.Horizontal ? _maxItemWidth : _maxItemHeight;
        double maxV = Orientation is Microsoft.UI.Xaml.Controls.Orientation.Horizontal ? _maxItemHeight : _maxItemWidth;
        double availableU = Orientation is Microsoft.UI.Xaml.Controls.Orientation.Horizontal ? availableSize.Width : availableSize.Height;

        if (stretch)
        {
            double totalU = availableU - (Spacing * (_visibleItemsCount - 1));
            maxU = totalU / _visibleItemsCount;
            uvU = availableU;
            uvV = maxV;
        }
        else
        {
            uvU = (maxU * _visibleItemsCount) + (Spacing * (_visibleItemsCount - 1));
            uvV = maxV;
        }

        return Orientation is Microsoft.UI.Xaml.Controls.Orientation.Horizontal
            ? new WinSize(uvU, uvV)
            : new WinSize(uvV, uvU);
    }

    /// <inheritdoc/>
    protected override WinSize ArrangeOverride(WinSize finalSize)
    {
        double posU = 0;
        ref double maxItemU = ref _maxItemWidth;
        double finalSizeU = finalSize.Width;

        if (Orientation is Microsoft.UI.Xaml.Controls.Orientation.Vertical)
        {
            maxItemU = ref _maxItemHeight;
            finalSizeU = finalSize.Height;
        }

        if (finalSizeU > _visibleItemsCount * maxItemU + (Spacing * (_visibleItemsCount - 1)))
        {
            maxItemU = (finalSizeU - (Spacing * (_visibleItemsCount - 1))) / _visibleItemsCount;
        }

        foreach (var child in Children.Where(e => e.Visibility == Microsoft.UI.Xaml.Visibility.Visible))
        {
            if (Orientation is Microsoft.UI.Xaml.Controls.Orientation.Horizontal)
            {
                child.Arrange(new WinRect(posU, 0, _maxItemWidth, _maxItemHeight));
            }
            else
            {
                child.Arrange(new WinRect(0, posU, _maxItemWidth, _maxItemHeight));
            }

            posU += maxItemU + Spacing;
        }

        return finalSize;
    }

    void OnAlignmentChanged(DependencyObject sender, DependencyProperty dp)
    {
        InvalidateMeasure();
    }

    static void OnEqualPanelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var panel = (EqualPanel)d;
        panel.InvalidateMeasure();
    }
}
#endif
