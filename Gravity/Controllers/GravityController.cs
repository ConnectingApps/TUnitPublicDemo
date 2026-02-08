using Gravity.Interfaces;
using Gravity.Models;
using Microsoft.AspNetCore.Mvc;

namespace Gravity.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GravityController(IGravityCalculator calculator) : ControllerBase
{
    [HttpGet("calculate")]
    public ActionResult GetGravity([FromQuery] double weightKg, [FromQuery] Planet planet)
    {
        try
        {
            double result = calculator.CalculateForce(weightKg, planet);
            return Ok(new 
            { 
                Mass = weightKg, 
                Planet = planet.ToString(), 
                ForceNewtons = result 
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
