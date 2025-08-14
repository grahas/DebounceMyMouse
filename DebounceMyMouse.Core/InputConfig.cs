using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.ComponentModel;

namespace DebounceMyMouse.Core;
public class InputConfig : INotifyPropertyChanged
{
    private string _name;
    private int _debounceMs;
    private bool _isEnabled;

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(nameof(Name)); }
    }

    public int DebounceMs
    {
        get => _debounceMs;
        set { _debounceMs = value; OnPropertyChanged(nameof(DebounceMs)); }
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set { _isEnabled = value; OnPropertyChanged(nameof(IsEnabled)); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public class DebounceConfig
{
    public List<InputConfig> Inputs { get; set; }

    // Static method to create a default config
    public static DebounceConfig CreateDefault()
    {
        return new DebounceConfig
        {
            Inputs = new List<InputConfig>
            {
                new InputConfig { Name = "Left", DebounceMs = 100, IsEnabled = true },
                new InputConfig { Name = "Right", DebounceMs = 100, IsEnabled = true },
                new InputConfig { Name = "Middle", DebounceMs = 300, IsEnabled = true }
            }
        };
    }

    public static DebounceConfig Load(string path)
    {
        if (!File.Exists(path))
        {
            // If the file doesn't exist, create a default config
            var defaultConfig = CreateDefault();
            defaultConfig.Save(path);
        }

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<DebounceConfig>(json);
    }

    public void Save(string path)
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }
}