using Microsoft.AspNetCore.Mvc;

namespace NotificationAPI.Controllers
{
    [ApiController]
    [Route("api/push-token")]
    public class PushTokenController : ControllerBase
    {
        private readonly IPushTokenRepository _repository;

        public PushTokenController(IPushTokenRepository repository)
        {
            _repository = repository;
        }
        [HttpPost("register")]
        public async Task<IActionResult> RegisterToken([FromBody] PushTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.Token))
            {
                return BadRequest("Token is required");
            }

            // Lấy userId từ token JWT
            var userId = User.FindFirst("userid")?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            var result = await _repository.SaveToken(userGuid, request.Token, request.DeviceType);

            if (result)
            {
                return Ok(new { message = "Token registered successfully" });
            }

            return StatusCode(500, new { message = "Failed to register token" });
        }
        [HttpDelete("unregister")]
        public async Task<IActionResult> UnregisterToken([FromBody] PushTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.Token))
            {
                return BadRequest("Token is required");
            }

            var result = await _repository.DeleteToken(request.Token);

            if (result)
            {
                return Ok(new { message = "Token unregistered successfully" });
            }

            return NotFound(new { message = "Token not found" });
        }
    }

}