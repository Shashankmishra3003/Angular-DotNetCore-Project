using DatingApp.API.Data;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace DatingApp.API.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        // First parameter is for the operations to be performed when action being executed and 
        // second is for action is executed
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // this will give access http context for which the action is being executed,
            // we also await for the action to get completed
            var resultContext = await next();

            var userId = int.Parse(resultContext.HttpContext.User
                            .FindFirst(ClaimTypes.NameIdentifier).Value);

            // serveices are being provided through DI in startup
            var repo = resultContext.HttpContext.RequestServices.GetService<IDatingRepository>();
            // Getting the user
            var user = await repo.GetUser(userId, true);
            // Updating the LastActive date
            user.LastActive = DateTime.Now;
            await repo.SaveAll();
        }
    }
}
