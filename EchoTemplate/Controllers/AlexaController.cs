using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using EWCAlexa.Model;
using EchoTemplate.Filters;

namespace EchoTemplate.Controllers
{
    [UnhandledExceptionFilter]
    [RoutePrefix("api/alexa")]
    public class AlexaController : ApiController
    {
        #region :   Fields   :
        private const double TimeStampTolerance = 150;
        private const string AppId = "<App_ID>";
        #endregion

        #region : Helpers :

        //use this within each method of controller to trap errors
        private async Task<AlexaResponse> ThrowSafeException(Exception exception, string methodName)
        {
            var response = new AlexaResponse();

            var content = @"We encountered some trouble, but don't worry, we have our team looking into it now.  We apologize for the
                            inconvenience, we should have this fixed shortly. Please try again later.";

            response.Response.OutputSpeech.Text = content;
            response.Response.ShouldEndSession = true;

            try
            {
                //perhaps log to loggly or email to developer/support with user request info
            }
            catch (Exception ex)
            {
                //TODO
            }

            return response;
        }

        #endregion : Helpers :
        public AlexaController()
        {
        }

        #region :   Main-End-Points   :
        [HttpPost, Route("main")]
        public async Task<AlexaResponse> Main(AlexaRequest alexaRequest)
        {
            AlexaResponse alexaResponse = new AlexaResponse();

            try
            {
                //check timestamp
                var totalSeconds = (DateTime.UtcNow - alexaRequest.Request.Timestamp).TotalSeconds;
                if (totalSeconds >= TimeStampTolerance)
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest));

                if (alexaRequest.Session.Application.ApplicationId != AppId)
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest));


                alexaResponse.SessionAttributes.SkillAttributes = alexaRequest.Session.Attributes.SkillAttributes;

                switch (alexaRequest.Request.Type)
                {
                    case "LaunchRequest":
                        alexaResponse = LaunchRequest(alexaRequest, alexaResponse);
                        break;
                    case "IntentRequest":
                        alexaResponse = await IntentRequest(alexaRequest, alexaResponse);
                        break;
                    case "SessionEndedRequest":
                        alexaResponse = SessionEndedRequest(alexaRequest, alexaResponse);
                        break;
                }

                //set value for repeat intent
                alexaResponse.SessionAttributes.SkillAttributes.OutputSpeech = alexaResponse.Response.OutputSpeech;
            }
            catch (Exception ex)
            {
                return await ThrowSafeException(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

            return alexaResponse;
        }

        #endregion :   Main-End-Points   :

        #region :   Alexa Type Handlers   :
        private AlexaResponse LaunchRequest(AlexaRequest request, AlexaResponse response)
        {
            var content = "";

            var reprompt = "";

            response.Response.OutputSpeech.Text = content;
            response.Response.ShouldEndSession = false;
            response.Response.Reprompt.OutputSpeech.Text = reprompt;

            return response;
        }

        private  AlexaResponse ProcessHelpIntent(AlexaRequest request, AlexaResponse response)
        {
            var content = "";

            var cardContent = content;

            response.Response.OutputSpeech.Text = content;
            response.Response.Reprompt.OutputSpeech.Text = "Have a great day!";

            return response;
        }

        private AlexaResponse ProcessCancelIntent(AlexaRequest request, AlexaResponse response)
        {
            response.Response.OutputSpeech.Text = "Goodbye gorgeous!. Go Strut your stuff!";
            response.Response.ShouldEndSession = true;
            return response;
        }

        private async Task<AlexaResponse> IntentRequest(AlexaRequest request, AlexaResponse response)
        {
            bool shouldSetLastIntent = true;

            switch (request.Request.Intent.Name)
            {
                case "AMAZON.RepeatIntent":
                    response = ProcessRepeatIntent(request, response);
                    shouldSetLastIntent = false;
                    break;
                case "ThanksIntent":
                    response = ProcessThanksIntent(request, response);
                    break;
                case "AMAZON.NoIntent":
                    response = ProcessNoIntent(request, response);
                    shouldSetLastIntent = false;
                    break;
                case "AMAZON.YesIntent":
                    response = ProcessYesIntent(request, response);
                    shouldSetLastIntent = false;
                    break;
                case "UnknownIntent":
                    response = ProcessUnknownIntent(request,response);
                    break;
                case "AMAZON.CancelIntent":
                    response = ProcessCancelIntent(request, response);
                    break;
                case "AMAZON.StopIntent":
                    response = ProcessCancelIntent(request, response);
                    break;
                case "AMAZON.HelpIntent":
                    response = ProcessHelpIntent(request, response);
                    break;
            }

            if (shouldSetLastIntent)
            {
                response.SessionAttributes.SkillAttributes.LastRequestIntent = request.Request.Intent.Name;
            }

            return response;
        }

        private AlexaResponse ProcessRepeatIntent(AlexaRequest request, AlexaResponse response)
        {

            response.Response.OutputSpeech = request.Session.Attributes.SkillAttributes.OutputSpeech;
            return response;
        }


        private AlexaResponse ProcessUnknownIntent(AlexaRequest request, AlexaResponse response)
        {
            if (string.IsNullOrEmpty(request.Session.Attributes.SkillAttributes.LastRequestIntent))
            {
                return ProcessHelpIntent(request, response);
            }
            else
            {
                return ProcessRepeatIntent(request, response);
            }
        }

        private AlexaResponse ProcessYesIntent(AlexaRequest request, AlexaResponse response)
        {

            response.Response.ShouldEndSession = true;
            response.Response.OutputSpeech.Text = "Thank you";
            response.SessionAttributes.SkillAttributes.OutputSpeech = response.Response.OutputSpeech;

            return response;
        }


        private AlexaResponse ProcessNoIntent(AlexaRequest request, AlexaResponse response)
        {
            string content = "";

            content = "OK, thanks for listening.";

            response.Response.ShouldEndSession = true;
            response.Response.OutputSpeech.Text = content;
            response.SessionAttributes.SkillAttributes.OutputSpeech = response.Response.OutputSpeech;
            response.Response.Reprompt = null;

            return response;
        }

        private AlexaResponse ProcessThanksIntent(AlexaRequest request, AlexaResponse response)
        {
            var content = "Have a great day!";

            response.Response.ShouldEndSession = true;
            response.Response.OutputSpeech.Text = content;
            response.SessionAttributes.SkillAttributes.OutputSpeech = response.Response.OutputSpeech;

            return response;
        }


        private  AlexaResponse SessionEndedRequest(AlexaRequest request, AlexaResponse response)
        {
            response.Response.OutputSpeech.Text = "Have a great day gorgeous!";
            response.Response.ShouldEndSession = true;

            return response;
        }

        #endregion :   Alexa Type Handlers   :

    }
}