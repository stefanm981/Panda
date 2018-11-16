namespace PandaWebApp.ViewModels.Receipts
{
    public class ReceiptDetailsViewModel
    {
        public int Id { get; set; }
        public string IssuedOnDateString { get; set; }
        public string DeliveryAddress { get; set; }
        public string Weight { get; set; }
        public string Description { get; set; }
        public string RecipientName { get; set; }
        public string TotalPrice { get; set; }
    }
}