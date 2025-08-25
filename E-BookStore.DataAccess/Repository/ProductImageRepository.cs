using E_BookStore.DataAccess.Data;
using E_BookStore.DataAccess.Repository.IRepository;
using E_BookStore.Models;

namespace E_BookStore.DataAccess.Repository
{
    public class ProductImageRepository:Repository<ProductImage>,IProductImageRepository
    {
        private ApplicationDbContext _db;
        public ProductImageRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ProductImage productImage)
        {
            _db.Update(productImage);
        }
    }
}

