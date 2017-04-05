using System;

namespace Athena.CommandHandling
{
    public class CommandHandlerNotFoundException : Exception
    {
        public CommandHandlerNotFoundException(Type commandType)
            : base($"Can't find handler for command of type: {commandType.FullName}")
        {

        }
    }
}