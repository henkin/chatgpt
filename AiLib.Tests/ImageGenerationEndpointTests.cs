using System;
using System.Linq;
using AiLib.Images;
using NUnit.Framework;

namespace AiLib.Tests
{
	public class ImageGenerationEndpointTests
	{
		[SetUp]
		public void Setup()
		{
			APIAuthentication.Default = new APIAuthentication(Environment.GetEnvironmentVariable("TEST_OPENAI_SECRET_KEY"));
		}

		[Test]
		public void CreateImageWithUrl()
		{
			var api = new OpenAIAPI();

			Assert.IsNotNull(api.ImageGenerations);

			var results = api.ImageGenerations.CreateImageAsync(new ImageGenerationRequest("A cyberpunk monkey hacker dreaming of a beautiful bunch of bananas, digital art", 1, ImageSize._256)).Result;
			Assert.IsNotNull(results);
			if (results.CreatedUnixTime.HasValue)
			{
				Assert.NotZero(results.CreatedUnixTime.Value);
				Assert.NotNull(results.Created);
				Assert.Greater(results.Created.Value, new DateTime(2018, 1, 1));
				Assert.Less(results.Created.Value, DateTime.Now.AddDays(1));
			}
			else
			{
				Assert.Null(results.Created);
			}
		 
			Assert.NotZero(results.Data.Count);
			Assert.NotNull(results.Data.First().Url);
			Assert.That(results.Data.First().Url.Length > 0);
			Assert.That(results.Data.First().Url.StartsWith("https://"));
		}


		[Test]
		public void CreateImageBase64Enc()
		{
			var api = new OpenAIAPI();

			Assert.IsNotNull(api.ImageGenerations);

			var results = api.ImageGenerations.CreateImageAsync(new ImageGenerationRequest("A cyberpunk monkey hacker dreaming of a beautiful bunch of bananas, digital art", size: ImageSize._256, responseFormat: ImageResponseFormat.B64_json)).Result;
			Assert.IsNotNull(results);
			if (results.CreatedUnixTime.HasValue)
			{
				Assert.NotZero(results.CreatedUnixTime.Value);
				Assert.NotNull(results.Created);
				Assert.Greater(results.Created.Value, new DateTime(2018, 1, 1));
				Assert.Less(results.Created.Value, DateTime.Now.AddDays(1));
			}
			else
			{
				Assert.Null(results.Created);
			}

			Assert.NotZero(results.Data.Count);
			Assert.NotNull(results.Data.First().Base64Data);
			Assert.That(results.Data.First().Base64Data.Length > 0);
		}

	}
}
