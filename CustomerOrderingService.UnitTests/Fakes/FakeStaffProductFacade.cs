using StaffProduct.Facade;
using StaffProduct.Facade.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CustomerOrderingService.UnitTests.Fakes
{
    class FakeStaffProductFacade : IStaffProductFacade
    {
        public Task<bool> UpdateStock(List<StockReductionDto> stockReductions)
        {
            throw new NotImplementedException();
        }
    }
}
