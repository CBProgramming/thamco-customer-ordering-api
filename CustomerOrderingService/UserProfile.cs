using AutoMapper;
using CustomerAccount.Facade.Models;
using CustomerOrderingService.Models;
using Invoicing.Facade.Models;
using Order.Repository.Data;
using Order.Repository.Models;
using OrderData;
using Review.Facade.Models;
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
            CreateMap<CustomerOrderingService.Models.ProductDto, ProductRepoModel>();
            CreateMap<ProductRepoModel, CustomerOrderingService.Models.ProductDto>();
            CreateMap<OrderInvoiceDto, OrderDto>();
            CreateMap<OrderDto, OrderInvoiceDto>();
            CreateMap<OrderedItemDto, InvoiceItemDto>();
            CreateMap<InvoiceItemDto, OrderedItemDto>();
            CreateMap<FinalisedOrderDto, PurchaseDto>();
            CreateMap<PurchaseDto, FinalisedOrderDto>();
            CreateMap<Review.Facade.Models.ProductDto, OrderedItemDto>();
            CreateMap<OrderedItemDto, Review.Facade.Models.ProductDto > ();
            CreateMap<FinalisedOrderDto, OrderInvoiceDto>();
            CreateMap<OrderInvoiceDto, FinalisedOrderDto>();

        }
    }
}
