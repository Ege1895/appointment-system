namespace AppointmentSystem.Api.Shared.Constants
{
    public static class ValidationConstants
    {
        public const string REQUIRED = "{PropertyName} is required";
        public const string MIN_LENGTH = "{PropertyName} must be at least {MinLength} characters";
        public const string MAX_LENGTH = "{PropertyName} must not exceed {MaxLength} characters";
        public const string INVALID_EMAIL = "Invalid email format";
        public const string EMAIL_CANNOT_END_WITH_SPECIAL_CHARACTER = "Email must not end with a special character";
        public const string MUST_BE_POSITIVE = "{PropertyName} must be greater than zero";
        public const string MUST_BE_NON_NEGATIVE = "{PropertyName} must be zero or greater";
        public const string MUST_BE_FUTURE = "{PropertyName} must be a future date";
        public const string INVALID_GUID = "{PropertyName} must be a valid identifier";
        public const string SUBDOMAIN_FORMAT = "Subdomain may only contain lowercase letters, numbers, and hyphens";
        public const string START_BEFORE_END = "Start time must be before end time";
        public const string BREAK_START_BEFORE_BREAK_END = "Break start time must be before break end time";

        public const string LATEST_KVKK_VERSION = "v1.0";
        public const string LATEST_PRIVACY_POLICY_VERSION = "v1.0";
    }
}
