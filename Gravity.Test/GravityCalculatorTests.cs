using Gravity.Interfaces;
using Gravity.Models;
using Gravity.Test.Data;

namespace Gravity.Test;

[GravityDI]
public class GravityCalculatorTests(IGravityCalculator calculator)
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



