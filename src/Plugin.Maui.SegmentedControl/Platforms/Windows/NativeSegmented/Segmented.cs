// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Extracted from https://github.com/CommunityToolkit/Windows (MIT License)
#if WINDOWS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace Plugin.Maui.SegmentedControl.Windows;

/// <summary>
/// A control that displays a set of items that can be selected by the user.
/// </summary>
public partial class Segmented : ListViewBase
{
    int _internalSelectedIndex = -1;
    bool _hasLoaded = false;

    /// <summary>
    /// Creates a new instance of <see cref="Segmented"/>.
    /// </summary>
    public Segmented()
    {
        this.DefaultStyleKey = typeof(Segmented);

        RegisterPropertyChangedCallback(SelectedIndexProperty, OnSelectedIndexChanged);
        RegisterPropertyChangedCallback(OrientationProperty, OnOrientationPropertyChanged);
    }

    /// <inheritdoc/>
    protected override DependencyObject GetContainerForItemOverride() => new SegmentedItem();

    /// <inheritdoc/>
    protected override bool IsItemItsOwnContainerOverride(object item)
        => item is SegmentedItem;

    /// <inheritdoc/>
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (!_hasLoaded)
        {
            SelectedIndex = -1;
            SelectedIndex = _internalSelectedIndex;
            _hasLoaded = true;
        }

        PreviewKeyDown -= Segmented_PreviewKeyDown;
        PreviewKeyDown += Segmented_PreviewKeyDown;
    }

    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is SegmentedItem segmentedItem)
        {
            segmentedItem.UpdateOrientation(Orientation);
        }
    }

    void Segmented_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
    {
        var dir = e.Key switch
        {
            VirtualKey.Left or VirtualKey.Up => -1,
            VirtualKey.Right or VirtualKey.Down => 1,
            _ => 0,
        };

        if (dir is not 0)
        {
            e.Handled = MoveFocus(dir);
        }
    }

    bool MoveFocus(int adjustment)
    {
        var currentContainerItem = GetCurrentContainerItem();
        if (currentContainerItem is null)
        {
            return false;
        }

        var currentItem = ItemFromContainer(currentContainerItem);
        var previousIndex = Items.IndexOf(currentItem);
        var index = Math.Clamp(previousIndex + adjustment, 0, Items.Count);

        if (index == previousIndex || ContainerFromIndex(index) is not SegmentedItem newItem)
        {
            return false;
        }

        newItem.Focus(FocusState.Keyboard);
        return true;
    }

    SegmentedItem? GetCurrentContainerItem()
    {
        if (XamlRoot is not null)
        {
            return FocusManager.GetFocusedElement(XamlRoot) as SegmentedItem;
        }

        return FocusManager.GetFocusedElement() as SegmentedItem;
    }

    void OnSelectedIndexChanged(DependencyObject sender, DependencyProperty dp)
    {
        // Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/8257
        if (_internalSelectedIndex == -1 && SelectedIndex > -1)
        {
            _internalSelectedIndex = SelectedIndex;
        }
    }

    void OnOrientationPropertyChanged(DependencyObject sender, DependencyProperty dp)
    {
        for (int i = 0; i < Items.Count; i++)
        {
            if (ContainerFromIndex(i) is SegmentedItem item)
            {
                item.UpdateOrientation(Orientation);
            }
        }
    }
}
#endif
