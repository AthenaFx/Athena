namespace Athena.Configuration
{
    public class ApplicationDefined : SetupEvent
    {
        public ApplicationDefined(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}