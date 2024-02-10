using SharpCompress.Crypto;
using System.Diagnostics;

namespace Prod_DDM_API.Classes
{
    public class HttpResponseController
    {
        // Handle errors for API Routes
        public object HandleErrors(Func<object> method)
        {
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                var response = method(); // Execute the method

                stopwatch.Stop();

                return new { status = 200, data = response, exectime = stopwatch.ElapsedMilliseconds };
            }
            catch (Exception err)
            {
                return new { status = ParseErrorCode(err), data = new { Message = err.Message } }; // Return the error response to the client
            }
        }

        // Parse every Exception into a http error code
        public int ParseErrorCode(Exception exception)
        {
            // Standard-HTTP-Errorcode
            int errorCode = 500;

            // Here you can check different types of exceptions and set the corresponding HTTP error codes.
            if (exception is UnauthorizedAccessException)
            {
                errorCode = 401; // Unauthorized
            }
            else if (exception is ArgumentException)
            {
                errorCode = 400; // Bad Request
            }
            else if (exception is FileNotFoundException)
            {
                errorCode = 404; // Not Found
            }
            else if (exception is NotSupportedException)
            {
                errorCode = 405; // Method Not Allowed
            }
            else if (exception is NotImplementedException)
            {
                errorCode = 501; // Not Implemented
            }
            else if (exception is TimeoutException)
            {
                errorCode = 408; // Request Timeout
            }
            else if (exception is DataLengthException)
            {
                errorCode = 404;
            }
            else if (exception is IndexOutOfRangeException)
            {
                errorCode = 400; // Bad request
            }
            // Default errorcode
            else
            {
                errorCode = 500; // Internal Server Error
            }


            return errorCode;
        }

    }
}
