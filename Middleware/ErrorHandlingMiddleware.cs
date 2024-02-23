using MongoDB.Bson.IO;
using ReadVideo.Services.YoutubeManagement;
using System.Net;
using Newtonsoft.Json;

namespace ReadVideo.Server.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            //catch (VideoDurationExceededException ex)
            //{
            //    context.Response.ContentType = "application/json";
            //    context.Response.StatusCode = (int)HttpStatusCode.BadRequest; // 400
            //    await context.Response.WriteAsync(new ErrorDetails
            //    {
            //        StatusCode = context.Response.StatusCode,
            //        Message = ex.Message
            //    }.ToString());
            //}
            catch(Exception ex) 
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest; // 400
                await context.Response.WriteAsync(new ErrorDetails
                {
                    StatusCode = context.Response.StatusCode,
                    Message = ex.Message
                }.ToString());
            }
        }
    }

    public class ErrorDetails
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }

}
