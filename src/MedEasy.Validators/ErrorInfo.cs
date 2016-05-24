namespace MedEasy.Validators
{
    public class ErrorInfo
    {
        public string Key { get; }

        public string Description { get; }
        public ErrorLevel Severity { get; }

        public ErrorInfo(string key, string description, ErrorLevel severity)
        {
            Key = key;
            Description = description;
            Severity = severity;
        }
    }
}
