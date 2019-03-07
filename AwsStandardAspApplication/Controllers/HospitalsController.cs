using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AwsStandardAspApplication.Models;
using Microsoft.AspNetCore.Mvc;

namespace AwsStandardAspApplication.Controllers
{
    //[Authorize(Policy = Policies.Hospital)]
    [Route("api/[controller]")]
    [ApiController]
    public class HospitalsController : ControllerBase
    {
        private readonly DataContext _db;

        public HospitalsController(DataContext db)
        {
            _db = db;
        }

        // route: GET /api/hospitals
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            await _db.MockQuery(); // simulate database query
            var model = new List<String>
            {
                "General Hospital",
                "Sacred Heart"
            };
            return Ok(model);
        }

        // route: POST /api/hospitals
        [HttpPost]
        public async Task<IActionResult> CreateHospital(HospitalDTO hospitalDTO)
        {
            await _db.MockQuery(); // simulate database query
            return CreatedAtAction(nameof(GetHospital), new { id = 2 });
        }

        // route: POST /api/hospitals/search
        [HttpPost]
        public async Task<IActionResult> SearchHospitals(SearchHospitalDTO hospitalDTO)
        {
            await _db.MockQuery(); // simulate database query
            var results = new List<string>();
            return Ok(results);
        }

        public class SearchHospitalDTO
        {
            public string Name { get; set; }
            public string ZipCode { get; set; }
        }

        // route: GET /api/hospitals/forzipcode/30009
        [HttpGet("forzipcode/{zipcode}")]
        public async Task<IActionResult> GetForZipCode(string zipcode)
        {
            await _db.MockQuery(); // simulate database query
            var model = new List<String>
            {
                "General Hospital",
                "Sacred Heart"
            };
            return Ok(model);
        }

        // route: GET /api/hospitals/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetHospital(int id)
        {
            await _db.MockQuery(); // simulate database query
            var hospital = "Sacred Heart";
            if (hospital == null)
                return NotFound();
            return Ok(hospital);
        }

        // route: DELETE /api/hospitals/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteHospital(int id)
        {
            await _db.MockQuery(); // simulate database query
            var hospital = "Sacred Heart";
            if (hospital == null)
                return NotFound();
            await _db.MockQuery(); // simulate database delete
            return Ok();
        }
    }
}