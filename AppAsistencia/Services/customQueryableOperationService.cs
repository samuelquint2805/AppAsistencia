using AppAsistencia.Core;
using AppAsistencia.Data.DBSET;
using AppAsistencia.Services.Implementations;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace AppAsistencia.Services
{
    public class customQueryableOperationService
    {
        private readonly DataContextAsistencia _context;
        private readonly IMapper _mapper;

        public customQueryableOperationService(DataContextAsistencia context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Response<TDTO>> CreateGenericAsync<TEntity, TDTO>(TDTO tDto) where TEntity : iID
        {
            try
            {
                TEntity entity = _mapper.Map<TEntity>(tDto);

                Guid id = Guid.NewGuid();

                entity.ID = id;

                await _context.AddAsync(entity);
                await _context.SaveChangesAsync();

                //entity.ID = id;
                return Response<TDTO>.Success(
                    tDto,
                    "Registro creado correctamente"
                );
            }
            catch (Exception)
            {
                return Response<TDTO>.Failure(
                    "Error al crear el registro"
                );
            }
        }
        public async Task<Response<TDTO>> UpdateGenericAsync<TEntity, TDTO>(Guid id, TDTO tDto) where TEntity : iID
        {
            try
            {
                TEntity entity = _mapper.Map<TEntity>(tDto);
                // Actualizar propiedades
                entity.ID = id;

                _context.Entry(entity).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                TDTO resultDto = _mapper.Map<TDTO>(entity);

                return Response<TDTO>.Success(
                    resultDto,
                    "Registro actualizado correctamente"
                );
            }
            catch (Exception)
            {
                return Response<TDTO>.Failure(
                    "Error al actualizar el Registro"
                );
            }
        }

        public async Task<Response<object>> DeleteGenericAsync<TEntity>(Guid id) where TEntity : class, iID
        {
            try
            {
                TEntity? entity = await _context.Set<TEntity>()
                    .FirstOrDefaultAsync(e => e.ID == id);

                if (entity == null)
                {
                    return Response<object>.Failure(
                        "Error al eliminar el registro"
                    );
                }
                _context.Remove(entity);
                await _context.SaveChangesAsync();
                return Response<object>.Success(
                    "Registro eliminado correctamente"
                );
            }
            catch (Exception)
            {
                return Response<object>.Failure(
                    "Error al eliminar el registro"
                );

            }
        }
        public async Task<Response<TDTO>> getOneGenericAsync<TEntity, TDTO>(Guid id) where TEntity : class, iID
        {
            try
            {
                TEntity? entity = await _context.Set<TEntity>()
                   .FirstOrDefaultAsync(e => e.ID == id);

                if (entity == null)
                {
                    return Response<TDTO>.Failure(
                        "Error al obtener el registro o no existe"
                    );
                }

                TDTO resultDto = _mapper.Map<TDTO>(entity);
                return Response<TDTO>.Success(
                    resultDto,
                    "Registro obtenido correctamente"
                );
            }
            catch (Exception)
            {

                return Response<TDTO>.Failure(
                     "Error al obtener el registro"
                 );
            }
        }
        public async Task<Response<TDTO>> getListGenericAsync<TEntity, TDTO>(IQueryable<TEntity> query = null) where TEntity : class, iID
        {
            try
            {
                if (query is null)
                {
                    query = _context.Set<TEntity>();
                }

                List<TEntity> entity = await query.ToListAsync();

                if (entity == null)
                {
                    return Response<TDTO>.Failure(
                        "Error al obtener el obtener lista o no existe"
                    );
                }

                TDTO resultDto = _mapper.Map<TDTO>(entity);
                return Response<TDTO>.Success(
                    resultDto,
                    "Registro obtenido correctamente"
                );
            }
            catch (Exception)
            {

                return Response<TDTO>.Success(
                     "Error al obtener el registro"
                 );
            }
        }
    }
}
