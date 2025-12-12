using Bev.Instruments.Thorlabs.FW;
using BevSpecRad.Abstractions;

namespace BevSpecRad.Domain
{
    public class FilterWheelShutter : IShutter
    {
        private IFilterWheel _filterWheel;
        private readonly int _blockPos;
        private int _openPos;
        private bool _isOpen = true;

        public FilterWheelShutter(IFilterWheel filterWheel, int blockPos)
        {
            _filterWheel = filterWheel;
            _blockPos = blockPos;
            _openPos = filterWheel.GetPosition();
        }

        public string Name => $"Shutter using {_filterWheel.Name}";

        public void Close()
        {
            if (_isOpen)
            {
                _openPos = _filterWheel.GetPosition();
            }
            _filterWheel.GoToPosition(_blockPos);
            _isOpen = false;
        }

        public void Open()
        {
            if (_isOpen)
            {
                _openPos = _filterWheel.GetPosition();
            }
            _filterWheel.GoToPosition(_openPos);
            _isOpen = true;
        }
    }
}
