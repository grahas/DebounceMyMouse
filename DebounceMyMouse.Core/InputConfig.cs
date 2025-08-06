using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DebounceMyMouse.Core;
public class InputConfig
{
    public string Name { get; set; }
    public int DebounceMs { get; set; }
}

public class DebounceConfig
{
    public List<InputConfig> Inputs { get; set; }

    public static DebounceConfig Load(string path)
    {
        if (!File.Exists(path))
            return new DebounceConfig { Inputs = new List<InputConfig>() };

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<DebounceConfig>(json);
    }

    public void Save(string path)
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }
}