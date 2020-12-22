using Review.Facade.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Review.Facade
{
    public class FakeReviewFacade : IReviewFacade
    {
        public bool Succeeds = true;

        public async Task<bool> NewPurchases(PurchaseDto purchases)
        {
            return Succeeds;
        }
    }
}
