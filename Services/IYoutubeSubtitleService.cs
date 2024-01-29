namespace ReadVideo.Services.YoutubeManagement
{
    public interface IYoutubeSubtitleService
    {
        Task<string> ExtractSubtitle(string videoId, string language);
    }
}
