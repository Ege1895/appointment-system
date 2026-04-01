using AppointmentSystem.Api.Modules.BusinessModule.Models;
using AppointmentSystem.Api.Modules.FeedbackModule.Enums;
using AppointmentSystem.Api.Modules.UserModule.Models;
using AppointmentSystem.Api.Shared.Models;

namespace AppointmentSystem.Api.Modules.FeedbackModule.Models
{
    public class Feedback : BaseEntity
    {
        public FeedbackCategory Category { get; set; }

        public required string Text { get; set; }

        /// <summary>
        /// JWT role claim: <see cref="AppointmentSystem.Api.Shared.Constants.Roles.Business"/> veya <see cref="AppointmentSystem.Api.Shared.Constants.Roles.User"/>.
        /// </summary>
        public required string SubmittedByRole { get; set; }

        public Guid? BusinessId { get; set; }
        public Business? Business { get; set; }

        public Guid? UserId { get; set; }
        public User? User { get; set; }
    }
}
