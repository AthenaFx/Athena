using System.Collections.Generic;
using System.Linq;
using Athena.Configuration;
using Athena.Web.Parsing;

namespace Athena.Web
{
    public class WebApplicationRequestUnAuthorizedSettings : AppFunctionDefinition
    {
        protected override AppFunctionBuilder DefineDefaultApplication(AppFunctionBuilder builder)
        {
            var outputParsers = new List<ResultParser>
            {
                new ParseOutputAsJson(),
                new ParseOutputAsHtml()
            };
            
            var mediaTypeFinders = new List<FindMediaTypesForRequest>
            {
                new StaticMediaTypeFinder(outputParsers.SelectMany(x => x.MatchingMediaTypes).ToArray())
            };
            
            return builder
                .First("UseCorrectOutputParser", 
                    next => new UseCorrectOutputParser(next, mediaTypeFinders, outputParsers).Invoke,
                    () => outputParsers.GetDiagnosticsData())
                .ContinueWith("SetStaticOutputResult", 
                    next => new SetStaticOutputResult(next, x => new UnAuthorizedResult()).Invoke)
                .Last("WriteOutput", next => new WriteOutput(next, new StaticStatusCodeFinder(403)).Invoke);
        }
    }
}