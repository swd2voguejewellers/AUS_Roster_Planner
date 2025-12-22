using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class BaseController : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var controller = context.RouteData.Values["controller"]?.ToString();

        if (controller == "Login")
            return;

        if (context.HttpContext.Session.GetString("UserName") == null)
        {
            context.Result = new RedirectToActionResult("Index", "Login", null);
        }
    }
}
