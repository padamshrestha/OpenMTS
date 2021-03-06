﻿using Microsoft.Extensions.Logging;
using OpenMTS.Models;
using OpenMTS.Repositories;
using OpenMTS.Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenMTS.Services
{
    /// <summary>
    /// A service for querying and managing the inventory.
    /// </summary>
    public class InventoryService
    {
        /// <summary>
        /// Provides transaction logging functionality.
        /// </summary>
        private TransactionLogService TransactionLogService { get; }

        /// <summary>
        /// The underlying repository for material batches.
        /// </summary>
        private IMaterialBatchRepository MaterialBatchRepository { get; }

        /// <summary>
        /// A logger instance for local logging needs
        /// </summary>
        private ILogger Logger { get; }

        public InventoryService(ILoggerFactory loggerFactory,
            IMaterialBatchRepository materialBatchRepository,
            TransactionLogService transactionLogService)
        {
            Logger = loggerFactory.CreateLogger<InventoryService>();
            MaterialBatchRepository = materialBatchRepository;
            TransactionLogService = transactionLogService;
        }

        /// <summary>
        /// Gets material batches, possibly filtered.
        /// </summary>
        /// <param name="materialId">The ID of a material to filter with.</param>
        /// <param name="siteId">The ID of a storage site to filter with.</param>
        /// <returns>Returns all matching batches.</returns>
        public IEnumerable<MaterialBatch> GetMaterialBatches(int? materialId = null, Guid? siteId = null)
        {
            IEnumerable<MaterialBatch> materialBatches = MaterialBatchRepository.GetMaterialBatches(materialId, siteId);
            return materialBatches;
        }

        /// <summary>
        /// Gets a material batch by its unique ID.
        /// </summary>
        /// <param name="batchId">The ID of the batch to get.</param>
        /// <exception cref="MaterialBatchNotFoundException">Thrown if no matching batch could be found.</exception>
        /// <returns>Returns the batch.</returns>
        public MaterialBatch GetMaterialBatch(Guid batchId)
        {
            return GetBatchOrThrowNotFoundException(batchId);
        }

        /// <summary>
        /// Creates a new material batch.
        /// </summary>
        /// <param name="material">The material the batch is composed of..</param>
        /// <param name="expirationDate">The expiration date of the material.</param>
        /// <param name="storageLocation">The storage location of the material.</param>
        /// <param name="batchNumber">The manufacturer provided batch number.</param>
        /// <param name="quantity">The quantity of the batch.</param>
        /// <param name="customProps">The custom prop values for this batch.</param>
        /// <param name="isLocked">Whether the batch should be locked.</param>
        /// <param name="userId">The ID of the user checking in the new batch..</param>
        /// <returns>Returns the newly created batch.</returns>
        public MaterialBatch CreateMaterialBatch(Material material,
            DateTime expirationDate,
            StorageLocation storageLocation,
            long batchNumber,
            double quantity,
            Dictionary<Guid, string> customProps,
            bool isLocked,
            string userId)
        {
            // Create batch
            MaterialBatch batch = MaterialBatchRepository.CreateMaterialBatch(material, expirationDate, storageLocation, batchNumber, quantity, customProps, isLocked);

            // Log transaction
            Transaction transaction = new Transaction()
            {
                Id = Guid.NewGuid(),
                MaterialBatchId = batch.Id,
                Quantity = batch.Quantity,
                Timestamp = DateTime.UtcNow,
                UserId = userId
            };
            TransactionLogService.LogTransaction(transaction);

            // Done - return newly create batch!
            return batch;
        }

        /// <summary>
        /// Updates a material batch.
        /// </summary>
        /// <param name="batchId">The ID of the batch to update.</param>
        /// <param name="material">The material this batch consists of.</param>
        /// <param name="expirationDate">The expiration date of the material.</param>
        /// <param name="storageLocation">The storage location of the batch.</param>
        /// <param name="batchNumber">The batch number.</param>
        /// <param name="customProps">The custom prop values for this batch.</param>
        /// <exception cref="MaterialBatchNotFoundException">Thrown if no matching batch could be found.</exception>
        /// <returns>Returns the updated batch.</returns>
        public MaterialBatch UpdateMaterialBatch(Guid batchId,
            Material material,
            DateTime expirationDate,
            StorageLocation storageLocation,
            long batchNumber,
            Dictionary<Guid, string> customProps)
        {
            // Get batch
            MaterialBatch batch = GetBatchOrThrowNotFoundException(batchId);

            // Validate expirationd date
            IEnumerable<Transaction> log = TransactionLogService.GetTransactionLog(batchId);
            DateTime originalCheckIn = log.Last().Timestamp.Date;
            if (expirationDate <= originalCheckIn)
            {
                throw new ArgumentException("The expiration date cannot be set prior to the original check in date of the material batch.");
            }

            // Proceed with update
            batch.Material = material;
            batch.ExpirationDate = expirationDate;
            batch.StorageLocation = storageLocation;
            batch.BatchNumber = batchNumber;
            batch.CustomProps = customProps;
            MaterialBatchRepository.UpdateMaterialBatch(batch);
            return batch;
        }

        /// <summary>
        /// Updates the status of a material batch (whether it is locked or not).
        /// </summary>
        /// <param name="batchId">The ID of the batch to lock or unlock.</param>
        /// <param name="isLocked">Whether the batch should be locked..</param>
        /// <exception cref="MaterialBatchNotFoundException">Thrown if no matching batch could be found.</exception>
        public void UpdateBatchStatus(Guid batchId, bool isLocked)
        {
            MaterialBatch batch = GetBatchOrThrowNotFoundException(batchId);
            batch.IsLocked = isLocked;
            MaterialBatchRepository.UpdateMaterialBatch(batch);
        }

        #region Transactions & log

        /// <summary>
        /// Gets the full transaction log of a material batch.
        /// </summary>
        /// <param name="batchId">The ID of the batch to get the log for..</param>
        /// <exception cref="MaterialBatchNotFoundException">Thrown if no matching batch could be found.</exception>
        /// <returns>Returns all matching transactions.</returns>
        public IEnumerable<Transaction> GetMaterialBatchTransactionLog(Guid batchId)
        {
            GetBatchOrThrowNotFoundException(batchId);
            return TransactionLogService.GetTransactionLog(batchId);
        }

        /// <summary>
        /// Gets the last material batch transaction.
        /// </summary>
        /// <param name="batchId">The ID of the batch to get the last transaction for.</param>
        /// <exception cref="MaterialBatchNotFoundException">Thrown if the batch could be found.</exception>
        /// <returns>Returns the trnasaction</returns>
        public Transaction GetLastMaterialBatchTransaction(Guid batchId)
        {
            GetBatchOrThrowNotFoundException(batchId);
            return TransactionLogService.GetLastTransactionLogEntry(batchId);

        }

        /// <summary>
        /// Performs a material transaction: checking material in or out of storage.
        /// </summary>
        /// <param name="batchId">The ID of the batch to perform a transaction on.</param>
        /// <param name="quantity">The quantity to check out or in. Negative numbers indicate a check-out, positive numbers a check-in.</param>
        /// <param name="userId">The ID of the user performing the transaction.</param>
        /// <returns>Returns the transcation.</returns>
        /// <exception cref="ArgumentException">Thrown if the new computed quantity of material is less than 0.</exception>
        public Transaction PerformMaterialTransaction(Guid batchId, double quantity, string userId)
        {
            MaterialBatch batch = GetBatchOrThrowNotFoundException(batchId);

            // Check whether batch is locked
            if (batch.IsLocked)
            {
                throw new UnauthorizedAccessException("This batch is locked!");
            }

            // Compute and validate new quantity
            double newQuantity = RoundMaterialQuantity(batch.Quantity + quantity);
            if (newQuantity < 0)
            {
                throw new ArgumentException("The quantity of a batch cannot be less than 0. You cannot check out more material than there is in the inventory!");
            }

            // Update quantity
            batch.Quantity = newQuantity;
            if (batch.Quantity == 0) {
                batch.IsArchived = true;
            }

            // Generate transaction
            Transaction transaction = new Transaction()
            {
                Id = Guid.NewGuid(),
                MaterialBatchId = batchId,
                Quantity = quantity,
                Timestamp = DateTime.UtcNow,
                UserId = userId
            };

            // Persist batch and log transaction
            MaterialBatchRepository.UpdateMaterialBatch(batch);
            TransactionLogService.LogTransaction(transaction);

            // Done - return transaction!
            return transaction;
        }

        /// <summary>
        /// Amends the last material batch transaction.
        /// </summary>
        /// <param name="batchId">The ID of the batch.</param>
        /// <param name="transactionId">The ID of the transaction.</param>
        /// <param name="quantity">The quantity to set.</param>
        /// <param name="userId">The ID of the user attempting to amend the transaction.</param>
        /// <exception cref="OpenMTS.Services.Exceptions.NotLastLogEntryException">Thrown if the passed transaction ID doesn't match the last transaction of this batch.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if the last transaction was performed by a different user.</exception>
        public void AmendLastMaterialBatchTransaction(Guid batchId, Guid transactionId, double quantity, string userId)
        {
            MaterialBatch batch = GetBatchOrThrowNotFoundException(batchId);

            // Get and validate last transaction
            Transaction transaction = TransactionLogService.GetLastTransactionLogEntry(batchId);
            if (transaction.Id != transactionId)
            {
                throw new NotLastLogEntryException(transactionId);
            }

            // Validate user
            if (transaction.UserId != userId)
            {
                throw new UnauthorizedAccessException("The last transaction was performed by a different user.");
            }

            // Calculate new quantity differential
            quantity = RoundMaterialQuantity(quantity);
            double newBatchQuantity = batch.Quantity - transaction.Quantity + quantity;

            // Attempt to amend last transaction
            TransactionLogService.AmendLastTransactionLogEntry(batchId, transactionId, quantity);

            // No exception thrown - update material batch
            batch.Quantity = newBatchQuantity;
            MaterialBatchRepository.UpdateMaterialBatch(batch);
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Attempts to get a batch from the underlying repository and throws a <see cref="MaterialBatchNotFoundException"/> if no matching batch could be found.
        /// </summary>
        /// <param name="id">ID of the material batch to get.</param>
        /// <exception cref="MaterialBatchNotFoundException">Thrown if no matching batch could be found.</exception>
        /// <returns>Returns the batch, if found.</returns>
        private MaterialBatch GetBatchOrThrowNotFoundException(Guid id)
        {
            MaterialBatch batch = MaterialBatchRepository.GetMaterialBatch(id);

            // Check for batch existence
            if (batch == null)
            {
                throw new MaterialBatchNotFoundException(id);
            }

            return batch;
        }

        /// <summary>
        /// Rounds a material quantity to 3 digits after the floating point.
        /// </summary>
        /// <param name="quantity">The quantity to round.</param>
        /// <returns>Returns the rounded quantity.</returns>
        private double RoundMaterialQuantity(double quantity)
        {
            return Math.Round(quantity, 3, MidpointRounding.AwayFromZero);
        }

        #endregion
    }
}
