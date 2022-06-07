namespace RESTworld.Common
{
    /// <summary>
    /// Contains constant values for RESTworld.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// This is the name of the embedded property which contains the collection in a GetList response.
        /// example: <code>myResource.Embedded[Constants.ListItems]</code>
        /// </summary>
        public const string ListItems = "items";

        /// <summary>
        /// This is the name of the link to Get a single resource.
        /// It should be present on the home resource, in the embedded resource of a GetList resource and on a single resource.
        /// </summary>
        public const string GetLinkName = "Get";

        /// <summary>
        /// This is the name of the link to a GET a list resource from the home resource.
        /// </summary>

        public const string GetListLinkName = "GetList";

        /// <summary>
        /// This is the name of the link to POST a new resource to the server.
        /// This should be present on the "New" resource.
        /// </summary>

        public const string PostLinkName = "Post";

        /// <summary>
        /// This is the name of the link to PUT (update) a resource.
        /// It should be present on any resource that represents one entity.
        /// </summary>

        public const string PutLinkName = "Put";

        /// <summary>
        /// This is the name of the link to a DELETE a single resource.
        /// It should be present in the embedded resource of a GetList resource and on a single resource.
        /// </summary>

        public const string DeleteLinkName = "Delete";

        /// <summary>
        /// This is the name of the link to a single new resource (template) from a list resource.
        /// </summary>

        public const string NewLinkName = "New";
    }
}
