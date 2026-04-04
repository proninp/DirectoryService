namespace DirectoryService.App;

public static class ApiEndpoints
{
    private const string ApiBase = "api";

    public static class Locations
    {
        private const string Base = $"{ApiBase}/locations";

        public const string Create = Base;
        public const string Get = $"{Base}/{{idOrSlug}}";
        public const string GetAll = Base;
        public const string Update = $"{Base}/{{id:guid}}";
        public const string Delete = $"{Base}/{{id:guid}}";
    }
}