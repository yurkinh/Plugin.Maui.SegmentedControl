# Windows support: release checklist

Status of branch `copilot/analyze-segmented-control-effort` as of commit `70e85d9`.
This branch adds a WinUI 3 implementation of `SegmentedControl`. It uses a copy of the
Community Toolkit `Segmented` control, adapted for this library. **It is not ready to publish yet.**
Work through the sections below in order on a Windows machine.

## What's on the branch

| File | Purpose |
| --- | --- |
| `src/Plugin.Maui.SegmentedControl/Platforms/Windows/SegmentedControlHandler.cs` | MAUI handler that maps `SegmentedControl` onto the native `Segmented` control |
| `.../Platforms/Windows/NativeSegmented/Segmented*.cs`, `SegmentedItem*.cs`, `EqualPanel.cs` | Copy of the CommunityToolkit/Windows `Segmented` control (MIT) |
| `.../Platforms/Windows/Themes/SegmentedStyles.xaml` | Styles, control templates, and the theme resource keys the handler overrides |
| `.../Platforms/Windows/Themes/Generic.xaml` | Merges `SegmentedStyles.xaml` (not used at the moment, see Minor) |
| `.../Extensions/AppHostBuilderExtensions.cs` | `UseSegmentedControl()` merges `SegmentedStyles.xaml` into `Application.Current.Resources` on Windows |
| `Plugin.Maui.SegmentedControl.csproj` | Adds `net10.0-windows10.0.19041.0` and the XAML `Page` items |

How the handler maps the control's properties:

| `SegmentedControl` property | Windows behavior |
| --- | --- |
| `Children` | Rebuilds `SegmentedItem`s with the option's `Text` and `IsEnabled` |
| `SelectedSegment` | Sets `Segmented.SelectedIndex`, raises `ValueChanged` |
| `GroupToggleBehavior` | `Radio` → `SelectionMode.Single`; `None` → `SelectionMode.None` (only read when the control is created) |
| `TintColor` | Background of the selected item; border while the control is enabled |
| `SelectedTextColor` / `TextColor` | Text color of the selected item / of unselected items |
| `DisabledBackgroundColor` / `DisabledTextColor` | Background / text color of disabled items |
| `DisabledTintColor` | Border color while the control is disabled |
| `FontSize`, `Padding` | Applied to each item (a `Padding` of 0 falls back to 11) |
| `SegmentTapped` | Raised from the `Tapped` event on `Segmented` |

## 1. Blockers (fix first)

### 1.1 The Windows handler doesn't compile

Commit `de508c4` replaced the closing brace of `MapTextColor` instead of adding a line above it.
In `Platforms/Windows/SegmentedControlHandler.cs`:

```csharp
static void MapTextColor(SegmentedControlHandler handler, SegmentedControl control)
{
    handler._textBrush = new WinBrush(control.TextColor.ToWindowsColor());
    handler.PlatformView.Resources["ButtonItemForeground"] = handler._textBrush;
    handler.PlatformView.Resources["ButtonItemForegroundPointerOver"] = handler._textBrush;
    handler.PlatformView.Resources["ButtonItemForegroundPressed"] = handler._textBrush;
}   // <-- restore this brace
```

`ApplyColors` should also set `segmented.Resources["ButtonItemForegroundPressed"] = _textBrush;`.

### 1.2 The library no longer builds on macOS or Linux

The WinUI XAML compiler (`XamlCompiler.exe`) only runs on Windows. With the Windows TFM listed
unconditionally, `dotnet build` and `dotnet pack` of the library fail on a Mac (exit code 126).
`EnableWindowsTargeting` doesn't help for projects that contain XAML pages.

Pick one of these:

- **Quick fix:** add the Windows TFM only on Windows (the sample projects already do this) and
  remove `EnableWindowsTargeting`:

  ```xml
  <TargetFrameworks>net10.0;net10.0-android;net10.0-ios;net10.0-maccatalyst</TargetFrameworks>
  <TargetFrameworks Condition="$([MSBuild]::IsOSPlatform('windows'))">$(TargetFrameworks);net10.0-windows10.0.19041.0</TargetFrameworks>
  ```

  The release workflow runs on `windows-latest`, so the published package still includes Windows.
  The catch: you have to pack on Windows, or the package ships without Windows support.

- **Better fix:** remove the `<Page>` items. Load the styles at runtime with
  `Microsoft.UI.Xaml.Markup.XamlReader.Load(...)` from an `EmbeddedResource`, or build them in C#.
  That needs no XAML compiler, builds anywhere, and also fixes 1.3.

### 1.3 The NuGet packaging hasn't been checked

The styles load from `ms-appx:///Plugin.Maui.SegmentedControl/Platforms/Windows/Themes/SegmentedStyles.xaml`.
That works with a `ProjectReference`. When the library comes from a NuGet package, the compiled XAML
(`.xbf`) and resource index (`.pri`) files must be inside the `.nupkg`, or the app fails when it
merges the dictionary. Check this (PowerShell):

```powershell
dotnet pack src\Plugin.Maui.SegmentedControl\Plugin.Maui.SegmentedControl.csproj -c Release -o .\artifacts
tar -tf .\artifacts\Plugin.Maui.SegmentedControl.<version>.nupkg | findstr /i "windows"
```

Expect files under `lib/net10.0-windows10.0.19041.0/`: the `.dll`, a `.pri`, and the
`SegmentedStyles.xbf` / `.xaml` (or a layout folder holding them). If they're missing, try
`<GenerateLibraryLayout>true</GenerateLibraryLayout>` for the Windows TFM, or switch to the
"better fix" in 1.2.

Then test the package for real: in a sample, temporarily replace the `ProjectReference` with a
`PackageReference` to the local `.\artifacts` feed. Use a `nuget.config` with a local source, or
`dotnet add package Plugin.Maui.SegmentedControl --source .\artifacts --version <version>`.
Run it and confirm the control shows up with its styles.

### 1.4 Version and release metadata

- `PackageVersion` is still `2.0.0`, and tag `2.0.0` already exists. The release workflow pushes with
  `--skip-duplicate`, so publishing would silently do nothing. Bump it (suggested: `2.1.0`, since
  this adds a platform).
- Update `PackageReleaseNotes` (currently "Updated to .net 10").
- Add Windows to the platform table in `README.md`:
  `|Windows|Yes|10.0.19041+|WinUI 3 Segmented (Community Toolkit port)|`

## 2. Functional fixes (do these and verify on Windows)

### 2.1 Out-of-range `SelectedSegment` throws

`CreatePlatformView` and `MapSelectedSegment` set `SelectedIndex` without a range check. If a binding
sets `SelectedSegment` before `Children` is filled, WinUI throws. Guard it the way Android does:

```csharp
var index = control.SelectedSegment;
if (index < -1 || index >= handler.PlatformView.Items.Count)
    return;
```

### 2.2 `SegmentTapped` may never fire

`ListViewItem` usually marks `Tapped` as handled, so a plain `+=` subscription may never get called.
Register for handled events too:

```csharp
Microsoft.UI.Xaml.Input.TappedEventHandler? _tappedHandler;

protected override void ConnectHandler(Segmented platformView)
{
    base.ConnectHandler(platformView);
    platformView.SelectionChanged += OnSelectionChanged;
    _tappedHandler = OnTapped;
    platformView.AddHandler(Microsoft.UI.Xaml.UIElement.TappedEvent, _tappedHandler, handledEventsToo: true);
}

protected override void DisconnectHandler(Segmented platformView)
{
    platformView.SelectionChanged -= OnSelectionChanged;
    if (_tappedHandler is not null)
        platformView.RemoveHandler(Microsoft.UI.Xaml.UIElement.TappedEvent, _tappedHandler);
    base.DisconnectHandler(platformView);
}
```

Keyboard selection (Space/Enter) doesn't raise `SegmentTapped`. Decide whether it should.

### 2.3 Changing colors at runtime probably doesn't update the control

The `Map*Color` methods replace entries in `segmented.Resources`. WinUI looks up `{ThemeResource}` values
when the template is applied and only refreshes them when the theme changes. So the starting colors work,
but changing them after the control is shown probably won't. Two values likely ignore the overrides
altogether, because they come from Style setters, which look up resources where the style is defined:

- an unselected item's `Foreground` (`ButtonItemForeground`)
- the control's `BorderBrush` (`SegmentedBorderBrush`)

Test first. If it's broken, set `item.Foreground` and `segmented.BorderBrush` directly. For the
selected and disabled colors, either rebuild the items (`BuildItems`) after a color change, or re-run
`VisualStateManager.GoToState` on each item.

### 2.4 `DisabledTintColor` behaves differently from iOS

On iOS, `DisabledTintColor` is the selected segment's color when the control is disabled (the
`ISegmentedControl` doc says the same). On Windows it only colors the border, and a disabled selected
item shows `DisabledBackgroundColor`. Also, the `Disabled` visual state applies 0.55 opacity on top of
the custom disabled colors, so they look faded. Line this up with iOS, or document the difference.

## 3. Manual test plan (Windows)

Run `samples/SegmentedControlSamples` (or `samples/Test.SegmentedControl`) on Windows. Use F5 in
Visual Studio with the `net10.0-windows10.0.19041.0` target, or:

```powershell
dotnet build samples\SegmentedControlSamples\SegmentedControlSamples.csproj -f net10.0-windows10.0.19041.0 -t:Run
```

- [ ] The control renders with equal-width segments and a border, in light, dark and high-contrast themes
- [ ] The starting `SelectedSegment` is shown; clicking a segment updates `SelectedSegment` and raises `ValueChanged`
- [ ] `SegmentTapped` fires on click, and not for disabled segments
- [ ] Setting `SelectedSegment` from code or a binding updates the UI; `-1` clears the selection; an out-of-range value doesn't crash
- [ ] `GroupToggleBehavior="None"` allows no selection but still raises `SegmentTapped`
- [ ] All colors apply at startup **and** when changed at runtime
- [ ] Disabling the control or a single option (`SegmentedControlOption.IsEnabled`) looks right and blocks input
- [ ] Changing an option's `Text` at runtime, or replacing `Children`, rebuilds the segments and keeps the selection
- [ ] `FontSize` and `Padding` apply
- [ ] Keyboard: Tab into the control, arrow keys move focus (reversed in `FlowDirection="RightToLeft"`), Space/Enter selects
- [ ] Navigate away and back (handler disconnect/reconnect): no crash, events still work
- [ ] Repeat the key checks with the **packed `.nupkg`** (section 1.3), not just the project reference

## 4. Minor clean-up

- `Platforms/Windows/Themes/Generic.xaml` isn't used: WinUI only finds it automatically at `Themes/Generic.xaml`,
  and `UseSegmentedControl()` merges the styles itself. Delete it or move it there.
- `UseSegmentedControl()` merges the dictionary again on every call; skip it if it's already merged.
- `Plugin.Maui.SegmentedControl.nuspec` is a leftover template the build doesn't use; delete it.
- Consider a `THIRD-PARTY-NOTICES.md` crediting CommunityToolkit/Windows (MIT); the copied files already have headers.

## 5. Release

1. Merge the branch into `main` (the workflow refuses commits that aren't on `origin/main`).
2. Tag `v<version>` (e.g. `v2.1.0`) and push the tag. `.github/workflows/release-nuget.yml` packs on `windows-latest` and pushes to nuget.org.
3. Check the published package on nuget.org lists `net10.0-windows10.0.19041.0` under Frameworks.
