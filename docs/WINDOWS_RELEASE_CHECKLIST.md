# Windows support: implementation plan and release checklist

Status of branch `copilot/analyze-segmented-control-effort` (PR #23).
This branch adds a WinUI 3 implementation of `SegmentedControl`. It uses a copy of the
Community Toolkit `Segmented` control, adapted for this library. **It is not ready to publish yet.**
Work through the phases below in order on a Windows machine.

## What's on the branch

| File | Purpose |
| --- | --- |
| `src/Plugin.Maui.SegmentedControl/Platforms/Windows/SegmentedControlHandler.cs` | MAUI handler that maps `SegmentedControl` onto the native `Segmented` control |
| `.../Platforms/Windows/NativeSegmented/Segmented*.cs`, `SegmentedItem*.cs`, `EqualPanel.cs` | Copy of the CommunityToolkit/Windows `Segmented` control (MIT) |
| `.../Platforms/Windows/Themes/SegmentedStyles.xaml` | Styles, control templates, and the theme resource keys the handler overrides |
| `.../Platforms/Windows/Themes/Generic.xaml` | Merges `SegmentedStyles.xaml` (not used at the moment, see phase 4) |
| `.../Extensions/AppHostBuilderExtensions.cs` | `UseSegmentedControl()` merges `SegmentedStyles.xaml` into `Application.Current.Resources` on Windows |
| `Plugin.Maui.SegmentedControl.csproj` | Adds `net10.0-windows10.0.19041.0` |

How the handler maps the control's properties:

| `SegmentedControl` property | Windows behavior |
| --- | --- |
| `Children` | Rebuilds `SegmentedItem`s with the option's `Text` and `IsEnabled` |
| `SelectedSegment` | Sets `Segmented.SelectedIndex`, raises `ValueChanged` |
| `GroupToggleBehavior` | `Radio` → `SelectionMode.Single`; `None` → `SelectionMode.None` (only read when the control is created) |
| `TintColor` | Background of the selected item; border while the control is enabled |
| `SelectedTextColor` / `TextColor` | Text color of the selected item / of unselected items (normal, pointer-over, pressed) |
| `DisabledBackgroundColor` / `DisabledTextColor` | Background / text color of disabled items |
| `DisabledTintColor` | Border color while the control is disabled |
| `FontSize`, `Padding` | Applied to each item (a `Padding` of 0 falls back to 11) |
| `SegmentTapped` | Raised from the `Tapped` event on `Segmented` |

## Done: PR review findings and build fixes

All review threads on PR #23 are addressed. The last round also fixed the Windows build, which was broken:

- [x] The XAML compiler failed silently (`XamlCompiler.exe exited with code 1`). The MAUI SDK already
  includes `Platforms\Windows\**\*.xaml` as WinUI `Page` items, and the explicit `<Page Include>` items
  in the csproj passed each file a second time. Removed the explicit items.
- [x] `MapTextColor` lost its closing brace in `de508c4`. Restored it, and `ApplyColors` now also sets
  `ButtonItemForegroundPressed` (review: *Apply configured pressed foreground resource*).
- [x] `FlowDirection is FlowDirection.RightToLeft` in `Segmented.cs` didn't compile (CS0176: the name
  resolved to the instance property). Qualified as `Microsoft.UI.Xaml.FlowDirection.RightToLeft`.
- [x] Renamed the handler's brush fields to unprefixed camelCase (review: *Rename underscore-prefixed brush fields*).
- [x] Added `#nullable enable` to the Windows files that use nullable annotations (CS8632).
- [x] Earlier rounds: RTL key mapping, focus result returned from `MoveFocus`, index clamped in `MoveFocus`,
  high-contrast resources switched to `*Brush`, disabled colors applied in `CommonStates/Disabled`,
  missing `IconTop`/`Horizontal`/`Vertical` states, field renames in `EqualPanel`/`Segmented`/`SegmentedItem`.

Verified on Windows: the library builds for `net10.0-windows10.0.19041.0`, `net10.0`, `net10.0-android`
with 0 warnings and 0 errors, and `samples/SegmentedControlSamples` builds for Windows. The output contains
`SegmentedStyles.xbf` and `Plugin.Maui.SegmentedControl.pri`. **Not yet run or tested in an app.**

## Phase 1: Build and packaging blockers

### 1.1 The library doesn't build on macOS or Linux

The WinUI XAML compiler (`XamlCompiler.exe`) only runs on Windows. With the Windows TFM listed
unconditionally, `dotnet build` and `dotnet pack` of the library fail on a Mac (exit code 126).
`EnableWindowsTargeting` doesn't help for projects that contain XAML pages.

Pick one:

- **Quick fix:** add the Windows TFM only on Windows (the sample projects already do this) and
  remove `EnableWindowsTargeting`:

  ```xml
  <TargetFrameworks>net10.0;net10.0-android;net10.0-ios;net10.0-maccatalyst</TargetFrameworks>
  <TargetFrameworks Condition="$([MSBuild]::IsOSPlatform('windows'))">$(TargetFrameworks);net10.0-windows10.0.19041.0</TargetFrameworks>
  ```

  The release workflow runs on `windows-latest`, so the published package still includes Windows.
  The catch: you have to pack on Windows, or the package ships without Windows support.

- **Better fix (recommended):** stop compiling XAML. Make `SegmentedStyles.xaml` an `EmbeddedResource`
  (and exclude it from `Page`), then load it at runtime with `Microsoft.UI.Xaml.Markup.XamlReader.Load(...)`
  in `UseSegmentedControl()`. This needs no XAML compiler, builds anywhere, and also removes the
  `ms-appx`/`.pri` dependency in 1.2.

### 1.2 Check the NuGet packaging

The styles load from `ms-appx:///Plugin.Maui.SegmentedControl/Platforms/Windows/Themes/SegmentedStyles.xaml`.
That works with a `ProjectReference`. When the library comes from a NuGet package, the `.xbf` and `.pri`
files must be inside the `.nupkg`, or the app fails when it merges the dictionary. (Skip this if you picked
the embedded-resource fix above.) Check (PowerShell):

```powershell
dotnet pack src\Plugin.Maui.SegmentedControl\Plugin.Maui.SegmentedControl.csproj -c Release -o .\artifacts
tar -tf .\artifacts\Plugin.Maui.SegmentedControl.<version>.nupkg | findstr /i "windows"
```

Expect files under `lib/net10.0-windows10.0.19041.0/`: the `.dll`, a `.pri`, and the
`SegmentedStyles.xbf`. If they're missing, try `<GenerateLibraryLayout>true</GenerateLibraryLayout>`
for the Windows TFM.

Then test the package for real: in a sample, temporarily replace the `ProjectReference` with a
`PackageReference` to the local `.\artifacts` feed, run it, and confirm the control shows up with its styles.

### 1.3 Version and release metadata

- `PackageVersion` is still `2.0.0`, and tag `2.0.0` already exists. The release workflow pushes with
  `--skip-duplicate`, so publishing would silently do nothing. Bump it (suggested: `2.1.0`).
- Update `PackageReleaseNotes` (currently "Updated to .net 10").
- Add Windows to the platform table in `README.md`:
  `|Windows|Yes|10.0.19041+|WinUI 3 Segmented (Community Toolkit port)|`

## Phase 2: Functional gaps (implement, then verify on Windows)

### 2.1 Out-of-range `SelectedSegment` throws

`CreatePlatformView` and `MapSelectedSegment` set `SelectedIndex` without a range check. If a binding
sets `SelectedSegment` before `Children` is filled, WinUI throws. Guard it the way Android does:

```csharp
var index = control.SelectedSegment;
if (index < -1 || index >= handler.PlatformView.Items.Count)
    return;
```

`MapSelectedSegment` also raises `ValueChanged` even when nothing changed on the native side. Only raise it
when the value actually changes, to match iOS/Android.

### 2.2 `SegmentTapped` may never fire

`ListViewItem` usually marks `Tapped` as handled, so a plain `+=` subscription may never get called.
Register for handled events too:

```csharp
Microsoft.UI.Xaml.Input.TappedEventHandler? tappedHandler;

protected override void ConnectHandler(Segmented platformView)
{
    base.ConnectHandler(platformView);
    platformView.SelectionChanged += OnSelectionChanged;
    tappedHandler = OnTapped;
    platformView.AddHandler(Microsoft.UI.Xaml.UIElement.TappedEvent, tappedHandler, handledEventsToo: true);
}

protected override void DisconnectHandler(Segmented platformView)
{
    platformView.SelectionChanged -= OnSelectionChanged;
    if (tappedHandler is not null)
        platformView.RemoveHandler(Microsoft.UI.Xaml.UIElement.TappedEvent, tappedHandler);
    base.DisconnectHandler(platformView);
}
```

Keyboard selection (Space/Enter) doesn't raise `SegmentTapped`. Decide whether it should (iOS raises it
for every selection, so probably yes).

### 2.3 Changing colors at runtime probably doesn't update the control

The `Map*Color` methods replace entries in `segmented.Resources`. WinUI resolves `{ThemeResource}` values
when the template is applied and only refreshes them when the theme changes. So the starting colors work,
but changing them after the control is shown probably won't. Two values likely ignore the overrides
altogether, because they come from Style setters, which look up resources where the style is defined:

- an unselected item's `Foreground` (`ButtonItemForeground`)
- the control's `BorderBrush` (`SegmentedBorderBrush`)

Test first. If it's broken, set `item.Foreground` and `segmented.BorderBrush` directly, and for the
state colors either rebuild the items (`BuildItems`) after a color change or re-run
`VisualStateManager.GoToState` on each item. A cheaper alternative: keep one `SolidColorBrush` instance per
key and change its `.Color` instead of replacing the brush, since bound brushes update live.

### 2.4 `DisabledTintColor` behaves differently from iOS

On iOS, `DisabledTintColor` is the selected segment's color when the control is disabled (the
`ISegmentedControl` doc says the same). On Windows it only colors the border, and a disabled selected
item shows `DisabledBackgroundColor`. Also, the `Disabled` visual state applies 0.55 opacity on top of
the custom disabled colors, so they look faded. Add a `DisabledSelected` treatment (selected + disabled →
`DisabledTintColor`) and drop the extra opacity when custom colors are set, or document the difference.

### 2.5 Children updates rebuild everything and lose focus

`SegmentedControlOption.Text`/`IsEnabled` changes call `NotifySegmentChanged`, which re-maps `Children`,
which clears and recreates every `SegmentedItem`. That works, but it drops keyboard focus and may flash.
Update the existing items in place when the count didn't change, and rebuild only when it did.

### 2.6 `GroupToggleBehavior` is read only once

`SelectionMode` is set in `CreatePlatformView` only. Add a mapper entry so changing `GroupToggleBehavior`
at runtime switches between `Single` and `None` (clear `SelectedIndex` when switching to `None`).

### 2.7 Sizing and layout parity

- The style sets `HorizontalAlignment="Left"`. The handler overrides it with `Stretch`, which ignores the
  MAUI `HorizontalOptions`. Let the MAUI layout decide (remove the override and check `Fill`/`Start`/`Center`).
- Check that `EqualPanel` gives equal-width segments when the control is narrower than its content
  (long texts), and that `HeightRequest` is respected.
- `CornerRadius`: iOS/Android show rounded corners by default. Confirm `ControlCornerRadius` looks right
  next to the other platforms.

### 2.8 Accessibility

- Set `AutomationProperties.Name` on each `SegmentedItem` from the option `Text`, so Narrator reads it.
- Pass the control's `SemanticProperties.Description` through to the `Segmented`
  (`AutomationProperties.Name`), the way the base `ViewHandler` mapper does for other controls.
- Check Narrator reads "selected"/"not selected" and the position ("2 of 3").

## Phase 3: Manual test plan (Windows)

Run `samples/SegmentedControlSamples` (or `samples/Test.SegmentedControl`) on Windows. Use F5 in
Visual Studio with the `net10.0-windows10.0.19041.0` target, or:

```powershell
dotnet build samples\SegmentedControlSamples\SegmentedControlSamples.csproj -f net10.0-windows10.0.19041.0 -t:Run
```

- [ ] The control renders with equal-width segments and a border, in light, dark and high-contrast themes
- [ ] Switching the Windows theme while the app runs keeps custom colors
- [ ] The starting `SelectedSegment` is shown; clicking a segment updates `SelectedSegment` and raises `ValueChanged` once
- [ ] `SegmentTapped` fires on click, and not for disabled segments
- [ ] Setting `SelectedSegment` from code or a binding updates the UI; `-1` clears the selection; an out-of-range value doesn't crash
- [ ] `GroupToggleBehavior="None"` allows no selection but still raises `SegmentTapped`
- [ ] All colors apply at startup **and** when changed at runtime (including pressed / pointer-over)
- [ ] Disabling the control or a single option (`SegmentedControlOption.IsEnabled`) looks right and blocks input
- [ ] Changing an option's `Text` at runtime, or replacing `Children`, updates the segments and keeps the selection
- [ ] `FontSize` and `Padding` apply
- [ ] Keyboard: Tab into the control, arrow keys move focus (reversed in `FlowDirection="RightToLeft"`), Space/Enter selects, arrows skip or stop at disabled items without trapping focus
- [ ] Narrator reads each segment's text and selection state
- [ ] Navigate away and back (handler disconnect/reconnect): no crash, events still work
- [ ] Repeat the key checks with the **packed `.nupkg`** (section 1.2), not just the project reference

Consider adding a Windows job to CI that builds the Windows sample, so a broken handler can't be merged again.

## Phase 4: Minor clean-up

- `Platforms/Windows/Themes/Generic.xaml` isn't used: WinUI only finds it automatically at `Themes/Generic.xaml`,
  and `UseSegmentedControl()` merges the styles itself. Delete it or move it there.
- `UseSegmentedControl()` merges the dictionary again on every call; skip it if it's already merged.
  `Application.Current` may also be `null` at builder time; consider merging lazily on first `CreatePlatformView`.
- `GetItemPadding()` and `MapPadding` duplicate the padding conversion; share one helper.
- `Plugin.Maui.SegmentedControl.nuspec` is a leftover template the build doesn't use; delete it.
- Consider a `THIRD-PARTY-NOTICES.md` crediting CommunityToolkit/Windows (MIT); the copied files already have headers.

## Phase 5: Release

1. Merge the branch into `main` (the workflow refuses commits that aren't on `origin/main`).
2. Tag `v<version>` (e.g. `v2.1.0`) and push the tag. `.github/workflows/release-nuget.yml` packs on `windows-latest` and pushes to nuget.org.
3. Check the published package on nuget.org lists `net10.0-windows10.0.19041.0` under Frameworks.
