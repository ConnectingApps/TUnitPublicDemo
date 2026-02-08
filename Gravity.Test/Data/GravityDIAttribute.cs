using Microsoft.Extensions.DependencyInjection;

namespace Gravity.Test.Data;

public class GravityDIAttribute : DependencyInjectionDataSourceAttribute<GravityDIAttribute.Scope>
{
    public override Scope CreateScope(DataGeneratorMetadata dataGeneratorMetadata)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddGravity();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        return new Scope(serviceProvider);
    }

    public override object Create(Scope scope, Type type)
    {
        return scope.ServiceProvider.GetRequiredService(type);
    }

    public class Scope(IServiceProvider serviceProvider) : IAsyncDisposable
    {
        public IServiceProvider ServiceProvider { get; } = serviceProvider;

        public ValueTask DisposeAsync()
        {
            if (serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
            return ValueTask.CompletedTask;
        }
    }
}







