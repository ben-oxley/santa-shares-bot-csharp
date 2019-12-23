using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace santa_shares
{
    class Program
    {
        private static readonly string LocalFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/santa/";
        private static readonly string LocalFile = LocalFolder+"/user.txt";
        public static readonly string APIUrl = @"http://santa-shares.azurewebsites.net/api/";
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Config in: "+LocalFolder);
            User user = await Init();
            // Monitor monitor = new Monitor();
            // await monitor.Run();
            Trader trader = new Trader(user);
            await trader.Run();
            Console.ReadLine();
        }

        private static async Task<User> Init()
        {
            Console.WriteLine("Type username");
            string username = Console.ReadLine();
            User user = null;
            while (user is null)
            {
                user = await findOrGenerateUser(username);
            }
            Console.WriteLine($"Successfully registered {user.user_name}, ID {user.user_id}, Token{user.token}");
            return user;
        }

        private static async Task<User> findOrGenerateUser(string username)
        {
            User user;
            if (File.Exists(LocalFile))
            {
                Users users = (Users)JsonSerializer.Deserialize(File.ReadAllText(LocalFile), typeof(Users));
                User user1 = users.AllUsers.Where(u => u.user_name == username).FirstOrDefault();
                if (user1 == null)
                {
                    Console.WriteLine("User not found, create user?");
                    string v = Console.ReadLine();
                    if (v.ToLowerInvariant().Contains("y"))
                    {
                        
                        if (!Directory.Exists(Environment.ExpandEnvironmentVariables(LocalFolder))) Directory.CreateDirectory(Environment.ExpandEnvironmentVariables(LocalFolder));
                        user = await CreateUser(username);
                        users.AllUsers = users.AllUsers.Append(user).ToArray();
                        await File.WriteAllTextAsync(LocalFile, JsonSerializer.Serialize(users, typeof(Users)));
                        return user;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return user1;
                }
            }
            else
            {
                Console.WriteLine("User not found, create user?");
                string v = Console.ReadLine();
                if (v.ToLowerInvariant().Contains("y"))
                {
                    if (!Directory.Exists(LocalFolder)) Directory.CreateDirectory(LocalFolder);
                    user = await CreateUser(username);
                    await File.WriteAllTextAsync(LocalFile, JsonSerializer.Serialize(new Users() { AllUsers = new[] { user } }, typeof(Users)));
                    return user;
                }
                else
                {
                    return null;
                }

            }
        }

        private static async Task<User> CreateUser(string username)
        {
            User user = new User()
            {
                user_name = username
            };
            HttpClient client = new HttpClient();
            HttpResponseMessage httpResponseMessage = await client.PostAsJsonAsync(APIUrl + "users", user);
            string response = await httpResponseMessage.Content.ReadAsStringAsync();
            Console.WriteLine(response);
            user = await httpResponseMessage.Content.ReadAsJsonAsync<User>();
            return user;
        }


    }
}
