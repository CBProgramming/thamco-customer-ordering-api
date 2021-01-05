using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using StaffProduct.Facade.Models;

namespace StaffProduct.Facade
{
    public class FakeStaffProductFacade : IStaffProductFacade
    {
        public bool CompletesStockReduction = true;
        public List<StockReductionDto> StockReductions { get; set; }

        public async Task<bool> UpdateStock(List<StockReductionDto> stockReductions)
        {
            StockReductions = stockReductions;
            if (stockReductions.Count > 0
                && CompletesStockReduction)
            {
                return true;
            }
            return false;
        }
    }
}
