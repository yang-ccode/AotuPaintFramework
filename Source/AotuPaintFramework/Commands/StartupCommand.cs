using System;
using Nice3point.Revit.Toolkit.External;
using AotuPaintFramework.ViewModels;
using AotuPaintFramework.Views;
using AotuPaintFramework.Utils;

namespace AotuPaintFramework.Commands;

/// <summary>
/// Entry point command for the Material Paint plugin.
/// Initializes and displays the Material Paint window.
/// </summary>
public class StartupCommand : ExternalCommand
{
    public override void Execute()
    {
        try
        {
            Logger.Info("StartupCommand.Execute() started");

            // Get UIDocument from base ExternalCommand property
            var uiDocument = UiDocument;
            
            if (uiDocument == null)
            {
                Logger.Warning("UIDocument is null in StartupCommand.Execute()");
                return;
            }

            Logger.Info("Creating MaterialPaintViewModel");
            
            // Create and show the Material Paint view
            var view = new MaterialPaintView(uiDocument);
            
            Logger.Info("Displaying MaterialPaintView window");
            view.ShowDialog();
            
            Logger.Info("StartupCommand.Execute() completed successfully");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error in StartupCommand.Execute()");
            throw;
        }
    }
}
