using Grand.Core;
using Grand.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Product reservation service interface
    /// </summary>
    public partial interface IProductReservationService
    {
        /// <summary>
        /// Deletes a product reservation
        /// </summary>
        /// <param name="productReservation">Product reservation</param>
        Task DeleteProductReservation(ProductReservation productReservation);

        /// <summary>
        /// Deletes multiple product reservations at once
        /// </summary>
        /// <param name="productReservation">Ids of product reservations to delete</param>
        Task DeleteProductReservation(IEnumerable<ProductReservation> productReservations);

        /// <summary>
        /// Adds product reservation
        /// </summary>
        /// <param name="productReservation">Product reservation</param>
        Task InsertProductReservation(ProductReservation productReservation);

        /// <summary>
        /// Adds multiple product reservations at once
        /// </summary>
        /// <param name="productReservations">Product reservation</param>
        Task InsertProductReservation(IEnumerable<ProductReservation> productReservations);

        /// <summary>
        /// Updates product reservation
        /// </summary>
        /// <param name="productReservation">Product reservation</param>
        Task UpdateProductReservation(ProductReservation productReservation);

        /// <summary>
        /// Updates product reservation
        /// </summary>
        /// <param name="productReservation">Product reservation</param>
        Task UpdateProductReservation(IEnumerable<ProductReservation> productReservations);

        /// <summary>
        /// Gets product reservations for product Id
        /// </summary>
        /// <param name="productId">Product Id</param>
        /// <returns>Product reservations</returns>
        Task<IPagedList<ProductReservation>> GetProductReservationsByProductId(string productId, bool? showVacant, DateTime? date,
            int pageIndex = 0, int pageSize = int.MaxValue, string resourceSystemName = null);

        Task<IPagedList<ProductReservation>> GetProductReservationsByProductId(string productId, bool? showVacant, DateTime? minDate,
            DateTime? maxDate, int pageIndex = 0, int pageSize = int.MaxValue, string resourceSystemName = null);

        /// <summary>
        /// Gets product reservation for specified Id
        /// </summary>
        /// <param name="Id">Product Id</param>
        /// <returns>Product reservation</returns>
        Task<ProductReservation> GetProductReservation(string Id);

        /// <summary>
        /// Adds customer reservations helper
        /// </summary>
        /// <param name="crh"></param>
        Task InsertCustomerReservationsHelper(CustomerReservationsHelper crh);

        /// <summary>
        /// Deletes customer reservations helper
        /// </summary>
        /// <param name="crh"></param>
        Task DeleteCustomerReservationsHelper(CustomerReservationsHelper crh);

        /// <summary>
        /// Cancel reservations by orderId 
        /// </summary>
        /// <param name="orderId"></param>
        Task CancelReservationsByOrderId(string orderId);

        /// <summary>
        /// Gets customer reservations helper by id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>CustomerReservationsHelper</returns>
        Task<CustomerReservationsHelper> GetCustomerReservationsHelperById(string Id);

        /// <summary>
        /// Gets customer reservations helpers
        /// </summary>
        /// <returns>List<CustomerReservationsHelper></returns>
        Task<IList<CustomerReservationsHelper>> GetCustomerReservationsHelpers();

        /// <summary>
        /// Gets customer reservations helper by Shopping Cart Item id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>List<CustomerReservationsHelper></returns>
        Task<IList<CustomerReservationsHelper>> GetCustomerReservationsHelperBySciId(string sciId);

        /// <summary>
        /// Checks if there are any reservations for the product
        /// </summary>
        /// <param name="productId"></param>
        Task<bool> HasAnyReservations(string productId);
    }
}
