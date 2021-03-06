using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace WebApiSample.Infrastructure.Constraits
{
    class GenderConstraint : IRouteConstraint
    {
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            return new string[] { "male", "female" }.Contains(values["gender"]);
        }
    }
}