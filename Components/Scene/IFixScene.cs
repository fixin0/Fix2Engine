namespace Fix2Engine.Components.Scene;

public interface IFixScene : IDisposable
{
    void Start();
    void Update(float dt);
    void Render();
    void RenderUI();
    void Unload();
}