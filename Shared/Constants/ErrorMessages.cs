namespace AppointmentSystem.Api.Shared.Constants;

public class ErrorMessages
{
    public const string GENERIC_SYSTEM_ERROR = "Beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
    public const string EMAIL_ALREADY_EXISTS = "Email already exists";
    public const string INVALID_EMAIL_OR_PASSWORD = "Invalid email or password";
    
    // Business Module Errors
    public const string BUSINESS_CONTEXT_NOT_FOUND = "Business context not found. Make sure you are logged in.";
    public const string BUSINESS_NOT_FOUND = "Business not found.";
    public const string CAPACITY_LIMIT_EXCEEDED = "Capacity limit reached. Max allowed employees:";
    public const string BULK_CAPACITY_LIMIT_EXCEEDED = "Capacity limit reached. You can only add";
    public const string UNAUTHORIZED_ACCESS = "Unauthorized access";
    
    // Employee Module Errors
    public const string EMPLOYEE_NOT_FOUND = "Employee not found or access denied.";
    public const string USER_NOT_FOUND_FOR_EMPLOYEE = "User not found. They must create an account first.";
    public const string USER_ALREADY_EMPLOYEE = "This user is already an employee in this business.";
    
    // Service Module Errors
    public const string OFFERING_NOT_FOUND = "Service not found or belongs to another business.";
    
    // Appointment Module Errors
    public const string APPOINTMENT_SLOT_TAKEN = "This time slot is already booked. Please choose another time.";
    public const string APPOINTMENT_NOT_FOUND = "Appointment not found or unauthorized access.";

    // Advanced Authentication Errors
    public const string INVALID_REFRESH_TOKEN = "Invalid or expired refresh token.";
    public const string SECURITY_BREACH_DETECTED = "Security breach detected. Token reused. All sessions have been invalidated.";

    public const string FEEDBACK_TEXT_REQUIRED = "Feedback metni boş olamaz.";
    public const string FEEDBACK_INVALID_CATEGORY = "Geçersiz feedback kategorisi.";
    
    // KVKK & Privacy Errors
    public const string KVKK_NOT_ACCEPTED = "KVKK metni onaylanmalıdır.";
    public const string PRIVACY_POLICY_NOT_ACCEPTED = "Gizlilik politikası onaylanmalıdır.";
    
    // Role Errors
    public const string INVALID_ROLE_ASSIGNMENT = "Seçilen rol, hesap türünüz ile uyumlu değil. Lütfen kontrol ediniz.";
}