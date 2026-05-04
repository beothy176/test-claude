namespace VToolProMerge.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    public string Title { get; } = "Cài đặt";
    public string Description { get; } =
        "Cấu hình đường dẫn lưu trữ, font mặc định, header/footer mẫu, tích hợp Office Interop, ...";
}
