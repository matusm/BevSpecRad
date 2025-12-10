using Bev.Instruments.Thorlabs.FW;
using BevSpecRad.Abstractions;

namespace BevSpecRad.Domain
{
    public class FilterWheelShutter : IShutter
    {
        private IFilterWheel _filterWheel;
        private int _blockPos;
        private int _openPos;

        public FilterWheelShutter(IFilterWheel filterWheel, int blockPos)
        {
            _filterWheel = filterWheel;
            _blockPos = blockPos;
            _openPos = filterWheel.GetPosition();
        }

        public string Name => $"Shutter using {_filterWheel.Name}";

        public void Close()
        {
            int _temp = _filterWheel.GetPosition();
            if(_temp != _blockPos)
            {
                _openPos = _temp;
            }
            _filterWheel.GoToPosition(_blockPos);
        }

        public void Open()
        {
            _filterWheel.GoToPosition(_openPos);
        }
    }
}
