using Fix2Engine.Graphics;

namespace Fix2Engine.Components;

/// <summary>Draws a model with a shader-driven material as a component of an <see cref="Object3D"/>.</summary>
public sealed class ModelRenderer3D : Component3D
{
    public Model3D? Model { get; private set; }
    public Material3D? Material { get; private set; }
    public bool OwnsModel { get; private set; }
    public bool OwnsMaterial { get; private set; }

    public ModelRenderer3D(Model3D? model = null, Material3D? material = null, bool ownsModel = false, bool ownsMaterial = false)
    {
        SetModel(model, ownsModel);
        SetMaterial(material, ownsMaterial);
    }

    public void SetModel(Model3D? model, bool ownsModel = false)
    {
        if (ReferenceEquals(Model, model)) { OwnsModel = ownsModel; return; }
        if (OwnsModel) Model?.Dispose();
        Model = model;
        OwnsModel = ownsModel;
    }
    public void SetMaterial(Material3D? material, bool ownsMaterial = false)
    {
        if (ReferenceEquals(Material, material)) { OwnsMaterial = ownsMaterial; return; }
        if (OwnsMaterial) Material?.Dispose();
        Material = material;
        OwnsMaterial = ownsMaterial;
    }

    protected override void OnRender3D()
    {
        if (Owner == null || Model?.IsDisposed != false || Material?.IsDisposed != false) return;
        Graphics3D.DrawModel(Model, Material, Owner.GlobalTransform);
    }
    protected override void DisposeResources()
    {
        if (OwnsModel) Model?.Dispose();
        if (OwnsMaterial) Material?.Dispose();
        Model = null;
        Material = null;
    }
}
