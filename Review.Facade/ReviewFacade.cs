using Review.Facade.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Review.Facade
{
    public class ReviewFacade : IReviewFacade
    {
        public Task<bool> NewPurchases(PurchaseDto purchases)
        {
            throw new NotImplementedException();
        }
    }
}
