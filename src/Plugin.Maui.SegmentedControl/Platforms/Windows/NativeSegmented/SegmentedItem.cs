// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Extracted from https://github.com/CommunityToolkit/Windows (MIT License)
#if WINDOWS
#nullable enable
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

    internal const string CommonStatesGroup = "CommonStates";
    internal const string NormalState = "Normal";
    internal const string SelectedState = "Selected";
    internal const string DisabledState = "Disabled";
    internal const string DisabledSelectedState = "DisabledSelected";

    static readonly DisabledAwareStateManager StateManager = new();

    bool isVertical = false;

    /// <summary>
    /// Creates a new instance of <see cref="SegmentedItem"/>.
    /// </summary>
    public SegmentedItem()
    {
        this.DefaultStyleKey = typeof(SegmentedItem);
        RegisterPropertyChangedCallback(VisibilityProperty, OnVisibilityChanged);
        IsEnabledChanged += (_, _) => RefreshCommonState();
        RegisterPropertyChangedCallback(IsSelectedProperty, OnIsSelectedChanged);
    }

    /// <inheritdoc/>
    protected override Microsoft.UI.Xaml.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        => new SegmentedItemAutomationPeer(this);

    void OnIsSelectedChanged(DependencyObject sender, DependencyProperty dp)
    {
        if (Microsoft.UI.Xaml.Automation.Peers.FrameworkElementAutomationPeer.FromElement(this) is SegmentedItemAutomationPeer peer)
        {
            peer.RaiseIsSelectedChanged(IsSelected);
        }
    }

    void RefreshCommonState()
    {
        // Disabled items are redirected to Disabled/DisabledSelected by DisabledAwareStateManager.
        Microsoft.UI.Xaml.VisualStateManager.GoToState(this, IsSelected ? SelectedState : NormalState, false);
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

        if (GetTemplateChild("PART_Root") is FrameworkElement root)
        {
            Microsoft.UI.Xaml.VisualStateManager.SetCustomVisualStateManager(root, StateManager);
        }

        UpdateVisualStates();
        RefreshCommonState();
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
        isVertical = orientation is Microsoft.UI.Xaml.Controls.Orientation.Vertical;
        UpdateVisualStates();
    }

    void UpdateVisualStates()
    {
        string contentState = (Icon is null, Content is null) switch
        {
            (false, false) => isVertical ? IconTopState : IconLeftState,
            (false, true) => IconOnlyState,
            (true, false) => ContentOnlyState,
            _ => ContentOnlyState,
        };

        Microsoft.UI.Xaml.VisualStateManager.GoToState(this, contentState, true);
        Microsoft.UI.Xaml.VisualStateManager.GoToState(this, isVertical ? VerticalState : HorizontalState, true);
    }

    /// <summary>
    /// ListViewItem's own peer gets its selection state and position from the ListView data peers, which a
    /// Segmented doesn't have, so screen readers couldn't tell which segment is selected or "2 of 3".
    /// </summary>
    sealed class SegmentedItemAutomationPeer(SegmentedItem owner)
        : Microsoft.UI.Xaml.Automation.Peers.FrameworkElementAutomationPeer(owner),
          Microsoft.UI.Xaml.Automation.Provider.ISelectionItemProvider
    {
        SegmentedItem Item => (SegmentedItem)Owner;

        Segmented? ParentSegmented => ItemsControl.ItemsControlFromItemContainer(Item) as Segmented;

        protected override Microsoft.UI.Xaml.Automation.Peers.AutomationControlType GetAutomationControlTypeCore()
            => Microsoft.UI.Xaml.Automation.Peers.AutomationControlType.ListItem;

        protected override string GetClassNameCore() => nameof(SegmentedItem);

        protected override string GetNameCore()
        {
            var name = base.GetNameCore();
            return string.IsNullOrEmpty(name) ? Item.Content?.ToString() ?? string.Empty : name;
        }

        protected override object? GetPatternCore(Microsoft.UI.Xaml.Automation.Peers.PatternInterface patternInterface)
            => patternInterface == Microsoft.UI.Xaml.Automation.Peers.PatternInterface.SelectionItem ? this : base.GetPatternCore(patternInterface);

        protected override int GetPositionInSetCore()
            => ParentSegmented is { } parent ? parent.IndexFromContainer(Item) + 1 : -1;

        protected override int GetSizeOfSetCore()
            => ParentSegmented?.Items.Count ?? -1;

        public bool IsSelected => Item.IsSelected;

        public Microsoft.UI.Xaml.Automation.Provider.IRawElementProviderSimple? SelectionContainer
            => ParentSegmented is { } parent && FromElement(parent) is { } parentPeer ? ProviderFromPeer(parentPeer) : null;

        public void Select()
        {
            if (ParentSegmented is { } parent && Item.IsEnabled && parent.SelectionMode != Microsoft.UI.Xaml.Controls.ListViewSelectionMode.None)
            {
                parent.SelectedIndex = parent.IndexFromContainer(Item);
            }
        }

        public void AddToSelection() => Select();

        // Single selection: a segment is deselected by selecting another one.
        public void RemoveFromSelection()
        {
        }

        internal void RaiseIsSelectedChanged(bool isSelected)
        {
            RaisePropertyChangedEvent(
                Microsoft.UI.Xaml.Automation.SelectionItemPatternIdentifiers.IsSelectedProperty, !isSelected, isSelected);

            if (isSelected)
            {
                RaiseAutomationEvent(Microsoft.UI.Xaml.Automation.Peers.AutomationEvents.SelectionItemPatternOnElementSelected);
            }
        }
    }

    /// <summary>
    /// ListViewItem keeps driving CommonStates (Normal, Selected, PointerOver, ...) when it is disabled, and has no
    /// state for "disabled and selected". This manager sends a disabled item to Disabled or DisabledSelected instead,
    /// so DisabledBackgroundColor / DisabledTintColor / DisabledTextColor apply like on the other platforms.
    /// </summary>
    sealed class DisabledAwareStateManager : Microsoft.UI.Xaml.VisualStateManager
    {
        protected override bool GoToStateCore(Microsoft.UI.Xaml.Controls.Control control, FrameworkElement templateRoot, string stateName,
            Microsoft.UI.Xaml.VisualStateGroup group, Microsoft.UI.Xaml.VisualState state, bool useTransitions)
        {
            if (control is SegmentedItem { IsEnabled: false } item && group?.Name == CommonStatesGroup)
            {
                var target = item.IsSelected ? DisabledSelectedState : DisabledState;
                if (stateName != target)
                {
                    foreach (var candidate in group.States)
                    {
                        if (candidate.Name == target)
                        {
                            return base.GoToStateCore(control, templateRoot, target, group, candidate, useTransitions);
                        }
                    }
                }
            }

            return base.GoToStateCore(control, templateRoot, stateName, group, state, useTransitions);
        }
    }
}
#endif
