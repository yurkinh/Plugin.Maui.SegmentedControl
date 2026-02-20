// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Extracted from https://github.com/CommunityToolkit/Windows (MIT License)
#if WINDOWS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Plugin.Maui.SegmentedControl.Windows;

/// <summary>
/// Represents an item in a <see cref="Segmented"/> control.
/// </summary>
[Microsoft.UI.Xaml.Markup.ContentProperty(Name = "Content")]
public partial class SegmentedItem : ListViewItem
{
    internal const string IconLeftState = "IconLeft";
    internal const string IconTopState = "IconTop";
    internal const string IconOnlyState = "IconOnly";
    internal const string ContentOnlyState = "ContentOnly";

    internal const string HorizontalState = "Horizontal";
    internal const string VerticalState = "Vertical";

    bool _isVertical = false;

    /// <summary>
    /// Creates a new instance of <see cref="SegmentedItem"/>.
    /// </summary>
    public SegmentedItem()
    {
        this.DefaultStyleKey = typeof(SegmentedItem);
        RegisterPropertyChangedCallback(VisibilityProperty, OnVisibilityChanged);
    }

    void OnVisibilityChanged(DependencyObject sender, DependencyProperty dp)
    {
        if ((this.Parent as Segmented)?.ItemsPanelRoot is Panel panel)
        {
            panel.InvalidateMeasure();
        }
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateVisualStates();
    }

    /// <inheritdoc/>
    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);
        UpdateVisualStates();
    }

    /// <summary>
    /// Handles changes to the Icon property.
    /// </summary>
    protected virtual void OnIconPropertyChanged(IconElement oldValue, IconElement newValue) => UpdateVisualStates();

    internal void UpdateOrientation(Microsoft.UI.Xaml.Controls.Orientation orientation)
    {
        _isVertical = orientation is Microsoft.UI.Xaml.Controls.Orientation.Vertical;
        UpdateVisualStates();
    }

    void UpdateVisualStates()
    {
        string contentState = (Icon is null, Content is null) switch
        {
            (false, false) => _isVertical ? IconTopState : IconLeftState,
            (false, true) => IconOnlyState,
            (true, false) => ContentOnlyState,
            _ => ContentOnlyState,
        };

        Microsoft.UI.Xaml.VisualStateManager.GoToState(this, contentState, true);
        Microsoft.UI.Xaml.VisualStateManager.GoToState(this, _isVertical ? VerticalState : HorizontalState, true);
    }
}
#endif
