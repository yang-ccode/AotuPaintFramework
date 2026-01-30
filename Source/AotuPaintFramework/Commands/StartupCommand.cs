using System;
using Nice3point.Revit.Toolkit.External;
using AotuPaintFramework.Views;
using AotuPaintFramework.Utils;

namespace AotuPaintFramework.Commands;

/// <summary>
/// Entry point command for AotuPaintFramework plugin.
/// Opens the Material Paint dialog for painting Revit elements with materials.
/// </summary>
public class StartupCommand : ExternalCommand
{
    public override void Execute()
    {
        try
        {
            Logger.Info("StartupCommand executed");
            
            if (UiApplication == null || UiDocument == null)
            {
                Logger.Error("UiApplication or UiDocument is null");
                return;
            }
            
            var view = new MaterialPaintView(UiDocument);
            
            Logger.Info("Opening Material Paint View");
            view.ShowDialog();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error executing StartupCommand");
        }
    }
}
