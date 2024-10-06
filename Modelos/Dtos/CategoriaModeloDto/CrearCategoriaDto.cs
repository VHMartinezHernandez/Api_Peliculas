using System.ComponentModel.DataAnnotations;

namespace ApiPeliculas.Modelos.Dtos.CategoriaModeloDto
{
    public class CrearCategoriaDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(100, ErrorMessage = "El numero maximo de caracteres es 100!")]
        public string Nombre { get; set; }
    }
}
