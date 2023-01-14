namespace PtzJoystickControl.Core.Model;

public class CommandValueOption
{
    public string Name { get; }
    public int Value { get; }

    public CommandValueOption(string name, int value)
    {
        Name = name;
        Value = value;
    }

    public static bool operator ==(CommandValueOption a, CommandValueOption b) => a?.Value == b?.Value;
    public static bool operator !=(CommandValueOption a, CommandValueOption b) => a?.Value != b?.Value;

    public override bool Equals(object? obj)
    {
        if (obj is CommandValueOption cmv) return this == cmv;
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

}

