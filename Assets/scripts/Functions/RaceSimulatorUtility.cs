using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class RaceSimulatorUtility
{
    public static float CalculateLapTime(CarSimulationState car, float trackFactor, float circuitLength)
    {
        float lapWear = TireDatabase.TireParameters[car.currentTire]["WearMod"] *
                        (1 + Mathf.Pow(circuitLength / 5300f, 4)) *
                        Mathf.Pow(car.totalLapTyre + 1, TireDatabase.TireParameters[car.currentTire]["WearProg"]);
        car.totalWear += lapWear;

        float tireFactor = ((car.totalLapTyre + 1) * TireDatabase.TireParameters[car.currentTire]["BaseWear"]) +
                         TireDatabase.TireParameters[car.currentTire]["WearCoef"] * trackFactor * car.totalLapTyre * (car.totalLapTyre + 1) /
                         TireDatabase.TireParameters[car.currentTire]["TireCoef"] +
                         TireDatabase.TireParameters[car.currentTire]["StartDelta"];

        float lapTime = car.carFactor + tireFactor + UnityEngine.Random.Range(-0.5f, 0.5f);
        car.totalTime += lapTime;

        return lapTime;
    }

    public static bool SimulateLap(CarSimulationState car, float trackFactor, float circuitLength, int totalLaps, int carIndex, Action<string> logPitStop)
    {
        if (car.lapsCompleted >= totalLaps)
            return false;

        float lapTime = CalculateLapTime(car, trackFactor, circuitLength);

        if (car.totalWear >= 100f && car.lapsCompleted < totalLaps - 1)
        {
            logPitStop?.Invoke($"Car {carIndex + 1} ({car.driver}) is pitting for tire change at lap {car.lapsCompleted + 1}!");

            if (car.currentTire == "Soft") car.currentTire = "Medium";
            else if (car.currentTire == "Medium") car.currentTire = "Soft";
            else car.currentTire = "Soft";

            car.totalWear = 0f;
            car.totalLapTyre = -1;
            car.totalTime += 30f;
        }

        car.totalLapTyre++;
        car.lapsCompleted++;
        return true;
    }

    public static List<CarSimulationState> CreateInitialGrid(
        List<Driver> drivers,
        List<Team> teams,
        string startingTire,
        Track trackData)
    {
        List<CarSimulationState> cars = new();
        for (int i = 0; i < drivers.Count; i++)
        {
            var driver = drivers[i];
            var team = teams.Find(t => t.id == driver.teamId);
            float carFactor = PerformanceCalculator.CalculateCarFactor(driver, team, trackData);
            cars.Add(new CarSimulationState(startingTire, carFactor, driver.lastName, team.teamName));
        }
        return cars;
    }

    public static float CalculateTrackFactor(float circuitLength)
    {
        return (1 + (circuitLength / 5300f)) / 10f;
    }
}
