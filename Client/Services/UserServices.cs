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

        Task<(int, UserDetailsResponseModel)> GetUserDetails();
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
                var res = await _http.PostAsAndGetFromJsonAsync<LoginRequestModel, BaseResponseModel>("/api/user/Login", model);
                
                return res?.Code ?? ErrorCodes.UnknownError;
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
                var res = await _http.PostAsAndGetFromJsonAsync<RegisterRequestModel,BaseResponseModel>("/api/user/Register", model);

                return res?.Code ?? ErrorCodes.UnknownError;
            }
            catch (HttpRequestException)
            {
                return ErrorCodes.UnknownError;
            }
        }

        public async Task<(int, UserDetailsResponseModel)> GetUserDetails()
        {
            UserDetailsResponseModel userDetails = null;
            try
            {
                var res = await _http.GetFromJsonAsync<UserDetailsResponseModel>("/api/user/UserDetails");

                if (res != null && res.Code != 0)
                {
                    userDetails = res;
                }

                return (res?.Code ?? ErrorCodes.UnknownError, userDetails);
            }
            catch (HttpRequestException)
            {
                return (ErrorCodes.UnknownError, null);
            }
        }
    }
}