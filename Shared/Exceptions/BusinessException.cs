using System.Net;

namespace AppointmentSystem.Api.Shared.Exceptions
{
    public class BusinessException : BaseException
    {
        public BusinessException(string message)
            : base(message, (int)HttpStatusCode.BadRequest)
        {
        }
    }
}