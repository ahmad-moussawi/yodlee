using System;
using System.Collections.Generic;

namespace Yodlee
{
    public class Transaction : Entity
    {
        public string Id { get; set; }
        public string Container { get; set; }
        public string AccountId { get; set; }
        public AmountType Amount { get; set; }
        public AmountType RunningBalance { get; set; }
        public string BaseType { get; set; }
        public string CategoryType { get; set; }
        public int CategoryId { get; set; }
        public int HighLevelCategoryId { get; set; }
        public string Category { get; set; }
        public string CategorySource { get; set; }
        public DateTime Date { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime PostDate { get; set; }
        public TransactionDescription Description { get; set; }
        public bool IsManual { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string SubType { get; set; }
        public Merchant Merchant { get; set; }


    }

    public class Merchant
    {
        public string Id { get; set; }
        public string Source { get; set; }
        public string Name { get; set; }
        public List<string> CategoryLabel { get; set; }
        public Address Address { get; set; }
    }

    public class Address
    {
        public string Address1 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }
    }

    public class TransactionDescription
    {
        public string Original { get; set; }
        public string Consumer { get; set; }
        public string Simple { get; set; }
    }
}
