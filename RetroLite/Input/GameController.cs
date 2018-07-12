using System;
using SDL2;

namespace RetroLite.Input
{
    public enum GameControllerButtonState
    {
        None = 0,
        Down,
        Up
    }
    
    public enum GameControllerButton
    {
         A = 0,
         B,
         X,
         Y,

         Back,
         Guide,
         Start,

         LeftStick,
         LeftShoulder,
         RightStick,
         RightShoulder,

         DpadUp,
         DpadDown,
         DpadLeft,
         DpadRight
    }

    public enum GameControllerAnalog
    {
        LeftTrigger = 0,
        RightTrigger,

        LeftX,
        LeftY,

        RightX,
        RightY
    }

    public class GameController : IDisposable
    {
        private IntPtr _sdlController;

        public bool IsJoystick => _sdlController != IntPtr.Zero;
        
        public int ID { get; private set; }
        public int Index { get; private set; }

        public GameControllerButtonState A { get; private set; }
        public GameControllerButtonState B { get; private set; }
        public GameControllerButtonState X { get; private set; }
        public GameControllerButtonState Y { get; private set; }

        public GameControllerButtonState Back { get; private set; }
        public GameControllerButtonState Guide { get; private set; }
        public GameControllerButtonState Start { get; private set; }

        public GameControllerButtonState LeftStick { get; private set; }
        public GameControllerButtonState LeftShoulder { get; private set; }
        public GameControllerButtonState RightStick { get; private set; }
        public GameControllerButtonState RightShoulder { get; private set; }

        public GameControllerButtonState DpadUp { get; private set; }
        public GameControllerButtonState DpadDown { get; private set; }
        public GameControllerButtonState DpadLeft { get; private set; }
        public GameControllerButtonState DpadRight { get; private set; }

        public short LeftTrigger { get; private set; }
        public short RightTrigger { get; private set; }

        public short LeftX { get; private set; }
        public short LeftY { get; private set; }

        public short RightX { get; private set; }
        public short RightY { get; private set; }

        public void InitializeAsJoystick(int index)
        {
            Index = index;

            if (SDL.SDL_IsGameController(index) != SDL.SDL_bool.SDL_TRUE) return;
            
            _sdlController = SDL.SDL_GameControllerOpen(index);

            if (IntPtr.Zero == _sdlController) return;
            
            var joystick = SDL.SDL_GameControllerGetJoystick(_sdlController);
            
            ID = SDL.SDL_JoystickInstanceID(joystick);
        }

        public GameControllerButtonState GetButtonState(GameControllerButton button)
        {
            switch (button)
            {
                case GameControllerButton.A:
                    return A;
                case GameControllerButton.B:
                    return B;
                case GameControllerButton.X:
                    return X;
                case GameControllerButton.Y:
                    return Y;
                case GameControllerButton.Back:
                    return Back;
                case GameControllerButton.Guide:
                    return Guide;
                case GameControllerButton.Start:
                    return Start;
                case GameControllerButton.LeftStick:
                    return LeftStick;
                case GameControllerButton.LeftShoulder:
                    return LeftShoulder;
                case GameControllerButton.RightStick:
                    return RightStick;
                case GameControllerButton.RightShoulder:
                    return RightShoulder;
                case GameControllerButton.DpadUp:
                    return DpadUp;
                case GameControllerButton.DpadDown:
                    return DpadDown;
                case GameControllerButton.DpadLeft:
                    return DpadLeft;
                case GameControllerButton.DpadRight:
                    return DpadRight;
                default:
                    return GameControllerButtonState.None;
            }
        }

        public short GetAnalogState(GameControllerAnalog analog)
        {
            switch (analog)
            {
                case GameControllerAnalog.LeftTrigger:
                    return LeftTrigger;
                case GameControllerAnalog.RightTrigger:
                    return RightTrigger;
                case GameControllerAnalog.LeftX:
                    return LeftX;
                case GameControllerAnalog.LeftY:
                    return LeftY;
                case GameControllerAnalog.RightX:
                    return RightX;
                case GameControllerAnalog.RightY:
                    return RightY;
                default:
                    return 0;
            }
        }

        public void Reset()
        {
            A = B = X = Y = DpadDown = DpadLeft = DpadRight = DpadUp = Back = Guide = Start = LeftShoulder = LeftStick = RightShoulder = RightStick = GameControllerButtonState.None;
            LeftTrigger = RightTrigger = LeftX = LeftY = RightX = RightY = 0;
        }

        public void CopyStateFrom(GameController other)
        {
            A = other.A;
            B = other.B;
            X = other.X;
            Y = other.Y;
            DpadDown = other.DpadDown;
            DpadLeft = other.DpadLeft;
            DpadRight = other.DpadRight;
            DpadUp = other.DpadUp;
            Back = other.Back;
            Start = other.Start;
            LeftShoulder = other.LeftShoulder;
            LeftStick = other.LeftStick;
            LeftTrigger = other.LeftTrigger;
            LeftX = other.LeftX;
            LeftY = other.LeftY;
            RightShoulder = other.RightShoulder;
            RightStick = other.RightStick;
            RightTrigger = other.RightTrigger;
            RightX = other.RightX;
            RightY = other.RightY;
            Guide = other.Guide;
        }

        public void ProcessAxisEvent(SDL.SDL_ControllerAxisEvent e)
        {
            switch ((SDL.SDL_GameControllerAxis)e.axis)
            {
                case SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX:
                    LeftX = e.axisValue;
                    break;
                case SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTY:
                    LeftY = e.axisValue;
                    break;
                case SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTX:
                    RightX = e.axisValue;
                    break;
                case SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTY:
                    RightY = e.axisValue;
                    break;
                case SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT:
                    LeftTrigger = e.axisValue;
                    break;
                case SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT:
                    RightTrigger = e.axisValue;
                    break;
            }
        }

        public void ProcessButtonEvent(SDL.SDL_ControllerButtonEvent e)
        {
            var state = e.state == SDL.SDL_PRESSED ? GameControllerButtonState.Down : GameControllerButtonState.Up;

            switch ((SDL.SDL_GameControllerButton)e.button)
            {
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A:
                    A = state;
                    break;
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_B:
                    B = state;
                    break;
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_BACK:
                    Back = state;
                    break;
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN:
                    DpadDown = state;
                    break;
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT:
                    DpadLeft = state;
                    break;
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT:
                    DpadRight = state;
                    break;
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP:
                    DpadUp = state;
                    break;
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_GUIDE:
                    Guide = state;
                    break;
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSHOULDER:
                    LeftShoulder = state;
                    break;
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSTICK:
                    LeftStick = state;
                    break;
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSHOULDER:
                    RightShoulder = state;
                    break;
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSTICK:
                    RightStick = state;
                    break;
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START:
                    Start = state;
                    break;
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_X:
                    X = state;
                    break;
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_Y:
                    Y = state;
                    break;
            }
        }

        public void ProcessKeyboardEvent(SDL.SDL_KeyboardEvent e)
        {
            if (e.repeat > 0) return;
            
            var state = e.state == SDL.SDL_PRESSED ? GameControllerButtonState.Down : GameControllerButtonState.Up;
            
            switch (e.keysym.sym)
            {
                case SDL.SDL_Keycode.SDLK_d:
                    A = state;
                    break;
                case SDL.SDL_Keycode.SDLK_c:
                    B = state;
                    break;
                case SDL.SDL_Keycode.SDLK_2:
                    Back = state;
                    break;
                case SDL.SDL_Keycode.SDLK_DOWN:
                    DpadDown = state;
                    break;
                case SDL.SDL_Keycode.SDLK_LEFT:
                    DpadLeft = state;
                    break;
                case SDL.SDL_Keycode.SDLK_RIGHT:
                    DpadRight = state;
                    break;
                case SDL.SDL_Keycode.SDLK_UP:
                    DpadUp = state;
                    break;
                case SDL.SDL_Keycode.SDLK_ESCAPE:
                    Guide = state;
                    break;
                case SDL.SDL_Keycode.SDLK_w:
                    LeftShoulder = state;
                    break;
                case SDL.SDL_Keycode.SDLK_q:
                    LeftTrigger = short.MaxValue;
                    break;
                case SDL.SDL_Keycode.SDLK_a:
                    LeftStick = state;
                    break;
                case SDL.SDL_Keycode.SDLK_e:
                    RightShoulder = state;
                    break;
                case SDL.SDL_Keycode.SDLK_r:
                    RightTrigger = short.MaxValue;
                    break;
                case SDL.SDL_Keycode.SDLK_f:
                    RightStick = state;
                    break;
                case SDL.SDL_Keycode.SDLK_1:
                    Start = state;
                    break;
                case SDL.SDL_Keycode.SDLK_s:
                    X = state;
                    break;
                case SDL.SDL_Keycode.SDLK_x:
                    Y = state;
                    break;
            }
        }

        public void Dispose()
        {
            if (_sdlController != IntPtr.Zero)
            {
                SDL.SDL_GameControllerClose(_sdlController);
            }
        }
    }
}
