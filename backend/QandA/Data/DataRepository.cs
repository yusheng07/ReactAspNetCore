using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QandA.Data.Models;
using static Dapper.SqlMapper;

namespace QandA.Data
{
    public class DataRepository : IDataRepository
    {
        private readonly string _connectionString;

        public DataRepository(IConfiguration configuration) 
        {
            _connectionString = configuration["ConnectionStrings:DefaultConnection"];
        }

        public AnswerGetResponse GetAnswer(int answerId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.
                        QueryFirstOrDefault<AnswerGetResponse>("exec dbo.Answer_Get_ByAnswerId @AnswerId=@AnswerId"
                                                                    , new { AnswerId = answerId });
            }
        }
        public async Task<AnswerGetResponse> GetAnswerAsync(int answerId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                return await connection.
                        QueryFirstOrDefaultAsync<AnswerGetResponse>("exec dbo.Answer_Get_ByAnswerId @AnswerId=@AnswerId"
                                                                        , new { AnswerId = answerId });
            }
        }

        public QuestionGetSingleResponse GetQuestion(int questionId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (GridReader results =
                    connection.QueryMultiple(@"exec dbo.Question_GetSingle @QuestionId=@QuestionId;
                                                exec dbo.Answer_Get_ByQuestionId @QuestionId=@QuestionId",
                                                new { QuestionId = questionId }))
                {
                    var question = results.Read<QuestionGetSingleResponse>().FirstOrDefault();
                    if (question != null)
                    {
                        question.Answers = results.Read<AnswerGetResponse>().ToList();
                    }
                    return question;
                }
            }
        }
        public async Task<QuestionGetSingleResponse> GetQuestionAsync(int questionId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (GridReader results =
                        await connection.QueryMultipleAsync(@"exec dbo.Question_GetSingle @QuestionId=@QuestionId;
                                                    exec dbo.Answer_Get_ByQuestionId @QuestionId=@QuestionId",
                                                    new { QuestionId = questionId }))
                {
                    var question = (await results.ReadAsync<QuestionGetSingleResponse>()).FirstOrDefault();
                    if (question != null)
                    {
                        question.Answers = (await results.ReadAsync<AnswerGetResponse>()).ToList();
                    }
                    return question;
                }
            }
        }

        public IEnumerable<QuestionGetManyResponse> GetQuestions()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.Query<QuestionGetManyResponse>("exec dbo.Question_GetMany");
            }
        }
        public async Task<IEnumerable<QuestionGetManyResponse>> GetQuestionsAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                return await connection.QueryAsync<QuestionGetManyResponse>("exec dbo.Question_GetMany");
            }
        }

        public IEnumerable<QuestionGetManyResponse> GetQuestionsWithAnswers()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var questionDictionary = new Dictionary<int, QuestionGetManyResponse>();

                return connection.Query<QuestionGetManyResponse, AnswerGetResponse, QuestionGetManyResponse>
                    ("exec dbo.Question_GetMany_WithAnswers",
                        map : (q,a) => 
                        {
                            QuestionGetManyResponse question;
                            if (!questionDictionary.TryGetValue(q.QuestionId, out question))
                            {
                                question = q;
                                question.Answers = new List<AnswerGetResponse>();                                
                                questionDictionary.Add(question.QuestionId, question);
                            }
                            question.Answers.Add(a);
                            return question;
                        },
                        splitOn: "QuestionId"
                    ).Distinct().ToList();
            }
        }
        public async Task<IEnumerable<QuestionGetManyResponse>> GetQuestionsWithAnswersAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var questionDictionary = new Dictionary<int, QuestionGetManyResponse>();

                return (await connection.QueryAsync<QuestionGetManyResponse, AnswerGetResponse, QuestionGetManyResponse>
                            ("exec dbo.Question_GetMany_WithAnswers",
                                map: (q, a) =>
                                {
                                    QuestionGetManyResponse question;
                                    if (!questionDictionary.TryGetValue(q.QuestionId, out question))
                                    {
                                        question = q;
                                        question.Answers = new List<AnswerGetResponse>();
                                        questionDictionary.Add(question.QuestionId, question);
                                    }
                                    question.Answers.Add(a);
                                    return question;
                                },
                                splitOn: "QuestionId"
                            )
                        ).Distinct().ToList();
            }
        }

        public IEnumerable<QuestionGetManyResponse> GetQuestionsBySearch(string search)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.
                        Query<QuestionGetManyResponse>("exec dbo.Question_GetMany_BySearch @Search=@Search"
                            ,new { Search = search });
            }
        }
        public async Task<IEnumerable<QuestionGetManyResponse>> GetQuestionsBySearchAsync(string search)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                return await connection.
                        QueryAsync<QuestionGetManyResponse>("exec dbo.Question_GetMany_BySearch @Search=@Search"
                            , new { Search = search });
            }
        }

        public IEnumerable<QuestionGetManyResponse> GetQuestionsBySearchWithPaging(string search, int pageNumber, int pageSize)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var parameters = new { Search = search, 
                                        PageNumber = pageNumber, 
                                        PageSize = pageSize };
                return connection.Query<QuestionGetManyResponse>(
                            @"exec dbo.Question_GetMany_BySearch_WithPaging
                                @Search=@Search,
                                @PageNumber=@PageNumber,
                                @PageSize=@PageSize"
                            , parameters);
            }
        }
        public async Task<IEnumerable<QuestionGetManyResponse>> GetQuestionsBySearchWithPagingAsync(string search, int pageNumber, int pageSize)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var parameters = new
                {
                    Search = search,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
                return await connection.QueryAsync<QuestionGetManyResponse>(
                            @"exec dbo.Question_GetMany_BySearch_WithPaging
                                @Search=@Search,
                                @PageNumber=@PageNumber,
                                @PageSize=@PageSize"
                            , parameters);
            }
        }

        public IEnumerable<QuestionGetManyResponse> GetUnansweredQuestions()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.Query<QuestionGetManyResponse>("exec dbo.Question_GetUnanswered");
            }
        }

        public async Task<IEnumerable<QuestionGetManyResponse>> GetUnansweredQuestionsAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();                
                return await connection.
                    QueryAsync<QuestionGetManyResponse>("exec dbo.Question_GetUnanswered");
            }
        }

        public bool QuestionExists(int questionId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.
                        QueryFirst<bool>("exec dbo.Question_Exists @QuestionId=@QuestionId"
                            , new { QuestionId = questionId });
            }
        }
        public async Task<bool> QuestionExistsAsync(int questionId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                return await connection.
                        QueryFirstAsync<bool>("exec dbo.Question_Exists @QuestionId=@QuestionId"
                            , new { QuestionId = questionId });
            }
        }


        public QuestionGetSingleResponse PostQuestion(QuestionPostFullRequest question)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var questionId = connection.QueryFirst<int>(
                                    @"exec dbo.Question_Post
                                        @Title=@Title, @Content=@Content,
                                        @UserId=@UserId, @UserName=@UserName,
                                        @Created=@Created", question);
                return GetQuestion(questionId);
            }
        }
        public async Task<QuestionGetSingleResponse> PostQuestionAsync(QuestionPostFullRequest question)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var questionId = await connection.QueryFirstAsync<int>(
                                    @"exec dbo.Question_Post
                                        @Title=@Title, @Content=@Content,
                                        @UserId=@UserId, @UserName=@UserName,
                                        @Created=@Created", question);
                return await GetQuestionAsync(questionId);
            }
        }
        public QuestionGetSingleResponse PutQuestion(int questionId, QuestionPutRequest question)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                connection.Execute(
                            @"exec dbo.Question_Put
                                @QuestionId=@QuestionId, @Title=@Title,
                                @Content=@Content", 
                            new { QuestionId = questionId, question.Title, question.Content });
                return GetQuestion(questionId);
            }
        }
        public async Task<QuestionGetSingleResponse> PutQuestionAsync(int questionId, QuestionPutRequest question)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync(
                            @"exec dbo.Question_Put
                                @QuestionId=@QuestionId, @Title=@Title,
                                @Content=@Content",
                            new { QuestionId = questionId, question.Title, question.Content });
                return await GetQuestionAsync(questionId);
            }
        }

        public void DeleteQuestion(int questionId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                connection.Execute(@"exec dbo.Question_Delete
                                        @QuestionId=@QuestionId",
                                    new { QuestionId = questionId });
            }
        }
        public async Task DeleteQuestionAsync(int questionId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync(@"exec dbo.Question_Delete
                                                @QuestionId=@QuestionId",
                                            new { QuestionId = questionId });
            }
        }

        public AnswerGetResponse PostAnswer(AnswerPostFullRequest answer)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.QueryFirst<AnswerGetResponse>(
                                    @"exec dbo.Answer_Post
                                        @QuestionId=@QuestionId, @Content=@Content,
                                        @UserId=@UserId, @UserName=@UserName,
                                        @Created=@Created", answer);
            }
        }
        public async Task<AnswerGetResponse> PostAnswerAsync(AnswerPostFullRequest answer)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                return await connection.QueryFirstAsync<AnswerGetResponse>(
                                    @"exec dbo.Answer_Post
                                        @QuestionId=@QuestionId, @Content=@Content,
                                        @UserId=@UserId, @UserName=@UserName,
                                        @Created=@Created", answer);
            }
        }
    }
}
