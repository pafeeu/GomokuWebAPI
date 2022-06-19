namespace GomokuWebAPI.Model.Enums
{
    [Flags]
    public enum PlayerRole : short
    {
        Creator = 1,
        Invited = 2,
        Win = 4,
        Close = 8,

        None = 0,
        CreatorWon = Creator | Win,
        CreatorClosed = Creator | Close,
        InvitedWon = Invited | Win,
        InvitedClosed = Invited | Close,
    }
}
