using Bev.Instruments.Thorlabs.Ctt;
using BevSpecRad.Abstractions;

namespace BevSpecRad.Domain
{
    public class CctShutter : IShutter
    {
        private readonly ThorlabsCct spectro;

        public string Name => "Thorlabs CCT Shutter (automatic)";

        public CctShutter(ThorlabsCct spectro) { this.spectro = spectro; }

        public void Open()
        {
            spectro.OpenShutter();
        }

        public void Close()
        {
            spectro.CloseShutter();
        }
    }
}
