using Bev.Instruments.Thorlabs.Ctt;
using BevSpecRad.Abstractions;

namespace BevSpecRad.Domain
{
    public class CctShutter : IShutter
    {
        private readonly ThorlabsCct _spectro;

        public string Name => "Thorlabs CCT Shutter (automatic)";

        public CctShutter(ThorlabsCct spectro) { _spectro = spectro; }

        public void Open()
        {
            _spectro.OpenShutter();
        }

        public void Close()
        {
            _spectro.CloseShutter();
        }
    }
}
