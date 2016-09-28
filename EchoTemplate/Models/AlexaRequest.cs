using System;
using Newtonsoft.Json;

namespace EWCAlexa.Model
{
    public class AlexaRequest
    {
        public string Version { get; set; }
        public Session Session { get; set; }
        public Request Request { get; set; }

        public AlexaRequest()
        {
            Version = "1.0";
            Session = new Session();
            Request = new Request();
        }
    }

    public class Session
    {
        public bool New { get; set; }
        public string SessionId { get; set; }
        public Application Application { get; set; }
        public Attributes Attributes { get; set; }
        public User User { get; set; }

        public Session()
        {
            Application = new Application();
            Attributes = new Attributes();
            User = new User();
        }
    }

    public class Application
    {
        public string ApplicationId { get; set; }
    }

    public class Attributes
    {
        public SkillAttributes SkillAttributes { get; set; }

        public Attributes()
        {
            SkillAttributes = new SkillAttributes();
        }
    }

    public class SkillAttributes
    {

        public string LastRequestIntent { get; set; }

        public Outputspeech OutputSpeech { get; set; }

        public SkillAttributes()
        {
            LastRequestIntent = "";
            OutputSpeech = new Outputspeech();
        }
    }
}

public class User
{
    public string UserId { get; set; }
}

public class Request
{

    public string Type { get; set; }
    public string RequestId { get; set; }
    public DateTime Timestamp { get; set; }
    public Intent Intent { get; set; }
    public string Locale { get; set; }

    public Request()
    {
        Intent = new Intent();
    }
}

public class Intent
{
    public string Name { get; set; }
    public dynamic Slots { get; set; }
}



