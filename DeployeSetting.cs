using System;
using System.IO;
using System.Text.Json;

public class DeployeSetting
{
	public string PHP { get; set; }
	public string MYSQL { get; set; }
	public string PROJECT { get; set; }
    public const string SETTING_FILE = "settings.json";
    public static string[] DEFAULT_PHP_PATH = { @"C:\\xampp\\php\\php.exe" };
    public static string[] DEFAULT_MYSQL_PATH = { @"C:\\xampp\\mysql\\bin\\mysql.exe"};
     public bool isSettingCorrect()
    {
        if (!File.Exists(PHP))
        {
            
            return false;
        }
        if (!File.Exists(MYSQL))
        {
            return false;
        }
        if (!File.Exists(PROJECT))
        {
            return false;

        }
        if (!Path.GetFileName(PROJECT).Equals("artisan"))
        {
            return false;
        }


        return true;
    }
    public void save() {
        if (isSettingCorrect())
        {
            string jsonString = JsonSerializer.Serialize(this);
            File.WriteAllText(SETTING_FILE, jsonString);
            Console.WriteLine("Success saving configurations.");

        }
    }
    public DeployeSetting()
	{
	}
}
