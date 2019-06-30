using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace FedagoApp.Managers
{
    public class TunnelManager : IDisposable
    {
        private const string ngrokUrl = "http://localhost:4040/api/tunnels";

        //private static SynchronizationContext _syncContext;
        private bool _ready = false;
        private string _ngrokAuthToken;
        private Process _ngrokProcess;

        public TunnelManager()
        {
        }

        public void CreateTunnel(string ngrokAuthToken = null)
        {
            _ready = false;
            _ngrokAuthToken = ngrokAuthToken;
            ThreadStart threadStart = new ThreadStart(RunNgrok);
            Thread commandThread = new Thread(threadStart);
            commandThread.IsBackground = true;
            commandThread.Start();
        }

        private void RunNgrok()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string ngrokFile = string.Format(@"{0}\{1}\{2}", currentDirectory, @"Assets\ngrok\",
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"Win64\ngrok.exe" :
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? @"Mac64\ngrok" : null);
            string configFile = string.Format(@"{0}\{1}", currentDirectory, @"Assets\ngrok\fedago.yml");

            var authProcess = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ngrokFile,
                    Arguments = string.Format("authtoken \"{0}\" -config=\"" + configFile + "\"", string.IsNullOrEmpty(_ngrokAuthToken) ? string.Empty : _ngrokAuthToken),
                    //RedirectStandardOutput = true,
                    //RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            authProcess.Start();
            authProcess.WaitForExit();

            if (_ngrokProcess != null)
            {
                _ngrokProcess.Kill();
            }
            _ngrokProcess = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ngrokFile,
                    Arguments = "http 49424 -host-header=\"localhost:49424\" -config=\"" + configFile + "\"",
                    //RedirectStandardOutput = true,
                    //RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            //process.EnableRaisingEvents = true;
            //process.OutputDataReceived += (sender, args) => Display(args.Data);
            //process.ErrorDataReceived += (sender, args) => Display(args.Data);
            _ngrokProcess.Start();
            //process.BeginOutputReadLine();
            //process.BeginErrorReadLine();
            _ngrokProcess.WaitForExit();

        }

        public string PublicUrl
        {
            get
            {
                string publicUrl;
                try
                {
                    HttpClient httpClient = new HttpClient();
                    Task<string> response = httpClient.GetStringAsync(ngrokUrl);
                    response.Wait();
                    string ngrokSite = response.Result;

                    //Regex regex = new Regex("<PublicURL>(.*)</PublicURL>");
                    //Match match = regex.Match(ngrokSite);
                    //string publicUrl = match.Value;

                    dynamic deserializedObject = JsonConvert.DeserializeObject(ngrokSite);
                    publicUrl = deserializedObject.tunnels[0].public_url.ToString(); //get https
                    if (!publicUrl.ToLower().StartsWith("https"))
                    {
                        publicUrl = publicUrl.ToLower().Replace("http", "https");
                    }
                }
                catch(Exception ex)
                {
                    publicUrl = string.Empty;
                }
                return publicUrl;
            }
        }

        public bool IsReady
        {
            get
            {
                if (!_ready)
                {
                    lock(this)
                    {
                        if (!_ready)
                        {
                            _ready = Uri.IsWellFormedUriString(PublicUrl, UriKind.RelativeOrAbsolute);
                        }
                    }
                }
                return _ready;
            }
        }

        //private static void Display(string output)
        //{
        //    _syncContext.Post(lmb => Debug.WriteLine(output), null);
        //}

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~TunnelManager() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
