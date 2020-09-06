using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
namespace SLDeploye
{
    class MYSQLHandler
    {
        public static Boolean isMySQLRunning()
        {
            Process[] processes = Process.GetProcessesByName("mysqld");
            if (processes.Length > 0)
            {
                return true;
            }
            return false;
        }
        public static void CheckStatus()
        {
            bool mysqlrun = isMySQLRunning();
            while (!mysqlrun)
            {
                Console.WriteLine("Your MySQL Service is NOT Running. Please activate in XAMPP manually before continue.");
                Console.WriteLine("Retrying in 5 seconds..");
                Thread.Sleep(5000);
                mysqlrun = isMySQLRunning();
                if (mysqlrun)
                {
                    Console.WriteLine("Successfully running MySQL Service");
                }
            }
            
        }
        public static void InitDB(DeployeSetting setting, DB database )
        {
            Console.WriteLine("Creating database...");
            string arg = string.Join(" ", " -u", database.user, "-e", "\"CREATE DATABASE " + database.db + "\"");

            ProcessManager.ProcessGenerator(setting.MYSQL, arg,setting);
            Console.WriteLine("Succesfully create database.");

        }
        
        public class DB
        {
            public DB(string db_, string us_, string p_)
            {
                db = db_;
                user = us_;
                pass = p_;
            }
            public DB() { }

            public string db = "";
            public string user = "";
            public string pass = "";

            public bool correct()
            {
                if (db.Length > 1 && user.Length > 1)
                {
                    return true;
                }
                return false;
            }
        }
    }
}
