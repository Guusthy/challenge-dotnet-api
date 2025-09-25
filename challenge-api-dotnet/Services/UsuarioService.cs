using challenge_api_dotnet.Data;
using challenge_api_dotnet.Dtos;
using challenge_api_dotnet.Hateoas;
using challenge_api_dotnet.Mappers;
using challenge_api_dotnet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace challenge_api_dotnet.Services;

public sealed class UsuarioService(ApplicationDbContext db) : IUsuarioService
{
    private readonly ApplicationDbContext _db = db;

    private static (int page, int size) Normalize(int page, int size)
    {
        page = page < 1 ? 1 : page;
        size = size is < 1 or > 100 ? 10 : size;
        return (page, size);
    }

    public async Task<PagedResult<UsuarioResponseDTO>> GetPagedAsync(int page, int size)
    {
        (page, size) = Normalize(page, size);

        var query = _db.Usuarios.AsNoTracking();
        var total = await query.LongCountAsync();

        var list = await query
            .OrderBy(u => u.IdUsuario)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var dtos = list.Select(UsuarioMapper.ToResponseDto).ToList();
        return new PagedResult<UsuarioResponseDTO>(dtos, page, size, total);
    }

    public async Task<UsuarioResponseDTO?> GetByIdAsync(int id)
    {
        var entity = await _db.Usuarios.FindAsync(id);
        return entity is null ? null : UsuarioMapper.ToResponseDto(entity);
    }

    public async Task<UsuarioResponseDTO?> GetByEmailAsync(string email)
    {
        var entity = await _db.Usuarios.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
        return entity is null ? null : UsuarioMapper.ToResponseDto(entity);
    }

    public async Task<UsuarioResponseDTO> CreateAsync(UsuarioCreateDTO dto)
    {
        var entity = UsuarioMapper.ToEntity(dto);
        _db.Usuarios.Add(entity);
        await _db.SaveChangesAsync();
        return UsuarioMapper.ToResponseDto(entity);
    }

    public async Task<UsuarioResponseDTO?> UpdateAsync(int id, UsuarioCreateDTO dto)
    {
        var entity = await _db.Usuarios.FindAsync(id);
        if (entity is null) return null;

        entity.Nome = dto.Nome;
        entity.Email = dto.Email;
        entity.Senha = dto.Senha;
        entity.Status = dto.Status;
        entity.PatioIdPatio = dto.PatioId;

        await _db.SaveChangesAsync();
        return UsuarioMapper.ToResponseDto(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.Usuarios.FindAsync(id);
        if (entity is null) return false;

        _db.Usuarios.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }
}