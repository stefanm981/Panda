using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.EntityFrameworkCore;
using PandaWebApp.ViewModels.Receipts;
using SIS.HTTP.Responses;
using SIS.MvcFramework;

namespace PandaWebApp.Controllers
{
    public class ReceiptsController : BaseController
    {
        [HttpGet("/receipts/index")]
        public IHttpResponse Index()
        {
            var user = this.Db.Users.FirstOrDefault(u => u.Username == this.User.Username);
            if (user == null)
            {
                return this.Redirect("/users/login");
            }

            var allReceiptsModel = new AllReceiptsViewModel
            {
                Receipts = this.Db.Receipts
                    .Include(r => r.Recipient)
                    .Where(r => r.RecipientId == user.Id)
                    .Select(r => new ReceiptViewModel
                    {
                        Id = r.Id,
                        Fee = $"{r.Fee:f2}",
                        IssuedOnDateString = r.IssuedOn.ToString(@"dd/MM/yyyy"),
                        RecipientName = r.Recipient.Username
                    }).ToList()
            };

            return this.View("Receipts/Index", allReceiptsModel);
        }

        [HttpGet("/receipts/details")]
        public IHttpResponse Details(int id)
        {
            var user = this.Db.Users.FirstOrDefault(u => u.Username == this.User.Username);
            if (user == null)
            {
                return this.Redirect("/users/login");
            }

            var receipt = this.Db.Receipts
                .Include(r => r.Package)
                .Include(r => r.Recipient)
                .FirstOrDefault(r => r.Id == id);
            if (receipt == null)
            {
                return this.BadRequestError("Invalid receipt id.");
            }

            if (this.User.Role == "User")
            {
                if (receipt.RecipientId != user.Id)
                {
                    return this.BadRequestError("You don't have permission to view this receipt.");
                }
            }

            var model = new ReceiptDetailsViewModel
            {
                Id = id,
                IssuedOnDateString = receipt.IssuedOn.ToString(@"dd/MM/yyyy"),
                DeliveryAddress = receipt.Package.ShippingAddress,
                Weight = $"{receipt.Package.Weight:f2}",
                Description = receipt.Package.Description,
                RecipientName = receipt.Recipient.Username,
                TotalPrice = $"{receipt.Fee:f2}"
            };

            return this.View("Receipts/Details", model);
        }
    }
}