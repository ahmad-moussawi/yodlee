using System;

namespace Yodlee
{
    public class Token
    {
        public string Value { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsValid
        {
            get
            {
                return !string.IsNullOrEmpty(Value) && ExpiresAt.HasValue && ExpiresAt.Value > DateTime.UtcNow;
            }
        }
    }
}