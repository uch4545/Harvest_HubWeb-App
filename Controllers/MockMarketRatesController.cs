using Microsoft.AspNetCore.Mvc;
using HarvestHub.WebApp.Models;
using System;
using System.Collections.Generic;

namespace HarvestHub.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MockMarketRatesController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetMockRates()
        {
            var mockData = new List<MarketRate>
            {
               // new MarketRate { CropName = "Wheat", CurrentRate = 4200, LastUpdated = DateTime.Now },
               // new MarketRate { CropName = "Rice", CurrentRate = 4800, LastUpdated = DateTime.Now },
              //  new MarketRate { CropName = "Sugarcane", CurrentRate = 220, LastUpdated = DateTime.Now },
              //  new MarketRate { CropName = "Cotton", CurrentRate = 8700, LastUpdated = DateTime.Now },
              //  new MarketRate { CropName = "Maize", CurrentRate = 3100, LastUpdated = DateTime.Now },
            };

            return Ok(mockData);
        }
    }
}
