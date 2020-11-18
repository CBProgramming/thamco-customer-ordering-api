using AutoMapper;
using CustomerOrderingService.Data;
using CustomerOrderingService.Models;
using Order.Repository.Data;
using Order.Repository.Models;
using OrderData;
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
            CreateMap<BasketItemDto, BasketItemModel>();
            CreateMap<BasketItemModel, BasketItemDto>();
            CreateMap<BasketItemModel, BasketItem>();
            CreateMap<BasketItem, BasketItemModel>();
            CreateMap<BasketItemModel, ProductEFModel>();
            CreateMap<ProductEFModel, BasketItemModel>();
            CreateMap<ProductEFModel, Product>();
            CreateMap<Product, BasketItemModel>();
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
        }
    }
}
