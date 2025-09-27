using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DebounceMyMouse.Core;
public sealed class AppSettings
{
    public List<ChannelSetting> Channels { get; set; } = new();
    public bool IsFirstRun { get; set; } = true;
    public bool LaunchOnStartup { get; set; } = false;

    [JsonIgnore] public string FilePath { get; private set; }

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    private AppSettings(string filePath) => FilePath = filePath;

    // REQUIRED for System.Text.Json to instantiate
    public AppSettings() { }

    // Load from disk or return defaults.
    public static AppSettings Load(string? appName = null, string? companyName = null)
    {
        var path = GetDefaultPath(appName, companyName);
        if (!File.Exists(path))
            return new AppSettings(path);

        try
        {
            var json = File.ReadAllText(path);
            var loaded = JsonSerializer.Deserialize<AppSettings>(json, JsonOpts);
            if (loaded is null) return new AppSettings(path);
            loaded.FilePath = path;
            return loaded;
        }
        catch
        {
            // Corrupt or unreadable file -> fall back to defaults.
            return new AppSettings(path);
        }
    }

    // Save atomically.
    public void Save()
    {
        var dir = Path.GetDirectoryName(FilePath)!;
        Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(this, JsonOpts);
        var tmp = Path.Combine(dir, $"settings.{Guid.NewGuid():N}.tmp");

        File.WriteAllText(tmp, json);

        // Atomic replace when possible; else move.
        try
        {
            if (File.Exists(FilePath))
                File.Replace(tmp, FilePath, destinationBackupFileName: null, ignoreMetadataErrors: true);
            else
                File.Move(tmp, FilePath);
        }
        catch
        {
            // Best-effort fallback.
            File.Copy(tmp, FilePath, overwrite: true);
            File.Delete(tmp);
        }
    }

    // Helpers
    public static string GetCanonicalDirectory(string? appName = null, string? companyName = null)
        => Path.GetDirectoryName(GetDefaultPath(appName, companyName))!;

    private static string GetDefaultPath(string? appName, string? companyName)
    {
        var asmName = Assembly.GetEntryAssembly()?.GetName().Name ?? "MyApp";
        var app = string.IsNullOrWhiteSpace(appName) ? asmName : appName.Trim();
        var company = string.IsNullOrWhiteSpace(companyName) ? "MyCompany" : companyName.Trim();

        var roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); // %AppData%
        var dir = Path.Combine(roaming, company, app);
        return Path.Combine(dir, "settings.json");
    }
}

public sealed class ChannelSetting
{
    public string Name { get; set; } = "";
    // TimeSpan serializes as an ISO8601-like string in modern .NET. Simple and readable.
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    [JsonConstructor]
    public ChannelSetting() { }

    public ChannelSetting(string name, TimeSpan timeout)
    {
        Name = name ?? "";
        Timeout = timeout;
    }
}

// Example usage:
//
// var settings = AppSettings.Load(appName: "DebounceMyMouse", companyName: "GrahamCo");
//
// if (settings.IsFirstRun)
// {
//     settings.Channels.Add(new ChannelSetting("A", TimeSpan.FromSeconds(10)));
//     settings.IsFirstRun = false;
//     settings.Save();
// }
//
// settings.LaunchOnStartup = true;
// settings.Save();
//
// var path = AppSettings.GetCanonicalDirectory("DebounceMyMouse", "GrahamCo");
// Console.WriteLine($"Settings live in: {path}");
