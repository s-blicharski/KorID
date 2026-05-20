using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using KorID.API.Services;

[ApiController]
[Route("api/[controller]")]
public class AccessController : ControllerBase
{
    private readonly IDistributedCache _cache;
    private readonly QrCodeService _qrCodeService;

    public AccessController(IDistributedCache cache, QrCodeService qrCodeService)
    {
        _cache = cache;
        _qrCodeService = qrCodeService;
    }
    
    [HttpPost("generate")]
    public async Task<IActionResult> GenerateAccessQr()
    {
        // 1. Pobierz ID zalogowanego u¿ytkownika (tu uproszczone)
        //TODO Dependecy for currentUser
        var userId = "user_123"; 
        
        var token = Guid.NewGuid().ToString();
        
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
        };
        
        await _cache.SetStringAsync($"qr_token:{token}", userId, cacheOptions);
        
        var qrCodeImage = _qrCodeService.GenerateQrCode(token);
        
        return File(qrCodeImage, "image/png");
    }
    
    [HttpPost("verify")]
    public async Task<IActionResult> VerifyAccessQr([FromBody] VerifyRequest request)
    {
        var cacheKey = $"qr_token:{request.Token}";
        var userId = await _cache.GetStringAsync(cacheKey);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { Message = "Kod QR wygas³ lub jest nieprawid³owy." });
        }
        
        await _cache.RemoveAsync(cacheKey);
        
        return Ok(new { Message = "Dostêp przyznany", UserId = userId });
    }
}

public class VerifyRequest
{
    public string Token { get; set; }
}