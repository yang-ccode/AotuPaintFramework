using System;
using System.IO;
using System.Runtime.Versioning;
using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;
using AotuPaintFramework.Commands;
using AotuPaintFramework.Utils;

namespace AotuPaintFramework;

/// <summary>
/// Main Revit application entry point
/// </summary>
[SupportedOSPlatform("windows7.0")]
public class Application : ExternalApplication
{
    public override void OnStartup()
    {
        try
        {
            Logger.Info("=== AotuPaintFramework Application Starting ===");
            Logger.Info($"Revit Version: {Application.ControlledApplication.VersionNumber}");
            Logger.Info($"Revit Build: {Application.ControlledApplication.VersionBuild}");
            Logger.Info($"Assembly Location: {typeof(Application).Assembly.Location}");
            Logger.Info($"Working Directory: {Directory.GetCurrentDirectory()}");
            
            CreateRibbonPanel(Application);
            
            Logger.Info("=== AotuPaintFramework Application Started Successfully ===");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Fatal error during application startup");
            
            // Write to a separate emergency log file
            try
            {
                var emergencyLog = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    $"AotuPaintFramework_Error_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
                );
                File.WriteAllText(emergencyLog, $"Error: {ex}\n\nStack Trace:\n{ex.StackTrace}");
            }
            catch { }
            
            // Show error to user
            TaskDialog mainDialog = new TaskDialog("AotuPaintFramework Startup Error");
            mainDialog.MainInstruction = "Failed to initialize AotuPaintFramework";
            mainDialog.MainContent = $"Error: {ex.Message}\n\nCheck logs for details.";
            mainDialog.ExpandedContent = $"Exception Type: {ex.GetType().Name}\n\n" +
                                        $"Stack Trace:\n{ex.StackTrace}";
            mainDialog.Show();
        }
    }

    public override void OnShutdown()
    {
        try
        {
            Logger.Info("=== AotuPaintFramework Application Shutting Down ===");
            
            // Application cleanup
            
            Logger.Info("=== AotuPaintFramework Application Shutdown Complete ===");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error during application shutdown");
        }
    }

    private void CreateRibbonPanel(UIControlledApplication application)
    {
        try
        {
            Logger.Info("Creating ribbon panel...");
            
            var panel = application.CreateRibbonPanel("AotuPaint");
            Logger.Info("Ribbon panel 'AotuPaint' created");

            var assemblyPath = typeof(Application).Assembly.Location;
            Logger.Info($"Assembly path: {assemblyPath}");
            
            // Verify assembly exists
            if (!File.Exists(assemblyPath))
            {
                Logger.Error($"Assembly file NOT FOUND: {assemblyPath}");
                throw new FileNotFoundException("Assembly not found", assemblyPath);
            }
            Logger.Info("Assembly file verified - exists");

            var commandType = typeof(StartupCommand);
            Logger.Info($"Command Type: {commandType.FullName}");
            Logger.Info($"Command Assembly: {commandType.Assembly.FullName}");

            var buttonData = new PushButtonData(
                "MaterialPaintButton",
                "Material\nPaint",
                assemblyPath,
                commandType.FullName);

            Logger.Info("PushButtonData created:");
            Logger.Info($"  Name: MaterialPaintButton");
            Logger.Info($"  Text: Material\\nPaint");
            Logger.Info($"  Assembly: {assemblyPath}");
            Logger.Info($"  ClassName: {commandType.FullName}");

            var button = panel.AddItem(buttonData) as PushButton;
            
            if (button != null)
            {
                button.ToolTip = "Material Paint Tool";
                button.LongDescription = "Paint Revit elements with materials on specific faces (side, bottom, and interface faces)";
                
                Logger.Info("? Push button created successfully");
                Logger.Info($"? Button enabled: {button.Enabled}");
                Logger.Info($"? Button visible: {button.Visible}");
            }
            else
            {
                Logger.Error("? Failed to create push button - AddItem returned null");
                throw new InvalidOperationException("AddItem returned null");
            }
            
            Logger.Info("Ribbon panel creation completed successfully");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error creating ribbon panel");
            throw;
        }
    }
}
