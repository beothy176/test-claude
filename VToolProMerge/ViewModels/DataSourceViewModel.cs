namespace VToolProMerge.ViewModels;

public class DataSourceViewModel : ViewModelBase
{
    public string Title { get; } = "Nguồn dữ liệu";
    public string Description { get; } =
        "Quản lý nguồn dữ liệu: JSON, SQLite, Excel. Mỗi nguồn được map vào placeholder qua DataMappingEngine.";
}
