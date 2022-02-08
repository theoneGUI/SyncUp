using Microsoft.AspNetCore.Mvc;

namespace SyncUpAPI.Controllers {
    [ApiController]
    [Route("/auth/userauthho")]
    public class UAuthHO : ControllerBase {
        [HttpGet]
        public string Get(string hash)
        {
            if (hash == null)
            {
                return "no user key given to authenticate";
            }
            else if (UserAuthMethods.hashExists(hash))
            {
                return "user_authentic";
            }
            else
            {
                return "user_not_authentic";
            }
        }

    }
}
