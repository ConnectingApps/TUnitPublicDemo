using Gravity.Models;

namespace Gravity.Interfaces;

public interface IGravityConfiguration
{
    double GetGravityFactor(Planet planet);
}
