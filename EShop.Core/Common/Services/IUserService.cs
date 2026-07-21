using EShop.Core.Common.Domain;
using EShop.Core.Data;
using EShop.Core.Platform.Identity.Domain;
using EShop.Infrastructure.Caching;
using EShop.Infrastructure.Data;
using EShop.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Common.Services;

public interface IAddressService
{
    Task<ICollection<Address>> GetAddressesByUserIdAsync(int userId, bool tracking = true);
    Task<Address> GetUserAddressById(int userId, int addressId);

    Address FindAddress(IEnumerable<Address> source, string firstName, string lastName, string phoneNumber,
        string addressLine1,
        string addressLine2, string zipCode, int cityId);
}

public class DefaultAddressService : IAddressService
{
    private readonly ApplicationDbContext _dbContext;
    readonly IRequestCache _requestCache;
    private const string UserAddressesCacheKey = "user:addresses:{0}"; //userId

    //userId, addressId
    private const string UserAddressCacheKey = "user:address:{0}:{1}";

    public DefaultAddressService(ApplicationDbContext dbContext, IRequestCache requestCache)
    {
        _dbContext = dbContext;
        _requestCache = requestCache;
    }

    public async Task<ICollection<Address>> GetAddressesByUserIdAsync(int userId, bool tracking = true)
    {
        if (!(userId > 0))
        {
            return [];
        }

        string cacheKey = string.Format(UserAddressesCacheKey, userId);
        return await _requestCache.GetOrCreateAsync(cacheKey,
            async () => await _dbContext
                .UserAddresses.ApplyTracking(tracking)
                .Where(x => x.UserId == userId)
                .Select(x => x.Address)
                .ToListAsync());
    }

    public async Task<Address> GetUserAddressById(int userId, int addressId)
    {
        if (userId == 0 || addressId == 0)
        {
            return null;
        }

        string cacheKey = string.Format(UserAddressCacheKey, userId, addressId);
        return await _requestCache.GetOrCreateAsync(cacheKey,
            async () => await _dbContext
                .UserAddresses.Where(x => x.UserId == userId && x.AddressId == addressId)
                .Select(x => x.Address)
                .FirstOrDefaultAsync());
    }

   

    public Address FindAddress(IEnumerable<Address> source, string firstName, string lastName, string phoneNumber,
        string addressLine1,
        string addressLine2, string zipCode, int cityId)
    {
        return source.SingleOrDefault(x => x.AddressLine1 == addressLine1 && x.AddressLine2 == addressLine2
                                                                          && x.FirstName == firstName &&
                                                                          x.LastName == lastName &&
                                                                          x.PhoneNumber == phoneNumber
                                                                          && x.ZipCode == zipCode &&
                                                                          x.CityId == cityId);
    }
}