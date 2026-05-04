using System.Collections.ObjectModel;
using System.Linq;
using VToolProMerge.Helpers;

namespace VToolProMerge.ViewModels;

public class CatalogField : ViewModelBase
{
    private string _placeholder = string.Empty;
    private string _label = string.Empty;
    private string _kind = "Văn bản";
    private string _sample = string.Empty;
    private bool _required;

    public string Placeholder { get => _placeholder; set => SetProperty(ref _placeholder, value); }
    public string Label { get => _label; set => SetProperty(ref _label, value); }
    public string Kind { get => _kind; set => SetProperty(ref _kind, value); }
    public string Sample { get => _sample; set => SetProperty(ref _sample, value); }
    public bool Required { get => _required; set => SetProperty(ref _required, value); }
}

public class CatalogGroup : ViewModelBase
{
    private string _name = string.Empty;
    private string _icon = "📂";
    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public string Icon { get => _icon; set => SetProperty(ref _icon, value); }
    public ObservableCollection<CatalogField> Fields { get; } = new();
    public int FieldCount => Fields.Count;
}

public class CatalogViewModel : ViewModelBase
{
    private CatalogGroup? _selectedGroup;
    private CatalogField? _selectedField;

    public string Title { get; } = "Danh mục dùng chung";
    public string Description { get; } =
        "Quản lý nhóm danh mục, danh mục, chi tiết. Khi tạo trường mới sẽ tự sinh placeholder dạng [TEN_TRUONG].";

    public ObservableCollection<CatalogGroup> Groups { get; }

    public CatalogGroup? SelectedGroup
    {
        get => _selectedGroup;
        set { SetProperty(ref _selectedGroup, value); SelectedField = value?.Fields.FirstOrDefault(); }
    }

    public CatalogField? SelectedField
    {
        get => _selectedField;
        set => SetProperty(ref _selectedField, value);
    }

    public RelayCommand AddGroupCommand { get; }
    public RelayCommand RemoveGroupCommand { get; }
    public RelayCommand AddFieldCommand { get; }
    public RelayCommand RemoveFieldCommand { get; }

    public CatalogViewModel()
    {
        Groups = new ObservableCollection<CatalogGroup>
        {
            BuildGroup("Đơn vị", "🏢", new[]
            {
                ("TEN_DON_VI", "Tên đơn vị", "Văn bản", "Công ty TNHH ABC", true),
                ("MA_DON_VI",  "Mã đơn vị",  "Văn bản", "ABC-001",          true),
                ("DIA_CHI",    "Địa chỉ",    "Văn bản", "12 Lê Lợi, Q.1",   false),
                ("DAI_DIEN",   "Người đại diện","Văn bản","Nguyễn Văn A",   false),
            }),
            BuildGroup("Nhà thầu", "🏗", new[]
            {
                ("NT_TEN",        "Tên nhà thầu",     "Văn bản",  "Cty Xây lắp DEF", true),
                ("NT_MA_SO_THUE", "Mã số thuế",       "Văn bản",  "0312345678",      true),
                ("NT_DIA_CHI",    "Địa chỉ",          "Văn bản",  "45 Trần Phú",     false),
                ("NT_NGAN_HANG",  "Tài khoản ngân hàng","Văn bản","12345-VCB",       false),
            }),
            BuildGroup("Gói thầu", "📦", new[]
            {
                ("SO_GOI_THAU",   "Số gói thầu",      "Văn bản",   "GT-01/2025", true),
                ("TEN_GOI_THAU",  "Tên gói thầu",     "Văn bản",   "Mua sắm thiết bị", true),
                ("GIA_GOI",       "Giá gói",          "Số tiền",   "1.250.000.000",   true),
                ("NGAY_DAU_THAU", "Ngày đấu thầu",    "Ngày",      "15/05/2026",      false),
            }),
            BuildGroup("Hợp đồng", "📑", new[]
            {
                ("SO_HOP_DONG",   "Số hợp đồng",   "Văn bản",  "HD-01/2026",  true),
                ("NGAY_KY",       "Ngày ký",       "Ngày",     "10/04/2026",  true),
                ("GIA_TRI_HD",    "Giá trị HĐ",    "Số tiền",  "950.000.000", true),
                ("THOI_HAN",      "Thời hạn (tháng)","Số",     "12",          false),
            }),
            BuildGroup("Quyết định", "🏛", new[]
            {
                ("SO_QUYET_DINH", "Số quyết định", "Văn bản", "QĐ-22/2026", true),
                ("NGAY_QD",       "Ngày ban hành", "Ngày",    "01/03/2026", true),
                ("NGUOI_KY",      "Người ký",      "Văn bản", "Giám đốc",   false),
            }),
        };
        SelectedGroup = Groups.FirstOrDefault();

        AddGroupCommand = new RelayCommand(_ =>
        {
            var g = new CatalogGroup { Name = $"Nhóm mới {Groups.Count + 1}", Icon = "📂" };
            Groups.Add(g);
            SelectedGroup = g;
        });

        RemoveGroupCommand = new RelayCommand(_ =>
        {
            if (SelectedGroup == null) return;
            if (!DialogHelper.Confirm(
                $"Xoá nhóm '{SelectedGroup.Name}' và toàn bộ {SelectedGroup.FieldCount} trường con?",
                "Xác nhận xoá nhóm")) return;
            Groups.Remove(SelectedGroup);
            SelectedGroup = Groups.FirstOrDefault();
        });

        AddFieldCommand = new RelayCommand(_ =>
        {
            if (SelectedGroup == null) return;
            var f = new CatalogField
            {
                Placeholder = $"TRUONG_MOI_{SelectedGroup.Fields.Count + 1}",
                Label = "Trường mới",
                Kind = "Văn bản",
                Sample = string.Empty,
            };
            SelectedGroup.Fields.Add(f);
            SelectedField = f;
        });

        RemoveFieldCommand = new RelayCommand(_ =>
        {
            if (SelectedGroup == null || SelectedField == null) return;
            SelectedGroup.Fields.Remove(SelectedField);
            SelectedField = SelectedGroup.Fields.FirstOrDefault();
        });
    }

    private static CatalogGroup BuildGroup(string name, string icon,
        (string ph, string label, string kind, string sample, bool req)[] rows)
    {
        var g = new CatalogGroup { Name = name, Icon = icon };
        foreach (var r in rows)
            g.Fields.Add(new CatalogField
            {
                Placeholder = r.ph,
                Label = r.label,
                Kind = r.kind,
                Sample = r.sample,
                Required = r.req,
            });
        return g;
    }
}
