using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DebounceMyMouse.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public List<MouseInputType> InputTypes { get; } = Enum.GetValues<MouseInputType>().ToList();

        [ObservableProperty]
        private MouseInputType selectedInput;

        public ObservableCollection<InputConfig> InputConfigs { get; }

    }
}
