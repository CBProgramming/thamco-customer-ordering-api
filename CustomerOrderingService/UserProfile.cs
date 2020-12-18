using AutoMapper;
using CustomerAccount.Facade.Models;
using CustomerOrderingService.Models;
using Order.Repository.Data;
using Order.Repository.Models;
using OrderData;
using StaffProduct.Facade.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerOrderingService
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<BasketItemDto, BasketItemRepoModel>();
            CreateMap<BasketItemRepoModel, BasketItemDto>();
            CreateMap<BasketItemDto, BasketItemRepoModel>();
            CreateMap<BasketItemRepoModel, BasketItemDto>();
            CreateMap<BasketItemRepoModel, BasketItem>();
            CreateMap<BasketItem, BasketItemRepoModel>();
            CreateMap<BasketItemRepoModel, ProductRepoModel>();
            CreateMap<ProductRepoModel, BasketItemRepoModel>();
            CreateMap<ProductRepoModel, Product>();
            CreateMap<OrderedItemDto, ProductRepoModel>();
            CreateMap<ProductRepoModel, OrderedItemDto>();
            CreateMap<Product, BasketItemRepoModel>();
            CreateMap<CustomerDto, CustomerRepoModel>();
            CreateMap<CustomerRepoModel, CustomerDto>();
            CreateMap<CustomerRepoModel, Customer>();
            CreateMap<Customer, CustomerRepoModel>();
            CreateMap<OrderData.Order, OrderRepoModel>();
            CreateMap<OrderRepoModel, OrderData.Order>();
            CreateMap<OrderRepoModel, OrderDto>();
            CreateMap<OrderDto, OrderRepoModel>();
            CreateMap<OrderedItem, OrderedItemRepoModel>();
            CreateMap<OrderedItemRepoModel, OrderedItem>();
            CreateMap<OrderedItemDto, OrderedItemRepoModel>();
            CreateMap<OrderedItemRepoModel, OrderedItemDto>();
            CreateMap<StockReductionDto, OrderedItemDto>();
            CreateMap<OrderedItemDto, StockReductionDto>();
            CreateMap<FinalisedOrderDto, FinalisedOrderRepoModel>();
            CreateMap<FinalisedOrderRepoModel, FinalisedOrderDto>();
            CreateMap<OrderRepoModel, OrderHistoryDto>();
            CreateMap<OrderHistoryDto, OrderRepoModel>();
            CreateMap<CustomerDto, CustomerFacadeDto>();
            CreateMap<CustomerFacadeDto, CustomerDto>();
            CreateMap<ProductDto, ProductRepoModel>();
            CreateMap<ProductRepoModel, ProductDto>();
        }
    }
}
