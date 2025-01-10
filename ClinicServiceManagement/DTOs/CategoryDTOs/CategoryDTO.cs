namespace ClinicServiceManagementAPI.DTOs.CategoryDTOs
{
    public class CategoryAddModel
    {
        public string Name { get; set; }
    }
    public class CategoryUpdateModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public byte? Status { get; set; }
    }
}
