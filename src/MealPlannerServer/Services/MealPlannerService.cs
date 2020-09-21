using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MealPlannerServer.Services
{
    public class MealPlannerService : MealPlanner.MealPlannerBase
    {
        private readonly ILogger<MealPlannerService> _logger;

        public MealPlannerService(ILogger<MealPlannerService> logger)
        {
            _logger = logger;
        }

        public override async Task<MealRegisterResponse> RegisterMeal(MealRegisterRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"Data received {request.MealName}  {request.MealDateTime}");

            return new MealRegisterResponse
            {
                Response = $"Registered sucessfully. Data {request.MealName}  {request.MealDateTime}"
            };
        }

        public override async Task GetRegisteredMeals(GetAllMealsRequest request, IServerStreamWriter<MealRegisterResponse> responseStream, ServerCallContext context)
        {
            var client = new HttpClient();

            var data = await client.GetAsync("https://jsonplaceholder.typicode.com/comments");

            if (data.IsSuccessStatusCode)
            {
                string responseBody = await data.Content.ReadAsStringAsync();

                var resp = JsonConvert.DeserializeObject<List<Comment>>(responseBody);
                
                _logger.LogInformation($"Received comments data from url Total {resp.Count}");

                foreach (var comment in resp)
                {
                    if (context.CancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    await Task.Delay(100);

                    await responseStream.WriteAsync(new MealRegisterResponse { Response = comment.ToString() });
                }

            }
        }
    }

    public class Comment
    {
        public int postId { get; set; }

        public int id { get; set; }

        public string name { get; set; }

        public string email { get; set; }

        public string body { get; set; }

        public override string ToString()
        {
           return $"PostId : {postId} Id : {id} Name : {name} Email : {email}";
        }
    }
}
