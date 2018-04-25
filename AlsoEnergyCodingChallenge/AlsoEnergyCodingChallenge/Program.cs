using System;
using System.Collections.Generic; //Import library needed for Lists
using System.Threading; //Import library needed for spawning and using threads
using System.Net; //Import library needed for HTTP interactions
using System.IO; //Import library needed for stream responses from HTTP requests
using System.Data.SqlClient; //Import library needed for interacting with an SQL database

namespace AlsoEnergyCodingChallenge
{
    class Program
    {
        //Use main() as a testbed for AlsoEnergy Native Coding Challenge functions
        static void Main(string[] args)
        {
            //Create a sample List of integers to feed into other functions
            List<int> sampleList = new List<int>();
            for (int i = 1; i < 11; i++)
            {
                Console.WriteLine("Adding element: " + i + " to input test List.");
                sampleList.Add(i);
            }

            //Test function that outputs the sum of even elements in an input List
            int sumOfEvenElements = sumEvenElementsInList(sampleList);
            Console.WriteLine("\n\rSum of even numbers in list = " + sumOfEvenElements);

            //Test function that makes an HTTP GET request and prints the output to the console         
            Console.WriteLine("\n\rMaking GET request for server's local time...");
            printHttpGetRequest("http://localhost:4368/TimeHandler.ashx");
            //printHttpGetRequest("http://localhost.fiddler:4368/TimeHandler.ashx");

            //Test function that makes an HTTP GET request resulting in a 500 - Internal Server Error
            Console.WriteLine("\n\rMaking GET request for server's local time simulating 500 - Internal Server Error...");
            printHttpGetRequest("http://localhost:4368/TimeHandler.ashx?error=true");
            //printHttpGetRequest("http://localhost.fiddler:4368/TimeHandler.ashx?error=true");

            //Test function that makes an HTTP GET request resulting in a timeout
            Console.WriteLine("\n\rMaking GET request for server's local time simulating a page taking to long to respond...");
            printHttpGetRequest("http://localhost:4368/TimeHandler.ashx?timeout=true");
            //printHttpGetRequest("http://localhost.fiddler:4368/TimeHandler.ashx?timeout=true");

            /* Other sites that can be used for testing the GET request function and various HTTP error codes
             * The value after the backslash will return an HTTP response of that type.
            Example: Calling http://httpstat.us/404 will return 404 - Not Found */
            //printHttpGetRequest("http://httpstat.us/418");
            //printHttpGetRequest("http://httpstat.us/200?sleep=5000");

            //Test function that spawns threads that print out elements in an input List on a specified timer 
            Console.WriteLine("\n\rTesting List print function using 2 threads...");
            threadedListPrint(sampleList);

        }


        static int sumEvenElementsInList(List<int> inputList)
        {
            int listLength = inputList.Count; //Fetch the length of the input list
            int evenElementTotal = 0; //Initialize a variable to act as the sum of the elements that are even numbers
            
            //Iterate through each element of the list and check whether it is even or odd. Add even numbers to the running sum, and disregard odd numbers
            for (int i = 0; i < listLength; i++)
            {
                /* Determine whether an element is even or odd with modular division. A result of 0 is an even number (no remainder) 
                 * whereas a result of 1 is an odd number (remainder of 1) */
                if (inputList[i] % 2 == 0)
                {
                    //Add any even elements to the running sum 
                    evenElementTotal = evenElementTotal + inputList[i];
                }
            }

            return evenElementTotal;
        }

        static void printHttpGetRequest(string sURL)
        {
            //Grab the current time when the request is being made
            DateTime sqlStartTime = DateTime.UtcNow;

            //Initialize variables to be written as row entries into the SQL database
            int sqlHttpStatusCode = 0;
            string sqlDataString = "";
            int sqlStatus = 0;
            string sqlStatusString = "";

            //Attempt to make an HTTP GET request and print the result to the console:
            try
            {
                //Create web request instance
                HttpWebRequest wrGETURL;
                wrGETURL = HttpWebRequest.CreateHttp(sURL);
                wrGETURL.Timeout = 6000; //Adjustable timeout setting for the request (Currently 6 seconds)

                //Create a web response and cast it to HttpWebResponse to provide further functionality (specifically grabbing the status code)
                HttpWebResponse readResponse = (HttpWebResponse) wrGETURL.GetResponse();

                /* Grab the HTTP status code of the response (200 - OK, 404 - Not found, etc.)
                 * The StatusCode comes as an Enumeration member of the HTTPWebResponse and can be converted to a string for a descriptive message or
                 * cast it to an int to grab the numerical status code describing the server's response to the GET request. */
                sqlHttpStatusCode = (int)readResponse.StatusCode;
                sqlStatusString = readResponse.StatusCode.ToString();

                //Create stream instance from the response to the GET request
                Stream responseStream;
                responseStream = readResponse.GetResponseStream();

                //Read through the response stream and write the output to the Console and SQL DataString 
                StreamReader objReader = new StreamReader(responseStream);
                sqlDataString = objReader.ReadToEnd();
                Console.WriteLine(sqlDataString);

            }

            //If response is bad, capture and process the exception
            catch (WebException ex)
            {
                //Grab the response associated with the exception that was thrown for the GET request
                HttpWebResponse readResponse = (HttpWebResponse)ex.Response;

                /* Determine whether the exception is caused by the program waiting too long or from the server's response then
                 * write the response code to the console and SQL StatusString entry when an error is encountered */
                if (ex.Status == WebExceptionStatus.Timeout) 
                {
                    
                    sqlHttpStatusCode = 0;
                    sqlStatusString = "Timeout! Website took too long to respond to console application.";
                    Console.WriteLine(ex.Message);
                }
                else
                {   
                    sqlHttpStatusCode = (int)readResponse.StatusCode;
                    sqlStatusString = readResponse.StatusCode.ToString();
                    Console.WriteLine(ex.Message);
                }

            }
            
            //Finalize the SQL entry values
            DateTime sqlEndTime = DateTime.UtcNow; //Grab the current time when the request has been fully processed
            if (sqlHttpStatusCode == 0)
            {
                sqlStatus = -999; //If the console application waited too long, code the timeout as -999
            }
            else if (sqlHttpStatusCode == 200)
            {
                sqlStatus = 1;  //If the request was successful, code the status as 1
            }
            else
            {
                sqlStatus = 2;  //If the request returned some form of HTTP error or otherwise misbehaved, code the status as 2
            }

            //Insert a new row into the linked SQL data table using the finalized values
            insertSQLrow(sqlStartTime, sqlEndTime, sqlHttpStatusCode, sqlDataString, sqlStatus, sqlStatusString);

        }


        static void threadedListPrint(List<int> inputList)
        {
            //Initialize threads to print out List elements on a specified timer in milliseconds
            Thread printThread1Start = new Thread(() => timedListPrint(inputList, 500, 1));
            Thread printThread2Start = new Thread(() => timedListPrint(inputList, 1000, 2));
            printThread1Start.Start();
            printThread2Start.Start();

            //Block further program execution until all threads finish executing
            printThread1Start.Join();
            printThread2Start.Join();
        }

        public static void timedListPrint(List<int> inputList, int msPrintTime, int threadNumID)
        {
            int listLength = inputList.Count; //Fetch the length of the input list

            //Iterate through list on a timer and print out elements to console
            for (int  i = 0; i < listLength; i++)
            {
                Thread.Sleep(msPrintTime);
                Console.WriteLine("Thread" + threadNumID + ": " + inputList[i]); //Can also use thread.Name as an identifying string for each thread 
            }
        }

        static void insertSQLrow(DateTime StartTimeUTC, DateTime EndTimeUTC, int HttpStatusCode, string DataString, int Status, string StatusString)
        {
            try
            {
                /* Connect to the local SQL Server, specifying user credentials for SQL authentication (not Windows account-based "Integrated Security")
                 * The session only exists for the execution of the 'using' block, and will then close the connection and reclaim the memory used for the connection object */
                using (SqlConnection conn = new SqlConnection("Data Source=.\\SQLExpress;Integrated Security=false;User Id=AlsoEnergy;Password=boulder2018;"))
                {
                    conn.Open(); //Open the connection to the SQL server

                    //Format the SQL INSERT command targeting the server_response_log table and paramaterize the input string
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO server_response_log(StartTimeUTC,EndTimeUTC,HTTPStatusCode,DataString,Status,StatusString) VALUES (" + "@StartTimeUTC,@EndTimeUTC,@HTTPStatusCode,@DataString,@Status,@StatusString)", conn))
                    {
                        //Write the following values into a new row in the server_response_log table
                        cmd.Parameters.AddWithValue("@StartTimeUTC", StartTimeUTC.ToString());
                        cmd.Parameters.AddWithValue("@EndTimeUTC", EndTimeUTC.ToString());

                        //If there is no HTTP status code associated with the response, write a null value to the SQL entry
                        if (HttpStatusCode == 0)
                        {
                            cmd.Parameters.AddWithValue("@HTTPStatusCode", DBNull.Value);
                        }
                        //Otherwise enter in the associated HTTP status code from the response
                        else
                        {
                            cmd.Parameters.AddWithValue("@HTTPStatusCode", HttpStatusCode);
                        }

                        cmd.Parameters.AddWithValue("@DataString", DataString);
                        cmd.Parameters.AddWithValue("@Status", Status);
                        cmd.Parameters.AddWithValue("@StatusString", StatusString);

                        //Execute the INSERT command (returns the number of rows affected)
                        int rows = cmd.ExecuteNonQuery();
                    }

                }

                //Write to the console to notify that a new row has successfully been written to the SQL database
                Console.WriteLine("Logged entry into SQL database.");
            }
            catch (SqlException ex)
            {
                //Capture exception on a failed SQL entry and dump the error to the console
                Console.WriteLine("Failed to log entry into SQL database.");
                Console.WriteLine(ex.ToString());
            }

        }

    }
}

