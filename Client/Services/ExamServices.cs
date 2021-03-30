using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.Serialization.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using OneOf.Types;
using SmartProctor.Client.Utils;
using SmartProctor.Shared.Questions;
using SmartProctor.Shared.Requests;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Client.Services
{
    public interface IExamServices
    {
        Task<(int, string)> JoinExam(int examId);
        Task<(int, string)> Attempt(int examId);
        Task<int> AttemptProctor(int examId);
        Task<(int, IList<ExamDetails>)> GetExams(int role);
        Task<(int, ExamDetailsResponseModel)> GetExamDetails(int examId);
        Task<(int, string[])> GetTestTakers(int examId);
        Task<(int, string[])> GetProctors(int examId);
        Task<(int, BaseQuestion)> GetQuestion(int examId, int questionNum);
        Task<int> CreateExam(CreateExamRequestModel model);
    }

    public class ExamServices : IExamServices
    {
        private HttpClient _http;

        public ExamServices(HttpClient http)
        {
            this._http = http;
        }

        public async Task<(int, string)> JoinExam(int examId)
        {
            try
            {
                var res = await _http.GetFromJsonAsync<AttemptExamResponseModel>("/api/exam/JoinExam/" + examId);

                return (res?.Code ?? ErrorCodes.UnknownError, res?.BanReason);
            }
            catch
            {
                return (ErrorCodes.UnknownError, null);
            }
        }
        
        public async Task<(int, string)> Attempt(int examId)
        {
            try
            {
                var res = await _http.GetFromJsonAsync<AttemptExamResponseModel>("/api/exam/Attempt/" + examId);

                return (res?.Code ?? ErrorCodes.UnknownError, res?.BanReason);
            }
            catch 
            {
                return (ErrorCodes.UnknownError, null);
            }
        }

        public async Task<int> AttemptProctor(int examId)
        {
            try
            {
                var res = await _http.GetFromJsonAsync<BaseResponseModel>("/api/exam/EnterProctor/" + examId);

                return res?.Code ?? ErrorCodes.UnknownError;
            }
            catch
            {
                return ErrorCodes.UnknownError;
            }
        }

        public async Task<(int, IList<ExamDetails>)> GetExams(int role)
        {
            try
            {
                var res = await _http.GetFromJsonAsync<GetUserExamsResponseModel>("api/exam/GetExams/" + role);
                
                if (res != null && res.Code == ErrorCodes.Success)
                {
                    return (res.Code, res.ExamDetailsList);
                }
                
                return (res?.Code ?? ErrorCodes.UnknownError, null);
            }
            catch
            {
                return (ErrorCodes.UnknownError, null);
            }
        }

        public async Task<(int, ExamDetailsResponseModel)> GetExamDetails(int examId)
        {
            try
            {
                var res = await _http.GetFromJsonAsync<ExamDetailsResponseModel>("api/exam/ExamDetails/" + examId);

                if (res != null && res.Code == ErrorCodes.Success)
                {
                    return (res.Code, res);
                }

                return (res?.Code ?? ErrorCodes.UnknownError, null);
            }
            catch
            {
                return (ErrorCodes.UnknownError, null);
            }
        }

        public async Task<(int, string[])> GetTestTakers(int examId)
        {
            try
            {
                var res = await _http.GetFromJsonAsync<GetExamTakersResponseModel>("api/exam/GetExamTakers/" + examId);

                if (res != null && res.Code == ErrorCodes.Success)
                {
                    return (res.Code, res.ExamTakers.ToArray());
                }

                return (res?.Code ?? ErrorCodes.UnknownError, null);
            }
            catch
            {
                return (ErrorCodes.UnknownError, null);
            }
        }
        
        public async Task<(int, string[])> GetProctors(int examId)
        {
            try
            {
                var res = await _http.GetFromJsonAsync<GetProctorsResponseModel>("api/exam/GetProctors/" + examId);

                if (res != null && res.Code == ErrorCodes.Success)
                {
                    return (res.Code, res.Proctors.ToArray());
                }

                return (res?.Code ?? ErrorCodes.UnknownError, null);
            }
            catch
            {
                return (ErrorCodes.UnknownError, null);
            }
        }

        public async Task<(int, BaseQuestion)> GetQuestion(int examId, int questionNum)
        {
            try
            {
                var res = await _http.PostAsAndGetFromJsonAsync<GetQuestionRequestModel, GetQuestionResponseModel>(
                    "/api/exam/GetQuestion", new GetQuestionRequestModel()
                    {
                        ExamId = examId,
                        QuestionNumber = questionNum
                    });

                if (res == null || res.Code != ErrorCodes.Success || res.QuestionJson == null)
                {
                    return (res?.Code ?? ErrorCodes.UnknownError, null);
                }

                return (res.Code, DeserializeQuestionFromJson(res.QuestionJson));
            }
            catch
            {
                return (ErrorCodes.UnknownError, null);
            }
        }

        public async Task<int> CreateExam(CreateExamRequestModel model)
        {
            try
            {
                var res = await _http.PostAsAndGetFromJsonAsync<CreateExamRequestModel, BaseResponseModel>(
                    "/api/exam/CreateExam", model);

                return res?.Code ?? ErrorCodes.UnknownError;
            }
            catch
            {
                return ErrorCodes.UnknownError;
            }
        }

        private string SerializeQuestionToJson(BaseQuestion question)
        {
            throw new NotImplementedException();
        }

        private BaseQuestion DeserializeQuestionFromJson(string questionJson)
        {
            var json = JsonDocument.Parse(questionJson).RootElement;

            var type = json.GetProperty("type").GetString();

            BaseQuestion ret = null;

            switch (type)
            {
                case "choice":
                {
                    ret = new ChoiceQuestion();
                    var choices = json.GetProperty("choices");
                    var multiChoice = json.GetProperty("multiChoice").GetBoolean();

                    ((ChoiceQuestion) ret).MultiChoice = multiChoice;
                    ((ChoiceQuestion) ret).Choices = new List<string>();
                    for (var i = 0; i < choices.GetArrayLength(); i++)
                    {
                        ((ChoiceQuestion) ret).Choices.Add(choices[i].GetString());
                    }

                    break;
                }
                case "fill":
                {
                    ret = new FillQuestion();
                    var blankPositions = json.GetProperty("blankPositions");
                    var blankTypes = json.GetProperty("blankTypes");

                    ((FillQuestion) ret).BlankType = new List<int>();

                
                    for (var i = 0; i < blankTypes.GetArrayLength(); i++)
                    {
                        ((FillQuestion) ret).BlankType.Add(blankTypes[i].GetInt32());
                    }

                    break;
                }
                case "short_answer":
                {
                    ret = new ShortAnswerQuestion();
                    var maxWordCount = json.GetProperty("maxWordCount").GetInt32();

                    ((ShortAnswerQuestion) ret).MaxWordCount = maxWordCount;
                    break;
                }
            }

            return ret;
        }
    }
}