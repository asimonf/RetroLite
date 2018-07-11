using System;
using System.Collections.Generic;
using Redbus;
using RetroLite.Event;
using RetroLite.Input;
using RetroLite.Scene;
using Xilium.CefGlue;

namespace RetroLite.Menu
{
    public class MenuScene: IScene
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        
        private GameController _prevControllerState;
        private GameControllerButton[] _buttons;
        private GameControllerAnalog[] _analogs;
        private int[] _buttonRepeatCounter;
        private const int RepeatFrameLimit = 30;
        private readonly List<SubscriptionToken> _eventTokenList;

        private readonly CefBrowser _browser;
        private readonly MenuBrowserClient _browserClient;
        private readonly SceneManager _manager;
        private readonly EventProcessor _eventProcessor;

        public bool IsLoaded => !_browser.IsLoading;

        public MenuScene(
            CefMainArgs mainArgs, 
            SceneManager manager, 
            EventProcessor eventProcessor,
            MenuBrowserClient browserClient
        )
        {
            _prevControllerState = new GameController();
            _buttons = (GameControllerButton[]) Enum.GetValues(typeof(GameControllerButton));
            _analogs = (GameControllerAnalog[]) Enum.GetValues(typeof(GameControllerAnalog));
            _buttonRepeatCounter = new int[_buttons.Length];
            _eventTokenList = new List<SubscriptionToken>();
            
            var settings = new CefSettings()
            {
                WindowlessRenderingEnabled = true,
            };
            
            CefRuntime.Initialize(mainArgs, settings, null, IntPtr.Zero);
            
            // Instruct CEF to not render to a window at all.
            var cefWindowInfo = CefWindowInfo.Create();
            cefWindowInfo.SetAsWindowless(IntPtr.Zero, false);

            var browserSettings = new CefBrowserSettings()
            {
                WindowlessFrameRate = 60
            };
            
            _manager = manager;
            _eventProcessor = eventProcessor;
            _browserClient = browserClient;
            _browser = CefBrowserHost.CreateBrowserSync(cefWindowInfo, _browserClient, browserSettings, "https://codepen.io/SoftwareRVG/pen/OXkOWj");

            _eventTokenList.Add(Program.EventBus.Subscribe<OpenMenuEvent>(OnOpenMenuEvent));
            _eventTokenList.Add(Program.EventBus.Subscribe<IntroFinishedEvent>(OnIntroFinishedEvent));
        }

        ~MenuScene()
        {
            foreach (var token in _eventTokenList)
            {
                Program.EventBus.Unsubscribe(token);                
            }
            _eventTokenList.Clear();
        }

        private void OnOpenMenuEvent(OpenMenuEvent openMenuEvent)
        {
            if (!_manager.IsCurrentScene(this))
            {
                _manager.PushScene(this);
            }
        }

        private void OnIntroFinishedEvent(IntroFinishedEvent introFinishedEvent)
        {
            _manager.ChangeScene(this);
        }
        
        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Pause()
        {
            _browser.GetHost().WasHidden(true);
        }

        public void Resume()
        {
            _browser.GetHost().WasHidden(false);
        }

        public void HandleEvents()
        {
            var menuController = _eventProcessor[0];

            foreach (var button in _buttons)
            {
                var currentState = menuController.GetButtonState(button);
                var transitioning = currentState != _prevControllerState.GetButtonState(button);
                var eventType = currentState ? CefKeyEventType.KeyDown : CefKeyEventType.KeyUp;

                if (!transitioning)
                {
                    if (!currentState) continue;
                    
                    _buttonRepeatCounter[(int) button] = (_buttonRepeatCounter[(int) button] + 1) % RepeatFrameLimit;

                    if (_buttonRepeatCounter[(int) button] != 0) continue;
                    
                    var keyEvent = new CefKeyEvent();
                    keyEvent.EventType = eventType;
                    keyEvent.WindowsKeyCode = (short) _eventProcessor.GetVirtualKey(button);
                    _browser.GetHost().SendKeyEvent(keyEvent);
                }
                else
                {
                    _buttonRepeatCounter[(int) button] = 0;
                    var keyEvent = new CefKeyEvent();
                    keyEvent.EventType = eventType;
                    keyEvent.WindowsKeyCode = (short) _eventProcessor.GetVirtualKey(button);
                    _browser.GetHost().SendKeyEvent(keyEvent);                    
                }
            }
            
            _prevControllerState.CopyStateFrom(menuController);
        }

        public void Update()
        {
            CefRuntime.DoMessageLoopWork();
        }

        public void Draw()
        {
            _browserClient.Draw();
        }
    }
}