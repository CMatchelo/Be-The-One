using System;

[Serializable]
public class GameData
{
    public string playerFirstName;
    public string playerLastName;
    public int teamId;
    public int companionId;
    public PlayerProfile profile;
    public int currentSeason = 2025;
    public string saveId;
    // Outros dados do jogo que você queira salvar
}