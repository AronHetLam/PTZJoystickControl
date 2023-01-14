using PtzJoystickControl.Core.Commands;

namespace PtzJoystickControl.Core.Model;

public class CommandDirectionOption
{
    public string Name { get; }
    public Direction Direction { get; }

    public CommandDirectionOption(string name, Direction direction)
    {
        Name = name;
        Direction = direction;
    }

    public static bool operator ==(CommandDirectionOption a, CommandDirectionOption b) => a?.Direction == b?.Direction;
    public static bool operator !=(CommandDirectionOption a, CommandDirectionOption b) => a?.Direction != b?.Direction;

    public override bool Equals(object? obj)
    {
        if (obj is CommandDirectionOption cmv) return this == cmv;
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return Direction.GetHashCode();
    }

}

