namespace AppPetManagementAPI.DTOs.VaccineDTOs
{
    public class AddUserPetVaccineReqModel
    {
        public Guid PetID { get; set; }  
        public string VaccineName { get; set; }  
        public int NumberOfDoses { get; set; }  
        public List<InjectionDetail> Injections { get; set; }  
    }
    public class InjectionDetail
    {
        public int DoseNumber { get; set; }  
        public DateTime DateGiven { get; set; }  
    }
    public class UserPetVaccineDTO
    {
        public Guid VaccineId { get; set; }
        public Guid? PetID { get; set; }
        public string VaccineName { get; set; }
        public int? NumberOfDoses { get; set; }
        public List<UserPetVaccineDoseDTO> Injections { get; set; } = new List<UserPetVaccineDoseDTO>();
    }

    public class UserPetVaccineDoseDTO
    {
        public int? DoseNumber { get; set; }
        public DateTime DateGiven { get; set; }
    }

    public class UpdateVaccineDoseReqModel
    {
        public Guid VaccineId { get; set; }  
        public string VaccineName { get; set; } 
        public int NumberOfDoses { get; set; } 
        public List<InjectionDetail> Injections { get; set; } 
    }
    public class VaccineListReqModel
    {
        public Guid VaccineId { get; set; }
        public string VaccineName { get; set; }
        public int? VaccineNumberOfDoses { get; set; }
        public string VaccineRecommendation { get; set; }
        public byte? VaccineStatus { get; set; }
    }
}
