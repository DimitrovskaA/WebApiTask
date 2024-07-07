using Microsoft.AspNetCore.Mvc;
using Task_Bank.Data;
using Task_Bank;

[ApiController]
[Route("api/[controller]")]
public class SwiftMessageController : ControllerBase
{
    private readonly SwiftMessageService _service;
    private readonly ILogger<SwiftMessageController> _logger;
    public SwiftMessageController(SwiftMessageService service, ILogger<SwiftMessageController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    public IActionResult ReceiveMessage([FromBody] SwiftMessage message)
    {
        _logger.LogInformation("Recieved a new Swift message.");
        try
        {
            _service.SaveMessage(message);
            _logger.LogInformation("Swift Message saved successfully.");
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while saving the message.");
            return StatusCode(500, "Internal Server error.");
        }       
    }

    [HttpPost("Upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("Uploaded file is empty or null.");
            return BadRequest("File is empty or null");
        }

        try
        {
            _logger.LogInformation("Reading the uploaded file.");
            string content = await _service.ReadFileAsync(file);
            _logger.LogInformation("File read successfully.");
            return Ok(new { FileContent = content });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while uploading the file.");
            return StatusCode(500, "Internal server error");
        }
    }
}
