using System;

namespace MareLib;

public struct Accumulator
{
    private float accum;
    private float interval = 1;
    private float max = float.MaxValue;

    public static Accumulator WithInterval(float interval)
    {
        return new Accumulator(interval);
    }

    private Accumulator(float interval)
    {
        this.interval = interval;
        max = float.MaxValue;
    }

    public void Add(float value)
    {
        accum += value;
        accum = Math.Clamp(accum, 0, max);
    }

    public void Max(float value)
    {
        max = value;
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
}