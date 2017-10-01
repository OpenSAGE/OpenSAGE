using System;
using LLGfx.Hosting;
using OpenSage.Input;
using OpenSage.Input.Providers;

namespace OpenSage.DataViewer.Framework.Services
{
    internal sealed class HwndHostInputProvider : IInputProvider, IDisposable
    {
        private readonly GraphicsDeviceControl _uiElement;
        private readonly HwndHostInputMapper _inputMapper;
        private bool _mouseEntered;

        public HwndHostInputProvider(GraphicsDeviceControl uiElement)
        {
            _uiElement = uiElement;
            _uiElement.HwndMouseEnter += OnHwndMouseEnter;

            _inputMapper = new HwndHostInputMapper(uiElement);
        }

        private void OnHwndMouseEnter(object sender, HwndMouseEventArgs e)
        {
            _mouseEntered = true;
        }

        public void UpdateInputState(InputState state)
        {
            if (_mouseEntered)
            {
                state.LastMouseState = _inputMapper.GetMouseState();
                _mouseEntered = false;
            }
            state.CurrentKeyboardState = _inputMapper.GetKeyboardState();
            state.CurrentMouseState = _inputMapper.GetMouseState();
        }

        public void Dispose()
        {
            _uiElement.HwndMouseEnter -= OnHwndMouseEnter;
            _inputMapper.Dispose();
        }
    }
}
