namespace DataAccess.DTOs.UserDTOs;
public class CreateHashPasswordDTO
{
    public required byte[] Salt { get; set; }
    public required byte[] HashedPassword { get; set; }
}