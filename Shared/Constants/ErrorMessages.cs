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
    public const string WRONG_PASSWORD_CONFIRMATION = "Onay şifresi hatalı.";
    public const string ACCOUNT_DELETED_RECENTLY = "Hesap silme işlemi yakın zamanda gerçekleştirildi. Lütfen daha sonra tekrar deneyin.";
    
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

    // Branch Module Errors
    public const string BRANCH_NOT_FOUND = "Şube bulunamadı.";
    public const string MAIN_BRANCH_DELETE_ERROR = "Ana şube silinemez. Lütfen önce başka bir şubeyi ana şube olarak işaretleyin.";
    public const string BRANCH_LIMIT_EXCEEDED = "Şube oluşturma limitine ulaşıldı. Maksimum şube sayısı:";
    public const string EMPLOYEE_NOT_ASSIGNED_TO_OFFERING = "Seçilen personel bu hizmeti vermemektedir.";
    public const string CUSTOMER_BLOCKED_BY_BUSINESS = "Şu an için bu işletmeden randevu alamazsınız.";

    // Import Errors
    public const string IMPORT_FILE_REQUIRED = "Lütfen bir dosya seçiniz.";
    public const string IMPORT_INVALID_FORMAT = "Geçersiz dosya formatı. Sadece .csv ve .xlsx desteklenmektedir.";
    public const string IMPORT_FILE_TOO_LARGE = "Dosya boyutu çok büyük. Maksimum limit: 5MB";
    public const string IMPORT_TOO_MANY_ROWS = "Dosyada çok fazla satır var. Maksimum limit: 500 satır";
    public const string IMPORT_USER_NOT_FOUND = "Bu e-posta adresine sahip bir kullanıcı bulunamadı. Çalışan eklemeden önce kullanıcının sisteme kayıt olması gerekir.";
    public const string IMPORT_ALREADY_EXISTS = "Bu kullanıcı zaten bu işletmede çalışan olarak kayıtlı.";
    public const string IMPORT_COLUMN_MISSING = "Dosya yapısı hatalı. Gerekli sütunlar eksik: ";

    // Loyalty
    public const string LOYALTY_PROGRAM_DISABLED = "Bu işletme için sadakat programı aktif değil.";
    public const string LOYALTY_INSUFFICIENT_POINTS = "Yetersiz sadakat puanı.";
    public const string LOYALTY_MIN_POINTS_REQUIRED = "Minimum puan bozdurma sınırı:";
    public const string LOYALTY_MAX_POINTS_EXCEEDED = "Bir randevuda kullanılabilecek maksimum puan sınırı aşıldı.";
}