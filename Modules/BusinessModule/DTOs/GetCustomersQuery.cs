namespace AppointmentSystem.Api.Modules.BusinessModule.DTOs
{
    public class GetCustomersQuery
    {
        private const int DEFAULT_PAGE = 1;
        private const int DEFAULT_PAGE_SIZE = 20;
        private const int MAX_PAGE_SIZE = 100;

        public int Page { get; set; } = DEFAULT_PAGE;
        public int PageSize { get; set; } = DEFAULT_PAGE_SIZE;
        public string? Search { get; set; }
        public string? SortBy { get; set; } // totalAppointments, lastAppointmentDate, name
        public string? SortDirection { get; set; } // asc, desc

        public void Normalize()
        {
            if (Page < 1)
            {
                Page = DEFAULT_PAGE;
            }

            if (PageSize < 1)
            {
                PageSize = DEFAULT_PAGE_SIZE;
            }
            else if (PageSize > MAX_PAGE_SIZE)
            {
                PageSize = MAX_PAGE_SIZE;
            }
        }
    }
}
