using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AlsoEnergyCodingChallengeWeb
{
    //A simple handler to return the time or specific errors when receiving an HTTP GET request
    public class TimeHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            //Check for optional paramater string for a 500 server error
            if (context.Request.QueryString["error"]=="true")
            {
                //Return a 500 - Internal Server Error as an HTTP Exception, by setting the status code and message
                throw new HttpException(500, "Internal Server Error");
            }
			
			//Check for optional paramater string for a timeout
			else if (context.Request.QueryString["timeout"]=="true")
            {
				//Sleep for 15 seconds to simulate a timeout
				System.Threading.Thread.Sleep(15000);
				context.Response.ContentType = "text/plain";
                context.Response.Write("Page is sleeping to simulate a timeout. The console application should never see this text.");
            }

            //If no 500 error or timeout is requested, return the current local time of the server
            else
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write(DateTime.Now.ToLongTimeString());
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}