using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitaskWebAPI.Data;
using RabbitaskWebAPI.DTOs.Common;
using RabbitaskWebAPI.DTOs.Tag;

namespace RabbitaskWebAPI.Controllers
{
    public class TagController : BaseController
    {

        private readonly RabbitaskContext _context;
        private readonly Services.IUserAuthorizationService _authService;

        public TagController(
            RabbitaskContext context,
            Services.IUserAuthorizationService authService,
            ILogger<TarefaController> logger)
            : base(logger)
        {
            _context = context;
            _authService = authService;
        }

        #region PesquisarTag por nome
        /// <summary>
        /// 
        /// </summary>
        //[HttpGet("{Nome:string}")]
        //public async Task<ActionResult<ApiResponse<TagDto>>> PesquisarTag(string nmTag)
        //{
            

        //}
        #endregion
    }
}
