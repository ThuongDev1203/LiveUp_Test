using System.Collections.Generic;
using UnityEngine;

public static class PatternGenerator
{
    static int lastLane = -1;

    public static List<int> ZigZag()
    {
        return new List<int> { 0, 1, 2, 3, 2, 1 };
    }

    public static List<int> Repeat()
    {
        return new List<int> { 1, 1, 2, 2, 3, 3 };
    }

    public static List<int> Jump()
    {
        return new List<int> { 0, 3, 1, 2 };
    }

    public static List<int> RandomPattern()
    {
        int r = Random.Range(0, 3);

        if (r == 0) return ZigZag();
        if (r == 1) return Repeat();
        return Jump();
    }
}