using Gravity.Models;

namespace Gravity.Interfaces;

public interface IGravityCalculator
{
    double CalculateForce(double massInKg, Planet planet);
}
