using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos.UsuarioModeloDto;

namespace ApiPeliculas.Repositorio.UsuarioRepositorio
{
    public interface IUsuarioRepositorio
    {
        ICollection<AppUsuario> GetUsuario();
        AppUsuario GetUsuario(string usuarioId);
        bool ValidarUsuarioUnico(string usuario);
        Task<UsuarioLoginRespuestaDto>Login(UsuarioLoginDto usuarioLoginDto);
        Task<UsuarioDatosDto> Registro(UsuarioRegistroDto usuarioRegistroDto);
    }
}
