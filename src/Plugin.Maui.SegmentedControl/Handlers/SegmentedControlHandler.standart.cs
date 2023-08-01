#if !(WINDOWS || ANDROID || IOS || MACCATALYST)
using Microsoft.Maui.Handlers;

namespace Plugin.Maui.SegmentedControl.Handlers;

public partial class SegmentedControlHandler : ViewHandler<ISegmentedControl, object>
{

    public SegmentedControlHandler(IPropertyMapper mapper, CommandMapper commandMapper) : base(mapper, commandMapper)
    {
    }    

    public static void MapSource(SegmentedControlHandler handler, ISegmentedControl control)
    {
    }

    public static Task MapSourceAsync(SegmentedControlHandler handler, ISegmentedControl icon)
    {
        return Task.CompletedTask;
    }

    public static void MapTintColor(SegmentedControlHandler handler, ISegmentedControl icon)
    {
    }    

    protected override object CreatePlatformView()
    {
        throw new NotImplementedException();
    }
}
#endif