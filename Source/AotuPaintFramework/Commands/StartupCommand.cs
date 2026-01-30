using System;
using System.Runtime.Versioning;
using System.Windows;
using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;
using AotuPaintFramework.Views;
using AotuPaintFramework.Utils;

namespace AotuPaintFramework.Commands;

/// <summary>
/// Entry point command for AotuPaintFramework plugin.
/// Opens the Material Paint dialog for painting Revit elements with materials.
/// </summary>
[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
[SupportedOSPlatform("windows7.0")]
public class StartupCommand : ExternalCommand
{
    public override void Execute()
    {
        try
        {
            Logger.Info("==========================================");
            Logger.Info("StartupCommand.Execute() called - Button clicked!");
            Logger.Info("==========================================");
            
            if (UiApplication == null)
            {
                Logger.Error("UiApplication is NULL!");
                MessageBox.Show("Error: UiApplication is null!", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            Logger.Info($"UiApplication OK");
            
            if (UiDocument == null)
            {
                Logger.Error("UiDocument is NULL!");
                MessageBox.Show("Please open or create a Revit document first.", 
                    "No Document Open", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            Logger.Info($"UiDocument OK: {UiDocument.Document.Title}");
            Logger.Info("Creating MaterialPaintView...");
            
            var view = new MaterialPaintView(UiDocument);
            
            Logger.Info("Showing MaterialPaintView dialog...");
            view.ShowDialog();
            
            Logger.Info("MaterialPaintView closed by user");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "EXCEPTION in StartupCommand.Execute()");
            
            MessageBox.Show(
                $"Error: {ex.Message}\n\nPlease check logs for details.", 
                "AotuPaintFramework Error", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }
    }
    
}
