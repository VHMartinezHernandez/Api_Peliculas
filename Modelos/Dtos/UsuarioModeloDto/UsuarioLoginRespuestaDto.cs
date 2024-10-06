namespace ApiPeliculas.Modelos.Dtos.UsuarioModeloDto
{
    public class UsuarioLoginRespuestaDto
    {
        public UsuarioDatosDto Usuario { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }

    }
}
