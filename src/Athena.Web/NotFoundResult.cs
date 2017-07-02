namespace Athena.Web
{
    public class NotFoundResult
    {
        public NotFoundResult()
        {
            Message = "Not found";
        }

        public string Message { get; }

        public override string ToString()
        {
            return Message;
        }
    }
}