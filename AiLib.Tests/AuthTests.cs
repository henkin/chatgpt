using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AiLib.Tests
{
	public class AuthTests
	{
		[SetUp]
		public void Setup()
		{
			File.WriteAllText(".openai", "OPENAI_KEY=pk-test12" + Environment.NewLine + "OPENAI_ORGANIZATION=org-testing123");
			Environment.SetEnvironmentVariable("OPENAI_API_KEY", "pk-test-env");
			Environment.SetEnvironmentVariable("OPENAI_ORGANIZATION", "org-testing123");
		}

		[Test]
		public void GetAuthFromEnv()
		{
			var auth = APIAuthentication.LoadFromEnv();
			Assert.IsNotNull(auth);
			Assert.IsNotNull(auth.ApiKey);
			Assert.IsNotEmpty(auth.ApiKey);
			Assert.AreEqual("pk-test-env", auth.ApiKey);
		}

		[Test]
		public void GetAuthFromFile()
		{
			var auth = APIAuthentication.LoadFromPath();
			Assert.IsNotNull(auth);
			Assert.IsNotNull(auth.ApiKey);
			Assert.AreEqual("pk-test12", auth.ApiKey);
		}


		[Test]
		public void GetAuthFromNonExistantFile()
		{
			var auth = APIAuthentication.LoadFromPath(filename: "bad.config");
			Assert.IsNull(auth);
		}


		[Test]
		public void GetDefault()
		{
			var auth = APIAuthentication.Default;
			var envAuth = APIAuthentication.LoadFromEnv();
			Assert.IsNotNull(auth);
			Assert.IsNotNull(auth.ApiKey);
			Assert.IsNotNull(envAuth);
			Assert.IsNotNull(envAuth.ApiKey);
			Assert.AreEqual(envAuth.ApiKey, auth.ApiKey);
			Assert.IsNotNull(auth.OpenAIOrganization);
			Assert.IsNotNull(envAuth.OpenAIOrganization);
			Assert.AreEqual(envAuth.OpenAIOrganization, auth.OpenAIOrganization);

		}



		[Test]
		public void testHelper()
		{
			APIAuthentication defaultAuth = APIAuthentication.Default;
			APIAuthentication manualAuth = new APIAuthentication("pk-testAA");
			OpenAIAPI api = new OpenAIAPI();
			APIAuthentication shouldBeDefaultAuth = api.Auth;
			Assert.IsNotNull(shouldBeDefaultAuth);
			Assert.IsNotNull(shouldBeDefaultAuth.ApiKey);
			Assert.AreEqual(defaultAuth.ApiKey, shouldBeDefaultAuth.ApiKey);

			APIAuthentication.Default = new APIAuthentication("pk-testAA");
			api = new OpenAIAPI();
			APIAuthentication shouldBeManualAuth = api.Auth;
			Assert.IsNotNull(shouldBeManualAuth);
			Assert.IsNotNull(shouldBeManualAuth.ApiKey);
			Assert.AreEqual(manualAuth.ApiKey, shouldBeManualAuth.ApiKey);
		}

		[Test]
		public void GetKey()
		{
			var auth = new APIAuthentication("pk-testAA");
			Assert.IsNotNull(auth.ApiKey);
			Assert.AreEqual("pk-testAA", auth.ApiKey);
		}

		[Test]
		public void ParseKey()
		{
			var auth = new APIAuthentication("pk-testAA");
			Assert.IsNotNull(auth.ApiKey);
			Assert.AreEqual("pk-testAA", auth.ApiKey);
			Assert.IsNull(auth.OpenAIOrganization);
			auth = "pk-testCC";
			Assert.IsNotNull(auth.ApiKey);
			Assert.AreEqual("pk-testCC", auth.ApiKey);

			auth = new APIAuthentication("sk-testBB", "orgTest");
			Assert.IsNotNull(auth.ApiKey);
			Assert.AreEqual("sk-testBB", auth.ApiKey);
			Assert.IsNotNull(auth.OpenAIOrganization);
			Assert.AreEqual("orgTest", auth.OpenAIOrganization);
		}

		[Test]
		public async Task TestBadKey()
		{
			var auth = new APIAuthentication("pk-testAA");
			Assert.IsFalse(await auth.ValidateAPIKey());

			auth = new APIAuthentication(null);
			Assert.IsFalse(await auth.ValidateAPIKey());
		}

		[Test]
		public async Task TestValidateGoodKey()
		{
			var auth = new APIAuthentication(Environment.GetEnvironmentVariable("TEST_OPENAI_SECRET_KEY"));
			Assert.IsTrue(await auth.ValidateAPIKey());
		}

	}
}