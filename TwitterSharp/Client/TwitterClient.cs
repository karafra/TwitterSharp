﻿using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using TwitterSharp.CustomConverter;
using TwitterSharp.Model;
using TwitterSharp.Request;
using TwitterSharp.Request.AdvancedSearch;
using TwitterSharp.Request.Internal;
using TwitterSharp.Response;

namespace TwitterSharp.Client
{
    public class TwitterClient
    {
        public TwitterClient(string bearerToken)
        {
            _httpClient = new();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            _jsonOptions.Converters.Add(new ExpressionConverter());
        }

        private static void IncludesParseUser(Answer<IHaveAuthor> answer)
        {
            answer.Data.Author = answer.Includes.Users.FirstOrDefault();
        }

        private static void IncludesParseUserArray<T>(Answer<IHaveAuthor[]> answer)
        {
            for (int i = 0; i < answer.Data.Length; i++)
            {
                answer.Data[i].Author = answer.Includes.Users.Where(x => x.Id == answer.Data[i].AuthorId).FirstOrDefault();
            }
        }

        private static readonly Type _authorInterface = typeof(IHaveAuthor);
        private static void InternalIncludesParse<T>(Answer<T> answer)
        {
            if (typeof(T).IsSubclassOf(_authorInterface)) IncludesParseUser((Answer<IHaveAuthor>)answer);
        }
        private static void InternalIncludesParse<T>(Answer<T[]> answer)
        {
            if (typeof(T).IsSubclassOf(_authorInterface)) IncludesParseUser((Answer<IHaveAuthor[]>)answer);
        }

        private T[] ParseArrayData<T>(string json)
        {
            var answer = JsonSerializer.Deserialize<Answer<T[]>>(json, _jsonOptions);
            if (answer.Detail != null)
            {
                throw new TwitterException(answer.Detail);
            }
            if (answer.Data == null)
            {
                return Array.Empty<T>();
            }
            InternalIncludesParse(answer);
            return answer.Data;
        }

        private Answer<T> ParseData<T>(string json)
        {
            var answer = JsonSerializer.Deserialize<Answer<T>>(json, _jsonOptions);
            if (answer.Detail != null)
            {
                throw new TwitterException(answer.Detail);
            }
            InternalIncludesParse(answer);
            return answer;
        }

        #region TweetSearch
        public async Task<Tweet[]> GetTweetsByIdsAsync(params string[] ids)
            => await GetTweetsByIdsAsync(ids, null);

        public async Task<Tweet[]> GetTweetsByIdsAsync(string[] ids, UserOption[] options)
        {
            var str = await _httpClient.GetStringAsync(_baseUrl + "tweets?ids=" + string.Join(",", ids.Select(x => HttpUtility.HtmlEncode(x)))
                    + (options == null ? "" : "&expansions=author_id&user.fields=" + string.Join(",", options.Select(x => x.ToString().ToLowerInvariant())))
                );
            return ParseArrayData<Tweet>(str);
        }

        public async Task<Tweet[]> GetTweetsFromUserIdAsync(string userId)
            => await GetTweetsFromUserIdAsync(userId, null);

        public async Task<Tweet[]> GetTweetsFromUserIdAsync(string userId, UserOption[] options)
        {
            var str = await _httpClient.GetStringAsync(_baseUrl + "users/" + HttpUtility.HtmlEncode(userId) + "/tweets"
                    + (options == null ? "" : "?expansions=author_id&user.fields=" + string.Join(",", options.Select(x => x.ToString().ToLowerInvariant())))
                );
            return ParseArrayData<Tweet>(str);
        }
        #endregion TweetSearch

        #region TweetStream
        public async Task<StreamInfo[]> GetInfoTweetStreamAsync()
        {
            var str = await _httpClient.GetStringAsync(_baseUrl + "tweets/search/stream/rules");
            return ParseArrayData<StreamInfo>(str);
        }

        public async Task NextTweetStreamAsync(Action<Tweet> onNextTweet)
            => await NextTweetStreamAsync(onNextTweet, null);

        public async Task NextTweetStreamAsync(Action<Tweet> onNextTweet, UserOption[] options)
        {
            var stream = await _httpClient.GetStreamAsync(_baseUrl + "tweets/search/stream"
                    + (options == null ? "" : "?expansions=author_id&user.fields=" + string.Join(",", options.Select(x => x.ToString().ToLowerInvariant())))
                );
            using StreamReader reader = new(stream);
            while (!reader.EndOfStream)
            {
                var str = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(str))
                    continue;
                onNextTweet(ParseData<Tweet>(str).Data);
            }
        }

        public async Task<StreamInfo[]> AddTweetStreamAsync(params StreamRequest[] request)
        {
            var content = new StringContent(JsonSerializer.Serialize(new StreamRequestAdd { Add = request }, _jsonOptions), Encoding.UTF8, "application/json");
            var str = await (await _httpClient.PostAsync(_baseUrl + "tweets/search/stream/rules", content)).Content.ReadAsStringAsync();
            return ParseArrayData<StreamInfo>(str);
        }

        public async Task<int> DeleteTweetStreamAsync(params string[] ids)
        {
            var content = new StringContent(JsonSerializer.Serialize(new StreamRequestDelete { Delete = new StreamRequestDeleteIds { Ids = ids } }, _jsonOptions), Encoding.UTF8, "application/json");
            var str = await (await _httpClient.PostAsync(_baseUrl + "tweets/search/stream/rules", content)).Content.ReadAsStringAsync();
            return ParseData<object>(str).Meta.Summary.Deleted;
        }
        #endregion TweetStream

        #region UserSearch
        public async Task<User[]> GetUsersAsync(params string[] usernames)
            => await GetUsersAsync(usernames, null);

        public async Task<User[]> GetUsersAsync(string[] usernames, UserOption[] options)
        {
            var str = await _httpClient.GetStringAsync(
                _baseUrl + "users/by?usernames=" + string.Join(",", usernames.Select(x => HttpUtility.HtmlEncode(x)))
                    + (options == null ? "" : "&user.fields=" + string.Join(",", options.Select(x => x.ToString().ToLowerInvariant())))
                );
            return ParseArrayData<User>(str);
        }
        #endregion UserSearch

        private const string _baseUrl = "https://api.twitter.com/2/";

        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
    }
}
