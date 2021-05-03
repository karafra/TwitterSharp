﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using TwitterSharp.Client;

namespace TwitterSharp.UnitTests
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public async Task GetTweetAsync()
        {
            var client = new TwitterClient(Environment.GetEnvironmentVariable("TWITTER_TOKEN"));
            var answer = await client.GetTweetsAsync("1389189291582967809");
            Assert.IsTrue(answer.Length == 1);
            Assert.AreEqual("1389189291582967809", answer[0].Id);
            Assert.AreEqual("たのしみ！！\uD83D\uDC93 https://t.co/DgBYVYr9lN", answer[0].Text);
        }

        [TestMethod]
        public async Task GetTweetsAsync()
        {
            var client = new TwitterClient(Environment.GetEnvironmentVariable("TWITTER_TOKEN"));
            var answer = await client.GetTweetsAsync("1389330151779930113", "1389331863102128130");
            Assert.IsTrue(answer.Length == 2);
            Assert.AreEqual("1389330151779930113", answer[0].Id);
            Assert.AreEqual("ねむくなーい！ねむくないねむくない！ドタドタドタドタ", answer[0].Text);
            Assert.AreEqual("1389331863102128130", answer[1].Id);
            Assert.AreEqual("( - ω・ )", answer[1].Text);
        }

        [TestMethod]
        public async Task GetUserAsync()
        {
            var client = new TwitterClient(Environment.GetEnvironmentVariable("TWITTER_TOKEN"));
            var answer = await client.GetUsersAsync("theindra5");
            Assert.IsTrue(answer.Length == 1);
            Assert.AreEqual("1022468464513089536", answer[0].Id);
            Assert.AreEqual("TheIndra5", answer[0].Username);
            Assert.AreEqual("TheIndra", answer[0].Name);
        }
    }
}
