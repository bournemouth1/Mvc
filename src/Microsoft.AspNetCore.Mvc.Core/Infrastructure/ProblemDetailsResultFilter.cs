// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Mvc.Infrastructure
{
    public sealed class ProblemDetailsResultFilter : IAlwaysRunResultFilter, IOrderedFilter
    {
        private readonly IDictionary<int, Func<ActionContext, ProblemDetails>> _problemDetailsFactory;
        private readonly ILogger<ProblemDetailsResultFilter> _logger;

        /// <summary>
        /// Gets the filter order. Defaults to -1000.
        /// </summary>
        public int Order => -1000;

        public ProblemDetailsResultFilter(
            ApiBehaviorOptions apiBehaviorOptions,
            ILogger<ProblemDetailsResultFilter> logger)
        {
            _problemDetailsFactory = apiBehaviorOptions.ProblemDetailsFactory;
            _logger = logger;
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Result is StatusCodeResult statusCodeResult &&
                _problemDetailsFactory.TryGetValue(statusCodeResult.StatusCode, out var factory))
            {
                _logger.LogTrace("Converting StatusCodeResult with status code '{0}' to ProblemDetails.");
                var problemDetails = factory(context);
                var result = new ObjectResult(problemDetails)
                {
                    StatusCode = statusCodeResult.StatusCode,
                };
                context.Result = result;
            }
        }
    }
}
