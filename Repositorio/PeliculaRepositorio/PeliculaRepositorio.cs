using ApiPeliculas.Data;
using ApiPeliculas.Modelos;
using Microsoft.EntityFrameworkCore;

namespace ApiPeliculas.Repositorio.PeliculaRepositorio
{
    public class PeliculaRepositorio : IPeliculaRepositorio
    {
        private readonly ApplicationDbContext _bd;

        public PeliculaRepositorio(ApplicationDbContext bd)
        {
            _bd = bd;
        }

        public bool ActualizarPelicula(Pelicula pelicula)
        {
            pelicula.FechaCreacion = DateTime.Now;

            var peliculaExistente = _bd.Peliculas.Find(pelicula.Id);

            if(peliculaExistente != null)
            {
                _bd.Entry(peliculaExistente).CurrentValues.SetValues(pelicula);
            }
            else
            {
                _bd.Peliculas.Update(pelicula);
            }
            //_bd.Peliculas.Update(pelicula);
            return Guardar();
        }

        public bool BorrarPelicula(Pelicula pelicula)
        {
            _bd.Peliculas.Remove(pelicula);
            return Guardar();
        }

        //public ICollection<Pelicula> Peliculas()
        //{
        //    return _bd.Peliculas.OrderBy(c => c.Nombre).ToList();
        //}

        public ICollection<Pelicula> Peliculas(int pageNumber, int pageSize)
        {
            return _bd.Peliculas.OrderBy(c => c.Nombre)
                .Skip((pageNumber -1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public int GetTotalPeliculas()
        {
            return _bd.Peliculas.Count();
        }


        public bool CrearPelicula(Pelicula pelicula)
        {
            pelicula.FechaCreacion = DateTime.Now;
            _bd.Peliculas.Add(pelicula);
            return Guardar();
        }

        public bool ExistePelicula(int id)
        {
            return _bd.Peliculas.Any(c => c.Id == id);
        }

        public bool ExistePelicula(string nombre)
        {
            bool valor = _bd.Peliculas.Any(c => c.Nombre.ToLower().Trim() == nombre.ToLower().Trim());
            return valor;
        }

        public Pelicula GetPelicula(int peliculaId)
        {
            return _bd.Peliculas.FirstOrDefault(c => c.Id == peliculaId);
        }

        public bool Guardar()
        {
            return _bd.SaveChanges() >= 0 ? true : false;
        }

        public ICollection<Pelicula> GetPeliculasEnCategoria(int catId)
        {
            return _bd.Peliculas.Include(c => c.Categoria).Where(ca => ca.categoriaId == catId).ToList();
        }

        public IEnumerable<Pelicula> BuscarPelicula(string nombre)
        {
            IQueryable<Pelicula> query = _bd.Peliculas;
            if (!string.IsNullOrEmpty(nombre))
            {
                //query = query = query.Where(e => e.Nombre.Contains(nombre) || e.Descripcion.Contains(nombre));
                query = query = query.Where(e => e.Nombre.Contains(nombre));

            }
            return query.ToList();
        }

        
    }
}
