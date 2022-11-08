using AzureCloner.Data;
using AzureCloner.Helpers;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace AzureCloner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("------------------------------ WELCOME TO AZURE CLONER ------------------------------");
            Console.WriteLine("-                                                                                   -");
            Console.WriteLine("- This software will clone all your project git repositories into a given directory -");
            Console.WriteLine("-                                                                                   -");
            Console.WriteLine("-------------------------------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\nBe aware that if your company sets two factor authentication you must provide an\nAccess Token instead of a password.\n\nIf you have any doubt or if you don't know how to generate an access token\nplease visit: https://bit.ly/2MWlBHW");
            Console.WriteLine("\nYou will also need git to be already installed on the system, if don't have it yet\nor you don't know how to install it please visit: https://bit.ly/1WQ50nb");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\n\tProject URL: ");

            string projectUrl = Console.ReadLine();

            Console.Write("\tLocal Destination: ");

            string directory = Console.ReadLine();

            Console.Write("\tEmail: ");

            string email = Console.ReadLine();

            Console.Write("\tPassword or Token: ");

            string password = PasswordHelper.ReadPassword();

            Console.Write("\n\tEmbed Auth in Git Config (Y)es/(N)o: ");

            string embedAuthString = Console.ReadLine();
            embedAuthString = embedAuthString.ToLower();

            if (string.IsNullOrEmpty(projectUrl)
                || string.IsNullOrEmpty(directory)
                || string.IsNullOrEmpty(email)
                || string.IsNullOrEmpty(password)
                || string.IsNullOrEmpty(embedAuthString))
            {
                Console.Clear();
                Console.WriteLine("Execution aborted, you must provide all info to execute this tool.");
                Console.WriteLine("Press any key to exit.");
            }
            else
            {
                bool embedAuth = embedAuthString == "y";
                if (!embedAuth && embedAuthString != "n")
                {
                    Console.WriteLine("\"Embed Auth in Git Config\" should be \"y\" (for yes) or \"n\" (for no). Assuming no..");
                    embedAuth = false;
                }

                WebRequest webRequest = WebRequest.CreateHttp($"{projectUrl}/_apis/git/repositories?api-version=1.0");

                ASCIIEncoding asciiEncoding = new ASCIIEncoding();

                byte[] bytes = asciiEncoding.GetBytes($"{email}:{password}");

                string authData = Convert.ToBase64String(bytes);

                webRequest.Headers.Add(HttpRequestHeader.Authorization, $"Basic {authData}");

                webRequest.Headers.Add(HttpRequestHeader.Accept, "application/json");

                WebResponse webResponse = webRequest.GetResponse();

                string json = string.Empty;

                using (StreamReader streamReader = new StreamReader(webResponse.GetResponseStream()))
                {
                    json = streamReader.ReadToEnd();
                }

                DevOpsResponse devOpsResponse = JsonConvert.DeserializeObject<DevOpsResponse>(json);

                Directory.CreateDirectory(directory);

                DevOpsRepo[] devOpsRepos = devOpsResponse.DevOpsRepos.ToList().OrderBy(i => i.Name).ToArray();

                int currentRepoIndex = 1;

                Environment.CurrentDirectory = directory;

                foreach (DevOpsRepo devOpsRepo in devOpsRepos)
                {
                    string repoRemoteUrl = devOpsRepo.RemoteUrl;
                    if (embedAuth)
                    {
                        repoRemoteUrl = repoRemoteUrl.Replace("://", $"://{HttpUtility.UrlEncode(email)}:{password}@");
                    }

                    Console.Clear();

                    Console.ForegroundColor = ConsoleColor.Green;

                    Console.WriteLine($"Cloning Repository: {devOpsRepo.Name}");

                    Console.WriteLine($"Repository {currentRepoIndex}/{devOpsResponse.Count}");

                    Console.ForegroundColor = ConsoleColor.DarkGray;

                    using (Process process = new Process())
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo();

                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                        startInfo.FileName = "git";

                        startInfo.Arguments = $"clone {repoRemoteUrl}";

                        process.StartInfo = startInfo;

                        process.EnableRaisingEvents = true;

                        process.Start();

                        process.WaitForExit();
                    };

                    currentRepoIndex++;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Clear();
                Console.WriteLine("All repos cloned with success!!");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Press any key to exit.");
            }

            Console.ReadKey();
        }
    }
}
