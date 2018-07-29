using System;

namespace RetroLite.Scene
{
    public interface IScene: IComparable<IScene>
    {
        int Order { get; }
        
        void HandleEvents();
        void Update();
        void Draw();

        float[] GetAudioData(int frames);
    }
}
