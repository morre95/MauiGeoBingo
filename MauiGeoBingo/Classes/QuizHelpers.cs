
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;

namespace MauiGeoBingo.Classes
{
    internal class QuizHelpers
    {
        public static async Task<Quiz> GetNewDB(ProgressBar progressBar)
        {
            Debug.WriteLine("Starting Download");
            string token = await _GetToken();
            Quiz quiz = new();
            quiz.ResponseCode = 0;

            int count = await _GetQuizCountAsync();

            while (quiz.ResponseCode != 4)
            {
                Quiz? results = await _GetNewResult(token);
                if (results != null)
                {
                    List<Result> escepedResults = [];
                    foreach (var result in results.Results)
                    {
                        result.Category = HttpUtility.HtmlDecode(result.Category);
                        result.CorrectAnswer = HttpUtility.HtmlDecode(result.CorrectAnswer);
                        result.Question = HttpUtility.HtmlDecode(result.Question);
                        result.IncorrectAnswers = result.IncorrectAnswers.Select(HttpUtility.HtmlDecode).ToList()!;

                        escepedResults.Add(result);
                    }

                    quiz.Results.AddRange(escepedResults);

                    //quiz.Results.AddRange(results.Results);
                    await progressBar.ProgressTo(quiz.Results.Count / (double)count, 4600, Easing.Linear);
                    quiz.ResponseCode = results.ResponseCode;
                }
                else
                {
                    Debug.WriteLine("No results");
                    //break;
                }
            }

            quiz.Results = quiz.Results.OrderBy(q => q.Category).ThenBy(q => q.Difficulty).ToList();

            quiz.ResponseCode = 0;

            return quiz;
        }


        private static async Task<int> _GetQuizCountAsync()
        {
            try
            {
                HttpRequest request = new("https://opentdb.com/api_count_global.php");
                var result = await request.GetAsync<ResultCount>();
                if (result != null) return result.Count;
                else throw new UnauthorizedAccessException("opentdb.com");
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error Occurred: " + e.Message);
                return default;
            }
        }

        private static async Task<Quiz?> _GetNewResult(string token)
        {
            string url = _GetUrl(token);
            try
            {
                HttpRequest request = new(url);
                return await request.GetAsync<Quiz>();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error Occurred: " + e.Message);
                return default;
            }
        }

        private static async Task<string> _GetToken()
        {
            try
            {
                HttpRequest request = new("https://opentdb.com/api_token.php?command=request");
                var result = await request.GetAsync<TokenObj>();
                if (result != null) return result.Token;
                else throw new FieldAccessException("opentdb.com");
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error Access Occurred: " + e.Message);
                return string.Empty;
            }
        }

        private static string _GetUrl(string token)
        {
            string url = "https://opentdb.com/api.php?amount=50";

            //url += "&category=any";
            //url += "&typte=any";
            //url += "&difficulty=any";
            url += "&token=" + token;
            return url;
        }
    }


    public class ResultCount
    {
        [JsonIgnore]
        public int Count { get => Overall.total_num_of_verified_questions; }

        [JsonPropertyName("overall")]
        public Overall Overall { get; set; }
    }

    public class Overall
    {
        public int total_num_of_questions { get; set; }
        public int total_num_of_pending_questions { get; set; }
        public int total_num_of_verified_questions { get; set; }
        public int total_num_of_rejected_questions { get; set; }
    }


    public class TokenObj
    {
        public int response_code { get; set; }
        public string response_message { get; set; }
        [JsonPropertyName("token")]
        public string Token { get; set; }
    }


}
