using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using SmartProctor.Client.Utils;
using SmartProctor.Shared.Requests;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Client.Services
{
    public interface IUserServices
    {
        Task<int> LoginAsync(LoginRequestModel model);
        Task<int> LogoutAsync();
        Task<int> RegisterAsync(RegisterRequestModel model);

        Task<UserDetailsResponseModel> GetUserDetails();
    }
    
    public class UserServices : IUserServices
    {
        private HttpClient _http;

        public UserServices(HttpClient http)
        {
            this._http = http;
        }
        
        public async Task<int> LoginAsync(LoginRequestModel model)
        {
            try
            {
                var res = await _http.PostAsJsonAsync<LoginRequestModel>("/api/user/Login", model);
                var resJson = await res.Content.ReadFromJsonAsync<BaseResponseModel>();
                
                return resJson?.Code ?? ErrorCodes.UnknownError;
            }
            catch(HttpRequestException)
            {
                return ErrorCodes.UnknownError;
            }
        }

        public async Task<int> LogoutAsync()
        {
            try
            {
                var res = await _http.GetFromJsonAsync<BaseResponseModel>("/api/user/Logout");

                return res?.Code ?? ErrorCodes.UnknownError;
            }
            catch (HttpRequestException)
            {
                return ErrorCodes.UnknownError;
            }
        }

        public async Task<int> RegisterAsync(RegisterRequestModel model)
        {
            try
            {
                var res = await _http.PostAsJsonAsync<RegisterRequestModel>("/api/user/Register", model);
                var resJson = await res.Content.ReadFromJsonAsync<BaseResponseModel>();

                return resJson?.Code ?? ErrorCodes.UnknownError;
            }
            catch (HttpRequestException)
            {
                return ErrorCodes.UnknownError;
            }
        }

        public async Task<UserDetailsResponseModel> GetUserDetails()
        {
            try
            {
                var res = await _http.GetFromJsonAsync<UserDetailsResponseModel>("/api/user/UserDetails");

                return (res == null || res.Code != 0) ? null : res;
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }
    }
}