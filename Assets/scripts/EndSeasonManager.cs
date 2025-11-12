using UnityEngine;
using System.Collections.Generic;
using System.IO;
using TMPro;
using System.Linq;

public class EndSeasonManager : MonoBehaviour
{
    [Header("UI Canvas")]
    public GameObject PlayerPanel;
    public GameObject StandingsPanel;
    public GameObject CalendarPanel;
    public GameObject DriversPanel;
    public GameObject TeamsPanel;


    [Header("UI Texts")]
    public TMP_Text driversChangeText;
    public TMP_Text teamsChangeText;
    public TMP_Text retiredDriversText;

    private EndSeasonTextList endSeasonTextList;
    private DriversList driversList;
    private DriversList outOfContractDriver = new();
    private DriversList toRetireDriver = new();
    private List<TeamSeasonChange> teamChanges = new();
    private TeamsList teamsList;
    private GameObject[] allPanels;


    private void Awake()
    {
        LoadUtility.LoadGame("Marcelo_FaIcXA");
        LoadDatabases();
        allPanels = new GameObject[] {
            PlayerPanel,
            StandingsPanel,
            CalendarPanel,
            DriversPanel,
            TeamsPanel
        };
    }
    private void Start()
    {
        UpdateDrivers();
        ApplyTeamsProgressionRegression();
    }

    public void GoToPanel(GameObject targetPanel)
    {
        foreach (var panel in allPanels)
            panel.SetActive(panel == targetPanel);
    }

    private void UpdateDrivers()
    {
        var toMove = new List<Driver>();
        var toRetire = new List<Driver>();
        foreach (Driver driver in driversList.drivers)
        {
            driver.yearsOfContract--;
            driver.age++;

            if (driver.id == SaveSession.CurrentGameData.profile.id) continue;

            ApplyProgressionRegression(driver);

            if (driver.yearsOfContract <= 0)
            {
                if (driver.age > 40)
                {
                    toRetire.Add(driver);
                    continue;
                }
                if (driver.age > 36 && Random.Range(0, 2) == 0)
                {
                    toRetire.Add(driver);
                    continue;
                }
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
                toMove.Add(driver);
            }
        }

        foreach (var driver in toRetire)
        {
            toRetireDriver.drivers.Add(driver);
            MoveToRetired(driver, driversList);
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

        // Retire inactive drivers
        var toRetire = new List<Driver>();
        foreach (var driver in inactiveList.drivers)
        {
            driver.age++;
            if (driver.age > 40)
            {
                toRetire.Add(driver);
                continue;
            }
            if (driver.age > 36 && Random.Range(0, 2) == 0)
            {
                toRetire.Add(driver);
                continue;
            }
        }
        foreach (var driver in toRetire)
        {
            toRetireDriver.drivers.Add(driver);
            MoveToRetired(driver, inactiveList);
        }

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

    private void MoveToInactive(Driver driver, DriversList activeList, DriversList inactiveList)
    {
        // Guardar nome da equipe antiga
        var oldTeam = teamsList.teams.FirstOrDefault(t => t.id == driver.lastTeamId);
        string oldTeamName = oldTeam != null ? oldTeam.teamName : $"Time ({driver.lastTeamId})";

        // Mensagem de saída sem equipe
        driversChangeText.text += $"{driver.firstName} deixou a {oldTeamName} e agora está sem equipe.\n";

        // Atualiza status do piloto
        driver.teamId = 0;
        driver.role = 3;
        driver.active = false;

        activeList.drivers.RemoveAll(d => d.id == driver.id);

        if (!inactiveList.drivers.Any(d => d.id == driver.id))
            inactiveList.drivers.Add(driver);
    }

    private void MoveToRetired(Driver driver, DriversList activeList)
    {
        string saveFolder = Path.Combine(Application.persistentDataPath, "saves", SaveSession.CurrentSaveId);
        string retiredPath = Path.Combine(saveFolder, "retiredDrivers.json");
        DriversList retiredList;

        if (File.Exists(retiredPath))
            retiredList = JsonUtility.FromJson<DriversList>(File.ReadAllText(retiredPath));
        else
            retiredList = new DriversList { drivers = new List<Driver>() };

        activeList.drivers.RemoveAll(d => d.id == driver.id);

        retiredDriversText.text += $"{driver.firstName} se aposentou aos {driver.age} anos \n";

        if (!retiredList.drivers.Any(d => d.id == driver.id))
            retiredList.drivers.Add(driver);

        File.WriteAllText(
            retiredPath,
            JsonUtility.ToJson(retiredList, true)
        );
    }

    private void ApplyProgressionRegression(Driver driver)
    {
        int baseChange;

        // Define tendência da variação dependendo da performance recente
        if (driver.recentResults >= 75)
            baseChange = Random.Range(-1, 3);   // Melhor fase
        else if (driver.recentResults >= 50)
            baseChange = Random.Range(-2, 2);  // Oscilando
        else
            baseChange = Random.Range(-3, 1);  // Má fase

        // Agora aplicamos pequenas variações para cada atributo individualmente
        driver.acceleration = ApplyStatChange(driver.acceleration, baseChange);
        driver.topSpeed = ApplyStatChange(driver.topSpeed, baseChange);
        driver.highSpeedCorners = ApplyStatChange(driver.highSpeedCorners, baseChange);
        driver.lowSpeedCorners = ApplyStatChange(driver.lowSpeedCorners, baseChange);
    }

    private int ApplyStatChange(int stat, int baseChange)
    {
        // Pequena variação aleatória ao redor da tendência
        int variation = Random.Range(-2, 2);

        // Soma tendência + micro variação
        int finalChange = baseChange + variation;

        // Arredonda para int
        stat += finalChange;

        // Limita entre 0 e 90
        stat = Mathf.Clamp(stat, 70, 94);

        return stat;
    }

    private void ApplyTeamsProgressionRegression()
    {
        teamChanges.Clear();
        string pathTeams = Path.Combine(
            Application.persistentDataPath,
            "saves",
            SaveSession.CurrentSaveId,
            SaveSession.CurrentGameData.currentSeason.ToString(),
            "championship_teamsStandings.json"
        );

        if (!File.Exists(pathTeams))
        {
            Debug.LogWarning("Arquivo de standings não encontrado: " + pathTeams);
            return;
        }

        string jsonTeams = File.ReadAllText(pathTeams);
        TeamsChampionshipStatus teamsChampionshipStatus = JsonUtility.FromJson<TeamsChampionshipStatus>(jsonTeams);

        teamsChampionshipStatus.teamStandings.Sort((a, b) => b.points.CompareTo(a.points));
        int totalTeams = teamsList.teams.Count;

        foreach (var team in teamsList.teams)
        {
            // Procura posição do time na tabela anterior
            int position = teamsChampionshipStatus.teamStandings.FindIndex(t => t.teamId == team.id) + 1;
            if (position == 0) position = totalTeams; // fallback caso não encontre

            // 🔹 Normaliza posição entre 0 (campeã) e 1 (último)
            float posNormalized = (float)(position - 1) / (totalTeams - 1);

            // 🔹 Campeã: −8 a +3  | Último: −3 a +8
            float minDelta = Mathf.Lerp(-8f, -3f, posNormalized);
            float maxDelta = Mathf.Lerp(3f, 8f, posNormalized);

            // 🔹 Sorteia a variação de performance
            float delta = Random.Range(minDelta, maxDelta);

            // Aplica variação nos atributos principais
            ApplyTeamChange(team, delta);

            // Armazena e loga
            teamChanges.Add(new TeamSeasonChange(team.teamName, delta));
            Debug.Log($"{team.teamName} ({position}º) mudou {delta:F1} pontos");
        }
        string path = Path.Combine(
            Application.persistentDataPath,
            "saves",
            SaveSession.CurrentSaveId,
            "teamsList.json"
        );

        File.WriteAllText(path, JsonUtility.ToJson(teamsList, true));
        DisplayTeamChanges();
    }

    private void ApplyTeamChange(Team team, float delta)
    {
        // Aplica a variação a cada atributo, com leve aleatoriedade individual
        team.acceleration = Mathf.Clamp(team.acceleration + CalculateChange(team, delta), 70, 94);
        team.topSpeed = Mathf.Clamp(team.topSpeed + CalculateChange(team, delta), 70, 94);
        team.highSpeedCorners = Mathf.Clamp(team.highSpeedCorners + CalculateChange(team, delta), 70, 94);
        team.lowSpeedCorners = Mathf.Clamp(team.lowSpeedCorners + CalculateChange(team, delta), 70, 94);
    }

    private int CalculateChange(Team team, float delta)
    {
        int deltaChange = Mathf.RoundToInt(delta + Random.Range(-2f, 2f));
        Debug.Log($"O time {team.teamName} mudou em {deltaChange}");
        return deltaChange;
    }

    private void DisplayTeamChanges()
    {
        teamsChangeText.text += "\n\nResultados da pré-temporada:\n";

        foreach (var t in teamChanges)
        {
            if (t.averageChange > 4f)
                teamsChangeText.text += $"• A {t.teamName} vem com um carro **muito mais forte** nesta temporada!\n";
            else if (t.averageChange > 1f)
                teamsChangeText.text += $"• A {t.teamName} vem com um carro mais forte nesta temporada!\n";
            else if (t.averageChange >= -1f && t.averageChange <= 1f)
                teamsChangeText.text += $"• A {t.teamName} manteve um desempenho similar ao do ano passado.\n";
            else if (t.averageChange >= -4f)
                teamsChangeText.text += $"• A {t.teamName} parece ter **piorado um pouco** no desenvolvimento do carro.\n";
            else
                teamsChangeText.text += $"• A {t.teamName} teve **grandes dificuldades** e regrediu bastante neste ano.\n";
        }
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

        GameDataCache.driversList = driversList;
        GameDataCache.teamsList = teamsList;
    }


}