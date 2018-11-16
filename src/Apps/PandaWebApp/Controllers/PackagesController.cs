using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PandaWebApp.Enums;
using PandaWebApp.Models;
using PandaWebApp.ViewModels.Packages;
using SIS.HTTP.Responses;
using SIS.MvcFramework;

namespace PandaWebApp.Controllers
{
    public class PackagesController : BaseController
    {
        [HttpGet("/packages/create")]
        public IHttpResponse Create()
        {
            if (!this.User.IsLoggedIn)
            {
                return this.Redirect("/users/login");
            }

            if (this.User.Role != "Admin")
            {
                return this.Redirect("/home/index");
            }

            var model = new CreatePackageViewModel
            {
                Recipients = this.Db.Users.Select(u => new RecipientSmallViewModel
                {
                    Name = u.Username
                }).ToList()
            };

            return this.View("Packages/Create", model);
        }

        [HttpPost("/packages/create")]
        public IHttpResponse DoCreate(PackagesCreateInputModel inputModel)
        {
            if (!this.User.IsLoggedIn)
            {
                return this.Redirect("/users/login");
            }

            if (this.User.Role != "Admin")
            {
                return this.Redirect("/home/index");
            }

            var recipientUser = this.Db.Users.FirstOrDefault(u => u.Username == inputModel.Recipient);
            if (recipientUser == null)
            {
                return this.BadRequestError("Invalid recipient.");
            }

            var package = new Package
            {
                Description = inputModel.Description,
                Weight = double.Parse(inputModel.Weight),
                ShippingAddress = inputModel.ShippingAddress,
                RecipientId = recipientUser.Id,
                Status = PackageStatus.Pending,
                EstimatedDeliveryDate = DateTime.MinValue
            };

            this.Db.Packages.Add(package);
            try
            {
                this.Db.SaveChanges();
            }
            catch (Exception e)
            {
                return this.ServerError(e.Message);
            }

            return this.Redirect("/packages/pending");
        }

        [HttpGet("/packages/ship")]
        public IHttpResponse Ship(int id)
        {
            if (!this.User.IsLoggedIn)
            {
                return this.Redirect("/users/login");
            }

            if (this.User.Role != "Admin")
            {
                return this.Redirect("/home/index");
            }

            var package = this.Db.Packages.FirstOrDefault(p => p.Id == id);
            if (package == null)
            {
                return this.BadRequestError("Invalid package id.");
            }

            if (package.Status != PackageStatus.Pending)
            {
                return this.BadRequestError("Package status must be 'pending' to be shipped.");
            }

            Random rnd = new Random();
            package.Status = PackageStatus.Shipped;
            package.EstimatedDeliveryDate = DateTime.UtcNow.AddDays(rnd.Next(20, 41));

            try
            {
                this.Db.SaveChanges();
            }
            catch (Exception e)
            {
                return this.BadRequestError(e.Message);
            }

            return this.Redirect("/packages/shipped");
        }

        [HttpGet("/packages/deliver")]
        public IHttpResponse Deliver(int id)
        {
            if (!this.User.IsLoggedIn)
            {
                return this.Redirect("/users/login");
            }

            if (this.User.Role != "Admin")
            {
                return this.Redirect("/home/index");
            }

            var package = this.Db.Packages.FirstOrDefault(p => p.Id == id);
            if (package == null)
            {
                return this.BadRequestError("Invalid package id.");
            }

            if (package.Status != PackageStatus.Shipped)
            {
                return this.BadRequestError("Package status must be 'shipped' to be delivered.");
            }

            package.Status = PackageStatus.Delivered;

            try
            {
                this.Db.SaveChanges();
            }
            catch (Exception e)
            {
                return this.BadRequestError(e.Message);
            }

            return this.Redirect("/packages/delivered");
        }

        [HttpGet("/packages/acquire")]
        public IHttpResponse Acquire(int id)
        {
            var user = this.Db.Users.FirstOrDefault(u => u.Username == this.User.Username);
            if (user == null)
            {
                return this.Redirect("/users/login");
            }

            var package = this.Db.Packages.FirstOrDefault(p => p.Id == id);
            if (package == null)
            {
                return this.BadRequestError("Invalid package id.");
            }

            if (package.RecipientId != user.Id)
            {
                return this.BadRequestError("You cannot acquire package that is not yours.");
            }

            package.Status = PackageStatus.Acquired;

            var receipt = new Receipt
            {
                PackageId = package.Id,
                RecipientId = user.Id,
                Fee = (decimal)package.Weight * 2.67m,
                IssuedOn = DateTime.UtcNow.Date
            };

            this.Db.Receipts.Add(receipt);

            try
            {
                this.Db.SaveChanges();
            }
            catch (Exception e)
            {
                return this.BadRequestError(e.Message);
            }

            return this.Redirect("/receipts/index");
        }

        [HttpGet("/packages/details")]
        public IHttpResponse Details(int id)
        {
            var user = this.Db.Users.FirstOrDefault(u => u.Username == this.User.Username);
            if (user == null)
            {
                return this.Redirect("/users/login");
            }

            var package = this.Db.Packages
                .Include(p => p.Recipient)
                .FirstOrDefault(p => p.Id == id);
            if (package == null)
            {
                return this.BadRequestError("Invalid package id.");
            }

            if (this.User.Role == "User")
            {
                if (package.RecipientId != user.Id)
                {
                    return this.BadRequestError("You don't have permission to view this package.");
                }
            }

            var viewModel = new PackageDetailsViewModel
            {
                Id = id,
                Address = package.ShippingAddress,
                Status = package.Status,
                Weight = package.Weight,
                RecipientName = package.Recipient.Username,
                Description = package.Description
            };  
            switch (package.Status)
            {
                case PackageStatus.Pending:
                    viewModel.EstimatedDeliveryDate = "N/A";
                    break;
                case PackageStatus.Shipped:
                    viewModel.EstimatedDeliveryDate = package.EstimatedDeliveryDate.ToString(@"dd/MM/yyyy");
                    break;
                default:
                    viewModel.EstimatedDeliveryDate = "Delivered";
                    break;
            }

            return this.View("Packages/Details", viewModel);
        }

        [HttpGet("/packages/pending")]
        public IHttpResponse Pending()
        {
            var user = this.Db.Users.FirstOrDefault(u => u.Username == this.User.Username);
            if (user == null)
            {
                return this.Redirect("/users/login");
            }

            if (this.User.Role != "Admin")
            {
                return this.Redirect("/home/index");
            }

            var model = new PackagesMiddleViewModel
            {
                Packages = this.Db.Packages
                    .Include(p => p.Recipient)
                    .Where(p => p.Status == PackageStatus.Pending)
                    .Select(p => new PackageMiddleViewModel
                    {
                        Id = p.Id,
                        Description = p.Description,
                        Weight = $"{p.Weight:f2}",
                        ShippingAddress = p.ShippingAddress,
                        RecipientName = p.Recipient.Username
                    }).ToList()
            };

            return this.View("Packages/Pending", model);
        }

        [HttpGet("/packages/shipped")]
        public IHttpResponse Shipped()
        {
            var user = this.Db.Users.FirstOrDefault(u => u.Username == this.User.Username);
            if (user == null)
            {
                return this.Redirect("/users/login");
            }

            if (this.User.Role != "Admin")
            {
                return this.Redirect("/home/index");
            }

            var model = new PackagesMiddleViewModel
            {
                Packages = this.Db.Packages
                    .Include(p => p.Recipient)
                    .Where(p => p.Status == PackageStatus.Shipped)
                    .Select(p => new PackageMiddleViewModel
                    {
                        Id = p.Id,
                        Description = p.Description,
                        Weight = $"{p.Weight:f2}",
                        EstimatedDeliveryDateString = p.EstimatedDeliveryDate.ToString(@"dd/MM/yyyy"),
                        RecipientName = p.Recipient.Username
                    }).ToList()
            };

            return this.View("Packages/Shipped", model);
        }

        [HttpGet("/packages/delivered")]
        public IHttpResponse Delivered()
        {
            var user = this.Db.Users.FirstOrDefault(u => u.Username == this.User.Username);
            if (user == null)
            {
                return this.Redirect("/users/login");
            }

            if (this.User.Role != "Admin")
            {
                return this.Redirect("/home/index");
            }

            var model = new PackagesMiddleViewModel
            {
                Packages = this.Db.Packages
                    .Include(p => p.Recipient)
                    .Where(p => p.Status == PackageStatus.Delivered || p.Status == PackageStatus.Acquired)
                    .Select(p => new PackageMiddleViewModel
                    {
                        Id = p.Id,
                        Description = p.Description,
                        Weight = $"{p.Weight:f2}",
                        ShippingAddress = p.ShippingAddress,
                        RecipientName = p.Recipient.Username
                    }).ToList()
            };

            return this.View("Packages/Delivered", model);
        }
    }
}