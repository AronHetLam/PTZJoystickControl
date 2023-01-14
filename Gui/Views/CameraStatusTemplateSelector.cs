using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using System.Collections.Generic;

namespace PtzJoystickControl.Gui.Views;

public class CameraStatusTemplateSelector : IDataTemplate
{
    [Content]
    public Dictionary<string, IDataTemplate> Templates { get; } = new();

    public bool SupportsRecycling => false;

    public IControl Build(object param)
    {
        IDataTemplate? returnTemplate;

        var connected = (bool)param;

        if (connected)
            returnTemplate = Templates["true"];
        else
            returnTemplate = Templates["false"];

        return returnTemplate.Build(param);
    }

    public bool Match(object data)
    {
        return data is bool;
    }
}
