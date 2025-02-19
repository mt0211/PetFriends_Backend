namespace AdminVaccineManagement.DTOs.VaccineDTOs
{
    public class VaccineListResModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int NumberOfDoses { get; set; }
        public int FirstInject { get; set; }
        public string Recommendation {  get; set; }
        public byte Status { get; set; }
    }

    public class VaccineUpdateStatusReqModel
    {
        public Guid Id { get; set; }
        public byte Status { get; set; }
    }
    public class VaccineAddReqModel
    {
        
        public string Name { get; set; }
        public int NumberOfDoses { get; set; }
        public List<int> Injecttions { get; set; }
        public string Recommendation { get; set; }
        
    }
    public class VaccineUpdateReqModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int NumberOfDoses { get; set; }
        public List<int> Injecttions { get; set; }
        public string Recommendation { get; set; }

    }
   
}
