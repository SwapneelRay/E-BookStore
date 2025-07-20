using E_BookStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_BookStore.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository: IRepository<OrderHeader>
    {
        void Update(OrderHeader obj);
        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null);
        public void UpdateRazorPayPaymentId(int id,string sessionId,string paymentIntentId);
    }
}
