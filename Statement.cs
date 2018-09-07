using System;

namespace Yodlee
{
    public class Statement : Entity
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public DateTime StatementDate { get; set; }
        public DateTime BillingPeriodStart { get; set; }
        public DateTime BillingPeriodEnd { get; set; }
        public DateTime DueDate { get; set; }
        public AmountType LastPaymentAmount { get; set; }
        public DateTime LastPaymentDate { get; set; }
        public bool IsLatest { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
