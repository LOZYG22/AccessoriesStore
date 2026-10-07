using AccessoriesStore.Application.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AccessoriesStore.Api.Filters
{
    public class ValidationFilter : IAsyncActionFilter
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidationFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument is null)
                    continue;

                var validatorType = typeof(IValidator<>)
                    .MakeGenericType(argument.GetType());

                var validator = _serviceProvider
                    .GetService(validatorType);

                if (validator is null)
                    continue;

                var validationContext = new ValidationContext<object>(argument);

                var validationResult = await ((IValidator)validator)
                    .ValidateAsync(validationContext);

                if (!validationResult.IsValid)
                {
                    var errors = string.Join(
                        " | ",
                        validationResult.Errors.Select(e => e.ErrorMessage));

                    context.Result = new BadRequestObjectResult(
                        ApiResponse<object>.Failure(errors)
                    );

                    return;
                }
            }

            await next();
        }
    }
}
