﻿using System.Threading.Tasks;
using Athena.Resources;
using Athena.Routing;

namespace Athena.Web.Sample.Home
{
    public class Valid
    {
        public Task<ValidGetResult> Get(ValidGetInput input)
        {
            return Task.FromResult(new ValidGetResult($"Hi {input.Slug}"));
        }

        public Task<EndpointValidationResult> ValidateGet(ValidGetInput input)
        {
            return Task.FromResult(new EndpointValidationResult());
        }
    }

    public class ValidGetInput
    {
        public string Slug { get; set; }
    }

    public class ValidGetResult
    {
        public ValidGetResult(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }
}