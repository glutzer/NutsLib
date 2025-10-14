using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace NutsLib;

[GameSystem(forSide = EnumAppSide.Client)]
public class TransitionManager : GameSystem, IRenderer
{
    private readonly Queue<IWidgetTransition> transitions = [];

    public static TransitionManager Instance { get; private set; } = null!;

    public double RenderOrder => -500;
    public int RenderRange => 0;

    public TransitionManager(bool isServer, ICoreAPI api) : base(isServer, api)
    {
    }

    public void RegisterTransition(IWidgetTransition transition)
    {
        transitions.Enqueue(transition);
    }

    /// <summary>
    /// Update all transitions in the before stage.
    /// </summary>
    private void UpdateTransitions(float dt)
    {
        int transitionCount = transitions.Count;
        for (int i = 0; i < transitionCount; i++)
        {
            IWidgetTransition transition = transitions.Dequeue();
            transition.UpdateTransition(dt);
            if (!transition.IsComplete())
            {
                transitions.Enqueue(transition);
                continue;
            }
            transition.OnComplete?.Invoke();
        }
    }

    public override void PostInitialize()
    {
        Instance = this;
        MainAPI.Capi.Event.RegisterRenderer(this, EnumRenderStage.Ortho);
    }

    public override void OnClose()
    {
        Instance = null!;
        MainAPI.Capi.Event.UnregisterRenderer(this, EnumRenderStage.Ortho);
    }

    public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
    {
        UpdateTransitions(deltaTime);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}