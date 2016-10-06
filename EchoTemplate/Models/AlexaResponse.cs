using Newtonsoft.Json;
using System.Collections.Generic;

namespace EWCAlexa.Model
{


    [JsonObject]
    public class AlexaResponse
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("sessionAttributes")]
        public Attributes SessionAttributes { get; set; }

        [JsonProperty("response")]
        public Response Response { get; set; }

        public AlexaResponse()
        {
            Version = "1.0";
            SessionAttributes = new Attributes();
            Response = new Response();
        }

        public AlexaResponse(string outputSpeechText)
            : this()
        {
            Response.OutputSpeech.Text = outputSpeechText;
            Response.Card.Content = outputSpeechText;
        }

        public AlexaResponse(string outputSpeechText, bool isGoodbye)
            : this()
        {
            Response.OutputSpeech.Text = outputSpeechText;

            if (isGoodbye)
            {
                Response.ShouldEndSession = true;
                Response.Card = null;
            }
            else
            {
                Response.Card.Content = outputSpeechText;
            }
        }

        public AlexaResponse(string outputSpeechText, string cardContent)
            : this()
        {
            Response.OutputSpeech.Text = outputSpeechText;
            Response.Card.Content = cardContent;
        }
    }


    [JsonObject("response")]
    public class Response
    {
        [JsonProperty("outputSpeech")]
        public Outputspeech OutputSpeech { get; set; }

        [JsonProperty("card")]
        public Card Card { get; set; }

        [JsonProperty("reprompt")]
        public Reprompt Reprompt { get; set; }

        [JsonProperty("shouldEndSession")]
        public bool ShouldEndSession { get; set; }

        public Response()
        {
            OutputSpeech = new Outputspeech();
            Card = new Card();
            Reprompt = new Reprompt();
            ShouldEndSession = false;
        }
    }

    [JsonObject("outputSpeech")]
    public class Outputspeech
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("ssml")]
        public string Ssml { get; set; }

        public Outputspeech()
        {
            Type = "PlainText";
        }
    }

    [JsonObject("card")]
    public class Card
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("image")]
        public Image Image { get; set; }

        public Card()
        {
            Type = "Simple";
            Image = new Image();
        }
    }

    [JsonObject("image")]
    public class Image
    {
        [JsonProperty("smallImageUrl")]
        public string SmallImageUrl { get; set; }

        [JsonProperty("largeImageUrl")]
        public string LargeImageUrl { get; set; }
    }

    [JsonObject("reprompt")]
    public class Reprompt
    {
        [JsonProperty("outputSpeech")]
        public Outputspeech OutputSpeech { get; set; }

        public Reprompt()
        {
            OutputSpeech = new Outputspeech();
        }
    }
}