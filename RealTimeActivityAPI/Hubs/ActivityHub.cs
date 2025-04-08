using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Models;
using Microsoft.AspNetCore.SignalR;
using RealTimeActivityAPI.DTOs;
using RealTimeActivityAPI.Repositories;

namespace RealTimeActivityAPI.Hubs
{
    
    public class ActivityHub : Hub
    {
        private readonly IRealTimeActivityAPIRepository _repository;
        public ActivityHub(IRealTimeActivityAPIRepository repository)
        {
            _repository = repository;
        }
        public async Task SendActivity(ActivityDTO activity)
        {
            await Clients.All.SendAsync("ReceiveActivity", activity);
        }
        public async Task<List<Activity>> GetRecentActivities()
        {
            // Lấy danh sách activities gần đây từ service hoặc repository
            return await _repository.GetRecentActivities(10); // 10 là số lượng activities muốn lấy
        }
        public override async Task OnConnectedAsync()
        {
            // Khi client kết nối, gửi 10 activities gần nhất
            await base.OnConnectedAsync();
        }
    }
}