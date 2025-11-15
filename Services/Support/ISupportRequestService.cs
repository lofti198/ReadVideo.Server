using ReadVideo.Server.Models;

namespace ReadVideo.Server.Services.Support
{
    public interface ISupportRequestService
    {
        Task<SupportRequestResponseDto> ProcessSupportRequestAsync(SupportRequestDto request);
    }
}
