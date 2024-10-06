using ApiPeliculas.Modelos;

namespace ApiPeliculas.Repositorio.PeliculaRepositorio
{
    public interface IPeliculaRepositorio
    {
        //ICollection<Pelicula> Peliculas();

        ICollection<Pelicula> Peliculas(int pageNumber, int pageSize );

        int GetTotalPeliculas();
        ICollection<Pelicula> GetPeliculasEnCategoria(int catId);
        IEnumerable<Pelicula> BuscarPelicula(string nombre);
        Pelicula GetPelicula(int peliculaId);
        bool ExistePelicula(int id);
        bool ExistePelicula(string nombre);
        bool CrearPelicula(Pelicula pelicula);
        bool ActualizarPelicula(Pelicula pelicula);
        bool BorrarPelicula(Pelicula pelicula);
        bool Guardar();


    }
}
