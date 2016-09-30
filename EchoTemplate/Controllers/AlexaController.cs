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

        public AlexaController()
        {
        }

        #region :   Main-End-Points   :
        [HttpPost, Route("main")]
        public async Task<AlexaResponse> Main(AlexaRequest alexaRequest)
        {
            AlexaResponse alexaResponse = null;

            //check timestamp
            var totalSeconds = (DateTime.UtcNow - alexaRequest.Request.Timestamp).TotalSeconds;
            if (totalSeconds >= TimeStampTolerance)
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest));

            if (alexaRequest.Session.Application.ApplicationId != AppId)
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest));

            switch (alexaRequest.Request.Type)
            {
                case "LaunchRequest":
                    alexaResponse = LaunchRequest(alexaRequest);
                    break;
                case "IntentRequest":
                    alexaResponse = await IntentRequest(alexaRequest);
                    break;
                case "SessionEndedRequest":
                    alexaResponse = SessionEndedRequest();
                    break;
            }

            return alexaResponse;
        }

        #endregion :   Main-End-Points   :

        #region :   Alexa Type Handlers   :
        private AlexaResponse LaunchRequest(AlexaRequest request)
        {
            var utterance = "";

            var reprompt = "";

            var response = new AlexaResponse();
            response.Response.OutputSpeech.Text = utterance;
            response.Response.ShouldEndSession = false;
            response.Response.Reprompt.OutputSpeech.Text = reprompt;
            response.Response.Card = null;

            return response;
        }

        private  AlexaResponse ProcessHelpIntent(AlexaRequest request)
        {
            var utterance = "";

            var cardContent = utterance;

            var response = new AlexaResponse();

            response.Response.OutputSpeech.Text = utterance;
            response.Response.Reprompt.OutputSpeech.Text = "Have a great day!";

            return response;
        }

        private AlexaResponse ProcessCancelIntent()
        {
            return new AlexaResponse("Goodbye and have a great day!", true);
        }

        private async Task<AlexaResponse> IntentRequest(AlexaRequest request)
        {
            var response = new AlexaResponse();


            switch (request.Request.Intent.Name)
            {
                case "AMAZON.RepeatIntent":
                    response = ProcessRepeatIntent(request);
                    break;
                case "ThanksIntent":
                    response = ProcessThanksIntent(request);
                    response.SessionAttributes.SkillAttributes.LastRequestIntent = "ThanksIntent";
                    break;
                case "AMAZON.NoIntent":
                    response = ProcessNoIntent(request);
                    response.SessionAttributes.SkillAttributes.LastRequestIntent = "NoIntent";
                    break;
                case "AMAZON.YesIntent":
                    response = ProcessYesIntent(request);
                    break;
                case "UnknownIntent":
                    response = ProcessUnknownIntent(request);
                    response.SessionAttributes.SkillAttributes.LastRequestIntent = "UnknownIntent";
                    break;
                case "AMAZON.CancelIntent":
                    response = ProcessCancelIntent();
                    response.SessionAttributes.SkillAttributes.LastRequestIntent = "CancelIntent";
                    break;
                case "AMAZON.StopIntent":
                    response = ProcessCancelIntent();
                    response.SessionAttributes.SkillAttributes.LastRequestIntent = "StopIntent";
                    break;
                case "AMAZON.HelpIntent":
                    response = ProcessHelpIntent(request);
                    response.SessionAttributes.SkillAttributes.LastRequestIntent = "HelpIntent";
                    break;
            }

            return response;
        }

        private  AlexaResponse ProcessRepeatIntent(AlexaRequest request)
        {
            var response = new AlexaResponse();

            response.SessionAttributes.SkillAttributes = request.Session.Attributes.SkillAttributes;

            response.Response.OutputSpeech = request.Session.Attributes.SkillAttributes.OutputSpeech = request.Session.Attributes.SkillAttributes.OutputSpeech;
            return response;
        }


        private AlexaResponse ProcessUnknownIntent(AlexaRequest request)
        {
            var response = new AlexaResponse
            {
                Response =
                {
                    ShouldEndSession = false,
                    OutputSpeech = { Text = "I'm sorry, I didn't get that. Can you please repeat that." }
                }
            };
            response.Response.Reprompt.OutputSpeech.Text = "Please repeat your last question.";
            response.Response.Card = null;

            return response;
        }

        private AlexaResponse ProcessYesIntent(AlexaRequest request)
        {
            var response = new AlexaResponse();
            response.SessionAttributes.SkillAttributes = request.Session.Attributes.SkillAttributes;
            string msg = string.Empty;

            response.Response.ShouldEndSession = true;
            response.Response.OutputSpeech.Text = "Thank you";
            response.SessionAttributes.SkillAttributes.OutputSpeech = response.Response.OutputSpeech;
            response.Response.Reprompt = null;

            return response;
        }


        private AlexaResponse ProcessNoIntent(AlexaRequest request)
        {
            var response = new AlexaResponse();
            string content = "";

            response.SessionAttributes.SkillAttributes = request.Session.Attributes.SkillAttributes;

            content = "OK, thanks for listening.";

            response.Response.ShouldEndSession = true;
            response.Response.OutputSpeech.Text = content;
            response.SessionAttributes.SkillAttributes.OutputSpeech = response.Response.OutputSpeech;
            response.Response.Reprompt = null;

            return response;
        }

        private AlexaResponse ProcessThanksIntent(AlexaRequest request)
        {
            var response = new AlexaResponse();
            var content = "Have a great day!";

            response.SessionAttributes.SkillAttributes = request.Session.Attributes.SkillAttributes;

            response.Response.ShouldEndSession = true;
            response.Response.OutputSpeech.Text = content;
            response.SessionAttributes.SkillAttributes.OutputSpeech = response.Response.OutputSpeech;

            return response;
        }


        private  AlexaResponse SessionEndedRequest()
        {
            return null;
        }

        #endregion :   Alexa Type Handlers   :

    }
}