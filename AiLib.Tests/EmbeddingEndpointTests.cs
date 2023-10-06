using System;
using System.Linq;
using AiLib.Embedding;
using NUnit.Framework;

namespace AiLib.Tests
{
    public class EmbeddingEndpointTests
    {
        [SetUp]
        public void Setup()
        {
            APIAuthentication.Default = new APIAuthentication(Environment.GetEnvironmentVariable("TEST_OPENAI_SECRET_KEY"));
        }

        [Test]
        public void GetBasicEmbedding()
        {
            var api = new OpenAIAPI();

            Assert.IsNotNull(api.Embeddings);

            var results = api.Embeddings.CreateEmbeddingAsync(new EmbeddingRequest(Model.Model.AdaTextEmbedding, "A test text for embedding")).Result;
            Assert.IsNotNull(results);
            if (results.CreatedUnixTime.HasValue)
            {
                Assert.NotZero(results.CreatedUnixTime.Value);
                Assert.NotNull(results.Created);
                Assert.Greater(results.Created.Value, new DateTime(2018, 1, 1));
				Assert.Less(results.Created.Value, DateTime.Now.AddDays(1));
			} else
            {
                Assert.Null(results.Created);
			}
            Assert.NotNull(results.Object);
            Assert.NotZero(results.Data.Count);
            Assert.That(results.Data.First().Embedding.Length == 1536);
        }

        [Test]
        public void ReturnedUsage()
        {
            var api = new OpenAIAPI();

            Assert.IsNotNull(api.Embeddings);

            var results = api.Embeddings.CreateEmbeddingAsync(new EmbeddingRequest(Model.Model.AdaTextEmbedding, "A test text for embedding")).Result;
            Assert.IsNotNull(results);

			Assert.IsNotNull(results.Usage);
			Assert.GreaterOrEqual(results.Usage.PromptTokens, 5);
			Assert.GreaterOrEqual(results.Usage.TotalTokens, results.Usage.PromptTokens);
		}

        [Test]
        public void GetSimpleEmbedding()
        {
            var api = new OpenAIAPI();

            Assert.IsNotNull(api.Embeddings);

            var results = api.Embeddings.GetEmbeddingsAsync("A test text for embedding").Result;
            Assert.IsNotNull(results);
            Assert.That(results.Length == 1536);
        }
    }
}
