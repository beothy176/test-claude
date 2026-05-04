using System.Collections.Generic;
using System.Collections.ObjectModel;
using VToolProMerge.Models;

namespace VToolProMerge.ViewModels;

public class HistoryViewModel : ViewModelBase
{
    public HistoryViewModel(IEnumerable<AuditLogItem> seed)
    {
        Logs = new ObservableCollection<AuditLogItem>(seed);
    }

    public ObservableCollection<AuditLogItem> Logs { get; }
}
