﻿using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KK.Models.Entities;
using KK.Models.Repositories;
using KK.Models.Interfaces;
using System.Collections.ObjectModel;

namespace Test
{
    [TestClass]
    public class EntryRepository_UnitTest
    {
        ObservableCollection<Customer> Customers = new ObservableCollection<Customer>();

        [TestInitialize]
        public void TestInitialize()
        {
            CustomerRepository customerRepo = new CustomerRepository();
            customerRepo.GetAll();
            Customers = customerRepo.Customers;
        }

        [TestMethod]
        public void AddEntry_Test()
        {
            // Arrange
            IEntryRepository entryRepository = new EntryRepository();
            Entry newEntry = new Entry
            {
                CheckInTime = DateTime.Now,
                CustomerId = Customers.FirstOrDefault().Id,
                Customer = Customers.FirstOrDefault()
            };

            // Act
            entryRepository.Add(newEntry);

            // Assert
            Assert.IsNotNull(newEntry.Id);
            Assert.AreNotEqual(0, newEntry.Id);

            entryRepository.Remove(newEntry);
        }

        [TestMethod]
        public void GetEntryById_Test()
        {
            // Arrange
            IEntryRepository entryRepository = new EntryRepository();
            Entry newEntry = new Entry
            {
                CheckInTime = DateTime.Now,
                CustomerId = Customers.FirstOrDefault().Id,
                Customer = Customers.FirstOrDefault()
            };

            // Act
            entryRepository.Add(newEntry);
            Entry retrievedEntry = entryRepository.GetById(newEntry.Id);

            // Assert
            Assert.IsNotNull(retrievedEntry);
            Assert.AreEqual(newEntry.Id, retrievedEntry.Id);

            entryRepository.Remove(newEntry);
        }

        [TestMethod]
        public void UpdateEntry_Test()
        {
            // Arrange
            IEntryRepository entryRepository = new EntryRepository();
            Entry newEntry = new Entry
            {
                CheckInTime = DateTime.Now,
                CustomerId = Customers.FirstOrDefault().Id,
                Customer = Customers.FirstOrDefault()
            };

            // Act
            entryRepository.Add(newEntry);
            newEntry.CheckInTime = new DateTime(2020, 11, 11); // Updated CheckInTime
            entryRepository.Update(newEntry);
            Entry updatedEntry = entryRepository.GetById(newEntry.Id);

            // Assert
            Assert.IsNotNull(updatedEntry);
            Assert.AreEqual(new DateTime(2020, 11, 11), updatedEntry.CheckInTime);

            entryRepository.Remove(updatedEntry);
        }

        [TestMethod]
        public void RemoveEntry_Test()
        {
            // Arrange
            IEntryRepository entryRepository = new EntryRepository();
            Entry newEntry = new Entry
            {
                CheckInTime = DateTime.Now,
                CustomerId = Customers.FirstOrDefault().Id,
                Customer = Customers.FirstOrDefault()
            };

            // Act
            entryRepository.Add(newEntry);
            entryRepository.Remove(newEntry);
            Entry deletedEntry = entryRepository.GetById(newEntry.Id);

            // Assert
            Assert.IsNull(deletedEntry);
        }

        [TestMethod]
        public void GetAllEntries_Test()
        {
            // Arrange
            IEntryRepository entryRepository = new EntryRepository();
            Entry entry1 = new Entry
            {
                CheckInTime = DateTime.Now,
                CustomerId = Customers.FirstOrDefault().Id,
                Customer = Customers.FirstOrDefault()
            };

            Entry entry2 = new Entry
            {
                CheckInTime = DateTime.Now,
                CustomerId = Customers.FirstOrDefault().Id,
                Customer = Customers.FirstOrDefault()
            };

            // Act
            entryRepository.Add(entry1);
            entryRepository.Add(entry2);
            IEnumerable<Entry> allEntries = entryRepository.GetAll();

            // Assert
            Assert.IsTrue(allEntries.Any(e => e.CustomerId == entry1.CustomerId));
            Assert.IsTrue(allEntries.Any(e => e.CustomerId == entry2.CustomerId));

            entryRepository.Remove(entry1);
            entryRepository.Remove(entry2);
        }
    }
}
