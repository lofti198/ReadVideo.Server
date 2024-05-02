using Microsoft.AspNetCore.Mvc;

namespace ReadVideo.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppStatsController : ControllerBase
    {
        [HttpPost("addRecord")]
        public IActionResult AddRecord([FromBody] RecordData data)
        {
            // Process the data here
            // For example, you might save it to a database or perform other actions

            return Ok(new { message = "Record added successfully", data });
        }
    }

    public class RecordData
    {
        public string Email { get; set; }
        public string Domain { get; set; }
    }

}
