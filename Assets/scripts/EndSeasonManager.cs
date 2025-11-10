using UnityEngine;
using System.Collections.Generic;
using System.IO;
using TMPro;
using System.Linq;

public class EndSeasonManager : MonoBehaviour
{
    [Header("UI Texts")]
    public TMP_Text driversChangeText;

    private EndSeasonTextList endSeasonTextList;
    public DriversList driversList;
    public DriversList outOfContractDriver = new();
    public TeamsList teamsList;

    private void Start()
    {
        LoadUtility.LoadGame("Marcelo_FaIcXA");
        LoadDatabases();
        UpdateDrivers();
    }


    private void UpdateDrivers()
    {
        var toMove = new List<Driver>();
        foreach (Driver driver in driversList.drivers)
        {
            driver.yearsOfContract--;

            if (driver.id == SaveSession.CurrentGameData.profile.id) continue;

            if (driver.yearsOfContract <= 0)
            {
                if (driver.recentResults > 75)
                {
                    driver.yearsOfContract = Random.Range(2, 5);
                    continue;
                }
                if (driver.recentResults > 40 && Random.Range(0, 2) == 0)
                {
                    driver.yearsOfContract = Random.Range(2, 5);
                    continue;
                }

                driver.lastTeamId = driver.teamId;
                toMove.Add(driver); // <-- Apenas marca para mover depois
            }
        }

        foreach (var driver in toMove)
        {
            outOfContractDriver.drivers.Add(driver);
            MoveToInactive(driver, driversList, outOfContractDriver);
        }

        // Salva versão modificada da active drivers
        string path = Path.Combine(Application.persistentDataPath, "saves", SaveSession.CurrentSaveId, "activeDriversList.json");
        File.WriteAllText(path, JsonUtility.ToJson(driversList, true));

        // Agora faz as contratações
        ProcessEndSeasonTransfers();
    }


    private void ProcessEndSeasonTransfers()
    {
        // Carregar inactiveDrivers
        string saveFolder = Path.Combine(Application.persistentDataPath, "saves", SaveSession.CurrentSaveId);
        string inactivePath = Path.Combine(saveFolder, "inactiveDriversList.json");

        DriversList inactiveList;
        if (File.Exists(inactivePath))
            inactiveList = JsonUtility.FromJson<DriversList>(File.ReadAllText(inactivePath));
        else
            inactiveList = new DriversList { drivers = new List<Driver>() };

        // Adiciona os sem contrato aos inativos
        foreach (var d in outOfContractDriver.drivers)
        {
            d.teamId = 0;
            d.role = 3;
            d.active = false;
            if (!inactiveList.drivers.Any(x => x.id == d.id))
                inactiveList.drivers.Add(d);
        }

        // Agora vamos recontratar
        foreach (var team in teamsList.teams)
        {
            var teamDrivers = driversList.drivers.Where(x => x.teamId == team.id).ToList();
            var role0 = teamDrivers.FirstOrDefault(x => x.role == 0);
            var role1 = teamDrivers.FirstOrDefault(x => x.role == 1);

            // Falta piloto titular?
            if (role0 == null)
                HireBestAvailable(team, 0, inactiveList);

            // Falta segundo piloto?
            if (role1 == null)
                HireBestAvailable(team, 1, inactiveList);
        }

        // Salvar listas atualizadas
        File.WriteAllText(
            Path.Combine(saveFolder, "activeDriversList.json"),
            JsonUtility.ToJson(driversList, true)
        );

        File.WriteAllText(
            inactivePath,
            JsonUtility.ToJson(inactiveList, true)
        );
    }

    private void HireBestAvailable(Team team, int roleNeeded, DriversList inactiveList)
    {
        // pega o melhor piloto disponível que NÃO acabou de sair deste mesmo time
        var bestAvailable = inactiveList.drivers
            .Where(d => d.lastTeamId != team.id) // evita quem saiu desse time agora
            .OrderByDescending(d => d.Average)
            .FirstOrDefault();

        if (bestAvailable == null)
        {
            driversChangeText.text += $"Nenhum piloto disponível para contratar para {team.teamName}\n";
            return;
        }

        // Mensagem dependendo do estado anterior do piloto
        if (bestAvailable.lastTeamId != 0)
        {
            // tenta encontrar o nome do time antigo
            var oldTeam = teamsList.teams.FirstOrDefault(t => t.id == bestAvailable.lastTeamId);
            string oldTeamName = oldTeam != null ? oldTeam.teamName : $"Time ({bestAvailable.lastTeamId})";
            driversChangeText.text += $"{bestAvailable.firstName} saiu da {oldTeamName} para {team.teamName}\n";
        }
        else
        {
            // estava sem time
            driversChangeText.text += $"{bestAvailable.firstName} que estava sem time agora faz parte da {team.teamName}\n";
        }

        // Atualizar dados do piloto
        bestAvailable.teamId = team.id;
        bestAvailable.role = roleNeeded;
        bestAvailable.active = true;
        bestAvailable.yearsOfContract = Random.Range(2, 5);

        // opcional: zerar lastTeamId porque agora está no time
        bestAvailable.lastTeamId = 0;

        // Remove dos inativos e adiciona aos ativos sem duplicatas
        inactiveList.drivers.RemoveAll(d => d.id == bestAvailable.id);

        // Evita duplicata caso já exista (por precaução)
        if (!driversList.drivers.Any(d => d.id == bestAvailable.id))
            driversList.drivers.Add(bestAvailable);
    }


    private static void MoveToInactive(Driver driver, DriversList activeList, DriversList inactiveList)
    {
        driver.teamId = 0;
        driver.role = 3;
        driver.active = false;

        activeList.drivers.RemoveAll(d => d.id == driver.id);

        if (!inactiveList.drivers.Any(d => d.id == driver.id))
            inactiveList.drivers.Add(driver);
    }

    private void LoadDatabases()
    {
        TextAsset endSeasonTextLocal = Resources.Load<TextAsset>("EndSeasonTexts");
        endSeasonTextList = JsonUtility.FromJson<EndSeasonTextList>(endSeasonTextLocal.text);

        string pathDrivers = Path.Combine(
            Application.persistentDataPath,
            "saves",
            SaveSession.CurrentSaveId,
            "activeDriversList.json"
        );
        string driversLocal = File.ReadAllText(pathDrivers);
        driversList = JsonUtility.FromJson<DriversList>(driversLocal);

        string pathTeams = Path.Combine(
            Application.persistentDataPath,
            "saves",
            SaveSession.CurrentSaveId,
            "teamsList.json"
        );
        string teamsLocal = File.ReadAllText(pathTeams);
        teamsList = JsonUtility.FromJson<TeamsList>(teamsLocal);
    }


}