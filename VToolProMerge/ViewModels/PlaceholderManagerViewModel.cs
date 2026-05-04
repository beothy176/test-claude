using System.Collections.Generic;
using System.Collections.ObjectModel;
using VToolProMerge.Models;

namespace VToolProMerge.ViewModels;

public class PlaceholderManagerViewModel : ViewModelBase
{
    public PlaceholderManagerViewModel(IEnumerable<PlaceholderItem> seed)
    {
        Items = new ObservableCollection<PlaceholderItem>(seed);
    }

    public ObservableCollection<PlaceholderItem> Items { get; }
}
