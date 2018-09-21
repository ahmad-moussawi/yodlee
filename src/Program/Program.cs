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


            var config = JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(".env"));

            var yodlee = new YodleeApi(config.CobrandName);

            yodlee.Debug = true;


            await yodlee.Login(config.CobrandLogin, config.CobrandPassword);

            await yodlee.LoginUser(config.UserLogin, config.UserPassword);

            // var accounts = (await yodlee.Accounts()).Json();

            // var transactions = (await yodlee.Transactions(accounts[0].Id, new DateTime(2007, 1, 1), new DateTime(2018, 1, 1))).Json();

            var tokens = await yodlee.FastLinkAccessToken();

            Console.WriteLine(tokens.Json().Value);
            Console.WriteLine(yodlee.userToken.Value);

            var fastlinkUrl = await yodlee.FastLinkUrl("", "http://localhost:5000");

            Console.WriteLine(fastlinkUrl.Raw());

        }
    }
}
