using System;

namespace PandaWebApp.ViewModels.Receipts
{
    public class ReceiptViewModel
    {
        public int Id { get; set; }
        public string Fee { get; set; }
        public string IssuedOnDateString { get; set; }
        public string RecipientName { get; set; }
    }
}