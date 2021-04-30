using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OneOf.Types;
using SmartProctor.Client.Utils;
using SmartProctor.Shared.Answers;
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
        Task<(int, IList<UserBasicInfo>)> GetTestTakers(int examId);
        Task<(int, IList<UserBasicInfo>)> GetProctors(int examId);
        Task<(int, IList<BaseQuestion>)> GetPaper(int examId);
        Task<int> CreateExam(CreateExamRequestModel model);
        Task<int> UpdatePaper(int examId, IList<BaseQuestion> baseQuestion);
        Task<int> SubmitAnswer(int examId, int questionNum, BaseAnswer answer);
        Task<(int, BaseAnswer, DateTime)> GetAnswer(string uid, int examId, int questionNum);
        Task<int> UpdateExamDetails(UpdateExamDetailsRequestModel model);
        Task<int> BanExamTaker(int examId, string userId, string reason);
        Task<int> SendEvent(int eid, int type, string message, string receipt);
        Task<(int, IList<EventItem>)> GetEvents(int eid, string sender, string receipt);
    }

    public class ExamServices : IExamServices
    {
        private HttpClient _http;

        private ILogger<ExamServices> _logger;

        public ExamServices(HttpClient http, ILogger<ExamServices> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<(int, string)> JoinExam(int examId)
        {
            _logger.LogInformation($"JoinExam examId = {examId}");
            try
            {
                var res = await _http.GetFromJsonAsync<AttemptExamResponseModel>("/api/exam/JoinExam/" + examId);

                return (res?.Code ?? ErrorCodes.UnknownError, res?.BanReason);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return (ErrorCodes.UnknownError, null);
            }
        }
        
        public async Task<(int, string)> Attempt(int examId)
        {
            _logger.LogInformation($"Attempt examId = {examId}");
            try
            {
                var res = await _http.GetFromJsonAsync<AttemptExamResponseModel>("/api/exam/Attempt/" + examId);

                return (res?.Code ?? ErrorCodes.UnknownError, res?.BanReason);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return (ErrorCodes.UnknownError, null);
            }
        }

        public async Task<int> AttemptProctor(int examId)
        {
            _logger.LogInformation($"AttemptProctor examId = {examId}");
            try
            {
                var res = await _http.GetFromJsonAsync<BaseResponseModel>("/api/exam/EnterProctor/" + examId);

                return res?.Code ?? ErrorCodes.UnknownError;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return ErrorCodes.UnknownError;
            }
        }

        public async Task<(int, IList<ExamDetails>)> GetExams(int role)
        {
            _logger.LogInformation($"GetExams role = {role}");
            try
            {
                var res = await _http.GetFromJsonAsync<GetUserExamsResponseModel>("api/exam/GetExams/" + role);
                
                if (res != null && res.Code == ErrorCodes.Success)
                {
                    return (res.Code, res.ExamDetailsList);
                }
                
                return (res?.Code ?? ErrorCodes.UnknownError, null);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return (ErrorCodes.UnknownError, null);
            }
        }

        public async Task<(int, ExamDetailsResponseModel)> GetExamDetails(int examId)
        {
            _logger.LogInformation($"GetExamDetails examId = {examId}");
            try
            {
                var res = await _http.GetFromJsonAsync<ExamDetailsResponseModel>("api/exam/ExamDetails/" + examId);

                if (res != null && res.Code == ErrorCodes.Success)
                {
                    return (res.Code, res);
                }

                return (res?.Code ?? ErrorCodes.UnknownError, null);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return (ErrorCodes.UnknownError, null);
            }
        }

        public async Task<(int, IList<UserBasicInfo>)> GetTestTakers(int examId)
        {
            try
            {
                var res = await _http.GetFromJsonAsync<GetExamTakersResponseModel>("api/exam/GetExamTakers/" + examId);

                if (res != null && res.Code == ErrorCodes.Success)
                {
                    return (res.Code, res.ExamTakers);
                }

                return (res?.Code ?? ErrorCodes.UnknownError, null);
            }
            catch
            {
                return (ErrorCodes.UnknownError, null);
            }
        }
        
        public async Task<(int, IList<UserBasicInfo>)> GetProctors(int examId)
        {
            try
            {
                var res = await _http.GetFromJsonAsync<GetProctorsResponseModel>("api/exam/GetProctors/" + examId);

                if (res != null && res.Code == ErrorCodes.Success)
                {
                    return (res.Code, res.Proctors);
                }

                return (res?.Code ?? ErrorCodes.UnknownError, null);
            }
            catch
            {
                return (ErrorCodes.UnknownError, null);
            }
        }

        public async Task<(int, IList<BaseQuestion>)> GetPaper(int examId)
        {
            _logger.LogInformation($"GetPaper examId = {examId}");
            try
            {
                var res = await _http.GetFromJsonAsync<GetPaperResponseModel>(
                    "/api/exam/GetPaper/" + examId);

                if (res == null || res.Code != ErrorCodes.Success || res.QuestionJsons == null)
                {
                    return (res?.Code ?? ErrorCodes.UnknownError, null);
                }

                return (res.Code, res.QuestionJsons.Select(q => DeserializeQuestionFromJson(q)).ToList());
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
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

        public async Task<int> UpdatePaper(int examId, IList<BaseQuestion> question)
        {
            try
            {
                var questions = question.Select(q => SerializeQuestionToJson(q)).ToList();
                
                var model = new UpdatePaperRequestModel()
                {
                    ExamId = examId,
                    QuestionJsons = questions
                };

                var res = await _http.PostAsAndGetFromJsonAsync<UpdatePaperRequestModel, BaseResponseModel>(
                    "/api/exam/UpdatePaper", model);

                return res?.Code ?? ErrorCodes.UnknownError;
            }
            catch
            {
                return ErrorCodes.UnknownError;
            }
        }

        public async Task<int> SubmitAnswer(int examId, int questionNum, BaseAnswer answer)
        {
            try
            {
                var model = new SubmitAnswerRequestModel()
                {
                    ExamId = examId,
                    QuestionNum = questionNum,
                    AnswerJson = SerializeAnswerToJson(answer)
                };

                var res = await _http.PostAsAndGetFromJsonAsync<SubmitAnswerRequestModel, BaseResponseModel>(
                    "/api/exam/SubmitAnswer", model);
                return res?.Code ?? ErrorCodes.UnknownError;
            }
            catch
            {
                return ErrorCodes.UnknownError;
            }
        }

        public async Task<(int, BaseAnswer, DateTime)> GetAnswer(string uid, int examId, int questionNum)
        {
            try
            {
                var res = await _http.PostAsAndGetFromJsonAsync<GetAnswerRequestModel, GetAnswerResponseModel>(
                    "/api/exam/GetAnswer", new GetAnswerRequestModel()
                    {
                        ExamId = examId,
                        UserId = uid,
                        QuestionNum = questionNum
                    });

                if (res.Code == ErrorCodes.Success)
                {
                    return (ErrorCodes.Success, DeserializeAnswerFromJson(res.AnswerJson), res.AnswerTime.Value);
                }

                return (res.Code, null, DateTime.MinValue);
            }
            catch
            {
                return (ErrorCodes.UnknownError, null, DateTime.MinValue);
            }
        }

        public async Task<int> UpdateExamDetails(UpdateExamDetailsRequestModel model)
        {
            try
            {
                var res = await _http.PostAsAndGetFromJsonAsync<UpdateExamDetailsRequestModel, BaseResponseModel>(
                    "/api/exam/UpdateExamDetails", model);

                return res?.Code ?? ErrorCodes.UnknownError;
            }
            catch
            {
                return ErrorCodes.UnknownError;
            }
        }

        public async Task<int> BanExamTaker(int examId, string userId, string reason)
        {
            try
            {
                var res = await _http.PostAsAndGetFromJsonAsync<BanExamTakerRequestModel, BaseResponseModel>(
                    "/api/exam/BanExamTaker", new BanExamTakerRequestModel()
                    {
                        ExamId = examId,
                        UserId = userId,
                        Reason = reason
                    });

                return res?.Code ?? ErrorCodes.UnknownError;
            }
            catch
            {
                return ErrorCodes.UnknownError;
            }
        }

        public async Task<int> SendEvent(int eid, int type, string message, string receipt)
        {
            _logger.LogInformation($"SendEvent eid = {eid}, type = {type}, message = {message}, receipt = {receipt}");
            try
            {
                var res = await _http.PostAsAndGetFromJsonAsync<SendEventRequestModel, BaseResponseModel>(
                    "/api/exam/SendEvent", new SendEventRequestModel
                    {
                        ExamId = eid,
                        Attachment = null,
                        Message = message,
                        Type = type,
                        Receipt = receipt
                    });

                return res?.Code ?? ErrorCodes.UnknownError;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return ErrorCodes.UnknownError;
            }
        }

        public async Task<(int, IList<EventItem>)> GetEvents(int eid, string sender, string receipt)
        {
            _logger.LogInformation($"GetEvents eid = {eid}, sender = {sender}, receipt = {receipt}");
            try
            {
                var res = await _http.PostAsAndGetFromJsonAsync<GetEventsRequestModel, GetEventsResponseModel>(
                    "/api/exam/GetEvents", new GetEventsRequestModel
                    {
                        ExamId = eid, Receipt = receipt, Sender = sender
                    });

                return (res.Code, res.Events);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return (ErrorCodes.UnknownError, null);
            }
        }
        
        private string SerializeQuestionToJson(BaseQuestion question)
        {
            var ms = new MemoryStream();
            var json = new Utf8JsonWriter(ms);

            json.WriteStartObject();
            json.WriteString("question", question.Question);
            json.WriteString("type", question.QuestionType);

            if (question is ChoiceQuestion choiceQuestion)
            {
                json.WriteBoolean("multiChoice", choiceQuestion.MultiChoice);
                json.WriteStartArray("choices");
                foreach (var choice in choiceQuestion.Choices)
                {
                    json.WriteStringValue(choice);
                }
                json.WriteEndArray();
            }
            else if (question is ShortAnswerQuestion shortAnswerQuestion)
            {
                json.WriteNumber("maxWordCount", shortAnswerQuestion.MaxWordCount);
                json.WriteBoolean("richText", shortAnswerQuestion.RichText);
            }
            
            json.WriteEndObject();
            json.Flush();
            
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private string SerializeAnswerToJson(BaseAnswer answer)
        {
            var ms = new MemoryStream();
            var json = new Utf8JsonWriter(ms);
            
            json.WriteStartObject();
            if (answer is ChoiceAnswer choiceAnswer)
            {
                json.WriteString("type", "choice");
                json.WriteStartArray("choices");
                foreach (var choice in choiceAnswer.Choices)
                {
                    json.WriteNumberValue(choice);
                }
                json.WriteEndArray();
            }
            else if (answer is ShortAnswer shortAnswer)
            {
                json.WriteString("type", "short_answer");
                json.WriteString("answer", shortAnswer.Answer);
            }
            
            json.WriteEndObject();
            json.Flush();
            
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private BaseQuestion DeserializeQuestionFromJson(string questionJson)
        {
            var json = JsonDocument.Parse(questionJson).RootElement;

            var question = json.GetProperty("question").GetString();
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
                    var richText = json.GetProperty("richText").GetBoolean();

                    ((ShortAnswerQuestion) ret).MaxWordCount = maxWordCount;
                    ((ShortAnswerQuestion) ret).RichText = richText;
                    break;
                }
            }

            if (ret != null)
            {
                ret.Question = question;
                ret.QuestionType = type;
            }

            return ret;
        }

        private BaseAnswer DeserializeAnswerFromJson(string answerJson)
        {
            var json = JsonDocument.Parse(answerJson).RootElement;
            var type = json.GetProperty("type").GetString();

            BaseAnswer ret = null;
            switch (type)
            {
                case "choice":
                {
                    var choices = json.GetProperty("choices");
                    
                    ret = new ChoiceAnswer();
                    ((ChoiceAnswer) ret).Choices = new List<int>();
                    for (var i = 0; i < choices.GetArrayLength(); i++)
                    {
                        ((ChoiceAnswer) ret).Choices.Add(choices[i].GetInt32());
                    }
                    break;
                }
                case "short_answer":
                {
                    var answer = json.GetProperty("answer");
                    
                    ret = new ShortAnswer();
                    ((ShortAnswer) ret).Answer = answer.GetString();
                    break;
                }
            }

            if (ret != null)
            {
                ret.Type = type;
            }

            return ret;
        }
    }
}