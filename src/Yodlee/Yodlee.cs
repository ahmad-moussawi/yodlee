using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
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
        public bool Debug { get; set; } = false;
        public string YodleeAppId { get; set; }
        public Token cobrandToken = new Token();
        public Token userToken = new Token();

        public string AuthorizationHeader
        {
            get
            {
                if (!cobrandToken.IsValid)
                {
                    return "";
                }

                if (!userToken.IsValid)
                {
                    return $"cobSession={cobrandToken.Value}";
                }

                return $"cobSession={cobrandToken.Value},userSession={userToken.Value}";
            }
        }

        public Client yapi;
        private readonly string cobrandName;
        private readonly string cobrandLogin;
        private readonly string cobrandPassword;

        public YodleeApi(string cobrandName)
        {
            this.cobrandName = cobrandName;

            var config = new Config();

            config.OnBeforeSend = async (req, c) =>
            {

                if (new[] { "POST", "PUT" }.Contains(req.Method.ToString()))
                {
                    req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                }

                if (Debug)
                {
                    Console.WriteLine($"{req.Method} {req.RequestUri}");

                    if (req.Headers != null)
                    {
                        foreach (var header in req.Headers)
                        {
                            Console.WriteLine(
                                $"{header.Key}: {string.Join(" ", header.Value)}"
                            );
                        }
                    }

                    if (req.Content?.Headers != null)
                    {
                        foreach (var header in req.Content.Headers)
                        {
                            Console.WriteLine(
                                $"{header.Key}: {string.Join(" ", header.Value)}"
                            );
                        }
                    }

                    if (req.Content != null)
                    {
                        Console.WriteLine(await req.Content.ReadAsStringAsync());
                    }
                }

            };

            yapi = new Yapi.Client(BASE_URL, config);

            yapi.HeadersCommon["Api-Version"] = new[] { API_VERSION };
            yapi.HeadersCommon["Cobrand-Name"] = new[] { cobrandName };

        }

        public YodleeApi(
            string cobrandName,
            string cobrandLogin,
            string cobrandPassword
        ) : this(cobrandName)
        {
            this.cobrandLogin = cobrandLogin;
            this.cobrandPassword = cobrandPassword;
        }
        public bool IsLogged()
        {
            return cobrandToken.IsValid;
        }

        public bool IsUserLogged()
        {
            return IsLogged() && userToken.IsValid;
        }

        public Task Login() => Login(cobrandLogin, cobrandPassword);

        public async Task Login(string login, string password, string local = "en_US")
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException("Cobrand login/password cannot be null or empty");
            }

            var response = await yapi.Post<dynamic>("cobrand/login", new
            {
                cobrand = new
                {
                    cobrandLogin = login,
                    cobrandPassword = password,
                    local,
                }
            });

            if (response.IsSuccess)
            {
                var content = response.Json();

                cobrandToken.Value = content.session.cobSession;

                cobrandToken.ExpiresAt = DateTime.UtcNow.AddMinutes(COBRAND_SESSION_DURATION);

                YodleeAppId = content.applicationId;

                Console.WriteLine(YodleeAppId);

                yapi.HeadersCommon["Authorization"] = new[] { AuthorizationHeader };

                return;
            }


            throw new InvalidOperationException("Cobrand login failed", new Exception(response.Raw()));

        }

        public async Task LoginUser(string login, string password, string local = "en_US", bool remember = true)
        {

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException("The parameters 'login' and 'password' cannot be null");
            }

            var payload = new
            {
                user = new
                {
                    loginName = login,
                    password = password,
                    local = local
                }
            };

            var response = await yapi.Post<dynamic>("user/login", payload);

            if (response.IsSuccess)
            {
                var content = response.Json();

                userToken.Value = content.user.session.userSession;

                userToken.ExpiresAt = DateTime.UtcNow.AddMinutes(USER_SESSION_DURATION);

                yapi.HeadersCommon["Authorization"] = new[] { AuthorizationHeader };

                return;
            }

            throw new InvalidOperationException("User login failed", new Exception(response.Raw()));
        }

        public async Task<Response<List<Account>>> Accounts(object query = null)
        {
            var config = new Config();

            config.ResponseTransformers.Add(content =>
            {
                if (content == "{}") return "[]";
                return traverse(content, "account");
            });

            return await yapi.Get<List<Account>>("accounts", query, config);
        }

        public async Task<Response<Account>> Account(string accountId, object query = null)
        {
            return await yapi.Get<Account>($"accounts/{accountId}", query);
        }

        public async Task<Response<List<Statement>>> Statements(object query = null)
        {
            return await yapi.Get<List<Statement>>("statements", query);
        }

        public async Task<Response<List<Transaction>>> Transactions(
            int accountId,
            DateTime from,
            DateTime to,
            object query = null
        )
        {
            query = merge(new
            {
                accountId,
                from = from.ToString("yyyy-MM-dd"),
                to = to.ToString("yyyy-MM-dd"),
            }, query);

            var config = new Config();

            config.ResponseTransformers.Add(content =>
            {
                if (content == "{}") return "[]";
                return traverse(content, "transaction");
            });

            return await yapi.Get<List<Transaction>>("transactions", query, config);
        }

        public async Task<Response<dynamic>> AccessTokens(string appIds)
        {
            var config = new Config();

            config.ResponseTransformers.Add(content =>
            {
                var ob = decode<dynamic>(content);
                return encode(ob.user.accessTokens[0]);
            });

            return await yapi.Get<dynamic>("/user/accessTokens", new
            {
                appIds
            }, config);

        }

        public async Task<Response<User>> CreateUser(
            string login,
            string email,
            string password
        )
        {

            var data = new
            {
                userParam = new
                {
                    user = new
                    {
                        loginName = login,
                        password = password,
                        email = email,
                    }
                }
            };

            var config = new Config();

            config.ResponseTransformers.Add(content => traverse(content, "user"));

            var response = await yapi.Post<User>("user/register", data, config);

            if (!response.IsSuccess)
            {
                throw new InvalidOperationException(
                    "Failed to create user", new Exception(response.Raw())
                );
            }

            userToken = new Token
            {
                Value = response.Json().Session.UserSession,
                ExpiresAt = DateTime.UtcNow.AddMinutes(USER_SESSION_DURATION)
            };

            return response;
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

        private string encode<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        private T decode<T>(string content)
        {
            return JsonConvert.DeserializeObject<T>(content);
        }

        private string traverse(string content, string prop)
        {
            return encode(decode<dynamic>(content)[prop]);
        }

    }
}
