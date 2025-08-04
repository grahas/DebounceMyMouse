using Microsoft.Win32;

public static class AutoStartManager
{
    public static void EnableStartup(string appName)
    {
        var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
        key.SetValue(appName, $"\"{System.Reflection.Assembly.GetExecutingAssembly().Location}\"");
    }
}