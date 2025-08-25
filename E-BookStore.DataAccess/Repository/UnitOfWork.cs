using E_BookStore.DataAccess.Data;
using E_BookStore.DataAccess.Repository.IRepository;


namespace E_BookStore.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;
        public ICategoryRepository Category {  get;private set; }
        public IProductRepository Product { get;private set; }
        public ICompanyRepository Company { get;private set; }
        public IShoppingCartRepository ShoppingCart { get;private set; }
        public IApplicationUserRepository ApplicationUser { get;private set; }
        public IOrderDetailRepository OrderDetail { get;private set; }
        public IOrderHeaderRepository OrderHeader { get;private set; }
        public IProductImageRepository ProductImage { get;private set; }
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Category = new CategoryRepository(db);
            Product = new ProductRepository(db);
            Company = new CompanyRepository(db);
            ApplicationUser = new ApplicationUserRepository(db);
            ShoppingCart = new ShoppingCartRepository(db);
            OrderDetail = new OrderDetailRepository(db);
            OrderHeader = new OrderHeaderRepository(db);
            ProductImage = new ProductImageRepository(db);
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
