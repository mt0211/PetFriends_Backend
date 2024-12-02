namespace PetManagementAPI.DTOs.PetDTOs
{
    public class PetModel
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }

        public string? Gender { get; set; }

        public string? Species { get; set; }

        public string? Breed { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string OwnerName { get; set; }
        public string OwnerPhoneNumber { get; set; }
        public string Vaccinated { get; set; } 
        public string VaccineNames { get; set; } 
    }
    public class AddPetReqModel
    {
        public string Name { get; set; }
        public string Species { get; set; }
        public string Breed { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool Vaccinated { get; set; }
        public string PhoneNumber { get; set; }
        public List<Guid> VaccineIds { get; set; } 
    }

    public class UpdatePetReqModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public string Species { get; set; }
        public string Breed { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool Vaccinated { get; set; }
        public string PhoneNumber { get; set; } // Số điện thoại của User
        public List<Guid> VaccineIds { get; set; } // Danh sách vaccine mới
    }
}
