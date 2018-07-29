using System;
using System.Runtime.InteropServices;
using LibRetro.Types;

namespace RetroLite.RetroCore
{
    public unsafe partial class RetroCore
    {
        private bool _environment(RetroEnvironmentCommand cmd, void* data)
        {
            if (cmd != RetroEnvironmentCommand.GetVariableUpdate)
            {
                Logger.Debug("Core calling command: {0}", cmd);
            }

            switch (cmd)
            {
                case RetroEnvironmentCommand.SetRotation:
                    return _setRotation((uint*)data);
                case RetroEnvironmentCommand.GetOverscan:
                    return _getOverscan((bool*)data);
                case RetroEnvironmentCommand.GetCanDupe:
                    return _getCanDupe((bool*)data);
                case RetroEnvironmentCommand.SetMessage:
                    return _setMessage((RetroMessage*)data);
                case RetroEnvironmentCommand.Shutdown:
                    return _shutdown();
                case RetroEnvironmentCommand.SetPerformanceLevel:
                    return _setPerformanceLevel((uint*)data);
                case RetroEnvironmentCommand.GetSystemDirectory:
                    return _getSystemDirectory(new IntPtr(data));
                case RetroEnvironmentCommand.SetPixelFormat:
                    return _setPixelFormat((RetroPixelFormat*)data);
                case RetroEnvironmentCommand.SetInputDescriptors:
                    return _setInputDescriptors((RetroInputDescriptor*)data);
                case RetroEnvironmentCommand.SetKeyboardCallback:
                    return _setKeyboardCallback((RetroKeyboardCallback*)data);
                case RetroEnvironmentCommand.SetDiskControlInterface:
                    return _setDiskControlInterface((RetroDiskControlCallback*)data);
                case RetroEnvironmentCommand.SetHwRender:
                    return _setHwRender((RetroHwRenderCallback*)data);
                case RetroEnvironmentCommand.GetVariable:
                    return _getVariable((RetroVariable*)data);
                case RetroEnvironmentCommand.SetVariables:
                    return _setVariables((RetroVariable*)data);
                case RetroEnvironmentCommand.GetVariableUpdate:
                    return _getVariableUpdate((bool*)data);
                case RetroEnvironmentCommand.SetSupportNoGame:
                    return _setSupportNoGame((bool*)data);
                case RetroEnvironmentCommand.GetLibretroPath:
                    return _getLibretroPath(new IntPtr(data));
                case RetroEnvironmentCommand.SetAudioCallback:
                    return true;
                case RetroEnvironmentCommand.SetFrameTimeCallback:
                    return _setFrameTimeCallback((RetroFrameTimeCallback*)data);
                case RetroEnvironmentCommand.GetRumbleInterface:
                    return true;
                case RetroEnvironmentCommand.GetInputDeviceCapabilities:
                    return _getInputDeviceCapabilities((long*)data);
                case RetroEnvironmentCommand.GetSensorInterface:
                    return true;
                case RetroEnvironmentCommand.GetCameraInterface:
                    return true;
                case RetroEnvironmentCommand.GetPerfInterface:
                    return _getPerfInterface((RetroPerfCallback*)data);
                case RetroEnvironmentCommand.GetLocationInterface:
                    return true;
                case RetroEnvironmentCommand.GetCoreAssetsDirectory:
                    return _getCoreAssetsDirectory(new IntPtr(data));
                case RetroEnvironmentCommand.GetSaveDirectory:
                    return _getSaveDirectory(new IntPtr(data));
                case RetroEnvironmentCommand.SetSystemAvInfo:
                    return _setSystemAvInfo((RetroSystemAvInfo*)data);
                case RetroEnvironmentCommand.SetProcAddressCallback:
                    return _setProcAddressCallback((RetroGetProcAddressInterface*)data);
                case RetroEnvironmentCommand.SetSubsystemInfo:
                    return _setSubsystemInfo((RetroSubsystemInfo*)data);
                case RetroEnvironmentCommand.SetControllerInfo:
                    return _setControllerInfo((RetroControllerInfo*)data);
                case RetroEnvironmentCommand.SetMemoryMaps:
                    return true;
                case RetroEnvironmentCommand.SetGeometry:
                    return _setGeometry((RetroGameGeometry*)data);
                case RetroEnvironmentCommand.GetUsername:
                    return _getUsername(new IntPtr(data));
                case RetroEnvironmentCommand.GetLanguage:
                    return _getLanguage((RetroLanguage*)data);
                case RetroEnvironmentCommand.GetCurrentSoftwareFramebuffer:
                    return _getCurrentSoftwareFramebuffer((RetroFramebuffer*)data);
                case RetroEnvironmentCommand.SetHwSharedContext:
                    return _setHwSharedContext();
                case RetroEnvironmentCommand.GetVfsInterface:
                    return _getVfsInterface((RetroVfsInterfaceInfo*)data);
                default:
                    return false;
            }
        }

        private bool _setRotation(uint* rotation)
        {
            return true;
        }

        private bool _getOverscan(bool* useOverscan)
        {
            *useOverscan = true;

            return true;
        }

        private bool _getCanDupe(bool* canDupe)
        {
            *canDupe = true;

            return true;
        }

        private bool _setMessage(RetroMessage* message)
        {
            RetroCore.Logger.Info("Message: " + message->Message);

            return true;
        }

        private bool _shutdown()
        {
            Program.Running = false;

            return true;
        }

        private bool _setPerformanceLevel(uint* performanceLevel)
        {
            RetroCore.Logger.Debug("Setting performance level to: {0}", *performanceLevel);

            return true;
        }

        private bool _getSystemDirectory(IntPtr pathPtr)
        {
            *((void**)pathPtr.ToPointer()) = Marshal.StringToHGlobalAnsi(_systemDirectory).ToPointer();

            return true;
        }

        private bool _setPixelFormat(RetroPixelFormat* pixelFormat)
        {
            if (_pixelFormat != *pixelFormat)
            {
                _pixelFormat = *pixelFormat;
                _videoContextUpdated = true;
            }
            return true;
        }

        private bool _setInputDescriptors(RetroInputDescriptor* inputDesc)
        {
            for (var i = 0; null != inputDesc[i].Description; i++)
            {
                var currInputDesc = inputDesc[i];
                _inputDescriptors.Add(new InputDescriptor(
                    currInputDesc.Port,
                    currInputDesc.Device,
                    currInputDesc.Index,
                    currInputDesc.Id,
                    currInputDesc.Description
                ));
            }
            
            return true;
        }

        private bool _setKeyboardCallback(RetroKeyboardCallback* callback)
        {
            callback->Callback = _keyboardEvent;
            return true;
        }

        private bool _setDiskControlInterface(RetroDiskControlCallback* callback)
        {
            callback->AddImageIndex = _addImageIndex;
            callback->GetEjectState = _getEjectState;
            callback->GetImageIndex = _getImageIndex;
            callback->GetNumImages = _getNumImages;
            callback->ReplaceImageIndex = _replaceImageIndex;
            callback->SetEjectState = _setEjectState;
            callback->SetImageIndex = _setImageIndex;

            return true;
        }

        private bool _setHwRender(RetroHwRenderCallback* callback)
        {
            return false;
        }

        private bool _getVariable(RetroVariable* variablePtr)
        {
            Logger.Debug($"Core asking for variable {(*variablePtr).Key}");

            if (!_coreVariables.ContainsKey((*variablePtr).Key))
            {
                RetroCore.Logger.Warn("Variable not set: {0}", (*variablePtr).Key);
                return false;
            }

//            (*variablePtr).value = IntPtr.Zero;
//
//            return true;

            return false;
        }

        private bool _setVariables(RetroVariable* variables)
        {
            for (var i = 0; null != variables[i].Key; i++)
            {
                var currVariable = variables[i];
                var coreVariable = new CoreVariable(currVariable);
                _coreVariables.Add(coreVariable.Name, coreVariable);
                Logger.Debug<string, string>("{1} -> Adding variable: {0}", currVariable.Key, _coreName);
            }

            return true;
        }

        private bool _getVariableUpdate(bool* updated)
        {
            *updated = false;
            return true;
        }

        private bool _setSupportNoGame(bool* noGameSupported)
        {
            // TODO: Do something about this
            return true;
        }

        private bool _getLibretroPath(IntPtr pathPtr)
        {
            *((void**)pathPtr.ToPointer()) = Marshal.StringToHGlobalAnsi(_systemDirectory).ToPointer();

            return true;
        }

        private bool _setFrameTimeCallback(RetroFrameTimeCallback* callback)
        {
            _prevFrameTimeCallback = _currentFrameTimeCallback;
            _currentFrameTimeCallback = Marshal.PtrToStructure<RetroFrameTimeCallback>(new IntPtr(callback));

            return true;
        }

        private bool _getInputDeviceCapabilities(long* caps)
        {
            return true;
        }

        private bool _getPerfInterface(RetroPerfCallback* iface)
        {
            iface->GetTimeUsec = _getTimeUsec;
            iface->GetCpuFeatures = _getCpuFeatures;
            iface->PerfGetCounter = _perfGetCounter;
            iface->PerfRegister = _perfRegister;
            iface->PerfStart = _perfStart;
            iface->PerfStop = _perfStop;
            iface->PerfLog = _perfLog;

            return true;
        }

        private bool _getCoreAssetsDirectory(IntPtr pathPtr)
        {
            *((void**)pathPtr.ToPointer()) = Marshal.StringToHGlobalAnsi(_systemDirectory).ToPointer();

            return true;
        }

        private bool _getSaveDirectory(IntPtr pathPtr)
        {
            *((void**)pathPtr.ToPointer()) = Marshal.StringToHGlobalAnsi(_systemDirectory).ToPointer();

            return true;
        }

        private bool _setSystemAvInfo(RetroSystemAvInfo* info)
        {
            Marshal.PtrToStructure<RetroSystemAvInfo>(new IntPtr(info));

            return true;
        }

        private bool _setProcAddressCallback(RetroGetProcAddressInterface* iface)
        {
            return true;
        }

        private bool _setSubsystemInfo(RetroSubsystemInfo* info)
        {
            return true;
        }

        private bool _setControllerInfo(RetroControllerInfo* info)
        {
            return true;
        }

        private bool _setGeometry(RetroGameGeometry* geometry)
        {
            if (!_currentSystemAvInfo.Geometry.Equals(*geometry))
            {
                _currentSystemAvInfo.Geometry = *geometry;
                _videoContextUpdated = true;
            }

            return true;
        }

        private bool _getUsername(IntPtr usernamePtr)
        {
            *((void**)usernamePtr.ToPointer()) = Marshal.StringToHGlobalAnsi("RetroLite").ToPointer();

            return true;
        }

        private bool _getLanguage(RetroLanguage* language)
        {
            *language = RetroLanguage.English;

            return true;
        }

        private bool _getCurrentSoftwareFramebuffer(RetroFramebuffer* framebuffer)
        {
            return false;
        }

        private bool _setHwSharedContext()
        {
            return false;
        }

        private bool _getVfsInterface(RetroVfsInterfaceInfo* info)
        {
            return false;
        }
    }
}
