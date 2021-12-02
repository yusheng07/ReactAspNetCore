using QandA.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace QandA.Data
{
    public class QuestionCache : IQuestionCache
    {
        // TODO - create a memory cache
        private MemoryCache _cache { get; set; }
        public QuestionCache() 
        {            
            _cache = new MemoryCache(new MemoryCacheOptions {
                        //set the cache limit to be 100 items
                        SizeLimit = 100 });
        }
        private string GetCacheKey(int questionId) => $"Question-{questionId}";

        // TODO - method to get a cached question
        public QuestionGetSingleResponse Get(int questionId)
        {
            QuestionGetSingleResponse question;
            _cache.TryGetValue(GetCacheKey(questionId),out question);
            return question;
        }

        // TODO - method to add a cached question
        public void Set(QuestionGetSingleResponse question)
        {
            _cache.Set(GetCacheKey(question.QuestionId)
                        , question
                        //specify the size of the question in the options
                        , new MemoryCacheEntryOptions().SetSize(1));
        }

        // TODO - method to remove a cached question
        public void Remove(int questionId)
        {
            _cache.Remove(GetCacheKey(questionId));
        }
    }
}
