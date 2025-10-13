using System;

[Serializable]
public class GameData
{
    public string playerFirstName;
    public string playerLastName;
    public int teamId;
    public int companionId;
    public PlayerProfile profile;
    public string saveId;
    // Outros dados do jogo que você queira salvar
}