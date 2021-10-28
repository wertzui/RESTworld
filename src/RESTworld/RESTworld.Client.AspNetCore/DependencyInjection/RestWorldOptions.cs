namespace RESTworld.Client.AspNetCore.DependencyInjection
{
    public class RestWorldOptions : RESTworld.AspNetCore.DependencyInjection.RestWorldOptions
    {
        public ClientSettings ClientSettings { get; set; }
    }
}
