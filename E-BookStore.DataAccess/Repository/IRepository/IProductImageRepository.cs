using E_BookStore.Models;


namespace E_BookStore.DataAccess.Repository.IRepository
{
    public interface IProductImageRepository:IRepository<ProductImage>
    {
        void Update(ProductImage productImage);
    }
}
