using StaffProduct.Facade.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StaffProduct.Facade
{
    public interface IStaffProductFacade
    {
        public Task<bool> UpdateStock(List<StockReductionDto> stockReductions);
    }
}
