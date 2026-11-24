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
| `Children` | Updates segments in place when the count is unchanged; otherwise rebuilds them |
| `SelectedSegment` | Sets `Segmented.SelectedIndex` (out of range is ignored), raises `ValueChanged` for valid indexes |
| `GroupToggleBehavior` | `Radio` → `SelectionMode.Single`; `None` → `SelectionMode.None` (can change at runtime) |
| `TintColor` | Background of the selected item; border while the control is enabled |
| `SelectedTextColor` / `TextColor` | Text color of the selected item / of unselected items (normal, pointer-over, pressed) |
| `DisabledBackgroundColor` / `DisabledTextColor` | Background / text color of disabled items |
| `DisabledTintColor` | Background of the selected item and the border while disabled |
| `FontSize`, `Padding` | Applied to each item (a `Padding` of 0 falls back to 11) |
| `SegmentTapped` | Raised on click/tap, also in `None` mode |

## Done: PR review findings and build fixes

All review threads on PR #23 are addressed. The last round also fixed the Windows build, which was broken:

- [x] The XAML compiler failed silently (`XamlCompiler.exe exited with code 1`). The MAUI SDK already
  includes `Platforms\Windows\**\*.xaml` as WinUI `Page` items, and the explicit `<Page Include>` items
  in the csproj passed each file a second time. Removed the explicit items.
- [x] `MapTextColor` lost its closing brace in `de508c4`. Restored it, and `TextColor` now also drives
  `ButtonItemForegroundPressed` (review: *Apply configured pressed foreground resource*).
- [x] `FlowDirection is FlowDirection.RightToLeft` in `Segmented.cs` didn't compile (CS0176: the name
  resolved to the instance property). Qualified as `Microsoft.UI.Xaml.FlowDirection.RightToLeft`.
- [x] Renamed the handler's brush fields to unprefixed camelCase (review: *Rename underscore-prefixed brush fields*).
- [x] Added `#nullable enable` to the Windows files that use nullable annotations (CS8632).
- [x] Earlier rounds: RTL key mapping, focus result returned from `MoveFocus`, index clamped in `MoveFocus`,
  high-contrast resources switched to `*Brush`, disabled colors applied in `CommonStates/Disabled`,
  missing `IconTop`/`Horizontal`/`Vertical` states, field renames in `EqualPanel`/`Segmented`/`SegmentedItem`.

Verified on Windows: the library builds for `net10.0-windows10.0.19041.0`, `net10.0`, `net10.0-android`
with 0 warnings and 0 errors, and `samples/SegmentedControlSamples` builds for Windows. Running it is covered in phase 1.

## Phase 1: Build and packaging blockers (done)

### 1.1 Builds on macOS and Linux again

The WinUI XAML compiler (`XamlCompiler.exe`) only runs on Windows, so the Windows TFM is now added only
when building on Windows, and `EnableWindowsTargeting` is gone:

```xml
<TargetFrameworks>net10.0;net10.0-android;net10.0-ios;net10.0-maccatalyst</TargetFrameworks>
<TargetFrameworks Condition="$([MSBuild]::IsOSPlatform('windows'))">$(TargetFrameworks);net10.0-windows10.0.19041.0</TargetFrameworks>
```

**Release packages must be packed on Windows** (`release-nuget.yml` runs on `windows-latest`), or they ship
without Windows support.

Loading the styles at runtime with `XamlReader.Load` instead was considered and rejected: without compiled
XAML, the library has no XAML metadata provider, so `local:Segmented` / `local:SegmentedItem` /
`local:EqualPanel` in the styles can't be resolved at runtime.

### 1.2 NuGet packaging verified

The Windows part of the package is `lib/net10.0-windows10.0.19041/` with the `.dll` and the `.pri`.
The `.pri` embeds the compiled XAML, and the consuming app merges it into its `resources.pri`, so
`ms-appx:///Plugin.Maui.SegmentedControl/Platforms/Windows/Themes/SegmentedStyles.xaml` resolves with no
loose `.xaml`/`.xbf` files.

Tested by building `SegmentedControlSamples` against the packed `.nupkg` from a local feed (unpackaged,
`WindowsPackageType=None`) and running it: styles, colors and selection render correctly. Also confirmed
with a copy of the package stripped of `.xaml`/`.xbf` files. **Not tested: MSIX-packaged deployment.**

### 1.3 Startup crash in the handler (found while testing 1.2)

Every app using the control crashed at startup on Windows (`COMException 0x800F0902`, stowed exception
`0xC000027B`): `MapIsEnabled` replaced an existing entry in `Segmented.Resources`, which WinUI rejects.
The handler now creates one `SolidColorBrush` per resource key, inserts it once, and afterwards only
changes its `Color`. It also sets `SegmentedItem.Foreground` and `Segmented.BorderBrush` directly, since
those come from style setters that never see the per-control overrides.

### 1.4 Version and release metadata

- `PackageVersion` bumped to `2.1.0` (tag `2.0.0` already exists).
- `PackageReleaseNotes`: "Added Windows (WinUI 3) support."
- Windows added to the platform table in `README.md`.

## Phase 2: Functional gaps (done)

Verified on Windows by driving `samples/SegmentedControlSamples` through UI Automation (button presses, real
mouse clicks, UIA selection) with screenshots and an event log. Unpackaged, project reference.

### 2.1 `SelectedSegment` edge cases

- Out-of-range values are ignored instead of throwing; `-1` clears the selection.
- `ValueChanged` is raised only for values that point at a segment (like Android), so an out-of-range value
  or `-1` raises nothing. It's raised once per real change (click, UIA selection, or code).
- A value set before `Children` is filled is applied when the children arrive (`MapChildren` re-applies it).

### 2.2 `SegmentTapped` fires on click

The handler registers for `Tapped` with `handledEventsToo: true` (ListViewItem marks taps handled).
Verified: a click raises `SegmentTapped` (and `ValueChanged` in `Radio` mode); in `None` mode it raises
`SegmentTapped` without selecting. Keyboard selection (Space/Enter) still doesn't raise `SegmentTapped`;
neither does iOS, so this is left as is.

### 2.3 Color changes at runtime

Verified: changing `TintColor`, `TextColor`, `SelectedTextColor` and `DisabledTintColor` while the control is
shown updates it immediately, including the border and the segments created later by replacing `Children`.

### 2.4 Disabled colors match iOS/Android

- Disabled, unselected segment: `DisabledBackgroundColor` background, `DisabledTextColor` text.
- Disabled, selected segment: `DisabledTintColor` background, `DisabledTextColor` text (new `DisabledSelected`
  visual state; `SegmentedItem` uses a custom `VisualStateManager` to route disabled items there, because
  ListViewItem keeps driving Normal/Selected while disabled).
- Border while disabled: `DisabledTintColor`.
- The extra 0.55 opacity on disabled segments is gone, so the configured colors show as set.

**Open decision:** the library defaults are `DisabledBackgroundColor = Gray` and `DisabledTextColor = Gray`, so
with default colors a disabled unselected segment's text is invisible (gray on gray). Consider a lighter
default text color, or document it.

### 2.5 Option changes update segments in place

Changing an option's `Text` or `IsEnabled` updates the existing segment (selection and focus are kept).
Replacing `Children` with a different number of options rebuilds the segments and keeps `SelectedSegment`.

### 2.6 `GroupToggleBehavior` can change at runtime

Switching to `None` clears the visible selection (taps still raise `SegmentTapped`); switching back to
`Radio` restores `SelectedSegment`.

### 2.7 Sizing follows `HorizontalOptions`

`EqualPanel` (from the Community Toolkit) reported the full available width when stretched, so `Start`,
`Center` and `End` looked like `Fill`, and its arrange step overwrote the measured item size, which left
segments in stale positions. It now measures the natural size (widest segment × count) and stretches only
when arranged. Verified `Fill`, `Start`, `Center`, `End`, and switching back.

### 2.8 Accessibility

- `Segmented` is exposed as a `List` (it was an unnamed `Custom` element) with the Selection pattern.
- Each segment is a `ListItem` named after its text, with position ("2 of 3"), `IsSelected`, and a working
  `Select()`; selection changes raise the UIA selection events.
- Verified with UI Automation. **Not yet tried with Narrator itself.**

## Phase 3: Manual test plan (Windows)

Run `samples/SegmentedControlSamples` (or `samples/Test.SegmentedControl`) on Windows. Use F5 in
Visual Studio with the `net10.0-windows10.0.19041.0` target, or:

```powershell
dotnet build samples\SegmentedControlSamples\SegmentedControlSamples.csproj -f net10.0-windows10.0.19041.0 -t:Run
```

- [ ] The control renders with equal-width segments and a border, in light, dark and high-contrast themes
- [ ] Switching the Windows theme while the app runs keeps custom colors
- [x] (automated, phase 2) The starting `SelectedSegment` is shown; clicking a segment updates `SelectedSegment` and raises `ValueChanged` once
- [ ] `SegmentTapped` fires on click, and not for disabled segments
- [x] (automated, phase 2) Setting `SelectedSegment` from code updates the UI; an out-of-range value doesn't crash
- [ ] Setting `SelectedSegment` through a binding works; `-1` clears the selection
- [x] (automated, phase 2) `GroupToggleBehavior="None"` allows no selection but still raises `SegmentTapped`
- [ ] All colors apply at startup **and** when changed at runtime (including pressed / pointer-over)
- [ ] Disabling the control or a single option (`SegmentedControlOption.IsEnabled`) looks right and blocks input
- [x] (automated, phase 2) Changing an option's `Text` at runtime, or replacing `Children`, updates the segments and keeps the selection
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
