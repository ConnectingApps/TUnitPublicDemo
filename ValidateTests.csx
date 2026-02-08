using Gravity.Interfaces;
using Gravity.Models;
using Microsoft.Extensions.DependencyInjection;

// Validate the DI setup
var services = new ServiceCollection();
services.AddGravity();
var serviceProvider = services.BuildServiceProvider();

// Try to resolve IGravityCalculator
var calculator = serviceProvider.GetRequiredService<IGravityCalculator>();

// Test the CalculateForce method
var testCases = new[]
{
    (Mass: 100.0, Planet: Planet.Earth, Expected: 980.7),
    (Mass: 100.0, Planet: Planet.Moon, Expected: 162.0),
    (Mass: 100.0, Planet: Planet.Mars, Expected: 371.0),
    (Mass: 50.0, Planet: Planet.Jupiter, Expected: 1239.5),
    (Mass: 75.0, Planet: Planet.Venus, Expected: 665.25)
};

bool allPassed = true;
foreach (var test in testCases)
{
    var result = calculator.CalculateForce(test.Mass, test.Planet);
    var passed = result == test.Expected;
    Console.WriteLine($"Test: {test.Mass}kg on {test.Planet} = {result}N (expected {test.Expected}N) - {(passed ? "PASS" : "FAIL")}");
    if (!passed) allPassed = false;
}

Console.WriteLine();
Console.WriteLine(allPassed ? "All tests PASSED!" : "Some tests FAILED!");

