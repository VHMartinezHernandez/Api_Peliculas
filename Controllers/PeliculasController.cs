using ApiPeliculas.Modelos;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ApiPeliculas.Repositorio.PeliculaRepositorio;
using ApiPeliculas.Modelos.Dtos.PeliculaModeloDto;
using Microsoft.AspNetCore.Authorization;

namespace ApiPeliculas.Controllers
{    
    //[Route("api/[controller]")]
    [Route("api/peliculas")]

    [ApiController]
    public class PeliculasController : ControllerBase
    {
        private readonly IPeliculaRepositorio _iPeliculaRepositorio;
        private readonly IMapper _imapper;

        public PeliculasController(IPeliculaRepositorio iPeliculaRepositorio, IMapper imapper)
        {
            _iPeliculaRepositorio = iPeliculaRepositorio;
            _imapper = imapper;
        }

        //[AllowAnonymous]
        //[HttpGet]
        //[ProducesResponseType(StatusCodes.Status403Forbidden)]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //public IActionResult GetPeliculas()
        //{
        //    var listaPeliculas = _iPeliculaRepositorio.Peliculas();
        //    var listaPeliculasDto = new List<PeliculaDto>();
        //    foreach (var item in listaPeliculas)
        //    {
        //        listaPeliculasDto.Add(_imapper.Map<PeliculaDto>(item));
        //    }
        //    return Ok(listaPeliculasDto);
        //}


        //Con paginacion
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetPeliculas([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 3)
        {
            try
            {
                var totalPeliculas = _iPeliculaRepositorio.GetTotalPeliculas();
                var peliculas = _iPeliculaRepositorio.Peliculas(pageNumber, pageSize);

                if (peliculas == null || !peliculas.Any())
                {
                    return NotFound("No se encontraron peliculas");
                }

                var peliculasDto = peliculas.Select(p => _imapper.Map<PeliculaDto>(p)).ToList();

                var response = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPelicula = (int)Math.Ceiling(totalPeliculas/ (double)pageSize),
                    TotalItems = totalPeliculas,
                    Items = peliculasDto
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error recuperando datos de la aplicacion");
            }
            
        }



        // Método para obtener una categoría por ID
        [AllowAnonymous]
        [HttpGet("{peliculaId:int}", Name = "GetPelicula")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetPelicula(int peliculaId)
        {
            var pelicula = _iPeliculaRepositorio.GetPelicula(peliculaId);
            if (pelicula == null)
            {
                return NotFound();
            }
            var peliculaDto = _imapper.Map<PeliculaDto>(pelicula);
            return Ok(peliculaDto);
        }


        // Método para crear una nueva categoría
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(PeliculaDto))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult CrearPelicula([FromForm] CrearPeliculaDto crearPeliculaDto)
        {
            if (!ModelState.IsValid)
            { return BadRequest(ModelState); }

            if (crearPeliculaDto == null)
            { return BadRequest(ModelState); }

            // Verificar si ya existe una categoría con el mismo nombre
            if (_iPeliculaRepositorio.ExistePelicula(crearPeliculaDto.Nombre))
            {
                ModelState.AddModelError("", "La pelicula ya existe.");
                return StatusCode(409, ModelState); // 409 Conflict
            }

            var pelicula = _imapper.Map<Pelicula>(crearPeliculaDto);

            //if (!_iPeliculaRepositorio.CrearPelicula(pelicula))
            //{
            //    ModelState.AddModelError("", $"Algo salió mal al guardar la pelicula {pelicula.Nombre}");
            //    return StatusCode(500, ModelState); // Error interno
            //}

            //Subida de Archivo
            if (crearPeliculaDto.Imagen != null)
            {
                string nombreArchivo = pelicula.Id + System.Guid.NewGuid().ToString() + Path.GetExtension(crearPeliculaDto.Imagen.FileName);
                string rutaArchivo = @"wwwroot\ImagenesPeliculas\" + nombreArchivo;

                var ubicacionDirectorio = Path.Combine(Directory.GetCurrentDirectory(), rutaArchivo);

                FileInfo file = new FileInfo(ubicacionDirectorio);

                if (file.Exists)
                {
                    file.Delete();
                }

                using (var fileStream = new FileStream(ubicacionDirectorio, FileMode.Create))
                {
                    crearPeliculaDto.Imagen.CopyTo(fileStream);
                }

                var baseUrl = $"{HttpContext.Request.Scheme}//{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";

                pelicula.RutaImagen = baseUrl + "/ImagenesPeliculas" + nombreArchivo;
                pelicula.RutaLocalImagen = rutaArchivo;
            }

            else
            {
                pelicula.RutaImagen = "https://placehold.co/600x400";
            }

            _iPeliculaRepositorio.CrearPelicula(pelicula);

            return CreatedAtRoute("GetPelicula", new { peliculaId = pelicula.Id }, pelicula);
        }


        // Método para actualización parcial de una categoría con PATCH
        [Authorize(Roles = "Admin")]
        [HttpPut("{peliculaId:int}", Name = "ActualizarPeliculaParcial")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ActualizarPeliculaPut(int peliculaId, [FromForm] ActualizarPeliculaDto actualizarPeliculaDto)
        {
            if (!ModelState.IsValid)
            { return BadRequest(ModelState); }

            if (actualizarPeliculaDto == null || peliculaId != actualizarPeliculaDto.Id)
            { return BadRequest(ModelState); }

            var peliculaExistente = _iPeliculaRepositorio.GetPelicula(peliculaId);

            if (peliculaExistente == null)
            {
                return NotFound($"No se encontro la pelicula con ID {peliculaId}");
            }

            var pelicula = _imapper.Map<Pelicula>(actualizarPeliculaDto);

            //if (!_iPeliculaRepositorio.ActualizarPelicula(pelicula))
            //{
            //    ModelState.AddModelError("", $"Algo salió mal al guardar la pelicula {pelicula.Nombre}");
            //    return StatusCode(500, ModelState); // Error interno
            //}

            //Subida de Archivo
            if (actualizarPeliculaDto.Imagen != null)
            {
                string nombreArchivo = pelicula.Id + System.Guid.NewGuid().ToString() + Path.GetExtension(actualizarPeliculaDto.Imagen.FileName);
                string rutaArchivo = @"wwwroot\ImagenesPeliculas\" + nombreArchivo;

                var ubicacionDirectorio = Path.Combine(Directory.GetCurrentDirectory(), rutaArchivo);

                FileInfo file = new FileInfo(ubicacionDirectorio);

                if (file.Exists)
                {
                    file.Delete();
                }

                using (var fileStream = new FileStream(ubicacionDirectorio, FileMode.Create))
                {
                    actualizarPeliculaDto.Imagen.CopyTo(fileStream);
                }

                var baseUrl = $"{HttpContext.Request.Scheme}//{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";

                pelicula.RutaImagen = baseUrl + "/ImagenesPeliculas" + nombreArchivo;
                pelicula.RutaLocalImagen = rutaArchivo;
            }

            else
            {
                pelicula.RutaImagen = "https://placehold.co/600x400";
            }

            _iPeliculaRepositorio.ActualizarPelicula(pelicula);

            return NoContent();
        }


        // Método para eliminar una categoría con DELETE
        [Authorize(Roles = "Admin")]
        [HttpDelete("{peliculaId:int}", Name = "BorrarPelicula")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult BorrarPelicula(int peliculaId)
        {
            if (peliculaId <= 0)
            {
                return BadRequest(ModelState); // Si el ID es inválido
            }

            if (!_iPeliculaRepositorio.ExistePelicula(peliculaId))
            {
                return NotFound(); // Si la categoría no existe
            }

            var pelicula = _iPeliculaRepositorio.GetPelicula(peliculaId);

            if (!_iPeliculaRepositorio.BorrarPelicula(pelicula))
            {
                ModelState.AddModelError("", $"Algo salió mal al intentar borrar la categoría {pelicula.Nombre}");
                return StatusCode(500, ModelState); // Error interno
            }

            return NoContent(); // Retorna 204 No Content si la eliminación fue exitosa
        }

        [AllowAnonymous]
        [HttpGet("GetPeliculasEnCategoria/{categoriaId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetPeliculasEnCategoria(int categoriaId)
        {
            try
            {
                var listaPeliculas = _iPeliculaRepositorio.GetPeliculasEnCategoria(categoriaId);
                if (listaPeliculas == null || !listaPeliculas.Any())
                {
                    return NotFound($"No se encoentraron peliculas en la categoria con ID {categoriaId} ");
                }

                //var itemPelicula = new List<PeliculaDto>();
                //foreach (var pelicula in listaPeliculas)
                //{
                //    itemPelicula.Add(_imapper.Map<PeliculaDto>(pelicula));
                //}

                var itemPelicula = listaPeliculas.Select(pelicula => _imapper.Map<PeliculaDto>(pelicula)).ToList();

                return Ok(itemPelicula);

            }
            catch (Exception ex)
            {
                return StatusCode(500, ModelState);
            }


            


        }

        [AllowAnonymous]
        [HttpGet("BuscarPorNombre")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult BuscarPorNombre(string nombre)
        {
            try
            {
                var peliculas = _iPeliculaRepositorio.BuscarPelicula(nombre);
                if(!peliculas.Any())
                {
                    return NotFound($"No se encoentraron peliculas con el nombre: {nombre} ");
                }
                var peliculasDto = _imapper.Map<IEnumerable<PeliculaDto>>(peliculas);
                return Ok(peliculasDto);

            }
            catch (Exception ex) 
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error de recuperacion");
            }            
        }


    }
}
