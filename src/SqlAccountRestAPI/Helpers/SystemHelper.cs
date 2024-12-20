using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
namespace SqlAccountRestAPI.Helpers
{
    public static class SystemHelper
    {
        public static string RunPowerShellCommand(string command)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe", 
                    Arguments = command,         
                    RedirectStandardOutput = true, 
                    RedirectStandardError = true,  
                    UseShellExecute = false,     
                    CreateNoWindow = true,        
                    Verb = "runas"
                };

                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();

                    
                    string result = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    
                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine("Error: " + error);
                    }

                    return result.Trim(); 
                }
            }
            catch (Exception ex)
            {
                
                return "Exception: " + ex.Message;
            }
        }
        public static async Task DownloadFileAsync(string url, string destinationPath)
        {
            try
            {
                // Check if the file already exists and delete it if it does
                string directoryPath = Path.GetDirectoryName(destinationPath)!;
                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using HttpClient client = new HttpClient();
                using HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                // Create a new file to store the downloaded content
                await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await using var contentStream = await response.Content.ReadAsStreamAsync();
                await contentStream.CopyToAsync(fileStream);

                Console.WriteLine($"File downloaded successfully to {destinationPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading file: {ex.Message}");
            }
        }
    }
}