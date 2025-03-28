using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using static Common.Literals.StringLiterals;

namespace Infrastructure.Validators
{
    public class CustomValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                var response = new
                {
                    ResponseCode = ResponseCode_FailedInputValidation,
                    ResponseDescription = ResponseMessage_WrongInput,
                    data = errors
                };

                context.Result = new UnprocessableEntityObjectResult(response);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Do nothing
        }
    }
}
