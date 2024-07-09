using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadVideo.Server.Data;
using ReadVideo.Server.Middleware.Attributes;
using ReadVideo.Server.Models;

namespace ReadVideo.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddRecordController : ControllerBase
    {
        private readonly DCStatsDbContext _context;

        public AddRecordController(DCStatsDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult AddRecord([FromBody] CampaignLaunchStatsCreateDto dataDto)
        {
            if (dataDto == null)
            {
                return BadRequest("No data provided");
            }


            string domain = ExtractDomain(dataDto.Domain);


            // Check if an entry with the same email and domain already exists
            var existingEntry = _context.CampaignLaunchStats
                                .Any(x => x.Email == dataDto.Email && x.Domain == domain);

            if (existingEntry)
            {
                return Conflict(new { message = "An entry with the same email and domain already exists." });
            }

            var data = new CampaignLaunchStatsElement
            {
                Email = dataDto.Email,
                Domain = domain
            };

            _context.CampaignLaunchStats.Add(data);
            _context.SaveChanges();

            return Ok(new { message = "Record added successfully", data });
        }

        [HttpGet("view-all")]
        // [ApiKey]
        public async Task<IActionResult> GetAllRecords([FromQuery] int delete = 0)
        {
            var data = await _context.CampaignLaunchStats.ToListAsync();

            if (delete == 1)
            {
                _context.CampaignLaunchStats.RemoveRange(_context.CampaignLaunchStats);
                await _context.SaveChangesAsync();
            }

            return Ok(data);
        }

        //[HttpGet("view-all")]
        //public async Task<IActionResult> GetAllRecords()
        //{
        //    var data = await _context.CampaignLaunchStats.ToListAsync();
        //    return Ok(data);
        //}


        private string ExtractDomain(string input)
        {
            if (Uri.TryCreate(input, UriKind.Absolute, out Uri uri))
            {
                return uri.Host;
            }
            return input;
        }
    }


}
