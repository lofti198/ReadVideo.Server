namespace ReadVideo.Services.YoutubeManagement
{
    public interface IYoutubeSubtitleService
    {
        Task<string> ExtractSubtitle(string videoId, string language, bool returnFullData);

        Task<string> ExtractSubtitleAsTextBlocks(string videoId, string language, int sentenceMinTimeSpan);
    }
}
