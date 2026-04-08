namespace AppointmentSystem.Api.Shared.Constants
{
    public static class BusinessRules
    {
        // Global business constraints
        public const int DEFAULT_EMPLOYEE_CAPACITY = 5;

        // Maximum string lengths for entity properties
        public const int MAX_COMPANY_NAME_LENGTH = 150;
        public const int MAX_SUBDOMAIN_LENGTH = 50;
        public const int MAX_NAME_LENGTH = 100;
        public const int MAX_EMAIL_LENGTH = 150;
        public const int MAX_PHONE_LENGTH = 20;

        public const int MAX_JOB_TITLE_LENGTH = 100;
        public const int MAX_JOB_DESCRIPTION_LENGTH = 500;

        public const int MAX_CITY_LENGTH = 100;
        public const int MAX_DISTRICT_LENGTH = 100;
        public const int MAX_NEIGHBORHOOD_LENGTH = 150;
        public const int MAX_DESCRIPTION_LENGTH = 2000;

        public const int SLOT_STEP_MINUTES = 15;

        public const int MAX_SERVICE_NAME_LENGTH = 100;
        public const int MAX_SERVICE_DESC_LENGTH = 500;

        public const int MAX_FEEDBACK_TEXT_LENGTH = 4000;

        // Token & session
        public const int REFRESH_TOKEN_EXPIRY_DAYS = 7;

        // Appointment lifecycle timeouts
        public const int ACTION_TIMEOUT_HOURS = 24;

        // Pagination limits
        public const int MAX_UPCOMING_LIMIT = 50;
        public const int DEFAULT_UPCOMING_LIMIT = 5;
        public const int MAX_POPULAR_TAKE = 30;
        public const int DEFAULT_POPULAR_TAKE = 10;
        public const int MAX_OFFERING_PAGE_SIZE = 100;
        public const int MAX_CATALOG_PAGE_SIZE = 50;
        public const int DEFAULT_PAGE_SIZE = 20;

        // Scheduling
        public const int DEFAULT_DURATION_MINUTES = 60;

        // Background worker
        public const int WORKER_INTERVAL_MINUTES = 5;
        public const int WORKER_LOCK_EXPIRY_MINUTES = 4;

        // Cache TTLs
        public const int CACHE_CATALOG_TTL_MINUTES = 10;
        public const int CACHE_AVAILABILITY_TTL_MINUTES = 5;
        public const int CACHE_CATEGORIES_TTL_HOURS = 1;

        // CORS
        public const string CORS_POLICY_NAME = "StrictCorsPolicy";

        // Password
        public const int MIN_PASSWORD_LENGTH = 8;
        public const int MAX_PASSWORD_LENGTH = 100;

        // Branch Module
        public const int MAX_BRANCHES_DEFAULT = 3;
        public const int MAX_BRANCH_NAME_LENGTH = 150;
        public const int MAX_BRANCH_ADDRESS_LENGTH = 300;

        // Appointment Notes
        public const int MAX_NOTE_CONTENT_LENGTH = 2000;
        public const int MAX_NOTES_PER_APPOINTMENT = 20;

        // Localization & Currency
        public const string DEFAULT_CURRENCY = "TRY";
        public const int MAX_CURRENCY_CODE_LENGTH = 3;
        // Business Account
        public const string BUSINESS_CLOSED_APPOINTMENT_NOTE = "İşletme hesabı kapatıldı.";
        public const string DELETED_EMAIL_SUFFIX = "@deleted.invalid";
        public const string DELETED_FIRST_NAME = "Deleted";
        public const string DELETED_LAST_NAME = "Account";
        public const int ACCOUNT_DELETE_RATE_LIMIT_HOURS = 24;
    }
}
