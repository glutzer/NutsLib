using System;
using System.Collections.Generic;
using Vintagestory.API.Common;

namespace NutsLib;

[GameSystem]
public class TickSystem : GameSystem
{
    private readonly Dictionary<long, Action<int>> registeredTickers = [];
    private long currentListenerId = 0;
    private long registeredListenerId = -1;
    private float accum;
    private const float TICK_RATE = 1 / 20f;
    private int currentTick;

    public static TickSystem? Client { get; private set; }
    public static TickSystem? Server { get; private set; }

    public TickSystem(bool isServer, ICoreAPI api) : base(isServer, api)
    {
    }

    public override void PreInitialize()
    {
        if (isServer)
        {
            Server = this;
        }
        else
        {
            Client = this;
        }
    }

    /// <summary>
    /// Register a ticker with the tick number delegate.
    /// Returns the listener id.
    /// 20 TPS.
    /// </summary>
    public long RegisterTicker(Action<int> onTick)
    {
        registeredTickers[currentListenerId] = onTick;
        if (registeredTickers.Count == 1) OnFirstTickRegistered();
        return currentListenerId++;
    }

    public void UnregisterTicker(long listenerId)
    {
        if (registeredTickers.Remove(listenerId) && registeredTickers.Count == 0)
        {
            OnLastTickRemoved();
        }
    }

    private void OnTick(float dt)
    {
        accum += dt;

        if (accum > 1f) accum = 1f; // Clamp to 1 second.

        while (accum > TICK_RATE)
        {
            accum -= TICK_RATE;

            foreach (Action<int> ticker in registeredTickers.Values)
            {
                ticker(currentTick);
            }

            currentTick++;
        }
    }

    private void OnFirstTickRegistered()
    {
        if (registeredListenerId != -1) api.Event.UnregisterGameTickListener(registeredListenerId);

        registeredListenerId = api.Event.RegisterGameTickListener(OnTick, 20);
    }

    private void OnLastTickRemoved()
    {
        if (registeredListenerId == -1) return;
        api.Event.UnregisterGameTickListener(registeredListenerId);
        registeredListenerId = -1;
    }

    public override void OnClose()
    {
        Server = null;
        Client = null;
    }
}