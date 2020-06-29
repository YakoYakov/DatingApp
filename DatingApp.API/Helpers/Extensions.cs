using Microsoft.AspNetCore.Http;

namespace DatingApp.API.Helpers
{
    //Class that is used to extend whatever the app needed
    public static class Extensions
    {
        //HttpResponse extensions
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
        #endregion
    }
}