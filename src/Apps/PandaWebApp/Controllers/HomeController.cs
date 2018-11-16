using System.Linq;
using PandaWebApp.Enums;
using PandaWebApp.ViewModels.Home;

namespace PandaWebApp.Controllers
{
    using SIS.HTTP.Responses;
    using SIS.MvcFramework;

    public class HomeController : BaseController
    {
        [HttpGet("/home/index")]
        public IHttpResponse Index()
        {
            var user = this.Db.Users.FirstOrDefault(u => u.Username == this.User.Username);
            if (user != null)
            {
                var model = new HomeViewModel
                {
                    PendingPackages = this.Db.Packages
                        .Where(p => p.RecipientId == user.Id && p.Status == PackageStatus.Pending)
                        .Select(p => new PackageSmallViewModel
                        {
                            Id = p.Id,
                            Description = p.Description
                        }).ToList(),

                    ShippedPackages = this.Db.Packages
                    .Where(p => p.RecipientId == user.Id && p.Status == PackageStatus.Shipped)
                    .Select(p => new PackageSmallViewModel
                    {
                        Id = p.Id,
                        Description = p.Description
                    }).ToList(),

                    DeliveredPackages = this.Db.Packages
                        .Where(p => p.RecipientId == user.Id && p.Status == PackageStatus.Delivered)
                        .Select(p => new PackageSmallViewModel
                        {
                            Id = p.Id,
                            Description = p.Description
                        }).ToList()
                };

                return this.View("Home/LoggedInIndex", model);
            }

            return this.View("Home/Index");
        }

        [HttpGet("/")]
        public IHttpResponse RootIndex()
        {
            return this.Redirect("/home/index");
        }
    }
}
