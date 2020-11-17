using AutoMapper;
using CustomerOrderingService.Data;
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
        }
    }
}
