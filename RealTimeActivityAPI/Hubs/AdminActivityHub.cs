using DataAccess.Models;
using Microsoft.AspNetCore.SignalR;
using RealTimeActivityAPI.Repositories;

public class AdminActivityHub : Hub
{
    private readonly IRealTimeActivityAPIRepository _repository;
    public AdminActivityHub(IRealTimeActivityAPIRepository repository)
    {
        _repository = repository;
    }
    public async Task SendActivity(Activity activity)
    {
        await Clients.All.SendAsync("ReceiveActivity", activity);
    }
    public async Task<List<Activity>> GetRecentActivities()
    {
        // Lấy danh sách activities gần đây từ service hoặc repository
        return await _repository.AdminGetRecentActivities(); // 10 là số lượng activities muốn lấy
    }
    public override async Task OnConnectedAsync()
    {
        // Khi client kết nối, gửi 10 activities gần nhất
        await base.OnConnectedAsync();
    }
}