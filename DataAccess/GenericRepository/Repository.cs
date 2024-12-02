using DataAccess.Models;
using Microsoft.EntityFrameworkCore;


namespace DataAccess.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly PetFriendsContext context;
    private readonly DbSet<T> _entities;

    public Repository(PetFriendsContext context)
    {
        this.context = context;
        _entities = context.Set<T>();
    }

    public async Task<T?> Get(Guid id)
    {
        return await _entities.FindAsync(id);
    }

    public async Task<Guid> Insert(T entity)
    {
        _ = await _entities.AddAsync(entity);
        _ = await context.SaveChangesAsync();
#pragma warning disable CS8605 // Unboxing a possibly null value.
        return (Guid)entity.GetType().GetProperty("Id").GetValue(entity);
#pragma warning restore CS8605 // Unboxing a possibly null value.
    }

    public async Task<bool> Remove(T entity)
    {
        _ = _entities.Remove(entity);
        _ = await context.SaveChangesAsync();
#pragma warning disable CS8605 // Unboxing a possibly null value.
        return true;
#pragma warning restore CS8605 // Unboxing a possibly null value.
    }

    public async Task<bool> Update(T entity)
    {
        _ = _entities.Update(entity);
        _ = await context.SaveChangesAsync();
#pragma warning disable CS8605 // Unboxing a possibly null value.
        return true;
#pragma warning restore CS8605 // Unboxing a possibly null value.
    }
    public async Task AddRange(List<T> entities)
    {
        _entities.AddRange(entities);
        await context.SaveChangesAsync();
    }

    public async Task DeleteRange(List<T> entities)
    {
        _entities.RemoveRange(entities);
        await context.SaveChangesAsync();
    }

    public async Task UpdateRange(List<T> entities)
    {
        _entities.UpdateRange(entities);
        await context.SaveChangesAsync();
    }

}

