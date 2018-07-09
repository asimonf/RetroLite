namespace RetroLite.Scene
{
    public interface IScene
    {
        void Init();
        void Cleanup();

        void Pause();
        void Resume();

        void HandleEvents();
        void Update();
        void Draw();
    }
}
