﻿using System.Collections.Generic;
using NLog;
using SDL2;

namespace RetroLite.Input
{
    public class InputProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        public const int MaxPorts = 4;

        private readonly Dictionary<int, GameController> _gameControllersById;
        private readonly GameController[] _ports;
        private int _lastFreePort;

        public GameController this[int port] => _ports[port];

        public int X { get; private set; }
        public int Y { get; private set; }

        public InputProcessor()
        {
            _gameControllersById = new Dictionary<int, GameController>();
            _ports = new GameController[MaxPorts];
            _ports[0] = new GameController();
            
            var joystickCount = SDL.SDL_NumJoysticks();

            for (var i = 0; i < joystickCount; i++)
            {
                if (SDL.SDL_IsGameController(i) == SDL.SDL_bool.SDL_FALSE)
                {
                    continue;
                }

                var controller = new GameController();
                controller.InitializeAsJoystick(i);
                _gameControllersById.Add(controller.ID, controller);
                _assignGameControllerToPort(controller);
            }
        }

        ~InputProcessor()
        {
            foreach (var controller in _gameControllersById.Values)
            {
                controller.Dispose();
            }

            _gameControllersById.Clear();
        }

        private void _assignGameControllerToPort(GameController controller)
        {
            if (_lastFreePort < _ports.Length)
            {
                _ports[_lastFreePort++] = controller;
            }
        }

        public void ResetControllers()
        {
            for (var index = 0; index < _ports.Length; index++)
            {
                var controller = _ports[index];
                controller?.Reset();
            }
        }

        public void Poll()
        {
            while (SDL.SDL_PollEvent(out var e) != 0)
            {
                switch (e.type)
                {
                    case SDL.SDL_EventType.SDL_CONTROLLERDEVICEADDED:
                    {
                        Logger.Debug("Added Device");
                        
                        var gameController = _lastFreePort > 0 ? new GameController() : _ports[_lastFreePort++];

                        gameController.InitializeAsJoystick(e.cdevice.which);
                        if (!_gameControllersById.ContainsKey(gameController.ID))
                        {
                            _gameControllersById.Add(gameController.ID, gameController);
                            _assignGameControllerToPort(gameController);
                        }
                        break;
                    }
                    
                    case SDL.SDL_EventType.SDL_CONTROLLERDEVICEREMOVED:
                    {
                        Logger.Debug("Removed Device");
                        
                        using (var gameController = _gameControllersById[e.cdevice.which])
                        {
                            _gameControllersById.Remove(e.cdevice.which);

                            for (var i = 0; i < _ports.Length; i++)
                            {
                                if (_ports[i] != gameController) continue;
                                
                                _ports[i] = null;
                                _lastFreePort = i;
                                
                                break;
                            }
                        }

                        break;
                    }
                    case SDL.SDL_EventType.SDL_CONTROLLERBUTTONDOWN:
                    case SDL.SDL_EventType.SDL_CONTROLLERBUTTONUP:
                        Logger.Debug("Device Button Pressed");

                        _gameControllersById[e.cbutton.which].ProcessButtonEvent(e.cbutton);
                        break;
                    case SDL.SDL_EventType.SDL_CONTROLLERAXISMOTION:
                        Logger.Debug("Device Axis Moved");
                        _gameControllersById[e.caxis.which].ProcessAxisEvent(e.caxis);
                        break;
                    case SDL.SDL_EventType.SDL_KEYDOWN:
                    case SDL.SDL_EventType.SDL_KEYUP:
                        _ports[0].ProcessKeyboardEvent(e.key);
                        break;
                    case SDL.SDL_EventType.SDL_MOUSEMOTION:
                        X = e.motion.x;
                        Y = e.motion.y;
                        break;
                    //User requests quit
                    case SDL.SDL_EventType.SDL_QUIT:
                        Program.Running = false;
                        break;
                }
            }
        }

        public void HandleEvents()
        {
            //Handle events on queue
            for (var index = 0; index < _ports.Length; index++)
            {
                var controller = _ports[index];
                controller?.CleanupKeyUp();
            }
            Poll();
        }

        public static VirtualKeys GetVirtualKey(GameControllerButton button)
        {
            switch (button)
            {
                case GameControllerButton.A:
                    return VirtualKeys.C;
                case GameControllerButton.B:
                    return VirtualKeys.D;
                case GameControllerButton.X:
                    return VirtualKeys.X;
                case GameControllerButton.Y:
                    return VirtualKeys.S;
                case GameControllerButton.Back:
                    return VirtualKeys.N2;
                case GameControllerButton.Guide:
                    return VirtualKeys.Escape;
                case GameControllerButton.Start:
                    return VirtualKeys.N1;
                case GameControllerButton.LeftStick:
                    return VirtualKeys.A;
                case GameControllerButton.LeftShoulder:
                    return VirtualKeys.W;
                case GameControllerButton.RightStick:
                    return VirtualKeys.F;
                case GameControllerButton.RightShoulder:
                    return VirtualKeys.E;
                case GameControllerButton.DpadUp:
                    return VirtualKeys.Up;
                case GameControllerButton.DpadDown:
                    return VirtualKeys.Down;
                case GameControllerButton.DpadLeft:
                    return VirtualKeys.Left;
                case GameControllerButton.DpadRight:
                    return VirtualKeys.Right;
                default:
                    return VirtualKeys.Noname;
            }
        }
    }
}