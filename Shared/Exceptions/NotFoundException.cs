using System.Net;

namespace AppointmentSystem.Api.Shared.Exceptions
{
    public class NotFoundException : BaseException
    {
        public NotFoundException(string message)
            : base(message, (int)HttpStatusCode.NotFound)
        {
        }
    }
}