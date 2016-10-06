using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using EWCAlexa.Model;
using EchoTemplate.Filters;
using System.Runtime.CompilerServices;
using EchoTemplate.Helpers;

namespace EchoTemplate.Controllers
{
    [UnhandledExceptionFilter]
    [RoutePrefix("api/alexa")]
    public class AlexaController : ApiController
    {
        #region :   Fields   :
        private const double TimeStampTolerance = 150;
        private const int DATE_MAX_IN_DAYS = 60; //example of timeframe you might to check for
        private const int CacheExpireMinutes = 5;

        //Using custom object to manage 
        RedisCacheManager.IRedisManager _cache;

        #endregion

        #region : Notes : 


        #endregion : Notes :

        #region : Redis :
        //Using cache for persiting important, yet non-permanent session related data
        //Exmaples of getting and setting. complex objects can be used as well
        //_cache.Set<string>("key", "value", CacheExpiry);
        //_cache.Get<string>("key");
        public DateTime CacheExpiry {
            get
            {
                return DateTime.Now.AddMinutes(CacheExpireMinutes);
            }
        }

        #endregion : Redis :

        #region : Helpers :

        //use this within each method of controller to trap errors
        private async Task<AlexaResponse> ThrowSafeException(Exception exception, AlexaResponse response, [CallerMemberName] string methodName = "")
        {
            var content = @"We encountered some trouble, but don't worry, we have our team looking into it now.  We apologize for the inconvenience, we should have this fixed shortly. Please try again later.";

            response.Response.OutputSpeech.Text = content;
            response.Response.ShouldEndSession = true;

            try
            {

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
            _cache = new RedisCacheManager.CacheManager(new RedisCacheManager.StackExchangeCacher(AppSettings.RedisCache));
        }

        #region :   Main-End-Points   :
        [HttpPost, Route("main")]
        public async Task<AlexaResponse> Main(AlexaRequest alexaRequest)
        {
            AlexaResponse response = new AlexaResponse();

            try
            {
                //check timestamp
                var totalSeconds = (DateTime.UtcNow - alexaRequest.Request.Timestamp).TotalSeconds;
                if (totalSeconds >= TimeStampTolerance)
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest));

                if (alexaRequest.Session.Application.ApplicationId != AppSettings.AmazonAppId)
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest));

                response.SessionAttributes.SkillAttributes = alexaRequest.Session.Attributes.SkillAttributes;

                switch (alexaRequest.Request.Type)
                {
                    case "LaunchRequest":
                        response = LaunchRequest(alexaRequest, response);
                        break;
                    case "IntentRequest":
                        response = await IntentRequest(alexaRequest, response);
                        break;
                    case "SessionEndedRequest":
                        response = SessionEndedRequest(alexaRequest, response);
                        break;
                }

                //set value for repeat intent
                response.SessionAttributes.SkillAttributes.OutputSpeech = response.Response.OutputSpeech;
            }
            catch (Exception ex)
            {
                return await ThrowSafeException(ex, response);
            }

            return response;
        }

        #endregion :   Main-End-Points   :

        #region :   Alexa Type Handlers   :

        //Example of Amazon.Time Intent
        private async Task<AlexaResponse> ProcessTimeIntent(AlexaRequest request, AlexaResponse response)
        {
            var content = "";
            var reprompt = "";
            bool isValid = false;

            try
            {
                if (request.Request.Intent != null && request.Request.Intent.Slots != null)
                {
                    var slot = request.Request.Intent.Slots;

                    string result = "";

                    if (slot["Time"] != null)
                    {
                        result = (String)slot["Time"].value;

                        isValid = true;

                        var fixedTime = Convert.ToDateTime(result).ToString("hh:mm");

                        content = "";
                        reprompt = content;
                    }
                }

                if (!isValid)
                {
                    content = "I didn't understand your last response.";
                    reprompt = @"";
                }

                response.Response.ShouldEndSession = false;
                response.Response.OutputSpeech.Text = content;
                response.Response.Reprompt.OutputSpeech.Text = reprompt;
            }
            catch (Exception ex)
            {

                if (ex.Message.Contains("String was not recognized as a valid DateTime"))
                {
                    return ProcessRepeatIntent(request, response);
                }
                return await ThrowSafeException(ex, response);
            }

            return response;
        }

        //Example of Amazon.Date Intent
        private async Task<AlexaResponse> ProcessDateIntent(AlexaRequest request, AlexaResponse response)
        {

            var content = "";
            string theDate = "";
            bool isValid = false;

            try
            {
                if (request.Request.Intent != null && request.Request.Intent.Slots != null)
                {
                    var slot = request.Request.Intent.Slots;

                    if (slot["Date"] != null)
                    {
                        theDate = (String)slot["Date"].value;

                        var formattedDate = Convert.ToDateTime(theDate);

                        if (formattedDate > DateTime.Today.AddDays(DATE_MAX_IN_DAYS))
                        {
                            content = "Sorry that date was over 60 days from now.  You can say things like: July 26th, or next Tuesday, or any day before " + DateTime.Today.AddDays(DATE_MAX_IN_DAYS).ToShortDateString() + ".";
                            response.Response.ShouldEndSession = false;
                            response.Response.Reprompt.OutputSpeech.Text = content;
                            response.Response.OutputSpeech.Text = content;
                            return response;
                        }
                        else
                        {
                            content = @"";
                            isValid = true;
                        }
                    }
                }

                if (!isValid)
                {
                    return ProcessRepeatIntent(request, response);
                }

                response.Response.Reprompt.OutputSpeech.Text = content;
                response.Response.ShouldEndSession = false;
                response.Response.OutputSpeech.Text = content;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("String was not recognized as a valid DateTime"))
                {
                    return ProcessRepeatIntent(request, response);
                }
                return await ThrowSafeException(ex, response);
            }

            return response;
        }

        private AlexaResponse LaunchRequest(AlexaRequest request, AlexaResponse response)
        {
            return ProcessHelpIntent(request, response);
        }

        private AlexaResponse ProcessHelpIntent(AlexaRequest request, AlexaResponse response)
        {

            response.Response.OutputSpeech.Text = @"";
            response.Response.ShouldEndSession = true;

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
                    response = await ProcessNoIntent(request, response);
                    shouldSetLastIntent = false;
                    break;
                case "AMAZON.YesIntent":
                    response = await ProcessYesIntent(request, response);
                    shouldSetLastIntent = false;
                    break;
                case "UnknownIntent":
                    response = ProcessUnknownIntent(request,response);
                    break;
                case "AMAZON.Date":
                    response = await ProcessDateIntent(request, response);
                    break;
                case "AMAZON.Time":
                    response = await ProcessTimeIntent(request, response);
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

        private async Task<AlexaResponse> ProcessYesIntent(AlexaRequest request, AlexaResponse response)
        {
            try
            {
                response.Response.ShouldEndSession = true;
                response.Response.OutputSpeech.Text = "Thank you";
                response.SessionAttributes.SkillAttributes.OutputSpeech = response.Response.OutputSpeech;
            }
            catch (Exception ex)
            {
                return await ThrowSafeException(ex, response);
            }
            return response;
        }


        private async Task<AlexaResponse> ProcessNoIntent(AlexaRequest request, AlexaResponse response)
        {
            string content = "";

            content = "OK, thanks for listening.";

            try
            {
                response.Response.ShouldEndSession = true;
                response.Response.OutputSpeech.Text = content;
                response.SessionAttributes.SkillAttributes.OutputSpeech = response.Response.OutputSpeech;
                response.Response.Reprompt = null;
            }
            catch (Exception ex)
            {
                return await ThrowSafeException(ex, response);
            }
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