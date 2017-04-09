namespace Athena.Web.Sample.Home
{
    public class Exists
    {
        public ExistsGetResult Get()
        {
            return new ExistsGetResult("Exists");
        }

        public bool GetExists()
        {
            return true;
        }
    }

    public class ExistsGetResult
    {
        public ExistsGetResult(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}