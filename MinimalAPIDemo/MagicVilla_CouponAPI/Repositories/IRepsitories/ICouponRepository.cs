namespace MagicVilla_CouponAPI.Repositories.IRepsitories
{
    public interface ICouponRepository
    {
        Task<ICollection<Coupon>> GetAllAsync();
        Task<Coupon> GetAsync(int id);
        Task<Coupon> GetAsync(string couponName);
        Task CreateAsync(Coupon coupon);
        Task UpdateAsync(Coupon coupon);
        Task RemoveAsysc(Coupon coupon);
        Task SaveAsync();
    }
}
