namespace XIVChatTools.Models;

class CommandDetails
{
    public required string Command { get; set; }
    public required string Description { get; set; }
    public bool ShowInHelp = true;
}
