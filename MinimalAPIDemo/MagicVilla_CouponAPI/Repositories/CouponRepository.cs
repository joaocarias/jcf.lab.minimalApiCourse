using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Repositories.IRepsitories;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_CouponAPI.Repositories
{
    public class CouponRepository : ICouponRepository
    {
        public readonly AppDbContext _db;

        public CouponRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task CreateAsync(Coupon coupon)
        {
           await _db.AddAsync(coupon);
        }

        public async Task<IEnumerable<Coupon>> GetAllAsync()
        {
            return await _db.Coupons.ToListAsync();
        }

        public async Task<Coupon> GetAsync(int id)
        {
            return await _db.Coupons.FirstOrDefaultAsync( x => x.Id == id);
        }

        public async Task<Coupon> GetAsync(string couponName)
        {
            return await _db.Coupons.FirstOrDefaultAsync(x => x.Name.ToLower() == couponName.ToLower());
        }

        public async Task RemoveAsysc(Coupon coupon)
        {
            _db.Coupons.Remove(coupon);
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Coupon coupon)
        {
            _db.Coupons.Update(coupon);
        }
    }
}
