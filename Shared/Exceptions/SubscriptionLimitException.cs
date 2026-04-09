using System.Net;

namespace AppointmentSystem.Api.Shared.Exceptions
{
    public class SubscriptionLimitException : Exception
    {
        public HttpStatusCode StatusCode { get; } = HttpStatusCode.Forbidden;
        public string? UpgradePath { get; }

        public SubscriptionLimitException(string message, string? upgradePath = null) : base(message)
        {
            UpgradePath = upgradePath;
        }
    }
}
