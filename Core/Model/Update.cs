namespace PtzJoystickControl.Core.Model;

public record Update
{
    public bool Available { get; set; }
    public string? Version { get; set; }
    public Uri? Uri { get; set; }
    public bool Error { get; set; }
    public string? ErrorMessage { get; set; }
}
