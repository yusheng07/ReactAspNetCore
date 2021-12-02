using QandA.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QandA.Data
{
    public interface IDataRepository
    {
        IEnumerable<QuestionGetManyResponse> GetQuestions();
        Task<IEnumerable<QuestionGetManyResponse>> GetQuestionsAsync();
        IEnumerable<QuestionGetManyResponse> GetQuestionsWithAnswers();
        Task<IEnumerable<QuestionGetManyResponse>> GetQuestionsWithAnswersAsync();
        IEnumerable<QuestionGetManyResponse> GetQuestionsBySearch(string search);
        Task<IEnumerable<QuestionGetManyResponse>> GetQuestionsBySearchAsync(string search);
        IEnumerable<QuestionGetManyResponse> GetQuestionsBySearchWithPaging(string search, int pageNumber, int pageSize);
        Task<IEnumerable<QuestionGetManyResponse>> GetQuestionsBySearchWithPagingAsync(string search, int pageNumber, int pageSize);
        IEnumerable<QuestionGetManyResponse> GetUnansweredQuestions();
        Task<IEnumerable<QuestionGetManyResponse>> GetUnansweredQuestionsAsync();
        QuestionGetSingleResponse GetQuestion(int questionId);
        Task<QuestionGetSingleResponse> GetQuestionAsync(int questionId);
        bool QuestionExists(int questionId);
        Task<bool> QuestionExistsAsync(int questionId);
        AnswerGetResponse GetAnswer(int answerId);
        Task<AnswerGetResponse> GetAnswerAsync(int answerId);

        QuestionGetSingleResponse PostQuestion(QuestionPostFullRequest question);
        Task<QuestionGetSingleResponse> PostQuestionAsync(QuestionPostFullRequest question);
        QuestionGetSingleResponse PutQuestion(int questionId, QuestionPutRequest question);
        Task<QuestionGetSingleResponse> PutQuestionAsync(int questionId, QuestionPutRequest question);
        void DeleteQuestion(int questionId);
        Task DeleteQuestionAsync(int questionId);
        AnswerGetResponse PostAnswer(AnswerPostFullRequest answer);
        Task<AnswerGetResponse> PostAnswerAsync(AnswerPostFullRequest answer);
    }
}
