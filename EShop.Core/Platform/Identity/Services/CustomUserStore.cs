using EShop.Core.Data;
using EShop.Core.Platform.Identity.Domain;
using EShop.Core.Platform.Identity.Extensions;
using EShop.Infrastructure.Caching;
using EShop.Infrastructure.Data;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Platform.Identity.Services;

public class UserStore : AsyncDbHandler<User>,
    IQueryableUserStore<User>,
    IUserEmailStore<User>,
    IUserRoleStore<User>,
    IUserPasswordStore<User>,
    IUserLoginStore<User>,
    IUserSecurityStampStore<User>
    
{
    private readonly ApplicationDbContext _db;
    private readonly IRequestCache _requestCache;
    protected IdentityErrorDescriber ErrorDescriber { get; set; }

    public UserStore(ApplicationDbContext db, IdentityErrorDescriber errorDescriber, IRequestCache requestCache)
    {
        _db = db;
        ErrorDescriber = errorDescriber;
        _requestCache = requestCache;
    }

    public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
    {
        Guard.NotNull(user);
        return Task.FromResult(user.Id.ToStringInvariant());
    }

    public Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken)
    {
        Guard.NotNull(user);
        var userName = user.Username;
        return Task.FromResult(userName);
    }

    public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
    {
        Guard.NotNull(user);
        user.Username = userName;
        return Task.CompletedTask;
    }

    public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
    {
        Guard.NotNull(user);
        return Task.FromResult(user.Username);
    }

    public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken)
    {
        Guard.NotNull(user);
        user.Username = normalizedName;
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Guard.NotNull(user);
        try
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            return Failed(e);
        }

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Guard.NotNull(user);
        try
        {
            
            _db.TryUpdate(user);
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Failed(ErrorDescriber.ConcurrencyFailure());
        }
        catch (Exception e)
        {
            return Failed(e);
        }

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Guard.NotNull(user);

        if (user.IsSystemAccount)
        {
            throw new InvalidOperationException($"System account with the name {user.SystemName} cannot be deleted");
        }

        try
        {
            _db.Remove(user);
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Failed(ErrorDescriber.ConcurrencyFailure());
        }

        return IdentityResult.Success;
    }

    public Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return _db
            .Users
            .IncludeRoles()
            .FindByIdAsync(userId.ToInt32(), cancellationToken)
            .AsTask();
    }

    public Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return _db
            .Users
            .IncludeRoles()
            .FirstOrDefaultAsync(x => x.Username == normalizedUserName, cancellationToken);
    }

    public IQueryable<User> Users => _db.Users.AsQueryable();

    public Task SetEmailAsync(User user, string email, CancellationToken cancellationToken)
    {
        Guard.NotNull(user);
        user.Email = email;
        return Task.CompletedTask;
    }

    public Task<string> GetEmailAsync(User user, CancellationToken cancellationToken)
    {
        Guard.NotNull(user);
        return Task.FromResult(user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(User user, CancellationToken cancellationToken)
    {
        Guard.NotNull(user);
        return Task.FromResult(user.IsActive);
    }

    public Task SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        user.IsActive = confirmed;
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public Task<User> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return _db
            .Users
            .IncludeRoles()
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);
    }

    public Task<string> GetNormalizedEmailAsync(User user, CancellationToken cancellationToken)
    {
        Guard.NotNull(user);
        return Task.FromResult(user.Email);
    }

    public Task SetNormalizedEmailAsync(User user, string normalizedEmail, CancellationToken cancellationToken)
    {
        Guard.NotNull(user);
        user.Email = normalizedEmail;
        return Task.CompletedTask;
    }

    public async Task AddToRoleAsync(User user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Guard.NotNull(user);
        Guard.NotEmpty(roleName);

        var role = await FindUserRoleAsync(roleName, true, cancellationToken);
        if (role == null)
        {
            throw new InvalidOperationException($"Role '{roleName}' does not exist.");
        }

        user.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id
        });
    }

    public async Task RemoveFromRoleAsync(User user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Guard.NotNull(user);
        Guard.NotEmpty(roleName);

        var role = await FindUserRoleAsync(roleName, false, cancellationToken);
        if (role == null)
        {
            return;
        }

        if (_db
            .Entry(user)
            .Collection(x => x.UserRoles)
            .IsLoaded)
        {
            var userRole = user.UserRoles.FirstOrDefault(x => x.RoleId == role.Id);
            _db.UserRoles.Remove(userRole);
        }
        else
        {
            var userRole = await _db.UserRoles.SingleOrDefaultAsync(x => x.RoleId == role.Id && x.UserId == user.Id,
                cancellationToken);
            _db.UserRoles.Remove(userRole);
        }
    }


    public async Task<IList<string>> GetRolesAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Guard.NotNull(user);
        return await _db
            .UserRoles.Where(x => x.UserId == user.Id)
            .Select(x => x.Role.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsInRoleAsync(User user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Guard.NotNull(user);
        Guard.NotEmpty(roleName);
        var role = await FindUserRoleAsync(roleName, true, cancellationToken);
        if (role == null)
        {
            return false;
        }

        if (_db
            .Entry(user)
            .Collection(x => x.UserRoles)
            .IsLoaded)
        {
            return user.UserRoles.Any(x => x.RoleId == role.Id);
        }
        else
        {
            return await _db.UserRoles.AnyAsync(x => x.RoleId == role.Id && x.UserId == user.Id, cancellationToken);
        }
    }

    public async Task<IList<User>> GetUsersInRoleAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Guard.NotEmpty(normalizedRoleName);
        var role = await FindUserRoleAsync(normalizedRoleName, false, cancellationToken);
        if (role == null)
        {
            return [];
        }

        return await _db
            .Users.Where(x => x.UserRoles.Any(y => y.RoleId == role.Id))
            .ToListAsync(cancellationToken);
    }

    public Task SetPasswordHashAsync(User user, string passwordHash, CancellationToken cancellationToken)
    {
        Guard.NotNull(user);
        user.Password = passwordHash;
        return Task.CompletedTask;
    }

    public Task<string> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
    {
        Guard.NotNull(user);
        return Task.FromResult(user.Password);
    }

    public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Password != null);
    }

    public async Task AddLoginAsync(User user, UserLoginInfo login, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Guard.NotNull(user);
        Guard.NotNull(login);
        _db.ExternalIdentityLogins.Add(new ExternalIdentityLogin()
        {
            UserId = user.Id,
            LoginProvider = login.LoginProvider,
            ProviderDisplayName = login.ProviderDisplayName,
            ProviderKey = login.ProviderKey
        });
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveLoginAsync(User user, string loginProvider, string providerKey,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Guard.NotNull(user);

        var login = await _db.ExternalIdentityLogins
            .SingleOrDefaultAsync(x => x.UserId == user.Id &&
                                       x.LoginProvider == loginProvider
                                       && x.ProviderKey == providerKey,
                cancellationToken);
        if (login == null)
        {
            return;
        }

        _db.ExternalIdentityLogins.Remove(login);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IList<UserLoginInfo>> GetLoginsAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Guard.NotNull(user);
        var logins = await _db
            .ExternalIdentityLogins.Where(x => x.UserId == user.Id)
            .ToListAsync(cancellationToken);
        return logins
            .Select(x => new UserLoginInfo(x.LoginProvider, x.ProviderKey, x.ProviderDisplayName))
            .ToArray();
    }

    public async Task<User> FindByLoginAsync(string loginProvider, string providerKey,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Guard.NotEmpty(loginProvider);
        Guard.NotEmpty(providerKey);
        var login = await _db.ExternalIdentityLogins.FirstOrDefaultAsync(
            x => x.LoginProvider == loginProvider && x.ProviderKey == providerKey,
            cancellationToken);
        return login == null ? null : await _db.Users.FirstOrDefaultAsync(x => x.Id == login.UserId, cancellationToken);
    }

    protected virtual async Task<Role> FindUserRoleAsync(string name, bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        Guard.NotEmpty(name);
        var cacheKey = $"roles.{name}-{activeOnly}";
        return await _requestCache.GetOrCreateAsync(cacheKey,
            async () =>
            {
                return await _db
                    .Roles.Where(x => x.Name == name)
                    .Where(x => !activeOnly || x.Active)
                    .SingleOrDefaultAsync(cancellationToken);
            });
    }

    protected static IdentityResult Failed(Exception e)
    {
        return e == null
            ? IdentityResult.Failed()
            : IdentityResult.Failed(new IdentityError { Description = e.Message });
    }

    protected static IdentityResult Failed(string msg)
    {
        return msg.IsEmpty() ? IdentityResult.Failed() : IdentityResult.Failed(new IdentityError { Description = msg });
    }

    protected static IdentityResult Failed(params IdentityError[] errors)
    {
        return IdentityResult.Failed(errors);
    }

    public void Dispose()
    {
    }

    public Task SetSecurityStampAsync(User user, string stamp, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Guard.NotNull(user);
        Guard.NotNull(stamp);
        user.SecurityStamp = stamp;
        return Task.CompletedTask;
    }

    public Task<string> GetSecurityStampAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Guard.NotNull(user);
        return Task.FromResult(user.SecurityStamp);
    }
}