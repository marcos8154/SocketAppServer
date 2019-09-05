namespace MobileAppServer.ServerObjects
{
    public class RequestParameter
    {
        public string Name { get; set; }
        public object Value { get; set; }

        internal RequestParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public RequestParameter()
        {

        }
    }
}
