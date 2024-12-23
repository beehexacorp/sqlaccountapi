using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.IO.Compression;
namespace SqlAccountRestAPI.Helpers

{
    public static class SystemHelper
    {
        public static async Task<string> RunPowerShellCommand(string command)
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

                    await Task.Run(() => process.WaitForExit());


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
        public static Dictionary<string, object> ReadJsonFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found at path: {filePath}");

            try
            {
                string jsonContent = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<Dictionary<string, object>>(jsonContent) ?? new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to read or parse the JSON file.", ex);
            }
        }
        public static void WriteJsonFile(string filePath, Dictionary<string, object> updatedData)
        {
            Dictionary<string, object> existingData;

            // Read the existing file content or initialize an empty dictionary
            if (File.Exists(filePath))
            {
                try
                {
                    string jsonContent = File.ReadAllText(filePath);
                    existingData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonContent) ?? new Dictionary<string, object>();
                }
                catch
                {
                    existingData = new Dictionary<string, object>();
                }
            }
            else
            {
                existingData = new Dictionary<string, object>();
            }

            // Update or add keys
            foreach (var item in updatedData)
            {
                existingData[item.Key] = item.Value;
            }

            // Write the updated data back to the file
            try
            {
                string newJsonContent = JsonSerializer.Serialize(existingData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, newJsonContent);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to write data to the JSON file.", ex);
            }
        }
        public static async Task<string> GetCliConfigurationFilePath(){
            var npmFolder = await RunPowerShellCommand("npm -g root");
            var configPath = Path.Combine(npmFolder, ApplicationConstants.NPM_PACKAGE_NAME,
                ApplicationConstants.CONFIGURATION_FOLDER_NAME, ApplicationConstants.CONFIGURATION_FILE_NAME);
            return configPath;
        }

    }
}