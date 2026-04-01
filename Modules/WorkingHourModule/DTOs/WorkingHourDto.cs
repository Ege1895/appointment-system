namespace AppointmentSystem.Api.Modules.WorkingHourModule.DTOs
{
    public class WorkingHourDto
    {
        public DayOfWeek DayOfWeek { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        
        public TimeSpan? BreakStartTime { get; set; }
        public TimeSpan? BreakEndTime { get; set; }

        public bool IsDayOff { get; set; }
    }
}
