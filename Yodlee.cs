using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Yapi;

namespace Yodlee
{
    public class YodleeApi
    {
        public const int COBRAND_SESSION_DURATION = 100;
        public const int USER_SESSION_DURATION = 100;
        public const string BASE_URL = "https://developer.api.yodlee.com/ysl/";
        public const int TIMEOUT = 30000;
        public const string API_VERSION = "1.1";

        public string cobrandSessionToken;
        public DateTime? cobrandSessionExpiry;
        public string userSessionToken;
        public DateTime? userSessionExpiry;

        public string AuthorizationHeader
        {
            get
            {
                if (string.IsNullOrEmpty(cobrandSessionToken))
                {
                    return "";
                }

                if (string.IsNullOrEmpty(userSessionToken))
                {
                    return "cobSession=" + cobrandSessionToken;
                }

                return "cobSession=" + cobrandSessionToken + ",userSession=" + userSessionToken;
            }
        }

        public Client yapi;

        private readonly string cobrandName;
        private readonly string cobrandLogin;
        private readonly string cobrandPassword;
        private readonly string userLogin;
        private readonly string userPassword;


        public YodleeApi(
          string cobrandName,
          string cobrandLogin,
          string cobrandPassword,
          string userLogin,
          string userPassword
        )
        {
            this.cobrandName = cobrandName;
            this.cobrandLogin = cobrandLogin;
            this.cobrandPassword = cobrandPassword;
            this.userLogin = userLogin;
            this.userPassword = userPassword;

            var config = new Config();
            config.OnBeforeSend = async (req, c) =>
            {

                if (req.Method.ToString() == "POST")
                {
                    req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                }

                // req.Headers.TryAddWithoutValidation("Content-Type", "application/json");

                // req.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                Console.WriteLine($"{req.Method} {req.RequestUri}");

                if (req.Headers != null)
                {
                    foreach (var header in req.Headers)
                    {
                        Console.WriteLine($"{header.Key}: {string.Join(" ", header.Value)}");
                    }
                }

                if (req.Content?.Headers != null)
                {
                    foreach (var header in req.Content.Headers)
                    {
                        Console.WriteLine($"{header.Key}: {string.Join(" ", header.Value)}");
                    }
                }

                if (req.Content != null)
                {
                    Console.WriteLine((await req.Content.ReadAsStringAsync()));
                }

            };

            // config.HeadersPost.Add("Content-Type", new[] { "application/json" });

            yapi = new Yapi.Client(BASE_URL, config);

            yapi.DefaultConfig.HeadersCommon["Api-Version"] = new[] { API_VERSION };
            yapi.DefaultConfig.HeadersCommon["Cobrand-Name"] = new[] { cobrandName };
            // yapi.DefaultConfig.HeadersCommon["Content-Type"] = new[] { "application/json" };



            // httpClient.DefaultRequestHeaders.Add("Api-Version", new[] { API_VERSION });
            // httpClient.DefaultRequestHeaders.Add("Cobrand-Name", new[] { cobrandName });
        }

        public bool IsLogged()
        {
            if (string.IsNullOrEmpty(cobrandSessionToken) || cobrandSessionExpiry == null)
            {
                return false;
            }

            var minutes = (DateTime.UtcNow - cobrandSessionExpiry.Value).TotalMinutes;

            return minutes < COBRAND_SESSION_DURATION;
        }

        public bool IsUserLogged()
        {
            if (!IsLogged() || string.IsNullOrEmpty(userSessionToken) || userSessionExpiry == null)
            {
                return false;
            }

            var minutes = (DateTime.UtcNow - userSessionExpiry.Value).TotalMinutes;

            return minutes < USER_SESSION_DURATION;
        }

        public async Task LoginAsync()
        {
            var payload = new
            {
                cobrand = new
                {
                    cobrandLogin,
                    cobrandPassword,
                    local = "en_US"
                }
            };

            var response = await yapi.Send("post", url: "cobrand/login", data: payload);

            if (response.IsSuccess)
            {
                var content = response.Json();

                cobrandSessionToken = content.session.cobSession;
                cobrandSessionExpiry = DateTime.UtcNow.AddMinutes(COBRAND_SESSION_DURATION);

                yapi.DefaultConfig.HeadersCommon["Authorization"] = new[] { AuthorizationHeader };

                return;
            }


            throw new InvalidOperationException("Cobrand Login Failed", new Exception(response.Raw()));

        }

        public async Task LoginUserAsync()
        {
            var payload = new
            {
                user = new
                {
                    loginName = userLogin,
                    password = userPassword,
                    local = "en_US"
                }
            };

            var response = await yapi.Send("post", url: "user/login", data: payload);

            if (response.IsSuccess)
            {
                var content = response.Json();

                userSessionToken = (string)content.user.session.userSession;

                userSessionExpiry = DateTime.UtcNow.AddMinutes(USER_SESSION_DURATION);

                yapi.DefaultConfig.HeadersCommon["Authorization"] = new[] { AuthorizationHeader };

                return;
            }

            throw new InvalidOperationException("User Login Failed", new Exception(response.Raw()));
        }

        public async Task EnsureLoginAsync()
        {
            if (!this.IsLogged())
            {
                await LoginAsync();
            }
        }

        public async Task EnsureUserLoginAsync()
        {
            if (!IsUserLogged())
            {
                await EnsureLoginAsync();
                await LoginUserAsync();
            }
        }

        public async Task<Response<List<Account>>> Accounts(object query = null)
        {
            await EnsureUserLoginAsync();

            return await yapi.Get<List<Account>>("accounts", query);
        }

        public async Task<Response<Account>> Account(string accountId, string container, object query = null)
        {
            var url = $"accounts/{accountId}";

            yapi.DefaultConfig.OnAfterReceive = content =>
            {
                var ob = JsonConvert.DeserializeObject<dynamic>(content);
                return JsonConvert.SerializeObject(ob.account);
            };

            return await yapi.Get<Account>(url, merge(query, new
            {
                container
            }));
        }

        public async Task<Response<List<Statement>>> Statements(object query = null)
        {
            return await yapi.Get<List<Statement>>("statements", query);
        }


        private object merge(object item1, object item2)
        {
            if (item1 == null || item2 == null)
                return item1 ?? item2 ?? new ExpandoObject();

            dynamic expando = new ExpandoObject();
            var result = expando as IDictionary<string, object>;

            foreach (System.Reflection.PropertyInfo fi in item1.GetType().GetProperties())
            {
                result[fi.Name] = fi.GetValue(item1, null);
            }

            foreach (System.Reflection.PropertyInfo fi in item2.GetType().GetProperties())
            {
                result[fi.Name] = fi.GetValue(item2, null);
            }

            return (object)result;
        }



    }
}
