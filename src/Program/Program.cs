using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Yodlee;

namespace Program
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            var config = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText("env.json"));

            Console.WriteLine(config);

            var yodlee = new YodleeApi(
                (string)config.cobrandName,
                (string)config.cobrandLogin,
                (string)config.cobrandPassword,
                (string)config.userLogin,
                (string)config.userPassword
            );

            var response = await yodlee.Accounts();

            Console.WriteLine(response.Json());

        }
    }
}
