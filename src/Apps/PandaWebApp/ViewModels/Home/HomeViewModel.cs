using System.Collections.Generic;

namespace PandaWebApp.ViewModels.Home
{
    public class HomeViewModel
    {
        public HomeViewModel()
        {
            this.PendingPackages = new List<PackageSmallViewModel>();
            this.ShippedPackages = new List<PackageSmallViewModel>();
            this.DeliveredPackages = new List<PackageSmallViewModel>();
        }

        public ICollection<PackageSmallViewModel> PendingPackages { get; set; }
        public ICollection<PackageSmallViewModel> ShippedPackages { get; set; }
        public ICollection<PackageSmallViewModel> DeliveredPackages { get; set; }
    }
}