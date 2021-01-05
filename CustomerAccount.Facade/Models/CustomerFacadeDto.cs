﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerAccount.Facade.Models
{
    public class CustomerFacadeDto
    {
        public int CustomerId { get; set; }

        public string CustomerAuthId { get; set; }

        public string GivenName { get; set; }

        public string FamilyName { get; set; }

        public string AddressOne { get; set; }

        public string AddressTwo { get; set; }

        public string Town { get; set; }

        public string State { get; set; }

        public string AreaCode { get; set; }

        public string Country { get; set; }

        public string EmailAddress { get; set; }

        public string TelephoneNumber { get; set; }

        public bool CanPurchase { get; set; }

        public bool Active { get; set; }
    }
}
