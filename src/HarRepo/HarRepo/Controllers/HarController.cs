using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HarRepo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;

namespace HarRepo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HarController : ControllerBase
    {

        private IMemoryCache _cache;
        private static int _id = 0;

        public HarController(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }

        [HttpGet("{id}", Name = "Get")]
        public IActionResult Get(int id, string query)
        {
            try
            {
                if (!_cache.TryGetValue(id, out Har value))
                {
                    return StatusCode(404, new { Success = false, Status = 404, Id = id });
                }

                if (query == null)
                {
                    return StatusCode(200, new { Success = true, Id = id, Status = 200, Model = value });
                }

                if (query == "blocked")
                {
                    BlockedQuery bq = CalculateBlockedStats(value);
                    return StatusCode(200, new { Success = true, Id = id, Status = 200, Model = value, Query = bq });
                }
                else if (query == "bodysize")
                {
                    BodySizeQuery bq = CalculateBodySizeStats(value);
                    return StatusCode(200, new { Success = true, Id = id, Status = 200, Model = value, Query = bq });
                }
                else if (query == "querystring")
                {
                    List<string> urls = GetUrlsWithInputString(value);
                    return StatusCode(200, new { Success = true, Id = id, Status = 200, Model = value, Query = urls });
                }
                else if (query == "slowestpage")
                {
                    var slowest = GetUrlsWithSlowestPage(value);
                    return StatusCode(200, new { Success = true, Id = id, Status = 200, Model = value, Query = slowest });
                }


                return StatusCode(400, new { Success = false, Status = 400, Message = "Unknown Query: " + query });
            }
            catch (Exception ex)
            {
                return StatusCode(200, new { Success = false, Status = 500, Message = ex.Message });
            }
        }

        private object GetUrlsWithSlowestPage(Har har)
        {
            Entry[] entries = har.Log.Entries;
            int length = entries.Length;
            double slowest = 0.0;
            string url = String.Empty;
            for (int i = 0; i < length; ++i)
            {
                Entry entry = entries[i];

                if (entry.Time > slowest)
                {
                    slowest = entry.Time;
                    url = entry.Request.Url;
                }
            }

            return new SlowestPageQuery { Url = url, Size = slowest};
        }

        private BlockedQuery CalculateBlockedStats(Har har)
        {
            Entry[] entries = har.Log.Entries;

            int length = entries.Length;

            Entry longest = null, shortest = null, secondShortest = null;

            for (int i = 0; i < length; ++i)
            {
                double blocked = entries[i].Timings.Blocked;

                if (blocked < 0.0)
                {
                    continue;
                }

                if (longest == null)
                {

                    longest = entries[i];
                    shortest = entries[i];
                    continue;
                }


                if (blocked > longest.Timings.Blocked)
                {
                    longest = entries[i];

                }
                else if (blocked < shortest.Timings.Blocked)
                {
                    secondShortest = shortest;
                    shortest = entries[i];
                }
                else if (secondShortest == null)
                {
                    secondShortest = entries[i];
                }
                else if (blocked < secondShortest.Timings.Blocked)
                {
                    secondShortest = entries[i];
                }
            }

            BlockedQuery toReturn = new BlockedQuery
            {
                Longest = longest,
                Shortest = shortest,
                SecondShortest = secondShortest
            };

            return toReturn;

        }

        private BodySizeQuery CalculateBodySizeStats(Har har)
        {
            Entry[] entries = har.Log.Entries;

            int length = entries.Length;

            int totalBodySize = 0;

            for (int i = 0; i < length; ++i)
            {
                totalBodySize += entries[i].Request.BodySize;
            }

            float average = 0.0f;

            if (length != 0)
            {
                average = ((float)totalBodySize) / length;
            }

            BodySizeQuery toReturn = new BodySizeQuery
            {
                AverageSize = average,
                TotalSize = totalBodySize
            };


            return toReturn;

        }

        private List<string> GetUrlsWithInputString(Har har)
        {
            Entry[] entries = har.Log.Entries;
            int length = entries.Length;
            List<string> toReturn = new List<string>();

            for (int i = 0; i < length; ++i)
            {
                Entry entry = entries[i];

                if (entry.Request.Url.Contains('?'))
                {
                    toReturn.Add(entry.Request.Url);
                }
            }

            return toReturn;

        }

        // POST: api/Har
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody]Har jsonbody)
        {
            try
            {
                if (ValidateModel(out var postAsync))
                {
                    return postAsync;
                }

                int newId = Interlocked.Increment(ref _id);
                _cache.Set<Har>(newId, jsonbody);
                return StatusCode(201, new { Success = true, Id = newId, Status = 201, Model = jsonbody });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Status = 500, Message = ex.Message });
            }

        }

        private bool ValidateModel(out IActionResult postAsync)
        {
            List<string> errors = new List<string>();
            if (!ModelState.IsValid)
            {
                foreach (ModelStateEntry modelState in ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }

                {
                    postAsync = StatusCode(400, new { Success = false, Status = 400, Errors = errors });
                    return true;
                }
            }

            postAsync = null;
            return false;
        }

        // PUT: api/Har/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Har jsonbody)
        {
            try
            {
                if (ValidateModel(out var postAsync))
                {
                    return postAsync;
                }
                _cache.Set<Har>(id, jsonbody);
                return StatusCode(201, new { Success = true, Id = id, Status = 201, Model = jsonbody });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Status = 500, Message = ex.Message });
            }
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                Har value;
                if (_cache.TryGetValue(id, out value))
                {
                    _cache.Remove(id);
                    return StatusCode(200, new { Success = true, Id = id, Status = 200, Model = value });
                }
                return StatusCode(404, new { Success = false, Status = 404, Id = id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Status = 500, Message = ex.Message });
            }
        }
    }

    public class BlockedQuery
    {
        public Entry Longest { get; set; }
        public Entry Shortest { get; set; }
        public Entry SecondShortest { get; set; }
    }

    public class BodySizeQuery
    {
        public float AverageSize { get; set; }
        public int TotalSize { get; set; }
    }

    public class SlowestPageQuery
    {
        public double Size { get; set; }
        public string Url { get; set; }
    }
}
