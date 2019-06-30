using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

using DataBazrPeer.Models;
using DataBazrPeer.Managers;
using Newtonsoft.Json;
using DataBazrPeer.Helpers;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Collections.Specialized;
using DataBazrPeer.Configurations;
using System.Text;
using System.Net.Http;
using System.Data.SqlClient;
using System.Data.Common;
using System.Data;
using FedagoApp.Managers;
using System.Timers;
using System.Net;
using System.Text.RegularExpressions;
using MvcDataTables;

namespace DataBazrPeer.Controllers
{
    public class HomeController : Controller
    {
        private static TunnelManager _tunnelManager = new TunnelManager();

        //private static Timer _timer = new Timer();

        private const string FIELD_Id = "Id";
        private const string FIELD_Token = "Token";
        private const string FIELD_ServerUrl = "ServerUrl";
        private const string FIELD_Secret = "Secret";
        private const string FIELD_CallbackUrl = "CallbackUrl";
        private const string FIELD_PublicUrl = "PublicUrl";
        private const string FIELD_NGrokAuthToken = "ngrokAuthToken";        

        //private IFileInfo _settingsFile;
        //private NameValueCollection _settings = new NameValueCollection();
        private static readonly IFileInfo _applicationConfigurationFile;                
        private static readonly ApplicationConfiguration _applicationConfiguration = new ApplicationConfiguration();
        private static List<string> _messages = new List<string>();

        private string _anchor = string.Empty;

        static HomeController()
        {
            _applicationConfigurationFile = new PhysicalFileProvider(Directory.GetCurrentDirectory()).GetFileInfo("application.json");
            if (_applicationConfigurationFile.Exists)
            {
                _applicationConfiguration = JsonConvert.DeserializeObject<ApplicationConfiguration>(System.IO.File.ReadAllText(_applicationConfigurationFile.PhysicalPath));
            }
            string ngrokAuthToken;
            _applicationConfiguration.KeyValuePairs.TryGetValue(FIELD_NGrokAuthToken, out ngrokAuthToken);
            _tunnelManager.CreateTunnel(ngrokAuthToken);
            //_timer.Elapsed += OnTimerElapsed;
            //_timer.Interval = 100; //immediately one-time (defaults to 100ms)
            //_timer.Start();            
        }

        private static void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {            
            try
            {
                //_timer.Stop();
                //_timer.Interval = 60000; //increase to a minute
                //GetPublicUrl();
            }
            finally
            {
                //_timer.Start();
            }            

            //HttpWebRequest webRequest = HttpWebRequest.CreateHttp(ngrokUrl);
            //HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
            //string encodedData = string.Empty;
            //using (Stream responseStream = webResponse.GetResponseStream())
            //{
            //    if (responseStream != null)
            //    {
            //        var streamReader = new StreamReader(responseStream);
            //        encodedData = streamReader.ReadToEnd();
            //        streamReader.Close();
            //    }
            //}
        }

        public HomeController(IConfiguration configuration)
        {
            //_settingsFile = new PhysicalFileProvider(Directory.GetCurrentDirectory()).GetFileInfo("settings.json");
            //if (_settingsFile.Exists)
            //{
            //    _settings = JsonConvert.DeserializeObject<IDictionary<string, string>>(System.IO.File.ReadAllText(_settingsFile.PhysicalPath)).ToNameValueCollection();
            //}
            //else
            //{
            //    _settings[FIELD_Id] = (configuration[FIELD_Id] ?? string.Empty).ToString();
            //    _settings[FIELD_Secret] = (configuration[FIELD_Secret] ?? string.Empty).ToString();
            //    _settings[FIELD_Token] = (configuration[FIELD_Token] ?? string.Empty).ToString();
            //    _settings[FIELD_CallbackUrl] = (configuration[FIELD_CallbackUrl] ?? string.Empty).ToString();
            //    System.IO.File.WriteAllText(_settingsFile.PhysicalPath, JsonConvert.SerializeObject(_settings.ToDictionary()));
            //}

            //else
            //{
            //    _applicationConfiguration.KeyValuePairs = _settings.ToDictionary();
            //    _applicationConfiguration.DatabaseConfigurations.Add(new DatabaseConfiguration()
            //    {
            //        ConnectionString = "Data Source=EL-2014;Initial Catalog=DataWarehouse;Integrated Security=True",
            //        Table = "Countries",
            //        IsFunction = false
            //    }
            //    );
            //    string serializedApplciationConfiguration = JsonConvert.SerializeObject(_applicationConfiguration);
            //    System.IO.File.WriteAllText(_applicationConfigurationFile.PhysicalPath, serializedApplciationConfiguration);
            //}
        }

        public IActionResult Index()
        {
            //var model = new AccessModel();
            ////model.AuthorizationModel = authorizationModel;
            //model.UserAccessModel = new UserAccessModel()
            //{
            //    Id = _applicationConfiguration.KeyValuePairs[FIELD_Id],
            //    Secret = _applicationConfiguration.KeyValuePairs[FIELD_Secret],
            //    //ServerUrl = _applicationConfiguration.KeyValuePairs[FIELD_Token],
            //    CallbackUrl = _applicationConfiguration.KeyValuePairs[FIELD_CallbackUrl]
            //};

            return View(this);
        }

        public IActionResult Ethereal()
        {
            return View("QueryWizard", this);
        }

        public IActionResult Settings()
        {
            return View(this);
        }

        public string Anchor
        {
            get
            {
                //One time use
                string tmpAnchor = _anchor;
                _anchor = string.Empty;
                return tmpAnchor;
            }
        }

        public ApplicationConfiguration ApplicationConfiguration
        {
            get { return _applicationConfiguration; }
        }

        public string ClientApiId
        {
            get { return _applicationConfiguration.KeyValuePairs[FIELD_Id]; }
        }

        public string ClientApiSecret
        {
            get { return _applicationConfiguration.KeyValuePairs[FIELD_Secret]; }
        }

        public string ServerUrl
        {
            get { return _applicationConfiguration.KeyValuePairs[FIELD_ServerUrl]; }
        }

        public string ClientApiCallbackUrl
        {
            get { return GetCallbackUrl(); }
        }

        public string ClientApiToken
        {
            get
            {
                return _applicationConfiguration.KeyValuePairs.ContainsKey(FIELD_Token) ?
                    _applicationConfiguration.KeyValuePairs[FIELD_Token] :
                    string.Empty;
            }
        }

        public string SerializedConfiguration
        {
            get
            {
                return JsonConvert.SerializeObject(_applicationConfiguration);
            }
        }

        public string Logs
        {
            get
            {
                return _messages.ToFlatFile("\n");
            }
        }

        #region OATH
        [HttpPost]
        public IActionResult GetToken(UserAccessModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var nopAuthorizationManager = new AuthorizationManager(model.Id, model.Secret, model.ServerUrl);

                    var callbackUrl = GetCallbackUrl(); // model.CallbackUrl; // Url.RouteUrl("GetAccessToken", null, Request.Url.Scheme);

                    // For demo purposes this data is kept into the current Session, but in production environment you should keep it in your database                    
                    _applicationConfiguration.KeyValuePairs[FIELD_Id] = model.Id;
                    _applicationConfiguration.KeyValuePairs[FIELD_Secret] = model.Secret;
                    _applicationConfiguration.KeyValuePairs[FIELD_ServerUrl] = model.ServerUrl;

                    // This should not be saved anywhere.
                    var state = Guid.NewGuid().ToString();
                    _applicationConfiguration.KeyValuePairs["State"] = state;

                    string authUrl = nopAuthorizationManager.BuildAuthUrl(callbackUrl, new string[] { }, state);

                    System.IO.File.WriteAllText(_applicationConfigurationFile.PhysicalPath, JsonConvert.SerializeObject(_applicationConfiguration));

                    _messages.Add(string.Format("{0}: GetToken(UserAccessModel) passed.", DateTime.Now.ToString("yyyyMMddHHmmss")));
                    return Redirect(authUrl);
                }
                catch (Exception ex)
                {
                    _messages.Add(string.Format("{0}: GetToken(UserAccessModel) failed.", DateTime.Now.ToString("yyyyMMddHHmmss")));
                    return BadRequest(ex.Message);
                }
            }

            return BadRequest();
            //return View();
        }

        private string GetCallbackUrl(string serverUrl = null)
        {
            if (serverUrl == null)
            {
                while (!_tunnelManager.IsReady)
                {
                    System.Threading.Thread.Sleep(1000);
                }
                serverUrl = _tunnelManager.PublicUrl;
                //serverUrl = _applicationConfiguration.KeyValuePairs.ContainsKey(FIELD_PublicUrl) 
                //    ? _applicationConfiguration.KeyValuePairs[FIELD_PublicUrl].ToString()
                //    : string.Empty;
            }
            string publicUrl = string.Format("{0}/Callback", serverUrl);
            return publicUrl;
        }

        [HttpGet]
        [Route("/Callback")]
        public ActionResult Callback(string code, string state)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(state))
            {
                if (state != _applicationConfiguration.KeyValuePairs["State"].ToString())
                {
                    _messages.Add(string.Format("{0}: Callback(code, state) failed, bad state.", DateTime.Now.ToString("yyyyMMddHHmmss")));
                    return BadRequest();
                }

                var model = new AccessModel();

                try
                {
                    // TODO: Here you should get the authorization user data from the database instead from the current Session.
                    string clientId = _applicationConfiguration.KeyValuePairs[FIELD_Id].ToString();
                    string clientSecret = _applicationConfiguration.KeyValuePairs[FIELD_Secret].ToString();
                    string serverUrl = _applicationConfiguration.KeyValuePairs[FIELD_ServerUrl].ToString();
                    string redirectUrl = GetCallbackUrl(_applicationConfiguration.KeyValuePairs[FIELD_PublicUrl].ToString());

                    var authParameters = new AuthParameters()
                    {
                        ClientId = clientId,
                        ClientSecret = clientSecret,
                        ServerUrl = serverUrl,
                        RedirectUrl = redirectUrl,
                        GrantType = "authorization_code",
                        Code = code
                    };

                    var nopAuthorizationManager = new AuthorizationManager(authParameters.ClientId, authParameters.ClientSecret, authParameters.ServerUrl);

                    string responseJson = nopAuthorizationManager.GetAuthorizationData(authParameters);

                    AuthorizationModel authorizationModel = JsonConvert.DeserializeObject<AuthorizationModel>(responseJson);

                    model.AuthorizationModel = authorizationModel;
                    model.UserAccessModel = new UserAccessModel()
                    {
                        Id = clientId,
                        Secret = clientSecret,
                        //ServerUrl = serverUrl,
                        CallbackUrl = redirectUrl
                    };

                    // TODO: Here you can save your access and refresh tokens in the database. For illustration purposes we will save them in the Session and show them in the view.
                    _applicationConfiguration.KeyValuePairs[FIELD_Token] = authorizationModel.AccessToken;

                    System.IO.File.WriteAllText(_applicationConfigurationFile.PhysicalPath, JsonConvert.SerializeObject(_applicationConfiguration));

                    _messages.Add(string.Format("{0}: Callback(code, state) passed.", DateTime.Now.ToString("yyyyMMddHHmmss")));
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }

                return View("Index", this);
            }

            return BadRequest();
        }

        //[HttpPost]
        //[Route("/Callback/{id}")]
        //public IActionResult GetData(int id)
        //{
        //    return PostData(new Dictionary<string, string>());
        //}

        [HttpGet]
        [Route("/Callback/{id}")]
        public IActionResult GetData(string id)
        {
            return PostData(id, null);
        }

        [HttpPost]
        [Route("/Callback/{id}/DataTables")]
        public IActionResult PostDataTables(DataTableViewModel model, string id, string token)
        {
            return PostData(id, token);
        }

        [HttpPost]
        [Route("/Callback/{id}/Json")]
        public JsonResult PostDataJson(string id, string token)
        {
            return Json(PostDataString(id, token));
        }

        [HttpPost]
        [Route("/Callback/{id}/String")]
        public string PostDataString(string id, string token)
        {
            _messages.Add(string.Format("{0}: PostData(id={1}) started.", DateTime.Now.ToString("yyyyMMddHHmmss"), id));
            //if (!_applicationConfiguration.KeyValuePairs[FIELD_Token].Equals(token)) return BadRequest(string.Format("Invalid token={0}", token));
            IDataSource dataSource = _applicationConfiguration.DataSources.SingleOrDefault(lmb => lmb.Id.Equals(id));
            if (dataSource == null) return string.Format("Invalid id={0}", id); // BadRequest(string.Format("Invalid id={0}", id));

            string response = string.Empty;
            List<IDictionary<string, object>> results = new List<IDictionary<string, object>>();
            if (dataSource is DatabaseConfiguration)
            {
                DatabaseConfiguration databaseConfiguration = (DatabaseConfiguration)dataSource;
                string sql = string.Format("select top 1000 * from {0}", databaseConfiguration.Table);
                using (SqlConnection dbConnection = new SqlConnection(databaseConfiguration.ConnectionString))
                {
                    dbConnection.Open();
                    SqlCommand sqlCommand = new SqlCommand();
                    sqlCommand.Connection = dbConnection;
                    sqlCommand.CommandText = sql;
                    using (SqlDataReader rdr = sqlCommand.ExecuteReader())
                    {
                        var columnSchema = rdr.GetColumnSchema();
                        while (rdr.Read())
                        {
                            IDictionary<string, object> record = columnSchema.ToDictionary(lmb => lmb.ColumnName, lmb => rdr.GetValue(lmb.ColumnOrdinal.Value));
                            results.Add(record);
                        }
                    }
                }

                //using (SqlConnection dbConnection = new SqlConnection(databaseConfiguration.ConnectionString))
                //{
                //    dbConnection.Open();
                //    SqlCommand sqlCommand = new SqlCommand(sql, dbConnection) { CommandTimeout = 0 };
                //    SqlDataReader sqlReader = sqlCommand.ExecuteReader(CommandBehavior.Default);
                //    while (sqlReader.Read())
                //    {
                //        Dictionary<string, object> result = new Dictionary<string, object>();
                //        for (int index = 0; index < sqlReader.FieldCount; index++)
                //        {
                //            object value = sqlReader.IsDBNull(index) ? string.Empty : sqlReader.GetValue(index);
                //            if (typeof(string).IsAssignableFrom(value.GetType()) && value.ToString().Length >= 256)
                //            {
                //                value = value.ToString().Substring(0, 255);
                //            }
                //            result[sqlReader.GetName(index)] = value;
                //        }
                //        results.Add(result);
                //    }
                //    sqlReader.Close();
                //}
                if (results.Any())
                {
                    Dictionary<string, string> columns = results.First().Keys.ToDictionary(lmb => lmb, lmb => lmb);
                    DataTablesDynamicModel dynamicModel = new DataTablesDynamicModel(results, columns);
                    response = JsonConvert.SerializeObject(dynamicModel);
                }
            }
            else if (dataSource is DirectoryConfiguration)
            {
                string callbackUrl = ClientApiCallbackUrl;
                DirectoryConfiguration directoryConfiguration = (DirectoryConfiguration)dataSource;
                DirectoryInfo directoryInfo = new DirectoryInfo(directoryConfiguration.Path);
                foreach (FileInfo fileInfo in directoryInfo.EnumerateFiles()) //refactor
                {
                    Dictionary<string, object> record = new Dictionary<string, object>();
                    record["Name"] = fileInfo.Name;
                    record["Extension"] = fileInfo.Extension;
                    record["Length"] = fileInfo.Length;
                    record["CreationTimeUtc"] = fileInfo.CreationTimeUtc;
                    record["LastWriteTimeUtc"] = fileInfo.LastWriteTimeUtc;
                    record["Url"] = string.Format(@"{2}/{0}/{1}", id, fileInfo.Name, callbackUrl);
                    results.Add(record);
                }
                foreach (DirectoryInfo innerDirectoryInfo in directoryInfo.EnumerateDirectories())
                {
                    foreach (FileInfo fileInfo in innerDirectoryInfo.EnumerateFiles())
                    {
                        Dictionary<string, object> record = new Dictionary<string, object>();
                        record["Name"] = fileInfo.Name;
                        record["Extension"] = fileInfo.Extension;
                        record["Length"] = fileInfo.Length;
                        record["CreationTimeUtc"] = fileInfo.CreationTimeUtc;
                        record["LastWriteTimeUtc"] = fileInfo.LastWriteTimeUtc;
                        record["Url"] = string.Format(@"{2}/{0}/{1}", id, fileInfo.Name, callbackUrl);
                        results.Add(record);
                    }
                }
                if (results.Any())
                {
                    Dictionary<string, string> columns = results.First().Keys.ToDictionary(lmb => lmb, lmb => lmb);
                    DataTablesDynamicModel dynamicModel = new DataTablesDynamicModel(results, columns);
                    response = JsonConvert.SerializeObject(dynamicModel);
                }
            }
            else if (dataSource is RestfulConfiguration)
            {
                RestfulConfiguration restfulConfiguration = (RestfulConfiguration)dataSource;
                response = RestExtensions.Rest(restfulConfiguration.Url, string.Empty, HttpMethod.Get);
                results = GenericDataset.ConvertResponse(response).ToList();
                Dictionary<string, string> columns = results.First().Keys.ToDictionary(lmb => lmb, lmb => lmb);
                DataTablesDynamicModel dynamicModel = new DataTablesDynamicModel(results, columns);
                response = JsonConvert.SerializeObject(dynamicModel);
            }
            else
            {
                response = string.Format("Unknown configuration"); //return BadRequest(string.Format("Unknown configuration"));
            }
            return response;
        }

        [HttpPost]
        [Route("/Callback/{id}")]
        public IActionResult PostData(string id, string token)
        {
            string response = PostDataString(id, token);
            if (response.Length < 100)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        public class DataTablesDynamicModel
        {
            public class Column
            {
                private readonly KeyValuePair<string, string> _keyValuePair;
                public Column(KeyValuePair<string, string> keyValuePair)
                {
                    _keyValuePair = keyValuePair;
                }

                public string data
                {
                    get
                    {
                        return _keyValuePair.Key;
                    }
                }

                public string name
                {
                    get
                    {
                        return _keyValuePair.Value;
                    }
                }
            }

            private readonly IEnumerable<IDictionary<string, object>> _data;
            private readonly IDictionary<string, string> _columns;
            public DataTablesDynamicModel(IEnumerable<IDictionary<string, object>> data, IDictionary<string, string> columns)
            {
                _data = data;
                _columns = columns;
            }

            public IEnumerable<IDictionary<string, object>> data
            {
                get
                {
                    return _data;
                }
            }

            public IEnumerable<Column> columns
            {
                get
                {
                    var columns = new List<Column>();
                    columns.AddRange(_columns.Select(lmb => new Column(lmb)));
                    return columns;
                }
            }
        }

        [HttpPost]
        [Route("/Callback/{id}/{name}")]
        public IActionResult GetFile(string id, string name, [FromBody] string token)
        {
            _messages.Add(string.Format("{0}: GetFile(id={1}) started.", DateTime.Now.ToString("yyyyMMddHHmmss"), id));
            //if (!_applicationConfiguration.KeyValuePairs[FIELD_Token].Equals(token)) return BadRequest(string.Format("Invalid token={0}", token));
            IDataSource dataSource = _applicationConfiguration.DataSources.SingleOrDefault(lmb => lmb.Id.Equals(id));
            if (dataSource == null) return null; // BadRequest(string.Format("Invalid id={0}", id));
            DirectoryConfiguration directoryConfiguration = (DirectoryConfiguration)dataSource;
            DirectoryInfo directoryInfo = new DirectoryInfo(directoryConfiguration.Path);
            FileInfo fileInfo = directoryInfo.GetFiles(name).FirstOrDefault();

            //IFileProvider provider = new PhysicalFileProvider(filePath);
            //IFileInfo fileInfo = provider.GetFileInfo(fileName);
            var readStream = fileInfo.OpenRead();
            var mimeType = "application/octet-stream";
            return File(readStream, mimeType, name);
        }

        [HttpGet]
        public JsonResult RefreshToken(string refreshToken, string clientId, string clientSecret, string serverUrl)
        {
            string json = string.Empty;

            if (ModelState.IsValid &&
                !string.IsNullOrEmpty(refreshToken) &&
                !string.IsNullOrEmpty(clientId) &&
                 !string.IsNullOrEmpty(clientSecret) &&
                !string.IsNullOrEmpty(serverUrl))
            {
                var model = new AccessModel();

                try
                {
                    var authParameters = new AuthParameters()
                    {
                        ClientId = clientId,
                        ClientSecret = clientSecret,
                        ServerUrl = serverUrl,
                        RefreshToken = refreshToken,
                        GrantType = "refresh_token"
                    };

                    var nopAuthorizationManager = new AuthorizationManager(authParameters.ClientId,
                        authParameters.ClientSecret, authParameters.ServerUrl);

                    string responseJson = nopAuthorizationManager.RefreshAuthorizationData(authParameters);

                    AuthorizationModel authorizationModel = JsonConvert.DeserializeObject<AuthorizationModel>(responseJson);

                    model.AuthorizationModel = authorizationModel;
                    model.UserAccessModel = new UserAccessModel()
                    {
                        Id = clientId
                        //ServerUrl = serverUrl
                    };

                    // Here we use the temp data because this method is called via ajax and here we can't hold a session.
                    // This is needed for the GetCustomers method in the CustomersController.
                    TempData[FIELD_Token] = authorizationModel.AccessToken;
                    TempData[FIELD_ServerUrl] = serverUrl;
                }
                catch (Exception ex)
                {
                    json = string.Format("error: '{0}'", ex.Message);

                    return Json(json); //, JsonRequestBehavior.AllowGet);
                }

                json = JsonConvert.SerializeObject(model.AuthorizationModel);
            }
            else
            {
                json = "error: 'something went wrong'";
            }

            return Json(json); //, JsonRequestBehavior.AllowGet);
        }

        //private ActionResult BadRequest(string message = "Bad Request")
        //{
        //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, message);
        //}
        #endregion
        
        public IEnumerable<IDataSource> DataSources
        {
            get
            {
                return _applicationConfiguration.DataSources;
            }
        }

        public IActionResult AddDatabase(string Id, string Name, string ConnectionString, string Table, bool IsFunction)
        {
            Debug.WriteLine(string.Format("AddDatabase: ConnectionString={0}, Table={1}, IsFunction={2}", ConnectionString, Table, IsFunction));
            DatabaseConfiguration configuration = _applicationConfiguration.DatabaseConfigurations.SingleOrDefault(lmb => lmb.Id.Equals(Id));
            if (configuration != null)
            {
                configuration.Name = Name;
                configuration.ConnectionString = ConnectionString;
                configuration.Table = Table;
                configuration.IsFunction = IsFunction;
            }
            else
            {
                configuration = new DatabaseConfiguration()
                {
                    Name = Name,
                    ConnectionString = ConnectionString,
                    Table = Table,
                    IsFunction = IsFunction
                };
                _applicationConfiguration.DatabaseConfigurations.Add(configuration);
            }            
            System.IO.File.WriteAllText(_applicationConfigurationFile.PhysicalPath, JsonConvert.SerializeObject(_applicationConfiguration));
            return View("Index", this);
        }

        public IActionResult AddRestfulApi(string Id, string Name, string Url, string Body)
        {
            Debug.WriteLine(string.Format("AddRestfulApi: Url={0}, Body={1}", Url, Body));
            RestfulConfiguration configuration = _applicationConfiguration.RestfulConfigurations.SingleOrDefault(lmb => lmb.Id.Equals(Id));
            if (configuration != null)
            {
                configuration.Name = Name;
                configuration.Url = Url;
                configuration.Body = Body;
            }
            else
            {
                configuration = new RestfulConfiguration()
                {
                    Name = Name,
                    Url = Url,
                    Body = Body
                };
                _applicationConfiguration.RestfulConfigurations.Add(configuration);
            }
            System.IO.File.WriteAllText(_applicationConfigurationFile.PhysicalPath, JsonConvert.SerializeObject(_applicationConfiguration));
            return View("Index", this);
        }

        public IActionResult AddDirectory(string Id, string Name, string Path)
        {
            Debug.WriteLine(string.Format("AddDirectory: Directory={0}", Path));

            //var provider = new PhysicalFileProvider(Path);
            //var contents = provider.GetDirectoryContents(string.Empty);
            //foreach(var file in contents)
            //{
            //    Console.WriteLine("File={0}", file.Name);
            //}

            DirectoryConfiguration configuration = _applicationConfiguration.DirectoryConfigurations.SingleOrDefault(lmb => lmb.Id.Equals(Id));
            if (configuration != null)
            {
                configuration.Name = Name;
                configuration.Path = Path;
            }
            else
            {
                configuration = new DirectoryConfiguration() { Name = Name, Path = Path };
                _applicationConfiguration.DirectoryConfigurations.Add(configuration);
            }
            System.IO.File.WriteAllText(_applicationConfigurationFile.PhysicalPath, JsonConvert.SerializeObject(_applicationConfiguration));

            return View("Index", this);
        }

        public IActionResult Publish()
        {            
            var accessToken = _applicationConfiguration.KeyValuePairs[FIELD_Token];
            var serverUrl = _applicationConfiguration.KeyValuePairs[FIELD_ServerUrl];

            var nopApiClient = new ApiClient(accessToken, serverUrl);

            var publishedDataSources = _applicationConfiguration.DataSources.Where(lmb => lmb.Id.IsPublished()).ToList();
            var unpublishedDataSources = _applicationConfiguration.DataSources.Except(publishedDataSources).ToList();

            //Update
            string updateUrl = @"/api/products/{0}";
            foreach (var dataSource in publishedDataSources)
            {
                var newProduct = new { product = new { name = dataSource.Name ?? dataSource.Display, show_on_home_page = true,
                    meta_title = dataSource.Display, meta_keywords = dataSource.Type,
                    sku = ClientApiId
                } };
                string newProductJson = JsonConvert.SerializeObject(newProduct);
                string resultJson = nopApiClient.Put(string.Format(updateUrl, dataSource.Id), newProductJson);
                dynamic result = JsonConvert.DeserializeObject(resultJson);
                string id = result.products[0].id;
                dataSource.Id = id;
            }

            //Create
            string createUrl = @"/api/products";
            foreach(var dataSource in unpublishedDataSources)
            {
                var newProduct = new { product = new { name = dataSource.Name ?? dataSource.Display, show_on_home_page = true,
                    meta_title = dataSource.Display, meta_keywords = dataSource.Type,
                    sku = ClientApiId
                } };
                string newProductJson = JsonConvert.SerializeObject(newProduct);
                string resultJson = nopApiClient.Post(createUrl, newProductJson);
                dynamic result = JsonConvert.DeserializeObject(resultJson);
                string id = result.products[0].id;
                dataSource.Id = id;
            }

            System.IO.File.WriteAllText(_applicationConfigurationFile.PhysicalPath, JsonConvert.SerializeObject(_applicationConfiguration));

            return View("Index", this);
            //return RedirectToAction("GetCustomers");
        }

        public IActionResult DeleteDataSource(string id)
        {
            IDataSource dataSource = _applicationConfiguration.DataSources.SingleOrDefault(lmb => lmb.Id == id);
            //dataSource.Active = false; //TODO: delete/active //visible_individually
            return View("Index", this);
        }

        public string NGrokAuthToken
        {
            get
            {
                string value;
                _applicationConfiguration.KeyValuePairs.TryGetValue(FIELD_NGrokAuthToken, out value);
                return value;
            }
        }

        public IActionResult UpdateSettings(string ngrokAuthToken)
        {
            ngrokAuthToken = ngrokAuthToken ?? string.Empty;
            Debug.WriteLine(string.Format("UpdateSettings: URL={0}", ngrokAuthToken));
            _applicationConfiguration.KeyValuePairs[FIELD_NGrokAuthToken] = ngrokAuthToken;
            System.IO.File.WriteAllText(_applicationConfigurationFile.PhysicalPath, JsonConvert.SerializeObject(_applicationConfiguration));

            System.Threading.ThreadPool.QueueUserWorkItem(
                (lmb) => {
                    //_timer.Stop();
                    //_timer.Interval = 100;
                    _tunnelManager.CreateTunnel(ngrokAuthToken);
                    //_timer.Start();
                    OnTimerElapsed(null, null);
                });

            return View("Index", this);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public string PublicUrl
        {
            get
            {
                return GetPublicUrl();
            }
        }

        private string GetPublicUrl()
        {
            string publicUrl = _tunnelManager.PublicUrl;
            _applicationConfiguration.KeyValuePairs[FIELD_PublicUrl] = publicUrl;
            ApiAuthorizer apiAuthorizer = new ApiAuthorizer(ClientApiId, ClientApiSecret, ServerUrl);
            apiAuthorizer.UpdateApiClientCallbackUrl(ClientApiCallbackUrl);
            return publicUrl;
        }
    }
}

