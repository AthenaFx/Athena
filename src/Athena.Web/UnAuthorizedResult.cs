namespace Athena.Web
{
    public class UnAuthorizedResult
    {
        public UnAuthorizedResult()
        {
            Message = "Unauthorized";
        }

        public string Message { get; }

        public override string ToString()
        {
            return Message;
        }
    }
}