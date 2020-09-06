using System;
using System.Collections.Generic;
using System.Text;
using DeviceId;
using DeviceId.Formatters;
using DeviceId.Encoders;
using System.Security.Cryptography;
using System.Net;
using System.IO;

namespace SLDeploye
{
    class Identifier
    {
        private static string GetIdentity()
        {
            string deviceId = new DeviceIdBuilder()
    .AddProcessorId()
    .AddMotherboardSerialNumber()
    .UseFormatter(new HashDeviceIdFormatter(() => SHA256.Create(), new Base64UrlByteArrayEncoder()))
    .ToString();
            return deviceId;
        }
        public static string toString() {
            return GetIdentity() + "@" + Environment.UserName;
        }
        public static Boolean run()
        {
            try
            {
                Console.WriteLine("Identified as " + toString());
                var byteArray = Encoding.UTF8.GetBytes("identity=" +toString());
                Console.WriteLine("In order to run this program, this computer must be connected to the internet.");
                Console.WriteLine("Connecting...");
                WebRequest request = WebRequest.Create("https://publikasi.tech/api/check");
                request.Timeout = 60000;
                request.Credentials = CredentialCache.DefaultCredentials;
                request.Method = "POST";
                request.ContentLength = byteArray.Length;
                request.ContentType = "application/x-www-form-urlencoded";
                Stream dataStream = request.GetRequestStream();
                // Write the data to the request stream.
                dataStream.Write(byteArray, 0, byteArray.Length);
                // Close the Stream object.
                dataStream.Close();

                // Get the response.

                WebResponse response = request.GetResponse();
                
                if (((HttpWebResponse)response).StatusCode != HttpStatusCode.OK)
                {
                    response.Close();
                    Console.WriteLine("Failure");
                    return false;
                }
                // Close the response.
                response.Close();
                Console.WriteLine("Successfully connected to the internet");
            }
            catch (WebException e) {
                Console.WriteLine("Failed to connect " + e.Response);
                return false;
            }
            return true;
        }
    }

}
