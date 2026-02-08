using Gravity.Interfaces;
using Gravity.Models;

namespace Gravity.Services;

public class GravityCalculator : IGravityCalculator
{
    private readonly IGravityConfiguration _configuration;

    public GravityCalculator(IGravityConfiguration configuration)
    {
        _configuration = configuration;
    }

    public double CalculateForce(double massInKg, Planet planet)
    {
        double factor = _configuration.GetGravityFactor(planet);
        return massInKg * factor;
    }
}
