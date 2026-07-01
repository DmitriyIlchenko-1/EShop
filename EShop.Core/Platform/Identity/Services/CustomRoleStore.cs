using EShop.Core.Data;
using EShop.Core.Platform.Identity.Domain;
using EShop.Infrastructure.Data;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Platform.Identity.Services;

public class CustomRoleStore : AsyncDbHandler<Role>, IRoleStore<Role>, IQueryableRoleStore<Role>
{
    private readonly Lazy<ApplicationDbContext> _db;
    public IdentityErrorDescriber ErrorDescriber { get; set; }

    public CustomRoleStore(Lazy<ApplicationDbContext> db, IdentityErrorDescriber errorDescriber)
    {
        _db = db;
        ErrorDescriber = errorDescriber;
    }

    public void Dispose()
    {
         
    }

    public async Task<IdentityResult> CreateAsync(Role role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Guard.NotNull(role);
        _db.Value.Add(role);
        await _db.Value.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(Role role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Guard.NotNull(role);

        _db.Value.TryUpdate(role);
        try
        {
            await _db.Value.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
        }

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(Role role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Guard.NotNull(role);
        if (role.IsSystemRole)
        {
            throw new InvalidOperationException(
                $"The role '{role.Name}' cannot be deleted because it is a system role.");
        }

        _db.Value.Remove(role);
        try
        {
            await _db.Value.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
        }

        return IdentityResult.Success;
    }

    public Task<string> GetRoleIdAsync(Role role, CancellationToken cancellationToken)
    {
        Guard.NotNull(role, nameof(role));
        return Task.FromResult(role.Id.ToString());
    }

    public Task<string> GetRoleNameAsync(Role role, CancellationToken cancellationToken)
    {
        Guard.NotNull(role, nameof(role));
        return Task.FromResult(role.Name);
    }

    public Task SetRoleNameAsync(Role role, string roleName, CancellationToken cancellationToken)
    {
        Guard.NotNull(role, nameof(role));
        role.Name = roleName;
        return Task.CompletedTask;
    }

    public Task<string> GetNormalizedRoleNameAsync(Role role, CancellationToken cancellationToken)
    {
        Guard.NotNull(role, nameof(role));
        return Task.FromResult(role.Name);
    }

    public Task SetNormalizedRoleNameAsync(Role role, string normalizedName, CancellationToken cancellationToken)
    {
        Guard.NotNull(role, nameof(role));
        role.Name = normalizedName;
        return Task.CompletedTask;
    }

    public Task<Role> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Guard.NotEmpty(roleId);
        return _db
            .Value.Roles.FindByIdAsync(roleId.ToInt32(), cancellationToken)
            .AsTask();
    }

    public async Task<Role> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Guard.NotEmpty(normalizedRoleName);
        return await _db
            .Value.Roles.FirstOrDefaultAsync(x => x.Name == normalizedRoleName, cancellationToken);
    }

    public IQueryable<Role> Roles => _db.Value.Roles;
}