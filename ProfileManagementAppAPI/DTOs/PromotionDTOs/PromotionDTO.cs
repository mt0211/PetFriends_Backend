using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Models;

namespace ProfileManagementAppAPI.DTOs.PromotionDTOs
{
    public class PromotionListResDTO
    {
        public List<Promotion> PromotionAllMember { get; set; }
        public List<Promotion> PromotionNewMember { get; set; }
        public List<Promotion> PromotionLoyaltyMember { get; set; }
    }
}