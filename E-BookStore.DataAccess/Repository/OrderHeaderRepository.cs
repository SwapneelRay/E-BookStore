using E_BookStore.DataAccess.Data;
using E_BookStore.DataAccess.Repository.IRepository;
using E_BookStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_BookStore.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbContext _db;

        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(OrderHeader obj)
        {
            _db.orderHeaders.Update(obj);
        }

       
        public void UpdateStatus(int id, string orderStatus, string paymentStatus) 
        {
            var orderFromDb = _db.orderHeaders.FirstOrDefault(u=>u.Id == id);
            if (orderFromDb != null)
            {
                orderFromDb.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    orderFromDb.PaymentStatus= paymentStatus;
                }
            }
        }
        public void UpdateRazorPaySessionId(int id, string sessionId)
        {
            var orderFromDb = _db.orderHeaders.FirstOrDefault(u => u.Id == id);
            if (!string.IsNullOrEmpty(sessionId))
            {
                orderFromDb.SessionId = sessionId;
            }
            else
            {

            }
        }
            public void UpdateRazorPayPaymentId(int id, string sessionId, string paymentIntentId)
        {
            var orderFromDb = _db.orderHeaders.FirstOrDefault(u => u.Id == id);
            if (!string.IsNullOrEmpty(sessionId))
            {
                orderFromDb.SessionId = sessionId;
            }
            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                orderFromDb.PaymentIntentId = paymentIntentId;
                orderFromDb.PaymentDate = DateTime.Now;
            }
        }

    }
}
