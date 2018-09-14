namespace Yodlee
{
    public class User
    {
        public int Id { get; set; }
        public string LoginName { get; set; }
        public Session Session { get; set; }
        public Name Name { get; set; }
    }

    public class Name
    {
        public string First { get; set; }
        public string Last { get; set; }
        public string Fullname { get; set; }
    }

    public class Session
    {
        public string UserSession { get; set; }
    }
}

