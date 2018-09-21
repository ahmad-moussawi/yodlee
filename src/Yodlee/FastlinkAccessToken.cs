using System;

namespace Yodlee
{
    public class FastlinkAccessToken : Entity
    {
        public string AppId { get; set; }
        public string Value { get; set; }
        public string Url { get; set; }
    }
}