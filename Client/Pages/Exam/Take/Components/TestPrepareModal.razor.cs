using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Client.Pages.Exam
{
    public partial class TestPrepareModal
    {
        [Inject] public HttpClient Http { get; set; }

        private int currentStep = 1;
        private bool isDetecting = false;
        private bool isDetected = false;

        private int tipType;
        private string tipText;

        private bool ethernetConnected;
        private string wirelessSSID;

        private DeepLensWifiListItem[] wifiList;

        private string selectedWifi = "";
        private string wifiPassword = "";
        private bool isWifiConnecting = false;

        private bool cameraVideoLoading = true;

        private const string DEEPLENS_SETTING_URL = "https://camera-amdc.net:8080";

        [Parameter] public EventCallback OnShareScreen { get; set; }

        [Parameter] public EventCallback OnFinish { get; set; }

        [Parameter] public string ExamId { get; set; }

        [Parameter] public bool Visible { get; set; }

        private async Task ToStep(int step)
        {
            tipText = "";
            tipType = 0;
            currentStep = step;
            if (currentStep == 2)
            {
                await GetNetworkStatus();
                await GetWifiList();
            }
            else if (currentStep == 3)
            {
                await DeepLensLogin();
            }

            StateHasChanged();
        }

        private async Task Previous()
        {
            await ToStep(--currentStep);
        }

        private async Task Next()
        {
            await ToStep(++currentStep);
        }

        private async Task DetectHardware()
        {
            try
            {
                isDetecting = true;
                StateHasChanged();
                var res = await Http.GetFromJsonAsync<DeepLensSerialNumberResponse>(DEEPLENS_SETTING_URL + "/sn");

                if (res != null)
                {
                    isDetected = true;
                    tipType = 1;
                    tipText = "Detected, device S/N:" + res.SerialNumber;
                }
                else
                {
                    tipType = -1;
                    tipText = "Detection failed";
                }
            }
            catch
            {
                tipType = -1;
                tipText = "Detection failed";
            }


            isDetecting = false;
            StateHasChanged();
        }

        private async Task GetNetworkStatus()
        {
            try
            {
                var res = await Http.GetFromJsonAsync<DeepLensNetworkStatusResponse>(DEEPLENS_SETTING_URL +
                    "/network_status");

                wirelessSSID = res.Wifi;
                ethernetConnected = res.Ethernet;
            }
            catch
            {
                tipType = -1;
                tipText = "Failed to obtain device network status";
            }

            StateHasChanged();
        }

        private async Task GetWifiList()
        {
            try
            {
                var res = await Http.GetFromJsonAsync<DeepLensWifiListResponse>(DEEPLENS_SETTING_URL + "/wifi_ssids");

                wifiList = res.WifiList;
            }
            catch
            {
                tipType = -1;
                tipText = "Failed to obtain the wireless network list";
            }

            StateHasChanged();
        }

        public bool ShareScreenComplete(string streamLabel)
        {
            if (streamLabel == "screen:0:0")
            {
                tipType = 1;
                tipText = "Screen capture obtained successfully";
                return true;
            }
            else if (streamLabel.StartsWith("screen"))
            {
                tipType = -1;
                tipText = "Please make sure that you have only one monitor";
            }
            else
            {
                tipType = -1;
                tipText = "Please share your entire screen, instead of a window or browser tab";
            }

            return false;
        }

        private async Task ConnectWifi()
        {
            try
            {
                isWifiConnecting = true;

                var encodedSsid = System.Web.HttpUtility.UrlEncode(selectedWifi);
                var encodedPassword = System.Web.HttpUtility.UrlEncode(wifiPassword);

                var res = await Http.GetFromJsonAsync<DeepLensActionResponse>(DEEPLENS_SETTING_URL + "/connect_wifi"
                    + "?ssid=" + encodedSsid + "&password=" + encodedPassword);
                if (res.Success)
                {
                    tipType = 1;
                    tipText = "Successfully connect to selected network";
                    await GetNetworkStatus();
                }
                else
                {
                    tipType = -1;
                    tipText = "Failed connect to selected network (probably wrong password or weak signal)";
                }
            }
            catch
            {
                tipType = -1;
                tipText = "An error occured when connecting to network";
            }

            isWifiConnecting = false;
            StateHasChanged();
        }

        public async Task DeepLensLogin()
        {
            try
            {
                var tokenRes =
                    await Http.GetFromJsonAsync<DeepLensTokenResponseModel>("/api/user/GenerateDeepLensToken");
                var token = tokenRes.Token;

                var res = await Http.GetFromJsonAsync<DeepLensActionResponse>(DEEPLENS_SETTING_URL + "/login" +
                                                                              "?token=" + token + "&eid=" + ExamId);
                if (!res.Success)
                {
                    tipType = -1;
                    tipText = "Failed to login your account on the DeepLens device";
                }
            }
            catch
            {
                tipType = -1;
                tipText = "An error occured when getting your DeepLens device connected";
            }
        }

        private class DeepLensSerialNumberResponse
        {
            public string SerialNumber { get; set; }
        }

        private class DeepLensNetworkStatusResponse
        {
            public bool Ethernet { get; set; }
            public string Wifi { get; set; }
        }

        private class DeepLensWifiListItem
        {
            public string Ssid { get; set; }
            public int Strength { get; set; }
            public string Security { get; set; }
        }

        private class DeepLensWifiListResponse
        {
            public DeepLensWifiListItem[] WifiList { get; set; }
        }

        private class DeepLensWifiConnectRequest
        {
            public string Name { get; set; }
            public string Password { get; set; }
        }

        private class DeepLensActionResponse
        {
            public bool Success { get; set; }
        }
    }
}