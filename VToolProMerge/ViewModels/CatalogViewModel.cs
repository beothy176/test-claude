namespace VToolProMerge.ViewModels;

public class CatalogViewModel : ViewModelBase
{
    public string Title { get; } = "Danh mục dùng chung";
    public string Description { get; } =
        "Quản lý nhóm danh mục, danh mục, chi tiết. Khi tạo trường mới sẽ tự sinh placeholder dạng [TEN_TRUONG].";
}
