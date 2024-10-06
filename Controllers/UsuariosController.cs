using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos.CategoriaModeloDto;
using ApiPeliculas.Modelos.Dtos.UsuarioModeloDto;
using ApiPeliculas.Repositorio.CategoriaRepositorio;
using ApiPeliculas.Repositorio.UsuarioRepositorio;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiPeliculas.Controllers
{
    //[Route("api/[controller]")]
    [Route("api/usuarios")]

    [ApiController]
    //[ApiVersion("1.0")]

    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioRepositorio _iUsuarioRepositorio;
        private readonly IMapper _imapper;
        protected RespuestaAPI _respuestaApi;

        public UsuariosController(IUsuarioRepositorio iUsuarioRepositorio, IMapper imapper, RespuestaAPI respuestaAPI)
        {
            _iUsuarioRepositorio = iUsuarioRepositorio;
            _imapper = imapper;
            _respuestaApi = respuestaAPI;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetUsuarios()
        {
            var listaUsuarios = _iUsuarioRepositorio.GetUsuario();
            var listaUsuarioDto = new List<UsuarioDto>();
            foreach (var item in listaUsuarios)
            {
                listaUsuarioDto.Add(_imapper.Map<UsuarioDto>(item));
            }
            return Ok(listaUsuarioDto);
        }

        // Método para obtener una categoría por ID
        [Authorize(Roles = "Admin")]
        [HttpGet("{usuarioId}", Name = "GetUsuario")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetUsuario(string usuarioId)
        {
            var usuario = _iUsuarioRepositorio.GetUsuario(usuarioId);
            if (usuario == null)
            {
                return NotFound();
            }
            var usuarioDto = _imapper.Map<UsuarioDto>(usuario);
            return Ok(usuarioDto);
        }


        [AllowAnonymous]
        [HttpPost("registro")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Registro([FromBody] UsuarioRegistroDto usuarioRegistroDto)
        {
            bool validarNombreUsuarioUnico = _iUsuarioRepositorio.ValidarUsuarioUnico(usuarioRegistroDto.NombreUsuario);

            if (!validarNombreUsuarioUnico)
            {
                _respuestaApi.statusCode = System.Net.HttpStatusCode.BadRequest;
                _respuestaApi.IsSuccess = false;
                _respuestaApi.ErrorMessages.Add("El nombre de usuario ya existe");
                return BadRequest(_respuestaApi);
            }

            var usuario = await _iUsuarioRepositorio.Registro(usuarioRegistroDto);

            if (usuario == null)
            {
                _respuestaApi.statusCode = System.Net.HttpStatusCode.BadRequest;
                _respuestaApi.IsSuccess = false;
                _respuestaApi.ErrorMessages.Add("Error en el registro");
                return BadRequest(_respuestaApi);
            }

            _respuestaApi.statusCode = System.Net.HttpStatusCode.OK;
            _respuestaApi.IsSuccess = true;
            return Ok(_respuestaApi);
        }


        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] UsuarioLoginDto usuarioLoginDto)
        {
            var respuestaLogin = await _iUsuarioRepositorio.Login(usuarioLoginDto);


            if (respuestaLogin.Usuario == null || string.IsNullOrEmpty(respuestaLogin.Token))
            {
                _respuestaApi.statusCode = System.Net.HttpStatusCode.BadRequest;
                _respuestaApi.IsSuccess = false;
                _respuestaApi.ErrorMessages.Add("El nombre de usuario o password son incorrectos");
                return BadRequest(_respuestaApi);
            }

            _respuestaApi.statusCode = System.Net.HttpStatusCode.OK;
            _respuestaApi.IsSuccess = true;
            _respuestaApi.Result = respuestaLogin;
            return Ok(_respuestaApi);

        }
    }
}