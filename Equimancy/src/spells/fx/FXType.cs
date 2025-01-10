using MareLib;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;

namespace Equimancy;

public class FXAttribute : ClassAttribute
{
    public FXAttribute()
    {

    }
}

public interface IFXType
{
    public void OnClosing();
}

/// <summary>
/// Represents a type of renderable effect.
/// Responsible for rendering things like lightning arcs, circles on the ground, fields, and spawning particles around them.
/// Renders a list of T instances.
/// Instances must be a class or they will need to be re-inserted into the dictionary.
/// </summary>
public abstract class FXType<T> : IRenderer, IFXType
{
    public virtual EnumRenderStage[] RenderStages => Array.Empty<EnumRenderStage>();
    public int RenderRange => 0;
    public virtual double RenderOrder => 0.5;

    private readonly Dictionary<long, T> instances = new();
    private readonly HashSet<long> deadInstances = new();
    private bool enabled;
    private int nextInstanceId;

    private readonly DummyRenderer dummyRenderer;

    public FXType()
    {
        dummyRenderer = new DummyRenderer() { action = ClearDeadInstances };
    }

    public virtual void OnRenderFrame(float dt, EnumRenderStage stage)
    {

    }

    public void ForEachInstance(Action<long, T> action)
    {
        foreach (KeyValuePair<long, T> instance in instances)
        {
            action(instance.Key, instance.Value);
        }
    }

    /// <summary>
    /// When atleast one instance is active, enable rendering.
    /// </summary>
    private void EnableRendering()
    {
        enabled = true;

        MainAPI.Client.eventManager.RegisterRenderer(dummyRenderer, EnumRenderStage.Before, "clear");

        foreach (EnumRenderStage stage in RenderStages)
        {
            MainAPI.Capi.Event.RegisterRenderer(this, stage);
        }
    }

    /// <summary>
    /// When the last instance is removed, disable rendering.
    /// </summary>
    private void DisableRendering()
    {
        enabled = false;

        MainAPI.Client.eventManager.UnregisterRenderer(dummyRenderer, EnumRenderStage.Before);

        foreach (EnumRenderStage stage in RenderStages)
        {
            MainAPI.Capi.Event.UnregisterRenderer(this, stage);
        }
    }

    /// <summary>
    /// Spawn an instance of this effect.
    /// Returns id for managing.
    /// </summary>
    public long SpawnInstance(T instance)
    {
        instances.Add(nextInstanceId, instance);
        nextInstanceId++;

        if (instances.Count > 0 && !enabled)
        {
            EnableRendering();
        }

        return nextInstanceId - 1;
    }

    /// <summary>
    /// Returns instance if it's still active.
    /// </summary>
    public T? GetInstance(long id)
    {
        instances.TryGetValue(id, out T? instance);
        return instance;
    }

    /// <summary>
    /// Removes an instance with this id.
    /// FX may remove an instance itself.
    /// </summary>
    public void RemoveInstance(long id)
    {
        instances.Remove(id);
        if (instances.Count == 0 && enabled)
        {
            DisableRendering();
        }
    }

    public void QueueInstanceRemoval(long id)
    {
        deadInstances.Add(id);
    }

    private void ClearDeadInstances(float dt)
    {
        foreach (long id in deadInstances)
        {
            RemoveInstance(id);
        }
        deadInstances.Clear();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public virtual void OnClosing()
    {

    }
}