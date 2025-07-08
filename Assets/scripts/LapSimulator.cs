/* using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LapSimulator
{
    public static IEnumerator SimulateLap(
        float circuitLength,
        int totalLaps,
        string tireType,
        System.Action<int, float, float> onLapCompleted,
        System.Action<string> onTireChangeNeeded)
    {
        float totalTime = 0f;
        float totalWear = 0f;
        var tireParams = TireDatabase.TireParameters[tireType];
        for (int lap = 1; lap <= totalLaps; lap++)
        {
            // C�lculo do desgaste
            float lapWear = CalculateLapWear(lap, circuitLength, tireParams);
            totalWear += lapWear;

            // C�lculo do tempo
            float lapTime = CalculateLapTime(lap, circuitLength, tireParams);
            totalTime += lapTime;

            // Reportar progresso

            // Verificar troca de pneus
            if (totalWear >= 100f && lap < totalLaps)
            {
                onTireChangeNeeded?.Invoke(tireType);
                yield break; // Sai da corrotina para permitir troca
            }

            yield return null; // Pausa at� o pr�ximo frame
        }
    }

    private static float CalculateLapWear(int lap, float circuitLength, Dictionary<string, float> tireParams)
    {
        return tireParams["WearMod"] * (1 + (circuitLength / 5300f)) *
               Mathf.Pow(lap, tireParams["WearProg"]);
    }

    private static float CalculateLapTime(int lap, float circuitLength, Dictionary<string, float> tireParams)
    {
        float trackFactor = (1 + (circuitLength / 5300f)) / 10f;
        float tireFactor = (lap * tireParams["BaseWear"]) +
                         (tireParams["WearCoef"] * trackFactor * (lap - 1) * lap) /
                         tireParams["TireCoef"] + tireParams["StartDelta"];
        return 107f + tireFactor;
    }
}
 */