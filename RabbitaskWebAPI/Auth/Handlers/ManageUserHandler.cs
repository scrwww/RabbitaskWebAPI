using Microsoft.AspNetCore.Authorization;
using RabbitaskWebAPI.Services;

public class ManageUserHandler : AuthorizationHandler<ManageUserRequirement>
{
    private readonly IUserAuthorizationService _authService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ManageUserHandler(
        IUserAuthorizationService authService,
        IHttpContextAccessor httpContextAccessor)
    {
        _authService = authService;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManageUserRequirement requirement)
    {
        try
        {
            var cdUsuarioAtual = _authService.ObterCdUsuarioAtual();

            // pega o alvo
            var httpContext = _httpContextAccessor.HttpContext;
            var routeData = httpContext?.GetRouteData();

            if (routeData?.Values.TryGetValue(requirement.UserIdRouteKey, out var objCdUsuario) == true)
            {
                if (int.TryParse(objCdUsuario?.ToString(), out int cdUsuarioAlvo))
                {
                    var podeGerenciar = await _authService.PodeGerenciarUsuarioAsync(cdUsuarioAtual, cdUsuarioAlvo);

                    if (podeGerenciar)
                    {
                        context.Succeed(requirement);
                    }
                }
            }
        }
        catch
        {
            // auth falhou
        }
    }
}