using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ShiftPlanner.Interfaces;

public class LoginController : Controller
{
    private readonly ILoginRepository _repo;

    public LoginController(ILoginRepository repo)
    {
        _repo = repo;
    }

    public IActionResult Index()
    {
        return View(); // modal-only view
    }

    [HttpPost]
    public IActionResult Auth(string username, string password)
    {
        var user = _repo.Authenticate(username, password);

        if (user == null)
            return Json(new
            {
                success = false,
                message = "Invalid username or password"
            });

        if (user.UserType != "MGR")
            return Json(new
            {
                success = false,
                message = "You are not authorized to access this system"
            });

        HttpContext.Session.SetString("UserName", user.UserName);
        HttpContext.Session.SetString("UserLevel", user.UserType);

        return Json(new { success = true });
    }


    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }
}
