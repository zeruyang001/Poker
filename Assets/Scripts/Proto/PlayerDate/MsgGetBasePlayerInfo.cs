public class MsgGetBasePlayerInfo : MsgBase
{
    public MsgGetBasePlayerInfo()
    {
        protoName = "MsgGetBasePlayerInfo";
    }

    public string playerId;
    public string playerName;
    public string avatarUrl;
    public int score;
    public int beans;
}