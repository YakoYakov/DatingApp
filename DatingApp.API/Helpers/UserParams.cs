namespace DatingApp.API.Helpers
{
    public class UserParams
    {
        private const int maxPageSize = 25;

        public int PageNumber { get; set; } = 1;
        private int pageSize = 10;
        public int MyProperty
        {
            get { return pageSize; }
            set { pageSize = (value > maxPageSize) ? maxPageSize : value; }
        }
        
    }
}