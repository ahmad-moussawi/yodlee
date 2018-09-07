using System;
using System.Collections.Generic;

namespace Yodlee
{
    public class Account : Entity
    {
        public Account()
        {
            _Type = "account";
        }

        public int Id { get; set; }
        public string ProviderName { get; set; }
        public string FullAccountNumber { get; set; }
        public string DisplayedName { get; set; }
        public string AccountNumber { get; set; }
        public string AggregationSource { get; set; }
        public List<AccountHolder> Holder { get; set; } = new List<AccountHolder>();
        public bool IncludeInNetWorth { get; set; }
        public string AccountName { get; set; }
        public bool IsManual { get; set; }
        public AmountType AvailableBalance { get; set; }
        public AmountType Balance { get; set; }
        public AmountType CurrentBalance { get; set; }
        public string AccountType { get; set; }
        public string AccountStatus { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsAsset { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ProviderId { get; set; }
        public string ProviderAccountId { get; set; }
        public string Container { get; set; }

    }

    public class AccountHolder
    {
        public string Ownership { get; set; }
        public Name Name { get; set; }
    }

    public class Name
    {
        public string Fullname { get; set; }
    }
}
