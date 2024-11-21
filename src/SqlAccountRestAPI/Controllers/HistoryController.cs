using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SqlAccountRestAPI.Core;
using SqlAccountRestAPI.Helpers;

namespace SqlAccountRestAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class HistoryController(ILogger<HistoryController> logger) : ControllerBase
{
    [HttpGet("logs")]
    public IActionResult GetHistoryLogs([FromQuery(Name = "ts")] long unixTimestamp)
    {
        // Convert Unix timestamp to DateTime
        var date = DateTimeOffset.FromUnixTimeMilliseconds(unixTimestamp).DateTime;

        // Get the current application folder
        // Get all files in the application directory matching the log file pattern
        var matchingFiles = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "log*.txt")
            .Union(Directory.EnumerateFiles(AppContext.BaseDirectory, "log*.txt"))
            .Where(file => IsFileDateMatch(file, date))
            .Select(file => Path.GetFileName(file)) // Return only the file name
            .ToList();

        return Ok(matchingFiles.Select(f => new
        {
            filename = f
        }));
    }

    [HttpGet("log-detail")]
    public IActionResult GetLogFileContent([FromQuery(Name = "fn")][Required] string fileName)
    {
        if (string.IsNullOrEmpty(fileName) || fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
        {
            return BadRequest($"Invalid filename (fileName).");
        }

        // Get the current working directory
        var appDirectory = Directory.GetCurrentDirectory();

        // Construct the full path to the file
        var filePath = Path.Combine(appDirectory, fileName);

        // Check if the file exists
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound($"File '{fileName}' not found.");
        }

        string content = ReadLogFileContent(filePath);
        return Ok(content);
    }


    [HttpGet("download")]
    public IActionResult Download([FromQuery(Name = "fn")][Required] string fileName)
    {
        if (string.IsNullOrEmpty(fileName) || fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
        {
            return BadRequest($"Invalid filename (fileName).");
        }

        // Get the current working directory
        var appDirectory = Directory.GetCurrentDirectory();

        // Construct the full path to the file
        var filePath = Path.Combine(appDirectory, fileName);

        // Check if the file exists
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound($"File '{fileName}' not found.");
        }

        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        // Return the file as a downloadable attachment
        return File(fileStream, "text/plain", fileName);
    }
    private string ReadLogFileContent(string filePath)
    {
        try
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(fileStream))
            {
                return reader.ReadToEnd();
            }
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException($"Failed to read log file: {ex.Message}", ex);
        }
    }
    private bool IsFileDateMatch(string filePath, DateTime targetDate)
    {
        // Extract date and hour from the filename
        var fileName = Path.GetFileNameWithoutExtension(filePath); // e.g., "log2024072410"
        if (fileName == null || !fileName.StartsWith("log"))
        {
            return false;
        }

        // Match the filename pattern "logYYYYMMDDhh"
        var datePart = fileName.Substring(3); // "2024072410"
        if (datePart.Length != 10)
        {
            return false;
        }

        // Parse the date and hour from the filename
        var year = int.Parse(datePart.Substring(0, 4));
        var month = int.Parse(datePart.Substring(4, 2));
        var day = int.Parse(datePart.Substring(6, 2));

        // Compare the file's date with the target date
        var fileDate = new DateTime(year, month, day);
        logger.LogInformation($@"File date: {fileDate}, Target Date: {targetDate}");
        return fileDate.Date == targetDate.Date;
    }
}