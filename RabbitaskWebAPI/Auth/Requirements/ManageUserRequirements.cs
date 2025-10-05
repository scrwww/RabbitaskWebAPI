using Microsoft.AspNetCore.Authorization;

public class ManageUserRequirement : IAuthorizationRequirement
{
    public string UserIdRouteKey { get; }

    public ManageUserRequirement(string cdUsuarioRouteKey = "cdUsuario")
    {
        UserIdRouteKey = cdUsuarioRouteKey;
    }
}