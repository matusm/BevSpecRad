using BevSpecRad.Abstractions;

namespace BevSpecRad.Domain
{
    public class ManualShutter : IShutter
    {
        public string Name => "Manual Shutter";

        public void Open()
        {
            UIHelper.WriteMessageAndWait("Please open the manual shutter and press any key to continue...");
        }

        public void Close()
        {
            UIHelper.WriteMessageAndWait("Please close the manual shutter and press any key to continue...");
        }
    }
}
