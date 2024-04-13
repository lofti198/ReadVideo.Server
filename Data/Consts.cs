namespace ReadVideo.Server.Data
{
    public static class Consts
    {
        public static int MaxAllowedVideoDurationMinutes = 90;

        public static string OpenAIAssistantID_StartSpeaking = "";
        public static string OpenAIAssistantID_DC = "asst_K4P3EOR3Ok9JoZwQ90AMryQ0";
        public static string GeneralOpenAIAssistantID_HelloDetector = "asst_oBFwHmknOvJxHRTNX0mYCI0C";

        public static string OpenAIApiKey = "GPT_API_KEY";
        public static string SingleStoreConnectionStr = "SINGLE_STORE_CONNECTION_STR";
        public static string DC_Setup_Key = "DC";
        public static string StartSpeaking_Setup_Key = "START_SPEAK";

        public static string MyTestEmail { get; internal set; } = "isolar2005@gmail.com";
    }
}
