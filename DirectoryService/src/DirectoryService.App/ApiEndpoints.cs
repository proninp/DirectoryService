namespace DirectoryService.App;

internal static class ApiEndpoints
{
    private const string ApiBase = "api";

    internal static class Locations
    {
        private const string Base = $"{ApiBase}/locations";

        public const string Create = Base;
        public const string Get = $"{Base}/{{id:guid}}";
        public const string GetAll = Base;
        public const string Update = $"{Base}/{{id:guid}}";
        public const string Delete = $"{Base}/{{id:guid}}";
    }

    internal static class Departments
    {
        private const string Base = $"{ApiBase}/departments";

        public const string Create = Base;
        public const string Get = $"{Base}/{{id:guid}}";
        public const string GetAll = Base;
        public const string Update = $"{Base}/{{id:guid}}";
        public const string Delete = $"{Base}/{{id:guid}}";
        public const string UpdateDepartmentLocation = $"{Base}/{{departmentId:guid}}/locations/{{locationId:guid}}";
    }

    internal static class Positions
    {
        private const string Base = $"{ApiBase}/pisitions";

        public const string Create = Base;
        public const string Get = $"{Base}/{{id:guid}}";
        public const string GetAll = Base;
        public const string Update = $"{Base}/{{id:guid}}";
        public const string Delete = $"{Base}/{{id:guid}}";
    }
}