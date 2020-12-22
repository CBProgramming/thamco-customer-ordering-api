using Review.Facade.Models;
using System;
using System.Threading.Tasks;

namespace Review.Facade
{
    public interface IReviewFacade
    {
        public Task<bool> NewPurchases(PurchaseDto purchases);
    }
}
