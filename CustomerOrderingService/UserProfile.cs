using AutoMapper;
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
            CreateMap<BasketItemDto, BasketItemEFModel>();
            CreateMap<BasketItemEFModel, BasketItemDto>();
            CreateMap<BasketItemDto, BasketProductsEFModel>();
            CreateMap<BasketProductsEFModel, BasketItemDto>();
            CreateMap<BasketItemEFModel, BasketItem>();
            CreateMap<BasketItem, BasketItemEFModel>();
            CreateMap<BasketItemEFModel, ProductEFModel>();
            CreateMap<ProductEFModel, BasketItemEFModel>();
            CreateMap<ProductEFModel, Product>();
            CreateMap<OrderedItemDto, ProductEFModel>();
            CreateMap<ProductEFModel, OrderedItemDto>();
            CreateMap<Product, BasketItemEFModel>();
            CreateMap<CustomerDto, CustomerEFModel>();
            CreateMap<CustomerEFModel, CustomerDto>();
            CreateMap<CustomerEFModel, Customer>();
            CreateMap<Customer, CustomerEFModel>();
            CreateMap<OrderData.Order, OrderEFModel>();
            CreateMap<OrderEFModel, OrderData.Order>();
            CreateMap<OrderEFModel, OrderDto>();
            CreateMap<OrderDto, OrderEFModel>();
            CreateMap<OrderedItem, OrderedItemEFModel>();
            CreateMap<OrderedItemEFModel, OrderedItem>();
            CreateMap<OrderedItemDto, OrderedItemEFModel>();
            CreateMap<OrderedItemEFModel, OrderedItemDto>();
            CreateMap<StockReductionDto, OrderedItemDto>();
            CreateMap<OrderedItemDto, StockReductionDto>();
            CreateMap<FinalisedOrderDto, FinalisedOrderEFModel>();
            CreateMap<FinalisedOrderEFModel, FinalisedOrderDto>();
            CreateMap<OrderEFModel, OrderHistoryDto>();
            CreateMap<OrderHistoryDto, OrderEFModel>();
        }
    }
}
