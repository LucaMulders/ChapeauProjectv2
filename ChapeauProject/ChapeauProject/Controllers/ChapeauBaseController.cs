using ChapeauProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChapeauProject.Controllers
{
    // Base controller so we don't have to copy-paste the session helpers into every controller that needs to touch the active order.
    public abstract class ChapeauBaseController : Controller
    {
        private const string SessionKey = "ActiveWorkingOrder";

        protected Order GetActiveOrder()
        {
            return HttpContext.Session.GetObject<Order>(SessionKey) ?? new Order();
        }

        protected void SetActiveOrder(Order order)
        {
            HttpContext.Session.SetObject(SessionKey, order);
        }

        protected void ClearActiveOrder()
        {
            HttpContext.Session.Remove(SessionKey);
        }

        protected IActionResult RedirectToBasket(int tableNumber)
        {
            return Redirect(Url.Action("Details", "Tables", new { tableNumber }) + "#basket");
        }

        protected Staff GetLoggedInStaff()
        {
            var staff = HttpContext.Session.GetObject<Staff>("LoggedInStaff");
            if (staff != null) return staff;

            if (int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out int staffId))
                return new Staff { StaffID = staffId };

            return new Staff();
        }
    }
}
