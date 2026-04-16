namespace AppointmentSystem.Api.Shared.Constants
{
    public static class SubscriptionConstants
    {
        public static readonly Guid FREE_PACKAGE_ID = new Guid("f0a3b2c1-d4e5-4f6a-8b9c-0d1e2f3a4b5c");
        public static readonly Guid PRO_PACKAGE_ID = new Guid("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d");
        public static readonly Guid ENTERPRISE_PACKAGE_ID = new Guid("b1c2d3e4-f5a6-4b7c-8d9e-0f1a2b3c4d5e");

        public const string FREE_PACKAGE_NAME = "Free";
        public const string PRO_PACKAGE_NAME = "Pro";
        public const string ENTERPRISE_PACKAGE_NAME = "Enterprise";
    }
}
