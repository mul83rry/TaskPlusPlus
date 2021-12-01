using Newtonsoft.Json.Linq;

namespace TaskPlusPlus.API
{
    public static class JsonMap
    {
        public static JObject TrueResult => new() { { "result", true } };
        public static JObject FalseResult => new() { { "result", false } };
        public static JObject FalseResultWithEmptyAccessToken => new() { { "result", false }, { "accessCode", string.Empty } };
        public static JObject GetSuccesfullAccessToken(string accessToken) => new() { { "result", true }, { "accessCode", accessToken } };
    }
}
