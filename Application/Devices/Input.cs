using PtzJoystickControl.Core.Commands;
using PtzJoystickControl.Core.Devices;
using PtzJoystickControl.Application.Commands;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PtzJoystickControl.Core.Model;
using System.Collections.Specialized;

namespace PtzJoystickControl.Application.Devices;

public class Input : IInput
{
    public string Name { get; private set; }
    public InputType InputType { get; private set; }
    public string Id { get; private set; }

    private float inputValue;
    private float rawInputValue;
    private ICommand? command;
    private CommandValueOption? commandValue;
    private CommandDirectionOption? commandDirection;
    private Direction currentDirection;
    private readonly IInput? secondInput;
    private int currentValue;
    private bool buttonPressed { get; set; }
    private float deadZone { get; set; } = 0.05F;
    private float saturation { get; set; } = 0.05F;
    private bool inverted { get; set; }
    private bool defaultCenter { get; set; } = true;

    private Input(string id, string name, InputType inputType, IEnumerable<ICommand> commands, bool isSecondInput)
    {
        Id = id;
        Name = name;
        InputType = inputType;
        Commands = commands;
        SecondInput = InputType == InputType.Axis && !isSecondInput ? new Input(string.Empty, string.Empty, InputType.Axis, commands, true) : null;
    }

    public Input(string id, string name, InputType inputType, IEnumerable<ICommand> commands) : this(id, name, inputType, commands, false)
    {
    }

    public Input(string id, string name, InputType inputType, IEnumerable<ICommand> commands, int value) : this(id, name, inputType, commands)
    {
        InputValue = value;
    }

    public Input(string id, string name, InputType inputType, IEnumerable<ICommand> commands, int value, ICommand command) : this(id, name, inputType, commands, value)
    {
        SelectedCommand = command;
    }

    public float DeadZone
    {
        get => deadZone;
        set
        {
            deadZone = value;
            NotifyPersistentPropertyChanged();
        }
    }
    public float Saturation
    {
        get => saturation;
        set
        {
            saturation = value;
            NotifyPersistentPropertyChanged();
        }
    }

    public bool Inverted
    {
        get => inverted;
        set
        {
            inverted = value;
            NotifyPersistentPropertyChanged();
        }
    }

    public bool DefaultCenter
    {
        get => defaultCenter;
        set
        {
            defaultCenter = value;
            NotifyPersistentPropertyChanged();
        }
    }

    public Direction CurrentDirection
    {
        get => currentDirection;
        private set
        {
            currentDirection = value;
            NotifyPropertyChanged();
        }
    }
    public int CurrentValue
    {
        get => currentValue;
        private set
        {
            currentValue = value;
            NotifyPropertyChanged();
        }
    }

    public IEnumerable<ICommand> Commands { get; private set; }

    public float InputValue
    {
        get => inputValue;
        set
        {
            inputValue = -1 <= value && value <= 1 ? value : throw new ArgumentOutOfRangeException($"Value must be between -1 and 1, but was {value}");
            RawInputValue = inputValue;
            if (!defaultCenter) inputValue = Util.Map(inputValue, -1, 1, 0, 1);
            var isNegative = inputValue < 0;
            var absVal = Math.Abs(inputValue);

            if (absVal <= DeadZone) inputValue = 0;
            else if (absVal >= 1 - Saturation) inputValue = 1;
            else inputValue = Util.Map(absVal, DeadZone, 1 - Saturation, 0, 1);

            if (isNegative ^ inverted) inputValue = -inputValue;

            ExecuteCommand();
            NotifyPropertyChanged();
        }
    }

    public float RawInputValue
    {
        get => rawInputValue;
        private set
        {
            rawInputValue = value;
            NotifyPropertyChanged();
        }
    }

    public CommandValueOption? CommandValue
    {
        get => commandValue;
        set
        {
            commandValue = value;
            NotifyPersistentPropertyChanged();
        }
    }

    public CommandDirectionOption? CommandDirection
    {
        get => commandDirection;
        set
        {
            commandDirection = value;
            if(secondInput != null) 
                 secondInput.CommandDirection = CommandDirection?.Direction == Direction.High 
                    ? staticDirections.First(d => d.Direction == Direction.Low)
                    : staticDirections.First(d => d.Direction == Direction.High);
            NotifyPersistentPropertyChanged();
        }
    }

    public ICommand? SelectedCommand
    {
        get => command;
        set
        {
            //Hack: Detect change of options for SelectCameraCommand
            if (command != null && command is SelectCameraCommand oldCommand)
            {
                oldCommand.CollectionChanged -= OnSelectCameraCommandCollectionChanged;
                oldCommand.PropertyChanged -= OnSelectCameraCommandPropertyChanged;
            }
            command = value;
            if (command != null && command is SelectCameraCommand newCommand)
            {
                newCommand.CollectionChanged += OnSelectCameraCommandCollectionChanged;
                newCommand.PropertyChanged += OnSelectCameraCommandPropertyChanged;
            }

            if(!(InputType == InputType.Axis && secondInput == null))
                CommandDirection = Directions?.FirstOrDefault();

            CommandValue = command is IStaticCommand ? Values?.FirstOrDefault() : Values?.LastOrDefault();
            NotifyPersistentPropertyChanged();
        }
    }

    public IEnumerable<CommandValueOption>? Values { get => command?.Options; }

    private static CommandDirectionOption[] staticDirections = new CommandDirectionOption[] {
                    new CommandDirectionOption("High", Direction.High),
                    new CommandDirectionOption("Low", Direction.Low)
                };

    public IEnumerable<CommandDirectionOption>? Directions
    {
        get
        {
            if (command is IDynamicCommand dynCommand) return dynCommand.ButtonDirections;
            else if (command is IStaticCommand) return staticDirections;
            return null;
        }
    }

    public IInput? SecondInput {
        get => secondInput;
        private init {
            secondInput = value;
            if(secondInput != null) {
                secondInput.DeadZone = 0;
                secondInput.Saturation = 0;
                secondInput.CommandDirection = CommandDirection?.Direction == Direction.High 
                    ? staticDirections.First(d => d.Direction == Direction.Low)
                    : staticDirections.First(d => d.Direction == Direction.High);
                secondInput.PersistentPropertyChanged += PersistentPropertyChanged;
            }
        }
    }

    private void ExecuteCommand()
    {
        if (command is IDynamicCommand dynCommand)
        {
            if (InputType == InputType.Axis)
            {
                int maxValue = CommandValue?.Value ?? dynCommand.MaxValue;
                int range = maxValue - dynCommand.MinValue;

                float mappedValue = Util.Map(InputValue, -1, 1, -range, range);

                CurrentDirection = Direction.Stop;

                if (mappedValue < 0)
                {
                    mappedValue = Math.Abs(mappedValue) + dynCommand.MinValue;
                    CurrentDirection = Direction.Low;
                }
                else if (mappedValue > 0)
                {
                    mappedValue += dynCommand.MinValue;
                    CurrentDirection = Direction.High;
                }
                CurrentValue = (int)mappedValue;
                dynCommand.Execute((int)mappedValue, CurrentDirection);
            }
            else if (InputType == InputType.Button)
            {
                if (inputValue != 0) dynCommand.Execute(commandValue!, CommandDirection!);
                else dynCommand.Execute(0, Direction.Stop);
            }
        }
        else if (command is IStaticCommand statCommand)
        {
            //Execute static command when Button goes from unpressed to pressed only
            bool executeCommand = false;

            CurrentDirection = Direction.Stop;

            if (InputType == InputType.Axis)
            {
                if (buttonPressed && (
                        (CommandDirection?.Direction == Direction.High && 0.25 > inputValue) ||
                        (CommandDirection?.Direction == Direction.Low && -0.25 < inputValue)))
                {
                    buttonPressed = false;
                    CurrentValue = 0;
                }
                else if (!buttonPressed && (
                        (CommandDirection?.Direction == Direction.High && inputValue > 0.25) ||
                        (CommandDirection?.Direction == Direction.Low && inputValue < -0.25)))
                {
                    buttonPressed = true;
                    executeCommand = true;
                    CurrentValue = commandValue!.Value;
                }

                if(secondInput != null)
                    secondInput.InputValue = commandDirection?.Direction == Direction.High ? Math.Min(inputValue, 0) : Math.Max(inputValue, 0);
            }
            else if (InputType == InputType.Button)
            {
                if (buttonPressed && inputValue == 0)
                    buttonPressed = false;
                else if (!buttonPressed && inputValue != 0)
                {
                    buttonPressed = true;
                    executeCommand = true;
                }
            }

            if (executeCommand) statCommand.Execute(commandValue!);
        }
    }

    public event PropertyChangedEventHandler? PersistentPropertyChanged;
    private void NotifyPersistentPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PersistentPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        NotifyPropertyChanged(propertyName);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void OnSelectCameraCommandCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (CommandValue! != null! && e?.OldStartingIndex >= 0)
        {
            if (CommandValue.Value == e.OldStartingIndex)
                CommandValue = null;
            else if (CommandValue.Value > e.OldStartingIndex)
                CommandValue = Values?.ElementAtOrDefault(CommandValue.Value - 1);
        }
        NotifyPropertyChanged(nameof(Values));
        NotifyPropertyChanged(nameof(CommandValue));
    }

    private void OnSelectCameraCommandPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var UpdatedValue = Values?.ElementAtOrDefault(CommandValue?.Value ?? -1);
        if (UpdatedValue?.Name != CommandValue?.Name)
            CommandValue = UpdatedValue;
        NotifyPropertyChanged(nameof(Values));
        NotifyPropertyChanged(nameof(CommandValue));
    }
}
