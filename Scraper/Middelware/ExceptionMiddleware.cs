using System;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Scraper.Middelware
{
    public static class ExceptionMiddlewareExtensions
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app, IWebHostEnvironment env, ILogger logger)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    var status = context.Features.Get<IStatusCodeReExecuteFeature>();

                    var error = context.Features.Get<IExceptionHandlerFeature>();
                    if (error != null)
                    {
                        var ex = error.Error;
                        string exceptionString = (new ExceptionModel
                        {
                            Message = ex.Message,
                            InnerException = ex?.InnerException?.Message,
                            StackTrace = ex?.StackTrace,
                            OccuredAt = DateTime.Now,
                            RouteOfException = status?.OriginalPath
                        }).ToString();

                        await context.Response.WriteAsync(exceptionString, Encoding.UTF8);
                    }
                });
            });
        }
    }

    public class ExceptionModel
    {
        public string Message { get; set; }
        public string InnerException { get; set; }
        public DateTime OccuredAt { get; set; }
        public string StackTrace { get; set; }
        public string RouteOfException { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}