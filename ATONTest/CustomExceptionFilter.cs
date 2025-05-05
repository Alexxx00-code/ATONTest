using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace ATONTest
{
    public class CustomExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is InvalidOperationException)
            {
                context.Result = new BadRequestObjectResult(new
                {
                    StatusCode = 400,
                    Message = context.Exception.Message,
                });
            }
            else
            {
                context.Result = new ObjectResult(new
                {
                    StatusCode = 500,
                    Message = "Server Error",
                });
            }
            context.ExceptionHandled = true;
        }
    }
}