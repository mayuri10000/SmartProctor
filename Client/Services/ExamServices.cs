using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using OneOf.Types;
using SmartProctor.Client.Utils;
using SmartProctor.Shared.Questions;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Client.Services
{
    public interface IExamServices
    {
        Task<int> Attempt(int examId);
    }
    
    public class ExamServices : IExamServices
    {
        private HttpClient _http;

        public ExamServices(HttpClient http)
        {
            this._http = http;
        }

        public async Task<int> Attempt(int examId)
        {
            try
            {
                var res = await _http.GetFromJsonAsync<BaseResponseModel>("/api/exam/Attempt/" + examId);

                return res?.Code ?? ErrorCodes.UnknownError;
            }
            catch (HttpRequestException)
            {
                return ErrorCodes.UnknownError;
            }
        }

        public async Task<int> AttemptProctor(int examId)
        {
            try
            {
                var res = await _http.GetFromJsonAsync<BaseResponseModel>("/api/exam/AttemptProctor/" + examId);

                return res?.Code ?? ErrorCodes.UnknownError;
            }
            catch (HttpRequestException)
            {
                return ErrorCodes.UnknownError;
            }
        }

        public async Task<BaseQuestion> GetQuestion(int examId, int questionNum)
        {
            try
            {
            }
        }
        
    }
}