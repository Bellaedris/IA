// a float interval

using UnityEngine;

public class Interval
{
    private float min, max;

    public Interval(float min, float max)
    {
        this.min = min;
        this.max = max;
    }

    public bool Contains(float value)
    {
        return value >= min && value <= max;
    }

    public float Clamp(float value)
    {
        return Mathf.Clamp(value, min, max);
    }

    public static Interval Empty()
    {
        return new Interval(Mathf.Infinity, Mathf.NegativeInfinity);
    }
    
    public static Interval R()
    {
        return new Interval(Mathf.NegativeInfinity, Mathf.Infinity);
    }
}
