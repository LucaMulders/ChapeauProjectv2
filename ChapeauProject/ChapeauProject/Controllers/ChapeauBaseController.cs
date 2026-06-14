using ChapeauProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChapeauProject.Controllers
{
    // Base controller so we don't have to copy-paste the session helpers into every controller that needs to touch the active order.
    public abstract class ChapeauBaseController : Controller
    {
        private const string SessionKey       = "ActiveWorkingOrder";
        public  const string LoggedInStaffKey = "LoggedInStaff";

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
            var staff = HttpContext.Session.GetObject<Staff>(LoggedInStaffKey);
            if (staff != null) return staff;

            // Session expired but auth cookie is still valid, reconstruct from claims.
            if (int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out int staffId))
            {
                var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                var role = Enum.TryParse<StaffRole>(roleClaim, out var parsed) ? parsed : StaffRole.Waiter;
                return new Staff { StaffID = staffId, Role = role };
            }

            return new Staff();
        }
    }
}
