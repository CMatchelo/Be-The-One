// TireDatabase.cs
using System.Collections.Generic;

public static class TireDatabase
{
    public static Dictionary<string, Dictionary<string, float>> TireParameters =
        new Dictionary<string, Dictionary<string, float>>
    {
        {
            "Soft", new Dictionary<string, float>
            {
                {"WearCoef", 0.08f},
                {"BaseWear", 0.05f},
                {"TireCoef", 0.8f},
                {"StartDelta", -0.5f},
                {"WearMod", 0.5f},
                {"WearProg", 1.3f}
            }
        },
        {
            "Medium", new Dictionary<string, float>
            {
                {"WearCoef", 0.04f},
                {"BaseWear", 0.08f},
                {"TireCoef", 2f},
                {"StartDelta", 0.0f},
                {"WearMod", 0.4f},
                {"WearProg", 1.1f}
            }
        },
        {
            "Hard", new Dictionary<string, float>
            {
                {"WearCoef", 0.02f},
                {"BaseWear", 0.10f},
                {"TireCoef", 6f},
                {"StartDelta", 1.0f},
                {"WearMod", 0.3f},
                {"WearProg", 0.5f}
            }
        }
    };
}