using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace SLDeploye
{
    class Env
    { 
        public static Boolean MakeEnv(MYSQLHandler.DB db,DeployeSetting setting)
        {
            try
            {
                Console.WriteLine("Downloading environment...");
                string urlAddress = "https://raw.githubusercontent.com/laravel/laravel/master/.env.example";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader readStream = null;

                    if (String.IsNullOrWhiteSpace(response.CharacterSet))
                        readStream = new StreamReader(receiveStream);
                    else
                        readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

                    string data = readStream.ReadToEnd();

                    response.Close();
                    readStream.Close();

                    string envPath = Path.GetDirectoryName(setting.PROJECT) + @"\.env";
                    //delete env if exists
                    if (File.Exists(envPath))
                    {
                        File.Delete(envPath);
                    }
                    using (StreamWriter sw = File.CreateText(envPath))
                    {
                        sw.WriteLine(data);
                    }
                    string text = File.ReadAllText(envPath);
                    text = text.Replace("DB_DATABASE=laravel", "DB_DATABASE=" + db.db);
                    text = text.Replace("DB_USERNAME=root", "DB_USERNAME=" + db.user);
                    File.WriteAllText(envPath, text);
                    Console.WriteLine("Succesfully created environment file.");


                }
            }
            catch (WebException e)
            {
                Console.WriteLine("Failed to connect, please check your connection.");
                return false;
            }
            return true;
        }

    }
}
