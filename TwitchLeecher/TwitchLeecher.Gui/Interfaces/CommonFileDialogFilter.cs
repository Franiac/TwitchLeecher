namespace TwitchLeecher.Gui.Interfaces;

public class CommonFileDialogFilter
{
    public string Extension { get; }
    public string Name { get; }

    public CommonFileDialogFilter(string extension, string name)
    {
        Extension = extension;
        Name = name;
    }
}