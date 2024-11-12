using System;
using System.Text.Json.Nodes;
using System.Xml;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using SqlAccountRestAPI.Core;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.InteropServices;


namespace SqlAccountRestAPI.Helpers;

public class SqlAccountingLoginHelper : SqlAccountingORM
{
    private readonly string _credentialsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "credentials.enc");

    public SqlAccountingLoginHelper(SqlAccountingFactory sqlAccountingFactory) 
        : base(sqlAccountingFactory)
    {
        Login();
    }
    public void Login(){
        var loginInfo = ReLogin();
        if (loginInfo.Count == 0)
            return;
        string username = loginInfo[0];
        string password = loginInfo[1];
        try
        {
            base.Login(username, password);
        }
        catch (Exception e)
        {
            ClearStoredCredentials();
            throw new InvalidOperationException("Login failed: " + e.Message);        
        }
    }

    public override void Login(string username, string password){
        try{
            base.Login(username, password);
            SaveEncryptedCredentials(username, password);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Login failed: " + e.Message);        
        }  
    }
        

    public string GenerateKeyFromSystemInfo()
    {
        string machineGuid = GetMachineGuid();

        string keySource = $"{machineGuid}";

        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(keySource));
            return Convert.ToBase64String(hash).Substring(0, 32); 
        }
    }

    public string GetMachineGuid()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using (var registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography"))
            {
                return registryKey?.GetValue("MachineGuid")?.ToString() ?? throw new Exception("MachineGuid not found.");
            }
        }
        else
        {
            using var rng = RandomNumberGenerator.Create();
            var guidBytes = new byte[16];
            rng.GetBytes(guidBytes);
            return new Guid(guidBytes).ToString();
        }
    }

    public void SaveEncryptedCredentials(string username, string password)
    {
        var data = $"{username}:{password}";
        var encrypted = EncryptAES(data, GenerateKeyFromSystemInfo());
        File.WriteAllText(_credentialsPath, encrypted);
    }

    public string EncryptAES(string plainText, string key)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
            aes.IV = new byte[16];

            using (var encryptor = aes.CreateEncryptor())
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }

                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    public string DecryptAES(string cipherText, string key)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
            aes.IV = new byte[16];

            using (var decryptor = aes.CreateDecryptor())
            using (var ms = new MemoryStream(Convert.FromBase64String(cipherText)))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (var sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }

    public List<string> ReLogin()
    {
        if (File.Exists(_credentialsPath))
        {   
            var encrypted = File.ReadAllText(_credentialsPath);
            var decrypted = DecryptAES(encrypted, GenerateKeyFromSystemInfo());
            var credentials = decrypted.Split(':');

            return new List<string>{credentials[0], credentials[1]};
            
        }
        return new List<string>{};
    }
    
    public void ClearStoredCredentials()
    {
        if (File.Exists(_credentialsPath))
        {
            File.Delete(_credentialsPath);
        }
    }
}