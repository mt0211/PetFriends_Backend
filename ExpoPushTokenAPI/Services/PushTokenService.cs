public class PushTokenService : IPushTokenService
{
    private readonly IPushTokenRepository _pushTokenRepository;
    public PushTokenService(IPushTokenRepository pushTokenRepository)
    {
        _pushTokenRepository = pushTokenRepository;
    }
}