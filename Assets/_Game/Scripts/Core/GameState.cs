namespace LavaQuest.Core
{
    public enum GameState : byte
    {
        Empty = 0,
        Intro = 1,
        Matchmaking = 2,
        Tutorial = 3,
        Playing = 4,
        RoundComplete = 5,
        Victory = 6,
        Eliminated = 7,
    }
}