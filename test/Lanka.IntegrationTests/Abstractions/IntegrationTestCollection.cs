namespace Lanka.IntegrationTests.Abstractions;

#pragma warning disable CA1515 // Type can be made internal
[CollectionDefinition(nameof(IntegrationTestCollection))]
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestWebAppFactory>;
