using Newtonsoft.Json;

namespace WebApi.Logic.Article.Struct
{
    public class ArticleInfo
    {
        [JsonProperty("title")]
        public string Title;

        [JsonProperty("image")]
        public string Image;

        [JsonProperty("time")]
        public long Time;

        [JsonProperty("author")]
        public string Author;

        [JsonProperty("platform")]
        public string Platform;

        [JsonProperty("intro")]
        public string Intro;

        [JsonProperty("link")]
        public string Link;
    }
}