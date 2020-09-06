using System;
using System.Diagnostics;
using System.IO;
namespace SLDeploye
{
    using System.Text.Json;
    using System.Text.Json.Serialization;

    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Net;
    using System.Text;
    using System.Threading;
    class Program
    {
        private ProcessStartInfo psInfo;
        private static Program MainProgram = new Program();
        public DeployeSetting setting = new DeployeSetting();

        static void Main(string[] args)
        {

            bool connected = false;
            while (!connected) {
                connected = Identifier.run();
                if (!connected) {
                    Console.WriteLine("Retrying in 5 seconds...");
                    Thread.Sleep(5000);
                }
            }
            ProcessManager.KillAll("php.exe", MainProgram.setting);
            Thread.Sleep(2000);
            Console.Clear();
            if (File.Exists(DeployeSetting.SETTING_FILE))
            {
                string jsonString = File.ReadAllText(DeployeSetting.SETTING_FILE);
                MainProgram.setting = JsonSerializer.Deserialize<DeployeSetting>(jsonString);
            }
            while (true)
            {
                Console.WriteLine(" ");
                Console.WriteLine("============================");
                Console.WriteLine("1. Clone Project");
                Console.WriteLine("2. Install Project");
                Console.WriteLine("3. Run Project");
                Console.WriteLine("4. Renew Project");
                Console.WriteLine("5. Migrate");
                Console.WriteLine("9. Settings");
                Console.WriteLine("10. Make ENV");
                Console.WriteLine();
                Console.Write("Number : ");
                string input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        MainProgram.CloneProject();
                        break;
                    case "2":
                        bool res = MainProgram.InstallProject();
                        if (!res)
                        {
                            Console.WriteLine("Project installation failure!");
                        }
                        break;
                    case "3":
                        MainProgram.RunProject();
                        break;
                    case "4":
                        MainProgram.GitPull();
                        break;
                    case "5":
                        MainProgram.Migrate();
                        break;
                    case "9":
                        MainProgram.Setting();
                        break;
                    case "0":
                        Environment.Exit(0);
                        break;
                    case "10":

                        break;
                    default:
                        Console.WriteLine("Invalid input, please enter a numeric value!");
                        break;
                }

            }
            Environment.Exit(0);
        }
        private string normalizeQuote(string val)
        {
            val = val.Replace("\"", "");
            return val;
        }

        private void Setting()
        {
            bool phpCorrect = false;
            bool mysqlCorrect = false;
            bool projectDir = false;
            //autodetect for php
            foreach (string fpath in DeployeSetting.DEFAULT_PHP_PATH)
            {
                if (File.Exists(fpath))
                {
                    setting.PHP = fpath;
                    phpCorrect = true;
                    Console.WriteLine("Automatically detects php.exe on " + setting.PHP);
                }
            }
            foreach (string fpath in DeployeSetting.DEFAULT_MYSQL_PATH)
            {
                if (File.Exists(fpath))
                {
                    mysqlCorrect = true;
                    setting.MYSQL = fpath;
                    Console.WriteLine("Automatically detects mysql.exe on " + setting.MYSQL);
                }
            }
            Console.WriteLine();
            while (!setting.isSettingCorrect())
            {
                if (!phpCorrect)
                {
                    

                    Console.Write("PHP Path : ");
                    setting.PHP = normalizeQuote(Console.ReadLine());
                    if (File.Exists(setting.PHP))
                    {
                        phpCorrect = true;
                    }
                    else
                    {
                        Console.WriteLine("Incorrect path given");
                    }

                }
                if (!mysqlCorrect)
                {

                    Console.Write("MYSQL Path : ");
                    setting.MYSQL = normalizeQuote(Console.ReadLine());
                    if (File.Exists(setting.MYSQL))
                    {
                        mysqlCorrect = true;
                    }
                    else
                    {
                        Console.WriteLine("Incorrect path given");
                    }
                }
                if (!projectDir)
                {
                    Console.Write("Project Artisan Path : ");
                    setting.PROJECT = normalizeQuote(Console.ReadLine());
                    if (File.Exists(setting.PROJECT))
                    {
                        projectDir = true;
                    }
                    else
                    {
                        Console.WriteLine("Incorrect path given");
                    }
                }
            }
            setting.save();

        }
        public bool InstallProject()
        {

            //meminta nama database
            while (!setting.isSettingCorrect())
            {
                Setting();
            }
            MYSQLHandler.DB database = new MYSQLHandler.DB();

            while (!database.correct())
            {
                Console.Write("Database Name : ");
                string db = Console.ReadLine().ToLower();
                Console.WriteLine();
                Console.Write("Database User : ");
                string usr = Console.ReadLine();
                Console.WriteLine();
                Console.Write("Password : ");
                string pass = Console.ReadLine();
                database = new MYSQLHandler.DB(db, usr, pass);
                if (database.correct())
                {
                    break;
                }
            }
            //env
            bool env = false;
            while (!env)
            {
                env = Env.MakeEnv(database, setting);
            }

            //check mysql running
            MYSQLHandler.CheckStatus();
            //create database
            MYSQLHandler.InitDB(setting, database);
            //install composer
            Console.WriteLine("Installing project....(this may take a while)");
            string composerPath = Path.GetDirectoryName(setting.PROJECT) + @"\sl.cmd";


            using (StreamWriter sw = File.CreateText(composerPath))
            {
                sw.WriteLine("composer install");
            }

            //check mysql is runing

            ProcessManager.SimpleProcessGenerator(composerPath, Path.GetDirectoryName(setting.PROJECT), "");
            string key = "artisan key:generate";
            ProcessManager.ProcessGenerator(setting.PHP, key, setting);
            string clear = "artisan config:clear";
            ProcessManager.ProcessGenerator(setting.PHP, clear, setting);
            ProcessManager.ProcessGenerator(setting.PHP, key, setting);

            string arg = "artisan migrate:fresh --seed";
            ProcessManager.ProcessGenerator(setting.PHP, arg, setting);
            Console.WriteLine("Instalation complete.");
            return true;
        }
        private void CloneProject()
        {
            Console.WriteLine();
            Console.Write("GIT URL : ");
            string gitURL = Console.ReadLine();
            string arg = string.Join(" ", "clone", " ", gitURL);
            ProcessManager.ProcessGenerator(@"git.exe", arg, setting);
        }

        public void GitPull()
        {
            Console.WriteLine("Updating project...");
            string arg = "pull origin master";
            ProcessManager.ProcessGenerator(@"git.exe", arg, setting);
        }
        public void RunProject()
        {
           
            Console.WriteLine("Checking settings...");
            while (!setting.isSettingCorrect())
            {
                Setting();
            }
            string clear = "artisan cache:clear";
            ProcessManager.ProcessGenerator(setting.PHP, clear, setting);
            //sini
            ProcessManager.KillAll("php.exe",setting);
            Console.WriteLine("Starting server...");
            Thread.Sleep(2000);
            Console.Clear();
            string arg = "artisan serve";
            ProcessManager.ProcessGenerator(setting.PHP, arg, setting);
            Console.WriteLine("Clearing resource...");
            
        }
        public void Migrate() {
            Console.WriteLine("Checking settings...");
            while (!setting.isSettingCorrect())
            {
                Setting();
            }
            bool isInputCorrect = false;
            string arg = "artisan migrate:fresh";
            while (!isInputCorrect) {
                Console.Write("With Seed ? Y/N : ");
                string input = Console.ReadLine().ToString().ToLower();
                if (input.Equals("y")) {
                    arg = arg + " --seed";
                    isInputCorrect = true;
                } else if (input.Equals("n")){
                    isInputCorrect = true;
                }
                else {
                    isInputCorrect = false;
                    Console.WriteLine("Input is not correct, please type y or n.");
                }
            }
            Console.WriteLine("Please wait, this may take a while.");
            ProcessManager.ProcessGenerator(setting.PHP, arg, setting);
            Console.WriteLine("Migration complete.");
        }
    }

    }

