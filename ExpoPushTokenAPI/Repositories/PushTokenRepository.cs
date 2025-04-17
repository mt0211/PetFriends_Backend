using DataAccess.Models;

public class PushTokenRepository : IPushTokenRepository
{
    private readonly PetfriendsContext _context;
    public PushTokenRepository(PetfriendsContext context)
    {
        _context = context;
    }
}