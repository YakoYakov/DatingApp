using System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace DatingApp.API.Helpers
{
    //Class that is used to extend whatever the app needed
    public static class Extensions
    {
        // HttpResponse extensions
        #region 
        // Add custom Application-Error header and pass it custom error message
        public static void AddApplicationError(this HttpResponse response, string message)
        {
            //The custom header that we want to expose
            response.Headers.Add("Application-Error", message);

            //The header that specify witch headers are exposed
            response.Headers.Add("Access-Control-Expose-Headers", "Application-Error");
            
            //For now allow all origins using the wild card "*"
            response.Headers.Add("Access-Control-Allow-Origin", "*");
        }

        // Add pagination to the headers of the response
        public static void AddPagination(this HttpResponse response, int currentPage, int itemsPerPage, int totalItems, int totalPages)
        {
            PaginationHeader paginationHeader = new PaginationHeader(currentPage, itemsPerPage, totalItems, totalPages);
            response.Headers.Add("Pagination", JsonConvert.SerializeObject(paginationHeader));
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
        }
        #endregion

        // DateTime extensions
        #region 
        // Add CalculateAge method that returns person age basen on his birthday
        public static int CalculateAge(this DateTime theDateTime)
        {
            int age = DateTime.Today.Year - theDateTime.Year;
            
            // TODO validate that this is correct
            if (theDateTime.AddYears(age) > DateTime.Today)
                age--;

            return age;    
        }
        #endregion
    }
}