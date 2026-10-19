using AnimeSpace.Application.Exceptions;
using AnimeSpace.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace AnimeSpace.Web.Filters
{
    /// <summary>
    /// Перетворює виключення шару Application на відповіді HTTP:
    /// NotFoundException → 404, ForbiddenException → 403. Для AJAX-запитів відповідь — JSON {"error": "..."},
    /// для звичайних — сторінка (404) або перенаправлення на сторінку «немає доступу» (403).
    /// ValidationException обробляється тут лише для AJAX (400); у звичайних формах її має перехопити сам контролер
    /// і додати помилку в ModelState.
    /// </summary>
    /// <param name="metadataProvider">Потрібен, щоб вручну створити ViewResult.</param>
    /// <param name="tempDataFactory">Потрібен, щоб вручну створити ViewResult.</param>
    public class GlobalExceptionFilter(IModelMetadataProvider metadataProvider, ITempDataDictionaryFactory tempDataFactory) : IExceptionFilter
    {
        ///<inheritdoc/>
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is NotFoundException)
            {
                if (context.HttpContext.Request.IsAjax())
                {
                    context.Result = new JsonResult(new { error = context.Exception.Message })
                    {
                        StatusCode = StatusCodes.Status404NotFound
                    };
                }
                else
                {
                    context.Result = new ViewResult
                    {
                        ViewName = "~/Views/Shared/NotFound.cshtml",
                        StatusCode = StatusCodes.Status404NotFound,
                        ViewData = new ViewDataDictionary(metadataProvider, context.ModelState),
                        TempData = tempDataFactory.GetTempData(context.HttpContext)
                    };
                }

                context.ExceptionHandled = true;
            }
            else if (context.Exception is ForbiddenException)
            {
                if (context.HttpContext.Request.IsAjax())
                {
                    context.Result = new JsonResult(new { error = context.Exception.Message })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }
                else
                {
                    context.Result = new ForbidResult();
                }

                context.ExceptionHandled = true;
            }
            else if (context.Exception is ValidationException)
            {
                if (context.HttpContext.Request.IsAjax())
                {
                    context.Result = new JsonResult(new { error = context.Exception.Message })
                    {
                        StatusCode = StatusCodes.Status400BadRequest
                    };

                    context.ExceptionHandled = true;
                }
            }
        }
    }
}
