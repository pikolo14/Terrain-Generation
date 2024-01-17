using System.Collections;
using System.Collections.Generic;
using System;

public static class RandomExtension
{
    public static double NextDouble(this Random random, double min, double max)
    {
        double range = max - min;
        double sample = random.NextDouble();
        return (sample * range) + min;
    }

    public static float NextFloat(this Random random, float min, float max)
    {
        return (float)random.NextDouble(min, max);
    }

    public static float NextFloat(this Random random)
    {
        return (float)random.NextDouble();
    }
}
