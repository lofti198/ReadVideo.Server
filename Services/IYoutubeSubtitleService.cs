namespace ReadVideo.Services.YoutubeManagement
{
    public interface IYoutubeSubtitleService
    {
        Task<string> ExtractSubtitle(string videoId, string language, bool returnFullData);
        Task<string> ExtractSubtitleAsRawText(string videoId, string language);
        Task<string> ExtractSubtitleAsTextBlocks(string videoId, string language, int sentenceMinTimeSpan);
    }
}
