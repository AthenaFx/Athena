using System;
using System.Collections.Generic;
using System.Linq;
using Athena.Configuration;
using Athena.Web.Parsing;

namespace Athena.Web
{
    public class WebApplicationRequestErrorSettings : AppFunctionDefinition
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
                .Last("UseCorrectOutputParser", 
                    next => new UseCorrectOutputParser(next, mediaTypeFinders, outputParsers).Invoke)
                .Last("SetStaticOutputResult", 
                    next => new SetStaticOutputResult(next, 
                        x => new ExceptionResult(x.Get<Exception>("exception"))).Invoke)
                .Last("WriteOutput", next => new WriteOutput(next, new StaticStatusCodeFinder(500)).Invoke);
        }
    }
}