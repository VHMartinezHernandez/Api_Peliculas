using ApiPeliculas.Modelos;

namespace ApiPeliculas.Repositorio.CategoriaRepositorio
{
    public interface ICategoriaRepositorio
    {
        ICollection<Categoria> Categorias();
        Categoria GetCategoria(int CategoriaId);
        bool ExisteCategoria(int id);
        bool ExisteCategoria(string nombre);

        bool CrearCategoria(Categoria categoria);
        bool ActualizarCategoria(Categoria categoria);
        bool BorrarCategoria(Categoria categoria);
        bool Guardar();


    }

}
