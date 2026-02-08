using Gravity.Interfaces;
using Gravity.Models;

namespace Gravity.Services;

public class GravityConfiguration : IGravityConfiguration
{
    private readonly Dictionary<Planet, double> _gravityFactors = new()
    {
        { Planet.Mercury, 3.7 },
        { Planet.Venus, 8.87 },
        { Planet.Earth, 9.807 },
        { Planet.Moon, 1.62 },
        { Planet.Mars, 3.71 },
        { Planet.Jupiter, 24.79 },
        { Planet.Saturn, 10.44 },
        { Planet.Uranus, 8.69 },
        { Planet.Neptune, 11.15 }
    };

    public double GetGravityFactor(Planet planet)
    {
        if (_gravityFactors.TryGetValue(planet, out var factor))
        {
            return factor;
        }
        throw new ArgumentException($"Gravity factor for {planet} is not configured.");
    }
}
