using System;
using System.IO;
using System.Text.Json;
using AotuPaintFramework.Models;

namespace AotuPaintFramework.Utils
{
    /// <summary>
    /// Manages configuration persistence for the AotuPaintFramework application.
    /// Handles saving and loading of mapping configurations to/from the user's AppData folder.
    /// </summary>
    public static class ConfigManager
    {
        private static readonly string AppDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AotuPaintFramework");

        private static readonly string ConfigFilePath = Path.Combine(AppDataFolder, "mapping.json");

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Saves the mapping configuration to the application data folder.
        /// Creates the directory if it doesn't exist.
        /// </summary>
        /// <param name="config">The configuration to save.</param>
        /// <exception cref="ArgumentNullException">Thrown when config is null.</exception>
        public static void SaveConfiguration(MappingConfiguration config)
        {
            // Validate parameter before try-catch to avoid double-logging
            if (config == null)
            {
                Logger.Warning("Configuration is null in SaveConfiguration");
                throw new ArgumentNullException(nameof(config));
            }

            try
            {
                Logger.Info($"Saving configuration to {ConfigFilePath}");

                if (!Directory.Exists(AppDataFolder))
                {
                    Directory.CreateDirectory(AppDataFolder);
                    Logger.Info($"Created directory {AppDataFolder}");
                }

                string jsonContent = JsonSerializer.Serialize(config, JsonOptions);
                File.WriteAllText(ConfigFilePath, jsonContent);
                
                Logger.Info("Configuration saved successfully");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to save configuration to {ConfigFilePath}");
                throw new InvalidOperationException(
                    $"Failed to save configuration to {ConfigFilePath}", ex);
            }
        }

        /// <summary>
        /// Loads the mapping configuration from the application data folder.
        /// Returns a new default configuration if the file doesn't exist.
        /// </summary>
        /// <returns>The loaded configuration, or a new default configuration if the file is not found.</returns>
        public static MappingConfiguration LoadConfiguration()
        {
            try
            {
                Logger.Info($"Loading configuration from {ConfigFilePath}");

                if (!File.Exists(ConfigFilePath))
                {
                    Logger.Info("Configuration file does not exist, returning default configuration");
                    return new MappingConfiguration();
                }

                string jsonContent = File.ReadAllText(ConfigFilePath);
                var config = JsonSerializer.Deserialize<MappingConfiguration>(jsonContent, JsonOptions);
                
                if (config == null)
                {
                    Logger.Warning("Deserialized configuration is null, returning default configuration");
                    return new MappingConfiguration();
                }

                Logger.Info("Configuration loaded successfully");
                return config;
            }
            catch (FileNotFoundException)
            {
                Logger.Info("Configuration file not found, returning default configuration");
                return new MappingConfiguration();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to load configuration from {ConfigFilePath}");
                throw new InvalidOperationException(
                    $"Failed to load configuration from {ConfigFilePath}", ex);
            }
        }

        /// <summary>
        /// Gets the full path to the configuration file.
        /// </summary>
        /// <returns>The full path to the mapping configuration file.</returns>
        public static string GetConfigFilePath()
        {
            return ConfigFilePath;
        }

        /// <summary>
        /// Deletes the configuration file if it exists.
        /// </summary>
        /// <returns>True if the file was deleted, false if it didn't exist.</returns>
        public static bool DeleteConfiguration()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    File.Delete(ConfigFilePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to delete configuration file at {ConfigFilePath}", ex);
            }
        }
    }
}
