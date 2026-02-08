# TUnit Dependency Injection Demo

A demonstration project showcasing how to use **dependency injection** with [TUnit](https://github.com/thomhurst/TUnit) - a modern .NET testing framework. This project uses a simple Gravity Calculator API as the system under test.

## 🎯 Purpose

This repository demonstrates how to:
- Integrate **dependency injection** into TUnit tests
- Create custom `DependencyInjectionDataSourceAttribute` implementations
- Reuse your application's DI configuration in tests
- Write clean, constructor-injected test classes

## 🧪 TUnit Dependency Injection

### How It Works

TUnit supports dependency injection through the `DependencyInjectionDataSourceAttribute<TScope>` base class. This allows you to inject services directly into your test class constructors, just like you would in ASP.NET Core.

### The Custom DI Attribute

The [`GravityDIAttribute`](Gravity.Test/Data/GravityDIAttribute.cs) extends TUnit's `DependencyInjectionDataSourceAttribute` to provide scoped dependency injection:

```csharp
public class GravityDIAttribute : DependencyInjectionDataSourceAttribute<GravityDIAttribute.Scope>
{
    public override Scope CreateScope(DataGeneratorMetadata dataGeneratorMetadata)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddGravity();  // Reuse app's DI registration
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
```

**Key points:**
1. `CreateScope()` - Sets up the DI container using your app's registration (via `AddGravity()`)
2. `Create()` - Resolves services from the container for constructor injection
3. `Scope` - Implements `IAsyncDisposable` for proper cleanup

### Using DI in Tests

Apply the attribute to your test class and inject dependencies via the constructor:

```csharp
[GravityDI]  // Enables dependency injection
public class GravityCalculatorTests(IGravityCalculator calculator)  // Constructor injection
{
    [Test]
    [Arguments(100.0, Planet.Earth, 980.7)]
    [Arguments(100.0, Planet.Moon, 162.0)]
    [Arguments(100.0, Planet.Mars, 371.0)]
    [Arguments(50.0, Planet.Jupiter, 1239.5)]
    public async Task CalculateForce_ReturnsCorrectForce(double mass, Planet planet, double expected)
    {
        // Act
        var result = calculator.CalculateForce(mass, planet);

        // Assert
        await Assert.That(result).IsEqualTo(expected);
    }
}
```

**Benefits:**
- ✅ No manual service instantiation in tests
- ✅ Reuses the same DI configuration as your application
- ✅ Clean separation of concerns
- ✅ Easy to swap implementations for testing

## 🏗️ Project Structure

```
├── Gravity/                        # Main API project
│   ├── Controllers/                # API controllers
│   ├── Interfaces/
│   │   └── IGravityCalculator.cs   # Service interface
│   ├── Models/
│   │   └── Planet.cs               # Planet enum
│   ├── Services/                   # Service implementations
│   └── DependencySetup.cs          # DI registration (AddGravity)
├── Gravity.Test/                   # Test project using TUnit
│   ├── Data/
│   │   └── GravityDIAttribute.cs   # Custom DI attribute for TUnit
│   └── GravityCalculatorTests.cs   # Tests with constructor injection
└── Gravity.slnx                    # Solution file
```

## 📋 Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## 🔧 Getting Started

### Clone and build

```bash
git clone https://github.com/your-username/TUnitPublicDemo.git
cd TUnitPublicDemo
dotnet build
```

### Run the tests

```bash
dotnet test
```

Or with detailed output:

```bash
dotnet test --logger "console;verbosity=detailed"
```

### Run the API

```bash
cd Gravity
dotnet run
```

The API will be available at `http://localhost:5073`

## 📡 API Usage

### Calculate Gravitational Force

```http
GET /api/gravity/calculate?weightKg=70&planet=Earth
```

| Parameter  | Type   | Description                     |
|------------|--------|---------------------------------|
| `weightKg` | double | Mass in kilograms               |
| `planet`   | string | Planet name (Earth, Mars, etc.) |

```bash
curl "http://localhost:5073/api/gravity/calculate?weightKg=70&planet=Earth"
```

## 📦 Dependencies

| Project      | Package                        | Version |
|--------------|--------------------------------|---------|
| Gravity      | Microsoft.AspNetCore.OpenApi   | 10.0.0  |
| Gravity.Test | TUnit                          | 1.13.8  |

## 🔗 Resources

- [TUnit Documentation](https://github.com/thomhurst/TUnit)
- [TUnit Dependency Injection](https://github.com/thomhurst/TUnit#dependency-injection)

## 📄 License

This project is licensed under the GNU General Public License v3.0 - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request