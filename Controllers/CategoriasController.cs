using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos.CategoriaModeloDto;
using ApiPeliculas.Repositorio.CategoriaRepositorio;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiPeliculas.Controllers
{
    //[Authorize(Roles ="Admin")]
    //[Route("api/[controller]")] //Opcion estatica
    //[ResponseCache(Duration = 20)] //Se usa para la Cache (Como se deben de almacenar en la cache las respuestas, cuantos segundos se guardan en la cache las respuestas)
    [Route("api/categorias")] //Opcion dinamica
    [ApiController]
    //[ApiVersion("1.0")]
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaRepositorio _iCategoriaRepositorio;
        private readonly IMapper _imapper;

        public CategoriasController(ICategoriaRepositorio iCategoriaRepositorio, IMapper imapper)
        {
            _iCategoriaRepositorio = iCategoriaRepositorio;
            _imapper = imapper;
        }

        //[AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetCategorias() 
        {
            var listaCategorias = _iCategoriaRepositorio.Categorias();
            var listaCategoriasDto = new List<CategoriaDto>();
            foreach (var item in listaCategorias)
            {
                listaCategoriasDto.Add(_imapper.Map<CategoriaDto>(item));
            }
            return Ok(listaCategoriasDto);
        }


        // Método para obtener una categoría por ID
        //[AllowAnonymous]
        [HttpGet("{categoriaId:int}", Name = "GetCategoria")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetCategoria(int categoriaId)
        {
           var categoria = _iCategoriaRepositorio.GetCategoria(categoriaId);
            if (categoria == null)
            {
                return NotFound();
            }
            var categoriaDto = _imapper.Map<CategoriaDto>(categoria);
            return Ok(categoriaDto); 
        }

        // Método para crear una nueva categoría
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public IActionResult CrearCategoria([FromBody] CrearCategoriaDto crearCategoriaDto)
        {
            if (! ModelState.IsValid)
            { return BadRequest(ModelState); }

            if (crearCategoriaDto == null)
            { return BadRequest(ModelState); }

            // Verificar si ya existe una categoría con el mismo nombre
            if (_iCategoriaRepositorio.ExisteCategoria(crearCategoriaDto.Nombre))
            {
                ModelState.AddModelError("", "La categoría ya existe.");
                return StatusCode(409, ModelState); // 409 Conflict
            }

            var categoria = _imapper.Map<Categoria>(crearCategoriaDto);

            if (!_iCategoriaRepositorio.CrearCategoria(categoria))
            {
                ModelState.AddModelError("", $"Algo salió mal al guardar la categoría {categoria.Nombre}");
                return StatusCode(500, ModelState); // Error interno
            }

            return CreatedAtRoute("GetCategoria", new { categoriaId = categoria.Id }, categoria);
        }

        // Método para actualización parcial de una categoría con PATCH
        [Authorize(Roles = "Admin")]
        [HttpPatch("{categoriaId:int}", Name = "ActualizarCategoriaParcial")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ActualizarCategoriaParcial(int categoriaId, [FromBody] CategoriaDto patchDoc)
        {
            if (!ModelState.IsValid)
            { return BadRequest(ModelState); }

            if (patchDoc == null || categoriaId != patchDoc.Id)
            { return BadRequest(ModelState); }            

            var categoria = _imapper.Map<Categoria>(patchDoc);

            if (!_iCategoriaRepositorio.ActualizarCategoria(categoria))
            {
                ModelState.AddModelError("", $"Algo salió mal al guardar la categoría {categoria.Nombre}");
                return StatusCode(500, ModelState); // Error interno
            }
            return NoContent();
            
        }


        // Método para actualización parcial de una categoría con PATCH
        [Authorize(Roles = "Admin")]
        [HttpPut("{categoriaId:int}", Name = "ActualizarCategoriaParcial")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ActualizarCategoriaPut(int categoriaId, [FromBody] CategoriaDto patchDoc)
        {
            if (!ModelState.IsValid)
            { return BadRequest(ModelState); }

            if (patchDoc == null || categoriaId != patchDoc.Id)
            { return BadRequest(ModelState); }            

            var categoria = _imapper.Map<Categoria>(patchDoc);

            if (!_iCategoriaRepositorio.ActualizarCategoria(categoria))
            {
                ModelState.AddModelError("", $"Algo salió mal al guardar la categoría {categoria.Nombre}");
                return StatusCode(500, ModelState); // Error interno
            }
            return NoContent();
        }


        // Método para eliminar una categoría con DELETE
        [Authorize(Roles = "Admin")]
        [HttpDelete("{categoriaId:int}", Name = "BorrarCategoria")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult BorrarCategoria(int categoriaId)
        {
            if (categoriaId <= 0)
            {
                return BadRequest(ModelState); // Si el ID es inválido
            }

            if (!_iCategoriaRepositorio.ExisteCategoria(categoriaId))
            {
                return NotFound(); // Si la categoría no existe
            }

            var categoria = _iCategoriaRepositorio.GetCategoria(categoriaId);

            if (!_iCategoriaRepositorio.BorrarCategoria(categoria))
            {
                ModelState.AddModelError("", $"Algo salió mal al intentar borrar la categoría {categoria.Nombre}");
                return StatusCode(500, ModelState); // Error interno
            }

            return NoContent(); // Retorna 204 No Content si la eliminación fue exitosa
        }



    }
}
