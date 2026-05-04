namespace VToolProMerge.ViewModels;

public class ProfileViewModel : ViewModelBase
{
    public string Title { get; } = "Hồ sơ";
    public string Description { get; } =
        "Quản lý các hồ sơ làm việc. Mỗi hồ sơ giữ nguồn dữ liệu, mẫu, lịch sử và file đã sinh ra.";
}
