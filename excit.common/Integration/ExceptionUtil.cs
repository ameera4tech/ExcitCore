using System;
using System.Net;
using System.Net.Http;

namespace excit.common.Integration
{
    public static class ExceptionUtil
    {
        //public static void ThrowOrderCreateException(string message)
        //{
        //    HttpResponseMessage httpMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        //    httpMessage.Content = new StringContent(message);
        //    var exception = new HttpResponseException(httpMessage);
        //    throw exception;
        //}

        public static void ThrowParameterException(string message)
        {
            HttpResponseMessage httpMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            httpMessage.Content = new StringContent(message);
            var exception = new Exception(httpMessage.Content.ToString());
            throw exception;
        }
        public static void ThrowImplementationException(string message)
        {
            HttpResponseMessage httpMessage = new HttpResponseMessage(HttpStatusCode.NotImplemented);
            httpMessage.Content = new StringContent(message);
            var exception = new Exception(httpMessage.Content.ToString());
            throw exception;
        }
        public static void ThrowGoneException(string message)
        {
            HttpResponseMessage httpMessage = new HttpResponseMessage(HttpStatusCode.Gone);
            httpMessage.Content = new StringContent(message);
            var exception = new Exception(httpMessage.Content.ToString());
            throw exception;
        }
        //public static void ThrowUnAuthorizedException(string message)
        //{
        //    HttpResponseMessage httpMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized);
        //    httpMessage.Content = new StringContent(message);
        //    var exception = new HttpResponseException(httpMessage);
        //    throw exception;
        //}

        //public static void ThrowAuthenticationException()
        //{
        //HttpResponseMessage httpMessage = new HttpResponseMessage(HttpStatusCode.Forbidden);
        //httpMessage.Content = new StringContent("Invalid Username and Password");
        //var exception = new HttpResponseException(httpMessage);
        //    throw exception;
        //}
    }
}
