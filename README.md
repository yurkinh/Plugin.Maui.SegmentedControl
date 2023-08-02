# Plugin.Maui.SegmentedControl
.NET MAUI Port of Alex Rainman XF Segmented control https://github.com/alexrainman/SegmentedControl
### Setup
* Available on NuGet: https://www.nuget.org/packages/Plugin.Maui.SegmentedControl/


**Platform Support**

|Platform|Supported|Version|Renderer|
| ------------------- | :-----------: | :-----------: | :------------------: |
|iOS Unified|Yes|iOS 8.1+|UISegmentedControl|
|Android|Yes|API 18+|RadioGroup|
|Mac OS  |Yes| 11.0+|UISegmentedControl|

#### Usage

In your MaiuProgram file call:

```
var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseSegmentedControl();
```

#### XAML

```xml
xmlns:controls="clr-namespace:Plugin.Maui.SegmentedControl;assembly=Plugin.Maui.SegmentedControl"
```

```xml
<controls:SegmentedControl x:Name="SegControl" TintColor="#007AFF" SelectedSegment="0">
  <controls:SegmentedControl.Children>
    <controls:SegmentedControlOption Text="Tab 1" />
    <controls:SegmentedControlOption Text="Tab 2" />
    <controls:SegmentedControlOption Text="Tab 3" />
    <controls:SegmentedControlOption Text="Tab 4" />
  </controls:SegmentedControl.Children>
</controls:SegmentedControl>
<StackLayout x:Name="SegContent">
</StackLayout>
```

#### Event handler

```
public void Handle_ValueChanged(object o, int e)
{
	SegContent.Children.Clear();

	switch (e)
	{
		case 0:
			SegContent.Children.Add(new Label() { Text="Tab 1 selected" });
			break;
		case 1:
			SegContent.Children.Add(new Label() { Text = "Tab 2 selected" });
			break;
		case 2:
			SegContent.Children.Add(new Label() { Text = "Tab 3 selected" });
			break;
		case 3:
			SegContent.Children.Add(new Label() { Text = "Tab 4 selected" });
			break;
	}
}
```

**Bindable Properties**

```TintColor```: Fill color for the control (Color, default #007AFF)

```SelectedTextColor```: Selected segment text color (Color, default #FFFFFF)

```SelectedSegment```: Selected segment index (int, default 0).

**Event Handlers**

```ValueChanged```: Called when a segment is selected.
