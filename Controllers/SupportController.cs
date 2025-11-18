using Microsoft.AspNetCore.Mvc;
using ReadVideo.Server.Models;
using ReadVideo.Server.Services.Support;

namespace ReadVideo.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupportController : ControllerBase
    {
        private readonly ISupportRequestService _supportRequestService;
        private readonly ILogger<SupportController> _logger;

        public SupportController(
            ISupportRequestService supportRequestService,
            ILogger<SupportController> logger)
        {
            _supportRequestService = supportRequestService;
            _logger = logger;
        }

        /// <summary>
        /// Test endpoint to verify the controller is working
        /// </summary>
        /// <returns>Simple status message</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            return Ok(new
            {
                message = "Support API is running",
                timestamp = DateTime.UtcNow,
                endpoint = "/api/support"
            });
        }

        /// <summary>
        /// Receives and processes support requests from AiPickerUI
        /// </summary>
        /// <param name="request">Support request containing email, question, and datacol configuration</param>
        /// <returns>Response indicating success/failure with ticket ID</returns>
        [HttpPost]
        [ProducesResponseType(typeof(SupportRequestResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SupportRequestResponseDto>> SubmitSupportRequest(
            [FromBody] SupportRequestDto request)
        {
            try
            {
                _logger.LogInformation(
                    "Received support request from {Email} for URL: {Url}",
                    request.Email,
                    request.CurrentUrl);

                //// Validate request
                //if (!ModelState.IsValid)
                //{
                //    return BadRequest(ModelState);
                //}

                // Process the support request
                var result = await _supportRequestService.ProcessSupportRequestAsync(request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing support request from {Email}", request.Email);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new SupportRequestResponseDto
                    {
                        Success = false,
                        Message = "An error occurred while processing your request. Please try again later."
                    });
            }
        }
    }
}
