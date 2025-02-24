namespace AppPetManagementAPI.DTOs.PetDTOs
{
    public class PetUpdateReqModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public string Species { get; set; }
        public string Breed { get; set; }
        public DateTime DateOfBirth { get; set; }
        public decimal Weight { get; set; }
        public string Description { get; set; }
    }
    public class PetAddReqModel
    {
        public string Name { get; set; }
        public string Gender { get; set; }
        public string Species { get; set; }
        public string Breed { get; set; }
        public DateTime DateOfBirth { get; set; }
        public decimal Weight { get; set; }
        public string Description { get; set; }
    }
}
