// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Extracted from https://github.com/CommunityToolkit/Windows (MIT License)
#if WINDOWS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Plugin.Maui.SegmentedControl.Windows;

public partial class Segmented
{
    /// <summary>
    /// The backing <see cref="DependencyProperty"/> for the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
        nameof(Orientation),
        typeof(Microsoft.UI.Xaml.Controls.Orientation),
        typeof(Segmented),
        new PropertyMetadata(Microsoft.UI.Xaml.Controls.Orientation.Horizontal, OnOrientationChanged));

    static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((Segmented)d).OnOrientationPropertyChanged(d, OrientationProperty);
    }

    /// <summary>
    /// Gets or sets the orientation.
    /// </summary>
    public Microsoft.UI.Xaml.Controls.Orientation Orientation
    {
        get => (Microsoft.UI.Xaml.Controls.Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
}
#endif
