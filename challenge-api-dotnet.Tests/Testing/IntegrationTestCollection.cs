using Xunit;

namespace challenge_api_dotnet.Tests.Testing;

[CollectionDefinition("IntegrationTests", DisableParallelization = true)]
public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>;
