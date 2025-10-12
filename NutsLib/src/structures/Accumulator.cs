using System;

namespace NutsLib;

public struct Accumulator
{
    private float accum;
    public float interval = 1f;
    private float max = float.MaxValue;

    public static Accumulator WithInterval(float interval)
    {
        return new Accumulator(interval);
    }

    public static Accumulator WithRandomInterval(float min, float max)
    {
        Accumulator acc = new();
        acc.SetRandomInterval(min, max);
        acc.max = float.MaxValue;
        return acc;
    }

    private Accumulator(float interval)
    {
        this.interval = interval;
        max = float.MaxValue;
    }

    public bool Progress(float value)
    {
        accum += value;
        accum = Math.Clamp(accum, 0, max);
        if (accum >= interval)
        {
            accum -= interval;
            return true;
        }
        return false;
    }

    public void Add(float value)
    {
        accum += value;
        accum = Math.Clamp(accum, 0, max);
    }

    public Accumulator Max(float value)
    {
        max = value;
        return this;
    }

    public void SetInterval(float value)
    {
        interval = value;
    }

    /// <summary>
    /// For use in a while loop.
    /// </summary>
    public bool Consume()
    {
        if (accum >= interval)
        {
            accum -= interval;
            return true;
        }

        return false;
    }

    public void Reset()
    {
        accum = 0;
    }

    public void SetRandomInterval(float min, float max)
    {
        interval = (Random.Shared.NextSingle() * (max - min)) + min;
    }
}