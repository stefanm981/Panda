using System;
using System.Collections.Generic;
using PandaWebApp.Enums;

namespace PandaWebApp.Models
{
    public class Package
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public double Weight { get; set; }
        public string ShippingAddress { get; set; }
        public PackageStatus Status { get; set; }
        public DateTime EstimatedDeliveryDate { get; set; }

        public int RecipientId { get; set; }
        public User Recipient { get; set; }

        public int ReceiptId { get; set; }
        public Receipt Receipt { get; set; }
    }
}