using System.Collections.Specialized;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Web;
using SonyHeadphones.Models;
using Linux.Bluetooth;
using Linux.Bluetooth.Extensions;
using Realms;
using Device = Linux.Bluetooth.Device;

// ReSharper disable ArrangeNamespaceBody StringLiteralTypo CommentTypo
namespace SonyHeadphones
{
    internal static class Program
    {
        private static Device1Properties _deviceProps = null!;
        private static DateTime _connectDate;
        private static DateTime _disconnectDate;
        private static AuthClient _authClient;
        
        private const string UploadUrlEndpoint = "https://hc.sens.bda.ndmdhs.com/media_data/yh_backup_file/upload_url";
        private const string MetaDataEndpoint = "https://hc.sens.bda.ndmdhs.com/media_data/yh_backup_file/meta_data";
        private const string RedirectUri = "headphonesconnect://signin";
        private const string ClientId = "afa6a98b-d0f2-4f85-b6a9-fc79780136e4";
        private const string UserAgent = "Dalvik/2.1.0 (Linux; U; Android 10; LM-G710 Build/QKQ1.191222.002)";
        
        // ReSharper disable once UnusedParameter.Local
        private static async Task Main(string[] args)
        {
            // TODO: okay what is actually left?
            // - multi-device tracking
            // - proper error handling
            // - clean up
            // - sync and merge
            // - configuration of some kind
            // - - configure what??
            // - - selection of devices
            
            if (args.Length == 1) HandleCallback(args[0]);

            _authClient = new AuthClient(ClientId);

            string authUri = _authClient.BuildAuthUri(RedirectUri); // arg is redirect uri, class generates state, nonce and codeChallenge and stores them in class instance
            Console.WriteLine($"\n\nOpen this URL in your browser: {authUri}\n\n"); // force open in browser instead? copy to clipboard? some other way?
            // maybe even store auth on disk since it should be fine to re-use?
            
            string authCode = WaitForCallback(_authClient.GetState());
            
            await _authClient.DoAuth(authCode, _authClient.GetState(), RedirectUri); // class instance verifies and does http request to /token endpoint with code, redirect_uri, code_verifier(== codeChallenge's unhashed value), stores access_token, token_type, expires_in, refresh_token in class instance
            
            Console.WriteLine($"\n\n{await _authClient.GetAccessTokenWithType()}\n\n");
            
            using (Realm realm = RealmManager.GetInstance())
            {
                IQueryable<YhUseTime>? yhUseTimes = realm.All<YhUseTime>();

                foreach (YhUseTime yhUseTime in yhUseTimes)
                {
                    Console.WriteLine($"YhUseTime - StartTime: {yhUseTime.StartTime}, EndTime: {yhUseTime.EndTime}, IsTemporaryRecord: {yhUseTime.IsTemporaryRecord}");
                    Console.WriteLine($"  Device Information - DeviceId: {yhUseTime.DeviceInformation.DeviceId}, DeviceName: {yhUseTime.DeviceInformation.DeviceName}");
                }
            }
            
            // Bluetooth Testing
            IReadOnlyList<Adapter>? adapters = await BlueZManager.GetAdaptersAsync();
            if (adapters.Count == 0) throw new Exception("No Bluetooth adapters found.");

            IAdapter1 adapter = adapters[0]; // User might have multiple adapters... for some reason, handle that... maybe ask for preference???
            
            IReadOnlyList<Device>? devices = await adapter.GetDevicesAsync();
            
            foreach (Device device in devices)
            {
                Device1Properties deviceProperties = await device.GetAllAsync();
                
                Console.WriteLine($"{deviceProperties.Alias} - {deviceProperties.Connected} - {deviceProperties.Paired}");
                
                if (deviceProperties.Alias != "Keychron K8") continue; // TODO: dont hardcode, either automatically detect from deviceProperties.Address & YhDevices, or allow user to select (and maybe add new devices)
                _deviceProps = deviceProperties;
                    
                // Subscribe to connection events
                device.Connected += Device_Connected;
                device.Disconnected += Device_Disconnected;
            }
            
            // just prevent exit
            await Task.Delay(-1);
        }
        
        #pragma warning disable CS1998
        private static async Task Device_Connected(Device device, BlueZEventArgs e)
        {
            _connectDate = DateTime.Now;
            Console.WriteLine("Device connected!");
        }

        private static async Task Device_Disconnected(Device device, BlueZEventArgs e)
        {
            _disconnectDate = DateTime.Now;

            TimeSpan difference = _disconnectDate - _connectDate;
            
            Console.WriteLine($"Device disconnected! Duration: {difference.TotalSeconds} seconds");
            
            // Save to Realm
            string deviceAddress = _deviceProps.Address;

            using Realm realm = RealmManager.GetInstance();
            IQueryable<YhDevice> yhDevices = realm.All<YhDevice>().Where(i => i.DeviceId == deviceAddress);
            
            // meh
            DateTimeOffset connectDateOffset = new DateTimeOffset(_connectDate);
            DateTimeOffset disconnectDateOffset = new DateTimeOffset(_disconnectDate);

            YhUseTime useTime = new YhUseTime()
            {
                DeviceInformation = yhDevices.First(),
                StartTime = connectDateOffset.ToUnixTimeMilliseconds(),
                EndTime = disconnectDateOffset.ToUnixTimeMilliseconds(),
                IsTemporaryRecord = false
            };
            
            Console.WriteLine($"{useTime.DeviceInformation.DeviceName}, {useTime.StartTime}, {useTime.EndTime}");
            
            try
            {
                await realm.WriteAsync(() =>
                {
                    realm.Add(useTime);
                });

                Console.WriteLine($"Logged {difference.TotalSeconds} seconds for {_deviceProps.Alias}");
                
                string accessTokenWithType = await _authClient.GetAccessTokenWithType();
                await WriteUpdates(RealmManager.RealmPath, accessTokenWithType);
                Console.WriteLine("Wrote realm changes to S3");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging use time: {ex.Message}");
                throw;
            }
        }
        
        private static async Task WriteUpdates(string realmPath, string accessTokenWithType)
        {
            try
            {
                // 1. GET https://hc.sens.bda.ndmdhs.com/media_data/yh_backup_file/upload_url
                // has:
                // - Authorization: accessTokenWithType
                // - x-api-version: v1
                // - User-Agent: Dalvik, blabla.
                string uploadUrl = await GetUploadUrl(accessTokenWithType);

                // 2. PUT to S3 (URL returned by #1 as JSON with key "upload_url")
                // has:
                // - Content-Type: binary/octet-stream
                // - User-Agent Dalvik, blabla.
                await UploadToS3(uploadUrl, realmPath);

                // 3. PUT https://hc.sens.bda.ndmdhs.com/media_data/yh_backup_file/meta_data
                // has:
                // - Authorization: accessTokenWithType
                // - x-api-version: v1
                // - User-Agent: Dalvik, blabla.
                // with:
                // - {"meta_data_key":"meta_data_key_value","meta_data":[{"internal_key":"{\"format_version\":3,\"last_modified_time\":1701141948481,\"total_usage_time\":922456343,\"device_name_array\":[{\"device_name\":\"WF-C500\"}]}"}]}
                // last_modified_time: set to now
                // total_usage_time: realm.All<YhUseTime>(), for each YhUseTime, startTime-endTime, sum all
                // device_name_array: realm.All<YhDevice>, get deviceName for each
                await UpdateMetaData(accessTokenWithType);

                Console.WriteLine("Updates written successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing updates: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }
        
        private static async Task<string> GetUploadUrl(string accessTokenWithType)
        {
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", accessTokenWithType);
            client.DefaultRequestHeaders.Add("x-api-version", "v1");
            client.DefaultRequestHeaders.Add("User-Agent", UserAgent);

            HttpResponseMessage response = await client.GetAsync(UploadUrlEndpoint);

            response.EnsureSuccessStatusCode();
            
            string jsonString = await response.Content.ReadAsStringAsync();
            
            JsonDocument jsonDocument = JsonDocument.Parse(jsonString);

            JsonElement uploadUrlElement = jsonDocument.RootElement.GetProperty("upload_url");
            string uploadUrl = uploadUrlElement.GetString() ?? throw new InvalidOperationException("No upload URL found in the response.");

            return uploadUrl;
        }

        private static async Task UploadToS3(string uploadUrl, string realmPath)
        {
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", UserAgent);

            byte[] realmData = await File.ReadAllBytesAsync(realmPath);

            ByteArrayContent content = new ByteArrayContent(realmData);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("binary/octet-stream");

            HttpResponseMessage response = await client.PutAsync(uploadUrl, content);

            response.EnsureSuccessStatusCode();
        }

        private static async Task UpdateMetaData(string accessTokenWithType)
        {
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", accessTokenWithType);
            client.DefaultRequestHeaders.Add("x-api-version", "v1");
            client.DefaultRequestHeaders.Add("User-Agent", UserAgent);

            using Realm realm = RealmManager.GetInstance();
            List<YhDevice> yhDevices = realm.All<YhDevice>().ToList();
            var deviceNameArray = yhDevices
                .AsEnumerable()
                .Select(device => new { device_name = device.DeviceName })
                .ToArray();

            var metaData = new
            {
                meta_data_key = "meta_data_key_value",
                meta_data = new[]
                {
                    new
                    {
                        internal_key = new
                        {
                            format_version = 3,
                            last_modified_time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            total_usage_time = GetTotalUsageTime(),
                            device_name_array = deviceNameArray
                        }
                    }
                }
            };

            string jsonPayload = JsonSerializer.Serialize(metaData);

            // Set necessary headers
            StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Send the PUT request to update meta data
            HttpResponseMessage response = await client.PutAsync(MetaDataEndpoint, content);

            response.EnsureSuccessStatusCode();
        }

        private static long GetTotalUsageTime()
        {
            using Realm realm = RealmManager.GetInstance();
            List<YhUseTime> yhUseTimes = realm.All<YhUseTime>().ToList();
            
            return yhUseTimes.Sum(useTime => (useTime.EndTime - useTime.StartTime));
        }

        // TODO: CALLBACK STUFF, JUST TEMPORARILY IN HERE!!!
        private static string WaitForCallback(string state)
        {
            using NamedPipeServerStream pipeServer = new NamedPipeServerStream(state);
            Console.WriteLine("Waiting for callback...");

            pipeServer.WaitForConnection();

            using StreamReader reader = new StreamReader(pipeServer);
            string message = reader.ReadToEnd();
            return message;
        }
        
        private static void HandleCallback(string callbackUrl)
        {
            NameValueCollection queryParams = HttpUtility.ParseQueryString(new Uri(callbackUrl).Query);

            string? code = queryParams.Get("code");
            string? state = queryParams.Get("state");

            if (code != null && state != null)
            {
                using NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", state, PipeDirection.Out);
                pipeClient.Connect();

                using StreamWriter writer = new StreamWriter(pipeClient);
                writer.Write(code);
            }
            else
            {
                Console.WriteLine("Missing code or state in callback parameters. Callback ignored.");
            }
            Environment.Exit(0);
        }

    }
}