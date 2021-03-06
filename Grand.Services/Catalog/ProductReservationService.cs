﻿using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Services.Events;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Product reservation service
    /// </summary>
    public partial class ProductReservationService : IProductReservationService
    {
        private readonly IRepository<ProductReservation> _productReservationRepository;
        private readonly IRepository<CustomerReservationsHelper> _customerReservationsHelperRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly IWorkContext _workContext;

        public ProductReservationService(IRepository<ProductReservation> productReservationRepository,
            IRepository<CustomerReservationsHelper> customerReservationsHelperRepository,
            IEventPublisher eventPublisher,
            IWorkContext workContext)
        {
            _productReservationRepository = productReservationRepository;
            _customerReservationsHelperRepository = customerReservationsHelperRepository;
            _eventPublisher = eventPublisher;
            _workContext = workContext;
        }

        /// <summary>
        /// Deletes a product reservation
        /// </summary>
        /// <param name="productReservation">Product reservation</param>
        public virtual async Task DeleteProductReservation(ProductReservation productReservation)
        {
            if (productReservation == null)
                throw new ArgumentNullException("productReservation");

            await _productReservationRepository.DeleteAsync(productReservation);
            await _eventPublisher.EntityDeleted(productReservation);
        }

        /// <summary>
        /// Deletes multiple product reservations at once
        /// </summary>
        /// <param name="productReservation">Product reservation</param>
        public virtual async Task DeleteProductReservation(IEnumerable<ProductReservation> productReservations)
        {
            if (productReservations == null)
                throw new ArgumentNullException("productReservation");

            await _productReservationRepository.DeleteAsyncById(productReservations.Select(x => x.Id));

            foreach (var productReservation in productReservations)
            {
                await _eventPublisher.EntityDeleted(productReservation);
            }
        }

        /// <summary>
        /// Gets product reservations for product Id
        /// </summary>
        /// <param name="productId">Product Id</param>
        /// <returns>Product reservations</returns>
        public virtual async Task<IPagedList<ProductReservation>> GetProductReservationsByProductId(string productId, bool? showVacant, DateTime? date,
            int pageIndex = 0, int pageSize = int.MaxValue, string resourceSystemName = null)
        {
            var min = date.HasValue ? new DateTime(date.Value.Year, date.Value.Month, date.Value.Day, 0, 0, 0, 0) : (DateTime?)null;
            var max = date.HasValue ? new DateTime(date.Value.Year, date.Value.Month, date.Value.Day, 23, 59, 59, 999) : (DateTime?)null;

            return await GetProductReservationsByProductId(productId, showVacant, min, max, pageIndex, pageSize, resourceSystemName);
        }

        public virtual async Task<IPagedList<ProductReservation>> GetProductReservationsByProductId(string productId, bool? showVacant, DateTime? minDate,
            DateTime? maxDate, int pageIndex = 0, int pageSize = int.MaxValue, string resourceSystemName = null)
        {
            var query = _productReservationRepository.Table.Where(x => x.ProductId == productId);

            if (showVacant.HasValue)
            {
                if (showVacant.Value)
                {
                    query = query.Where(x => (x.OrderId == "" || x.OrderId == null));
                }
                else
                {
                    query = query.Where(x => (x.OrderId != "" && x.OrderId != null));
                }
            }

            if (minDate.HasValue)
            {
                query = query.Where(x => x.Date >= minDate);
            }

            if (maxDate.HasValue)
            {
                query = query.Where(x => x.Date <= maxDate);
            }

            query = query.Where(x => resourceSystemName == null || x.Resource == resourceSystemName);
            query = query.OrderBy(x => x.Date);
            return await PagedList<ProductReservation>.Create(query, pageIndex, pageSize);
        }


        public async Task<IPagedList<ProductReservation>> GetReservations(bool? showVacant, DateTime? minDate, DateTime? maxDate, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _productReservationRepository.Table;

            if (showVacant.HasValue)
            {
                if (showVacant.Value)
                {
                    query = query.Where(x => (x.OrderId == "" || x.OrderId == null));
                }
                else
                {
                    query = query.Where(x => (x.OrderId != "" && x.OrderId != null));
                }
            }

            if (minDate.HasValue)
            {
                query = query.Where(x => x.Date >= minDate);
            }

            if (maxDate.HasValue)
            {
                query = query.Where(x => x.Date <= maxDate);
            }

            query = query.OrderBy(x => x.Date);
            return await PagedList<ProductReservation>.Create(query, pageIndex, pageSize);
        }

        public virtual async Task<IEnumerable<ProductReservation>> GetProductReservationsByOrderId(string orderId)
        {
            return await _productReservationRepository.Table.Where(x => x.OrderId == orderId)
                                                                 .OrderBy(x => x.Date)
                                                                 .ToListAsync();
        }

        /// <summary>
        /// Adds product reservation
        /// </summary>
        /// <param name="productReservation">Product reservation</param>
        public virtual async Task InsertProductReservation(ProductReservation productReservation)
        {
            if (productReservation == null)
                throw new ArgumentNullException("productAttribute");

            await _productReservationRepository.InsertAsync(productReservation);
            await _eventPublisher.EntityInserted(productReservation);
        }

        /// <summary>
        /// Adds multiple product reservations at once
        /// </summary>
        /// <param name="productReservations">Product reservation</param>
        public virtual async Task InsertProductReservation(IEnumerable<ProductReservation> productReservations)
        {
            if (productReservations == null)
                throw new ArgumentNullException("productAttribute");

            await _productReservationRepository.InsertAsync(productReservations);

            foreach (var productReservation in productReservations)
            {
                await _eventPublisher.EntityInserted(productReservation);
            }
        }

        /// <summary>
        /// Updates product reservation
        /// </summary>
        /// <param name="productReservation">Product reservation</param>
        public virtual async Task UpdateProductReservation(ProductReservation productReservation)
        {
            if (productReservation == null)
                throw new ArgumentNullException("productAttribute");

            await _productReservationRepository.UpdateAsync(productReservation);
            await _eventPublisher.EntityInserted(productReservation);
        }

        /// <summary>
        /// Updates product reservation
        /// </summary>
        /// <param name="productReservation">Product reservation</param>
        public virtual async Task UpdateProductReservation(IEnumerable<ProductReservation> productReservations)
        {
            if (productReservations == null)
                throw new ArgumentNullException("productAttribute");

            await _productReservationRepository.UpdateAsync(productReservations);

            foreach (var productReservation in productReservations)
            {
                await _eventPublisher.EntityInserted(productReservation);
            }
        }

        /// <summary>
        /// Gets product reservation for specified Id
        /// </summary>
        /// <param name="Id">Product Id</param>
        /// <returns>Product reservation</returns>
        public virtual Task<ProductReservation> GetProductReservation(string Id)
        {
            return _productReservationRepository.GetByIdAsync(Id);
        }

        /// <summary>
        /// Adds customer reservations helper
        /// </summary>
        /// <param name="crh"></param>
        public virtual async Task InsertCustomerReservationsHelper(CustomerReservationsHelper crh)
        {
            if (crh == null)
                throw new ArgumentNullException("CustomerReservationsHelper");

            await _customerReservationsHelperRepository.InsertAsync(crh);
            await _eventPublisher.EntityInserted(crh);
        }

        /// <summary>
        /// Deletes customer reservations helper
        /// </summary>
        /// <param name="crh"></param>
        public virtual async Task DeleteCustomerReservationsHelper(CustomerReservationsHelper crh)
        {
            if (crh == null)
                throw new ArgumentNullException("CustomerReservationsHelper");

            await _customerReservationsHelperRepository.DeleteAsync(crh);
            await _eventPublisher.EntityDeleted(crh);
        }


        /// <summary>
        /// Cancel reservations by orderId 
        /// </summary>
        /// <param name="orderId"></param>
        public virtual async Task CancelReservationsByOrderId(string orderId)
        {
            if (!string.IsNullOrEmpty(orderId))
            {
                var update = new UpdateDefinitionBuilder<ProductReservation>().Set(x => x.OrderId, "").Set(x => x.OrderItemId, "");
                await _productReservationRepository.Collection.UpdateManyAsync(x => x.OrderId == orderId, update);
            }
        }

        /// <summary>
        /// Gets customer reservations helper by id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>CustomerReservationsHelper</returns>
        public virtual Task<CustomerReservationsHelper> GetCustomerReservationsHelperById(string Id)
        {
            return _customerReservationsHelperRepository.GetByIdAsync(Id);
        }

        /// <summary>
        /// Gets customer reservations helpers
        /// </summary>
        /// <returns>List<CustomerReservationsHelper></returns>
        public virtual async Task<IList<CustomerReservationsHelper>> GetCustomerReservationsHelpers()
        {
            return await _customerReservationsHelperRepository.Table.Where(x => x.CustomerId == _workContext.CurrentCustomer.Id).ToListAsync();
        }

        /// <summary>
        /// Gets customer reservations helper by Shopping Cart Item id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>List<CustomerReservationsHelper></returns>
        public virtual async Task<IList<CustomerReservationsHelper>> GetCustomerReservationsHelperBySciId(string sciId)
        {
            return await _customerReservationsHelperRepository.Table.Where(x => x.ShoppingCartItemId == sciId).ToListAsync();
        }

        public virtual async Task<bool> HasAnyReservations(string productId)
        {
            return await _productReservationRepository.Table.Where(x => x.ProductId == productId).AnyAsync();
        }
    }
}
