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
    double maxItemWidth = 0;
    double maxItemHeight = 0;
    int visibleItemsCount = 0;

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
        maxItemWidth = 0;
        maxItemHeight = 0;

        var elements = Children.Where(e => e.Visibility == Microsoft.UI.Xaml.Visibility.Visible);
        visibleItemsCount = elements.Count();

        foreach (var child in elements)
        {
            child.Measure(availableSize);
            maxItemWidth = Math.Max(maxItemWidth, child.DesiredSize.Width);
            maxItemHeight = Math.Max(maxItemHeight, child.DesiredSize.Height);
        }

        if (visibleItemsCount <= 0)
        {
            return new WinSize(0, 0);
        }

        // Always report the natural size (widest item x count). The Community Toolkit version reported the whole
        // available size when stretched, which made MAUI's Start/Center/End layouts as wide as Fill.
        // Stretching still happens: ArrangeOverride widens the items to fill whatever size the panel is given.
        double maxU = Orientation is Microsoft.UI.Xaml.Controls.Orientation.Horizontal ? maxItemWidth : maxItemHeight;
        double maxV = Orientation is Microsoft.UI.Xaml.Controls.Orientation.Horizontal ? maxItemHeight : maxItemWidth;
        double uvU = (maxU * visibleItemsCount) + (Spacing * (visibleItemsCount - 1));
        double uvV = maxV;

        return Orientation is Microsoft.UI.Xaml.Controls.Orientation.Horizontal
            ? new WinSize(uvU, uvV)
            : new WinSize(uvV, uvU);
    }

    /// <inheritdoc/>
    protected override WinSize ArrangeOverride(WinSize finalSize)
    {
        // Use locals only: the Community Toolkit version wrote the stretched size back into maxItemWidth/Height,
        // so an arrange that WinUI ran without a new measure (cached) reused a stale, too-wide item size.
        bool horizontal = Orientation is Microsoft.UI.Xaml.Controls.Orientation.Horizontal;
        double finalSizeU = horizontal ? finalSize.Width : finalSize.Height;
        double itemU = horizontal ? maxItemWidth : maxItemHeight;
        double itemV = horizontal ? maxItemHeight : maxItemWidth;

        if (visibleItemsCount > 0 && finalSizeU > visibleItemsCount * itemU + (Spacing * (visibleItemsCount - 1)))
        {
            itemU = (finalSizeU - (Spacing * (visibleItemsCount - 1))) / visibleItemsCount;
        }

        double posU = 0;
        foreach (var child in Children.Where(e => e.Visibility == Microsoft.UI.Xaml.Visibility.Visible))
        {
            child.Arrange(horizontal
                ? new WinRect(posU, 0, itemU, itemV)
                : new WinRect(0, posU, itemV, itemU));

            posU += itemU + Spacing;
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
