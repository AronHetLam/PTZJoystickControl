using PtzJoystickControl.Core.Devices;
using PtzJoystickControl.Core.Model;

namespace PtzJoystickControl.Core.Commands
{
    public enum Direction
    {
        Stop,
        High,
        Low,
    }

    public abstract class IDynamicCommand : ICommand
    {
        private IGamepad gamepad = null!;
        public IDynamicCommand(IGamepad gamepad)
        {
            Gamepad = gamepad;
        }

        public IGamepad Gamepad { get => gamepad; private set => gamepad = value ?? throw new ArgumentNullException("Gamepad cannot be Null!"); }

        public abstract string CommandName { get; }
        public abstract string AxisParameterName { get; }
        public abstract string ButtonParameterName { get; }
        public IEnumerable<CommandValueOption> Options
        {
            get => Enumerable.Range(MinValue, Math.Abs(MinValue - MaxValue) + 1)
                .Select(i => new CommandValueOption(i.ToString(), i));
        }

        public abstract int MaxValue { get; }
        public abstract int MinValue { get; }
        public abstract IEnumerable<CommandDirectionOption> ButtonDirections { get; }

        public void Execute(int value)
        {
            Direction direction;

            if (value < 0)
            {
                direction = Direction.Low;
                value = Math.Abs(value);
            }
            else if (value > 0)
                direction = Direction.High;
            else
                direction = Direction.Stop;

            Execute(value, direction);
        }

        public void Execute(CommandValueOption value)
        {
            Execute(value.Value);
        }

        public abstract void Execute(int value, Direction direction);
        public void Execute(CommandValueOption value, CommandDirectionOption direction)
        {
            if (value != null!)
                Execute(value.Value, direction.Direction);
        }
    }
}
