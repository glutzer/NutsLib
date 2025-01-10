using System;
using Vintagestory.API.Common;

namespace MareLib;

[GameSystem]
public class TickManager : GameSystem
{
    /// <summary>
    /// 20 TPS tick.
    /// </summary>
    public event Action? OnTick;

    /// <summary>
    /// 10 TPS tick.
    /// </summary>
    public event Action? OnOtherTick;

    /// <summary>
    /// 1 TPS tick.
    /// </summary>
    public event Action? OnSecond;

    public int CurrentTick { get; private set; }
    private float delta;
    private readonly long listenerId;

    public TickManager(bool isServer, ICoreAPI api) : base(isServer, api)
    {
        listenerId = api.Event.RegisterGameTickListener(OnGameTick, 20);
    }

    private void OnGameTick(float dt)
    {
        delta += dt;

        while (delta > 0.05f)
        {
            delta -= 0.05f;
            CurrentTick++;

            OnTick?.Invoke();
            if (CurrentTick % 2 == 0) OnOtherTick?.Invoke();
            if (CurrentTick % 20 == 0) OnSecond?.Invoke();
        }
    }

    public override void OnClose()
    {
        api.Event.UnregisterGameTickListener(listenerId);
    }
}
