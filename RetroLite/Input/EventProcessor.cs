using System;
using System.Collections.Generic;
using RetroLite.Scene;
using SDL2;

namespace RetroLite.Input
{
    public class EventProcessor
    {
        public const int MaxPorts = 4;

        private readonly Dictionary<int, GameController> _gameControllersByID;
        private GameController[] _ports;
        private int _lastFreePort = 0;
        private readonly SceneManager _manager;

        public GameController this[int port] => _ports[port];

        public EventProcessor(SceneManager manager)
        {
            _gameControllersByID = new Dictionary<int, GameController>();
            _ports = new GameController[MaxPorts];
            _ports[0] = new GameController();
            _manager = manager;
        }

        public void Init()
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_GAMECONTROLLER) != 0)
            {
                Console.WriteLine("Game Controller Init error");
                throw new Exception("SDL Game Controller Initialization error");
            }
            
            var joystickCount = SDL.SDL_NumJoysticks();

            for (var i = 0; i < joystickCount; i++)
            {
                if (SDL.SDL_IsGameController(i) == SDL.SDL_bool.SDL_FALSE)
                {
                    continue;
                }

                var controller = new GameController();
                controller.InitializeAsJoystick(i);
                _gameControllersByID.Add(controller.ID, controller);
                _assignGameControllerToPort(controller);
            }
        }

        public void Cleanup()
        {
            foreach (var controller in _gameControllersByID.Values)
            {
                controller.Dispose();
            }

            _gameControllersByID.Clear();
            _ports = new GameController[MaxPorts];
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
            foreach (var controller in _ports)
            {
                controller?.Reset();
            }
        }

        public void HandleEvents()
        {
            //Handle events on queue
            while (SDL.SDL_PollEvent(out var e) != 0)
            {
                switch (e.type)
                {
                    case SDL.SDL_EventType.SDL_CONTROLLERDEVICEADDED:
                    {
                        GameController gameController;
                        gameController = _lastFreePort > 0 ? new GameController() : _ports[_lastFreePort++];

                        gameController.InitializeAsJoystick(e.cdevice.which);
                        if (!_gameControllersByID.ContainsKey(gameController.ID))
                        {
                            _gameControllersByID.Add(gameController.ID, gameController);
                            _assignGameControllerToPort(gameController);
                        }
                        break;
                    }
                    
                    case SDL.SDL_EventType.SDL_CONTROLLERDEVICEREMOVED:
                    {
                        var gameController = _gameControllersByID[e.cdevice.which];
                        _gameControllersByID.Remove(e.cdevice.which);
                        gameController.Dispose();

                        for (var i = 0; i < _ports.Length; i++)
                        {
                            if (_ports[i] == gameController)
                            {
                                _ports[i] = null;
                                _lastFreePort = i;
                                break;
                            }
                        }
                        break;
                    }
                    case SDL.SDL_EventType.SDL_CONTROLLERBUTTONDOWN:
                    case SDL.SDL_EventType.SDL_CONTROLLERBUTTONUP:
                        _gameControllersByID[e.cbutton.which].ProcessButtonEvent(e.cbutton);
                        break;
                    case SDL.SDL_EventType.SDL_CONTROLLERAXISMOTION:
                        _gameControllersByID[e.caxis.which].ProcessAxisEvent(e.caxis);
                        break;
                    case SDL.SDL_EventType.SDL_KEYDOWN:
                    case SDL.SDL_EventType.SDL_KEYUP:
                        _ports[0].ProcessKeyboardEvent(e.key);
                        break;
                    //User requests quit
                    case SDL.SDL_EventType.SDL_QUIT:
                        _manager.Running = false;
                        break;
                }
            }
        }

        public VirtualKeys GetVirtualKey(GameControllerButton button)
        {
            switch (button)
            {
                case GameControllerButton.A:
                    return VirtualKeys.D;
                case GameControllerButton.B:
                    return VirtualKeys.C;
                case GameControllerButton.X:
                    return VirtualKeys.S;
                case GameControllerButton.Y:
                    return VirtualKeys.X;
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