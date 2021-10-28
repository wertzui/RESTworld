namespace RESTworld.Client.AspNetCore.DependencyInjection
{
    public class ApiUrl
    {
        public string Name { get; set; }

        public int? Version { get; set; }

        private string _url;
        public string Url
        {
            get => _url;
            set => _url = value.EndsWith('/') ? value : value + '/';
        }
    }
}
