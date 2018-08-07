// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class ApiBehaviorOptionsSetup :
        ConfigureCompatibilityOptions<ApiBehaviorOptions>,
        IConfigureOptions<ApiBehaviorOptions>
    {
        private static readonly Func<ActionContext, IActionResult> DefaultFactory = DefaultInvalidModelStateResponse;
        private static readonly Func<ActionContext, IActionResult> ProblemDetailsFactory = ProblemDetailsInvalidModelStateResponse;

        public ApiBehaviorOptionsSetup(
            ILoggerFactory loggerFactory,
            IOptions<MvcCompatibilityOptions> compatibilityOptions)
            : base(loggerFactory, compatibilityOptions)
        {
        }

        protected override IReadOnlyDictionary<string, object> DefaultValues
        {
            get
            {
                var dictionary = new Dictionary<string, object>();

                if (Version >= CompatibilityVersion.Version_2_2)
                {
                    dictionary[nameof(ApiBehaviorOptions.AllowUseProblemDetailsForClientErrorResponses)] = true;
                }

                return dictionary;
            }
        }

        public void Configure(ApiBehaviorOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.InvalidModelStateResponseFactory = DefaultFactory;
        }

        public override void PostConfigure(string name, ApiBehaviorOptions options)
        {
            // Let compatibility switches do their thing.
            base.PostConfigure(name, options);

            if (options.AllowUseProblemDetailsForClientErrorResponses &&
                object.ReferenceEquals(options.InvalidModelStateResponseFactory, DefaultFactory))
            {
                options.InvalidModelStateResponseFactory = ProblemDetailsInvalidModelStateResponse;
            }
        }

        private static IActionResult DefaultInvalidModelStateResponse(ActionContext context)
        {
            var result = new BadRequestObjectResult(context.ModelState);

            result.ContentTypes.Add("application/json");
            result.ContentTypes.Add("application/xml");

            return result;
        }

        private static IActionResult ProblemDetailsInvalidModelStateResponse(ActionContext context)
        {
            var result = new BadRequestObjectResult(new ValidationProblemDetails(context.ModelState));

            result.ContentTypes.Add("application/problem+json");
            result.ContentTypes.Add("application/problem+xml");

            return result;
        }
    }
}
