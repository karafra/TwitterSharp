using System.Text.Json.Serialization;


namespace TwitterSharp.Request.Internal
{
    internal class LikeRequestPost
    {
        [JsonPropertyName("tweet_id")]
        public string TweetId { init; get; }
    }
}