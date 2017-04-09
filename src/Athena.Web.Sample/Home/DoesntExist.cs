namespace Athena.Web.Sample.Home
{
    public class DoesntExist
    {
        public DoesntExistGetResult Get()
        {
            return new DoesntExistGetResult();
        }

        public bool GetExists()
        {
            return false;
        }
    }

    public class DoesntExistGetResult
    {

    }
}