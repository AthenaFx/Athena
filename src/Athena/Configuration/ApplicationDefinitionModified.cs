namespace Athena.Configuration
{
    public class ApplicationDefinitionModified : SetupEvent
    {
        public ApplicationDefinitionModified(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}