using AppointmentSystem.Api.Modules.FeedbackModule.DTOs;
using AppointmentSystem.Api.Modules.FeedbackModule.Enums;
using AppointmentSystem.Api.Modules.FeedbackModule.Models;
using AppointmentSystem.Api.Shared.Authentication;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Database;
using AppointmentSystem.Api.Shared.Exceptions;

namespace AppointmentSystem.Api.Modules.FeedbackModule.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public FeedbackService(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<FeedbackResponseDto> CreateAsync(CreateFeedbackRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
            {
                throw new BusinessException(ErrorMessages.FEEDBACK_TEXT_REQUIRED);
            }

            if (!Enum.IsDefined(typeof(FeedbackCategory), request.Category))
            {
                throw new BusinessException(ErrorMessages.FEEDBACK_INVALID_CATEGORY);
            }

            string? role = _currentUserService.Role;
            string? userIdClaim = _currentUserService.UserId;

            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(userIdClaim)
                || !Guid.TryParse(userIdClaim, out Guid accountId))
            {
                throw new BusinessException(ErrorMessages.UNAUTHORIZED_ACCESS);
            }

            Guid? businessId = null;
            Guid? userId = null;
            string submittedByRole;

            if (role == Roles.Business)
            {
                submittedByRole = Roles.Business;
                businessId = accountId;
            }
            else if (role == Roles.User)
            {
                submittedByRole = Roles.User;
                userId = accountId;
            }
            else
            {
                throw new BusinessException(ErrorMessages.UNAUTHORIZED_ACCESS);
            }

            Feedback entity = new Feedback
            {
                Category = request.Category,
                Text = request.Text.Trim(),
                SubmittedByRole = submittedByRole,
                BusinessId = businessId,
                UserId = userId
            };

            await _context.Feedbacks.AddAsync(entity);
            await _context.SaveChangesAsync();

            return new FeedbackResponseDto
            {
                FeedbackId = entity.Id,
                Category = entity.Category,
                SubmittedByRole = entity.SubmittedByRole,
                BusinessId = entity.BusinessId,
                UserId = entity.UserId,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}
