using System;

namespace XIVChatTools.Models.Configuration;

[Serializable]
public class CharacterWatcher
{
    public string Character { get; set; }
    public string World { get; set; }
    public string Watchers { get; set; }

    public CharacterWatcher(string character, string world, string watchers)
    {
        Character = character;
        World = world;
        Watchers = watchers;
    }
}
