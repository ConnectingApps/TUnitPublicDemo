# TUnit Dependency Injection Demo

A demonstration project showcasing how to use **dependency injection** with [TUnit](https://github.com/thomhurst/TUnit) — a modern, source-generated .NET testing framework. The system under test is a simple Gravity Calculator API that computes gravitational force on different planets.

## 🎯 Purpose

This repository exists to answer one question: **how do you wire up dependency injection in TUnit tests?**

Along the way it shows how to:

- Create a custom [`DependencyInjectionDataSourceAttribute`](Gravity.Test/Data/GravityDIAttribute.cs) implementation
- Reuse the exact same DI registrations your application uses
- Write test classes that receive services through **constructor injection** — no `new` keywords, no service locators

## ✨ The Elegance — One Registration, Two Consumers

The core insight of this demo is that **TUnit lets your test classes consume dependencies the same way your production code does**. The glue is a single extension method — [`AddGravity()`](Gravity/DependencySetup.cs) — called from two places:

**Production — [`Program.cs`](Gravity/Program.cs):**

```csharp
builder.Services.AddGravity();
```

**Tests — [`GravityDIAttribute.cs`](Gravity.Test/Data/GravityDIAttribute.cs):**

```csharp
var serviceCollection = new ServiceCollection();
serviceCollection.AddGravity();
```

That single shared call means the object graph is identical. Now look at how similar the *consumers* become:

| | Production controller | Test class |
|---|---|---|
| **File** | [`GravityController.cs`](Gravity/Controllers/GravityController.cs) | [`GravityCalculatorTests.cs`](Gravity.Test/GravityCalculatorTests.cs) |
| **DI trigger** | ASP.NET Core's built-in DI | `[GravityDI]` attribute |
| **Injection style** | Primary constructor parameter | Primary constructor parameter |

**The controller:**

```csharp
[ApiController]
[Route("api/[controller]")]
public class GravityController(IGravityCalculator calculator) : ControllerBase
{
    [HttpGet("calculate")]
    public ActionResult GetGravity([FromQuery] double weightKg, [FromQuery] Planet planet)
    {
        double result = calculator.CalculateForce(weightKg, planet);
        // ...
    }
}
```

**The test class:**

```csharp
[GravityDI]
public class GravityCalculatorTests(IGravityCalculator calculator)
{
    [Test]
    [Arguments(100.0, Planet.Earth, 980.7)]
    public async Task CalculateForce_ReturnsCorrectForce(double mass, Planet planet, double expected)
    {
        var result = calculator.CalculateForce(mass, planet);
        await Assert.That(result).IsEqualTo(expected);
    }
}
```

Notice the symmetry: both classes declare `IGravityCalculator calculator` as a **primary constructor parameter** and simply *use* it. Neither class knows how the calculator was built, what its dependencies are, or how it was registered. The framework — ASP.NET Core in production, TUnit in tests — resolves it from the same [`AddGravity()`](Gravity/DependencySetup.cs) registrations.

### Why this makes TUnit special

Most testing frameworks treat DI as an afterthought. You either `new` up your dependencies by hand, or you bolt on a third-party container with ceremony. TUnit's `DependencyInjectionDataSourceAttribute` makes DI a **first-class concept** at the test-class level:

- **The pattern is identical to production code.** A controller receives `IGravityCalculator` via its constructor; a test class receives it the same way. There is no mental context switch.
- **The registrations are shared.** You call [`AddGravity()`](Gravity/DependencySetup.cs) in both [`Program.cs`](Gravity/Program.cs) and [`GravityDIAttribute`](Gravity.Test/Data/GravityDIAttribute.cs). If you add a new service to the app, the tests get it automatically.
- **Swapping is trivial.** Need a mock? Create a `TestGravityDIAttribute` that registers fakes instead — the test class itself doesn't change at all.
- **Cleanup is built-in.** The `IAsyncDisposable` scope means the `ServiceProvider` (and all its disposable services) is torn down automatically after each test class.

In short: **TUnit lets you write test classes that look, feel, and behave like production consumers of your DI container.** That is the elegance.

## 🧪 TUnit Dependency Injection — Step by Step

### 1. Register your application services in one place

The Gravity API registers all of its services through the [`AddGravity()`](Gravity/DependencySetup.cs) extension method:

```csharp
public static class DependencySetup
{
    public static IServiceCollection AddGravity(this IServiceCollection services)
    {
        services.AddSingleton<IGravityConfiguration, GravityConfiguration>();
        services.AddScoped<IGravityCalculator, GravityCalculator>();
        return services;
    }
}
```

Both the production API ([`Program.cs`](Gravity/Program.cs)) and the test project call this same method — a single source of truth for your object graph.

### 2. Create a custom DI attribute for TUnit

TUnit provides the abstract base class `DependencyInjectionDataSourceAttribute<TScope>`. You subclass it to tell TUnit *how* to build a DI container and *how* to resolve services from it.

The full implementation lives in [`GravityDIAttribute.cs`](Gravity.Test/Data/GravityDIAttribute.cs):

```csharp
public class GravityDIAttribute : DependencyInjectionDataSourceAttribute<GravityDIAttribute.Scope>
{
    // Called once per test class instance — build the container here
    public override Scope CreateScope(DataGeneratorMetadata dataGeneratorMetadata)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddGravity();                       // ← reuse app registrations
        var serviceProvider = serviceCollection.BuildServiceProvider();
        return new Scope(serviceProvider);
    }

    // Called to resolve each constructor parameter
    public override object Create(Scope scope, Type type)
    {
        return scope.ServiceProvider.GetRequiredService(type);
    }

    // Disposable wrapper so the container is cleaned up after tests
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

| Override | Responsibility |
|---|---|
| `CreateScope()` | Builds a fresh `ServiceProvider` using the app's DI registrations |
| `Create()` | Resolves a service from the container for constructor injection |
| `Scope` | Implements `IAsyncDisposable` so the `ServiceProvider` is disposed after the test class is done |

### 3. Apply the attribute to your test class

Decorate the class with `[GravityDI]` and declare the dependencies you need as constructor parameters. TUnit takes care of the rest.

The full test class lives in [`GravityCalculatorTests.cs`](Gravity.Test/GravityCalculatorTests.cs):

```csharp
[GravityDI]                                                    // ← activates DI
public class GravityCalculatorTests(IGravityCalculator calculator)  // ← constructor injection
{
    [Test]
    [Arguments(100.0, Planet.Earth, 980.7)]
    [Arguments(100.0, Planet.Moon, 162.0)]
    [Arguments(100.0, Planet.Mars, 371.0)]
    [Arguments(50.0, Planet.Jupiter, 1239.5)]
    public async Task CalculateForce_ReturnsCorrectForce(
        double mass, Planet planet, double expected)
    {
        // Act
        var result = calculator.CalculateForce(mass, planet);

        // Assert
        await Assert.That(result).IsEqualTo(expected);
    }
}
```

**What happens at runtime:**

1. TUnit sees `[GravityDI]` and calls `CreateScope()` → a new `ServiceProvider` is built.
2. TUnit calls `Create(scope, typeof(IGravityCalculator))` → the [`GravityCalculator`](Gravity/Services/GravityCalculator.cs) instance is resolved (along with its own dependency on [`IGravityConfiguration`](Gravity/Interfaces/IGravityConfiguration.cs)).
3. The resolved service is passed into the constructor.
4. After the tests complete, `Scope.DisposeAsync()` tears down the container.

## 🏗️ Project Structure

```
├── Gravity/                            # ASP.NET Core Web API
│   ├── Controllers/
│   │   └── GravityController.cs        # API endpoint (receives IGravityCalculator via DI)
│   ├── Interfaces/
│   │   ├── IGravityCalculator.cs       # Calculator contract
│   │   └── IGravityConfiguration.cs    # Configuration contract
│   ├── Models/
│   │   └── Planet.cs                   # Planet enum
│   ├── Services/
│   │   ├── GravityCalculator.cs        # Calculator implementation
│   │   └── GravityConfiguration.cs     # Planet gravity factors
│   ├── DependencySetup.cs              # AddGravity() — shared by app and tests ⭐
│   └── Program.cs                      # Calls AddGravity() for production
│
├── Gravity.Test/                       # TUnit test project
│   ├── Data/
│   │   └── GravityDIAttribute.cs       # Calls AddGravity() for tests ⭐
│   └── GravityCalculatorTests.cs       # Receives IGravityCalculator via DI ⭐
│
└── Gravity.slnx                        # Solution file
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
cd Gravity.Test
dotnet run
```

Or with detailed output:

```bash
dotnet run -- --report-trx --output ./test-results
```

> [!NOTE]
> **Why `dotnet run` instead of `dotnet test`?**
>
> TUnit is architecturally different from traditional .NET test frameworks. It uses **source generators** to discover and wire up tests at compile time rather than relying on runtime reflection. A TUnit test project compiles into a **standalone executable** with its own entry point — it is not a class library that needs a separate test host to load it.
>
> Because of this, `dotnet run` simply executes the compiled binary directly. While `dotnet test` still works (TUnit ships a VSTest adapter for IDE compatibility), `dotnet run` is the **recommended** approach because:
>
> | | `dotnet run` | `dotnet test` |
> |---|---|---|
> | **Speed** | Runs the executable directly — no hosting overhead | Spins up the VSTest engine first |
> | **Architecture fit** | TUnit projects *are* executables; running them as such is natural | Treats the project as a test library loaded by an external host |
> | **CLI output** | TUnit's built-in console reporter with rich, real-time output | Standard VSTest output |
> | **Argument passing** | Pass TUnit flags directly after `--` | Must conform to `dotnet test` argument format |

### Run the API

```bash
cd Gravity
dotnet run
```

The API will be available at `http://localhost:5073`.

## 📡 API Usage

### Calculate gravitational force

```http
GET /api/gravity/calculate?weightKg=70&planet=Earth
```

| Parameter   | Type     | Description                                  |
|-------------|----------|----------------------------------------------|
| `weightKg`  | `double` | Mass in kilograms                            |
| `planet`    | `string` | Planet name (`Earth`, `Mars`, `Jupiter`, …)  |

```bash
curl "http://localhost:5073/api/gravity/calculate?weightKg=70&planet=Earth"
```

## 📦 Dependencies

| Project        | Package                      | Version |
|----------------|------------------------------|---------|
| Gravity        | Microsoft.AspNetCore.OpenApi | 10.0.0  |
| Gravity.Test   | TUnit                        | 1.13.8  |

## 🔗 Resources

- [TUnit GitHub](https://github.com/thomhurst/TUnit)
- [TUnit Dependency Injection docs](https://github.com/thomhurst/TUnit#dependency-injection)
- [TUnit — why `dotnet run`?](https://thomhurst.github.io/TUnit/)

## 📄 License

This project is licensed under the **GNU General Public License v3.0** — see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request