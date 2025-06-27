using System;

public class RandomNumberGenerator
{
    private static readonly Random _random = new Random();
    
    public static int GetRandomBetween(int min, int max)
    {
        // Swap values if min is greater than max
        if (min > max)
        {
            int temp = min;
            min = max;
            max = temp;
        }
        
        return _random.Next(min, max + 1);
    }
}