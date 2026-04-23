using Integration.Tests.Common;

using Xunit;

namespace Integration.Tests;

[CollectionDefinition(nameof(IntegrationTestCollection))]
public class IntegrationTestCollection : ICollectionFixture<IntegrationContainerFixture>;
