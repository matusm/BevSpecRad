using BevSpecRad.Abstractions;

namespace BevSpecRad.Domain
{
    internal class NullShutter : IShutter
    {
        public string Name => "Null Shutter";

        public void Open()
        {
            // Do nothing
        }

        public void Close()
        {
            // Do nothing
        }
    }
}
