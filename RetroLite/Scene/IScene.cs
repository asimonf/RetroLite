using System;

namespace RetroLite.Scene
{
    public interface IScene
    {
        void Start();
        void Resume();
        void Pause();
        void Stop();

        void HandleEvents();
        void Update();
        void Draw();

        void GetAudioData(IntPtr buffer, int frames);
    }
}
