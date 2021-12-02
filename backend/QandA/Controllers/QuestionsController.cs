using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using QandA.Data;
using QandA.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace QandA.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly IDataRepository _dataRepository;

        private readonly IQuestionCache _cache;

        private readonly IHttpClientFactory _clientFactory;
        private readonly string _auth0UserInfo;

        public QuestionsController(IDataRepository dataRepository, IQuestionCache questionCache
                                    , IHttpClientFactory clientFactory
                                    , IConfiguration configuration) 
        {
            // TODO - set reference to _dataRepository
            _dataRepository = dataRepository;
            //
            _cache = questionCache;
            //
            _clientFactory = clientFactory;
            _auth0UserInfo = $"{configuration["Auth0:Authority"]}userinfo";
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IEnumerable<QuestionGetManyResponse>> GetQuestions(string search,bool includeAnswers,int page=1,int pageSize=20) 
        {
            if (string.IsNullOrEmpty(search))
            {
                if (includeAnswers)
                {
                    return await _dataRepository.GetQuestionsWithAnswersAsync();
                }
                else
                {
                    return await _dataRepository.GetQuestionsAsync();
                }
            }
            else
            {
                return await _dataRepository.GetQuestionsBySearchWithPagingAsync(search,page,pageSize);
            }
        }
        //[AllowAnonymous]
        //[HttpGet]
        //public IEnumerable<QuestionGetManyResponse> GetQuestions(string search, bool includeAnswers, int page = 1, int pageSize = 20)
        //{
        //    if (string.IsNullOrEmpty(search))
        //    {
        //        if (includeAnswers)
        //        {
        //            return _dataRepository.GetQuestionsWithAnswers();
        //        }
        //        else
        //        {
        //            return _dataRepository.GetQuestions();
        //        }
        //    }
        //    else
        //    {
        //        return _dataRepository.GetQuestionsBySearchWithPaging(search, page, pageSize);
        //    }
        //}

        [AllowAnonymous]
        [HttpGet("unanswered")]
        public async Task<IEnumerable<QuestionGetManyResponse>> GetUnansweredQuestions()
        {
            return await _dataRepository.GetUnansweredQuestionsAsync();            
        }
        //[HttpGet("unanswered")]
        //public IEnumerable<QuestionGetManyResponse> GetUnansweredQuestions()
        //{
        //    return _dataRepository.GetUnansweredQuestions();
        //}

        [AllowAnonymous]
        [HttpGet("{questionId}")]
        public async Task<ActionResult<QuestionGetSingleResponse>> GetQuestion(int questionId)
        {
            var question = _cache.Get(questionId);
            if (question==null) //not in the cache
            {
                // TODO - call the data repository to get the question
                question = await _dataRepository.GetQuestionAsync(questionId);
                // TODO - return HTTP status code 404 if the question isn't found
                if (question == null)
                {
                    return NotFound();
                }
                //put it into the cache
                _cache.Set(question);
            }
            // TODO - return question in response with status code 200
            return question;
        }
        //[AllowAnonymous]
        //[HttpGet("{questionId}")]
        //public ActionResult<QuestionGetSingleResponse> GetQuestion(int questionId)
        //{
        //    var question = _cache.Get(questionId);
        //    if (question == null) //not in the cache
        //    {
        //        // TODO - call the data repository to get the question
        //        question = _dataRepository.GetQuestion(questionId);
        //        // TODO - return HTTP status code 404 if the question isn't found
        //        if (question == null)
        //        {
        //            return NotFound();
        //        }
        //        //put it into the cache
        //        _cache.Set(question);
        //    }
        //    // TODO - return question in response with status code 200
        //    return question;
        //}

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<QuestionGetSingleResponse>> PostQuestion(QuestionPostRequest questionPostRequest)
        {
            // TODO - call the data repository to save the question
            var savedQuestion = await _dataRepository.PostQuestionAsync(
                new QuestionPostFullRequest
                {
                    Title = questionPostRequest.Title,
                    Content = questionPostRequest.Content,
                    UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value,
                    UserName = await GetUserName(),
                    Created = DateTime.UtcNow
                }); ;
            // TODO - return HTTP status code 201
            return CreatedAtAction(nameof(GetQuestion)
                    , new { questionId = savedQuestion.QuestionId }
                    , savedQuestion);
        }
        //[Authorize]
        //[HttpPost]
        //public ActionResult<QuestionGetSingleResponse> PostQuestion(QuestionPostRequest questionPostRequest)
        //{
        //    // TODO - call the data repository to save the question
        //    var savedQuestion = _dataRepository.PostQuestion(
        //        new QuestionPostFullRequest
        //        {
        //            Title = questionPostRequest.Title,
        //            Content = questionPostRequest.Content,
        //            UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value,
        //            UserName = await GetUserName(),
        //            Created = DateTime.UtcNow
        //        }); ;
        //    // TODO - return HTTP status code 201
        //    return CreatedAtAction(nameof(GetQuestion)
        //            , new { questionId = savedQuestion.QuestionId }
        //            , savedQuestion);
        //}

        [Authorize(Policy = "MustBeQuestionAuthor")]
        [HttpPut("{questionId}")]
        public async Task<ActionResult<QuestionGetSingleResponse>> PutQuestion(int questionId,QuestionPutRequest questionPutRequest)
        {
            // TODO - get the question from the data repository
            var question = await _dataRepository.GetQuestionAsync(questionId);
            // TODO - return HTTP status code 404 if the question isn't found
            if (question == null)
            {
                return NotFound();
            }
            // TODO - update the question model
            questionPutRequest.Title = string.IsNullOrEmpty(questionPutRequest.Title) ? question.Title : questionPutRequest.Title;
            questionPutRequest.Content = string.IsNullOrEmpty(questionPutRequest.Content) ? question.Title : questionPutRequest.Content;
            // TODO - call the data repository with the updated question model
            // to update the question in the database
            var savedQuestion = await _dataRepository.PutQuestionAsync(questionId, questionPutRequest);

            //when a question changes, remove the item from cache
            _cache.Remove(savedQuestion.QuestionId);

            // TODO - return the saved question
            return savedQuestion;
        }
        //[Authorize(Policy = "MustBeQuestionAuthor")]
        //[HttpPut("{questionId}")]
        //public ActionResult<QuestionGetSingleResponse> PutQuestion(int questionId, QuestionPutRequest questionPutRequest)
        //{
        //    // TODO - get the question from the data repository
        //    var question = _dataRepository.GetQuestion(questionId);
        //    // TODO - return HTTP status code 404 if the question isn't found
        //    if (question == null)
        //    {
        //        return NotFound();
        //    }
        //    // TODO - update the question model
        //    questionPutRequest.Title = string.IsNullOrEmpty(questionPutRequest.Title) ? question.Title : questionPutRequest.Title;
        //    questionPutRequest.Content = string.IsNullOrEmpty(questionPutRequest.Content) ? question.Title : questionPutRequest.Content;
        //    // TODO - call the data repository with the updated question model
        //    // to update the question in the database
        //    var savedQuestion = _dataRepository.PutQuestion(questionId, questionPutRequest);

        //    //when a question changes, remove the item from cache
        //    _cache.Remove(savedQuestion.QuestionId);

        //    // TODO - return the saved question
        //    return savedQuestion;
        //}

        [Authorize(Policy = "MustBeQuestionAuthor")]
        [HttpDelete("{questionId}")]
        public async Task<ActionResult> DeleteQuestion(int questionId)
        {
            var question = await _dataRepository.GetQuestionAsync(questionId);
            if (question == null)
            {
                return NotFound();
            }
            await _dataRepository.DeleteQuestionAsync(questionId);

            //remove the item from cache
            _cache.Remove(questionId);

            return NoContent();
        }
        //[Authorize(Policy = "MustBeQuestionAuthor")]
        //[HttpDelete("{questionId}")]
        //public ActionResult DeleteQuestion(int questionId)
        //{
        //    var question = _dataRepository.GetQuestion(questionId);
        //    if (question == null)
        //    {
        //        return NotFound();
        //    }
        //    _dataRepository.DeleteQuestion(questionId);

        //    //remove the item from cache
        //    _cache.Remove(questionId);

        //    return NoContent();
        //}

        [Authorize]
        [HttpPost("answer")]
        public async Task<ActionResult<AnswerGetResponse>> PostAnswer(AnswerPostRequest answerPostRequest)
        {
            var questionExists = await _dataRepository.QuestionExistsAsync(answerPostRequest.QuestionId.Value);
            if (!questionExists)
            {
                return NotFound();
            }
            var savedAnswer = await _dataRepository.PostAnswerAsync(
                new AnswerPostFullRequest
                {
                    QuestionId = answerPostRequest.QuestionId.Value,
                    Content = answerPostRequest.Content,
                    UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value,
                    UserName = await GetUserName(),
                    Created = DateTime.UtcNow
                });

            //when an answer is being posted, remove the question item from cache
            _cache.Remove(answerPostRequest.QuestionId.Value);

            return savedAnswer;
        }
        //[Authorize]
        //[HttpPost("answer")]
        //public ActionResult<AnswerGetResponse> PostAnswer(AnswerPostRequest answerPostRequest)
        //{
        //    var questionExists = _dataRepository.QuestionExists(answerPostRequest.QuestionId.Value);
        //    if (!questionExists)
        //    {
        //        return NotFound();
        //    }
        //    var savedAnswer = _dataRepository.PostAnswer(
        //        new AnswerPostFullRequest
        //        {
        //            QuestionId = answerPostRequest.QuestionId.Value,
        //            Content = answerPostRequest.Content,
        //            UserId = "1",
        //            UserName = "user1@test.com",
        //            Created = DateTime.UtcNow
        //        });

        //    //when an answer is being posted, remove the question item from cache
        //    _cache.Remove(answerPostRequest.QuestionId.Value);

        //    return savedAnswer;
        //}
        private async Task<string> GetUserName()
        {
            var request = new HttpRequestMessage(HttpMethod.Get,_auth0UserInfo);
            request.Headers.Add("Authorization",Request.Headers["Authorization"].First());

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<User>(jsonContent
                            ,new JsonSerializerOptions { PropertyNameCaseInsensitive=true });
                return user.Name;
            }
            else
            {
                return string.Empty;
            }
        }
    
    }
}
