namespace RetroLite.Scene
{
    public interface IScene
    {
        void Init(SceneManager manager);
        void Cleanup();

        void Pause();
        void Resume();

        void HandleEvents();
        void Update();
        void Draw();
    }
}
