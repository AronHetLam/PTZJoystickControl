using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using PtzJoystickControl.Core.Commands;
using PtzJoystickControl.Core.Devices;
using PtzJoystickControl.Gui.ViewModels;
using System.Collections.Generic;
using System.ComponentModel;

namespace PtzJoystickControl.Gui.Views;

public class CommandPropsTemplateSelector : IDataTemplate
{
    [Content]
    public Dictionary<string, IDataTemplate> Templates { get; } = new();

    public bool SupportsRecycling => false;

    public IControl Build(object param)
    {
        IDataTemplate? returnTemplate = null;

        var inputViewModel = (InputViewModel)param;
        if (inputViewModel.SelectedCommand != null)
        {
            if (inputViewModel.InputType == InputType.Axis)
            {
                if (inputViewModel.SelectedCommand is IDynamicCommand)
                    returnTemplate = Templates["AxisWithDynamicCommand"];

                else if (inputViewModel.SelectedCommand is IStaticCommand)
                    returnTemplate = Templates["AxisWithStaticCommand"];
            }

            else if (inputViewModel.InputType == InputType.Button)
            {
                if (inputViewModel.SelectedCommand is IDynamicCommand)
                    returnTemplate = Templates["ButtonWithDynamicCommand"];

                else if (inputViewModel.SelectedCommand is IStaticCommand)
                    returnTemplate = Templates["ButtonWithStaticCommand"];
            }
        }
        else
            returnTemplate = Templates["NoCommandSelected"];


        var control = returnTemplate?.Build(param)!;
        void lambda(object? sender, PropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.PropertyName == nameof(inputViewModel.SelectedCommand))
            {
                inputViewModel.PropertyChanged -= lambda;
                var parent = control.Parent;
                var tmp = control.Parent?.DataContext;
                control.Parent!.DataContext = null;
                parent!.DataContext = tmp;
            }
        }

        inputViewModel.PropertyChanged += lambda;

        return control;
    }

    public bool Match(object data)
    {
        return data is InputViewModel;
    }
}
