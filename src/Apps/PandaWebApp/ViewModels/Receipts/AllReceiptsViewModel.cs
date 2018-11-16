using System.Collections.Generic;

namespace PandaWebApp.ViewModels.Receipts
{
    public class AllReceiptsViewModel
    {
        public AllReceiptsViewModel()
        {
            this.Receipts = new List<ReceiptViewModel>();
        }

        public ICollection<ReceiptViewModel> Receipts { get; set; }
    }
}