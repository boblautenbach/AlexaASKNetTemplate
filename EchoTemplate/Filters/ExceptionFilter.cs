using EWCAlexa.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;

namespace EchoTemplate.Filters
{

    public class UnhandledExceptionFilter : System.Web.Http.Filters.ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            HttpStatusCode status = HttpStatusCode.InternalServerError;

            var exType = context.Exception.GetType();

            //Can use the status to do something specific
            if (exType == typeof(UnauthorizedAccessException))
                status = HttpStatusCode.Unauthorized;
            else if (exType == typeof(ArgumentException))
                status = HttpStatusCode.NotFound;

            var response = new AlexaResponse();

            var content = "We encountered some trouble, but don't worry, we have our team looking into it now.  We apologize for the inconvenience, we should have this fixed shortly. Please try again later.";

            response.Response.ShouldEndSession = true;
            response.Response.OutputSpeech.Text = content;

            context.Response = context.Request.CreateResponse<AlexaResponse>(response);

            base.OnException(context);
        }
    }
}