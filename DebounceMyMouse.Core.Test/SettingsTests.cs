using System;
using System.IO;
using DebounceMyMouse.Core;
using Xunit;

namespace DebounceMyMouse.CoreTest
{
    public class SettingsTests
    {
        [Fact]
        public void FirstRun_Defaults_AreCorrect()
        {
            var settings = AppSettings.Load(appName: "TestApp", companyName: "TestCo");

            Assert.True(settings.IsFirstRun);
            Assert.False(settings.LaunchOnStartup);
            Assert.Empty(settings.Channels);
        }

        [Fact]
        public void SaveAndLoad_PersistsValues()
        {
            var settings = AppSettings.Load(appName: "TestApp", companyName: "TestCo");

            settings.IsFirstRun = false;
            settings.LaunchOnStartup = true;
            settings.Channels.Add(new ChannelSetting("ch1", TimeSpan.FromSeconds(5)));

            settings.Save();

            var loaded = AppSettings.Load(appName: "TestApp", companyName: "TestCo");

            Assert.False(loaded.IsFirstRun);
            Assert.True(loaded.LaunchOnStartup);
            Assert.Single(loaded.Channels);
            Assert.Equal("ch1", loaded.Channels[0].Name);
            Assert.Equal(TimeSpan.FromSeconds(5), loaded.Channels[0].Timeout);
        }

        [Fact]
        public void CorruptFile_LoadsDefaults()
        {
            var path = AppSettings.GetCanonicalDirectory("TestApp", "TestCo");
            Directory.CreateDirectory(path);
            var file = Path.Combine(path, "settings.json");

            File.WriteAllText(file, "{not valid json}");

            var settings = AppSettings.Load(appName: "TestApp", companyName: "TestCo");

            Assert.True(settings.IsFirstRun);
            Assert.Empty(settings.Channels);
        }
    }
}
