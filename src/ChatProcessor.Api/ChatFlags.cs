namespace ChatProcessor.API;

[Flags]
public enum ChatFlags
{
    None = 0,
    Team = (1 << 0),
    Dead = (1 << 1)
}
