﻿using System;
using System.Collections.Generic;
using System.Data;
using KK.Models.Entities;
using KK.Models.Entities.Enum;
using KK.Models.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace KK.Models.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly string _connectionString;

        public CustomerRepository()
        {
            IConfigurationRoot config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            _connectionString = config.GetConnectionString("MyDBConnection");
        }

        // CRUD Methods
        public void Add(Customer entity)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("INSERT INTO kk_CUSTOMER (Name, DateOfBirth, Phone, Email, Qualification, HasSignedDisclaimer) VALUES (@Name, @DateOfBirth, @Phone, @Email, @Qualification, @HasSignedDisclaimer); SELECT SCOPE_IDENTITY();", connection))
                {
                    command.Parameters.AddWithValue("@Name", entity.Name);
                    command.Parameters.AddWithValue("@DateOfBirth", entity.DateOfBirth);
                    command.Parameters.AddWithValue("@Phone", entity.Phone ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Email", entity.Email ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Qualification", (int)entity.Qualification);
                    command.Parameters.AddWithValue("@HasSignedDisclaimer", entity.HasSignedDisclaimer);

                    // Execute the SQL command and get the inserted ID
                    entity.Id = Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public IEnumerable<Customer> GetAll()
        {
            List<Customer> customers = new List<Customer>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT * FROM kk_CUSTOMER", connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            customers.Add(MapCustomer(reader));
                        }
                    }
                }
            }

            return customers;
        }

        public Customer GetById(int id)
        {
            Customer customer = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT * FROM kk_CUSTOMER WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            customer = MapCustomer(reader);
                        }
                    }
                }
            }

            return customer;
        }

        public void Remove(Customer entity)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("DELETE FROM kk_CUSTOMER WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", entity.Id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Update(Customer entity)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("UPDATE kk_CUSTOMER SET Name = @Name, DateOfBirth = @DateOfBirth, Phone = @Phone, Email = @Email, Qualification = @Qualification, HasSignedDisclaimer = @HasSignedDisclaimer WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", entity.Id);
                    command.Parameters.AddWithValue("@Name", entity.Name);
                    command.Parameters.AddWithValue("@DateOfBirth", entity.DateOfBirth);
                    command.Parameters.AddWithValue("@Phone", entity.Phone ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Email", entity.Email ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Qualification", (int)entity.Qualification);
                    command.Parameters.AddWithValue("@HasSignedDisclaimer", entity.HasSignedDisclaimer);

                    command.ExecuteNonQuery();
                }
            }
        }

        // Get Customer and all the coresponding tables in the database usses mapping tables to set the data
        public Customer GetCustomer(int customerId)
        {
            Customer customer = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("kk_spGetCustomerDetailsById", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CustomerId", customerId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            customer = MapDataToCustomer(reader);
                        }
                    }
                }
            }
            return customer;
        }


        // Mapping Profiles
        private static Customer MapCustomer(SqlDataReader reader)
        {
            return new Customer
            {
                Id = Convert.ToInt32(reader["Id"]),
                Name = reader["Name"].ToString(),
                DateOfBirth = Convert.ToDateTime(reader["DateOfBirth"]),
                Phone = reader["Phone"] is DBNull ? null : reader["Phone"].ToString(),
                Email = reader["Email"] is DBNull ? null : reader["Email"].ToString(),
                Qualification = (Qualification)Convert.ToInt32(reader["Qualification"]),
                HasSignedDisclaimer = Convert.ToBoolean(reader["HasSignedDisclaimer"])
            };
        }

        private static Customer MapDataToCustomer(SqlDataReader reader)
        {
            Customer customer = new Customer
            {
                Id = Convert.ToInt32(reader["CustomerId"]),
                Name = Convert.ToString(reader["CustomerName"]),
                DateOfBirth = Convert.ToDateTime(reader["DateOfBirth"]),
                Phone = reader["Phone"] as string,
                Email = reader["Email"] as string,
                HasSignedDisclaimer = Convert.ToBoolean(reader["HasSignedDisclaimer"]),
                Qualification = (Qualification)Enum.Parse(typeof(Qualification), reader["Qualifications"].ToString()),
                Membership = MapDataToMembership(reader),
                Entries = MapDataToEntries(reader)
            };

            return customer;
        }

        private static Membership MapDataToMembership(SqlDataReader reader)
        {
            if (reader["MembershipId"] == DBNull.Value)
            {
                return null;
            }

            Membership membership = new Membership
            {
                Id = Convert.ToInt32(reader["MembershipId"]),
                StartDate = Convert.ToDateTime(reader["MembershipStartDate"]),
                EndDate = Convert.ToDateTime(reader["MembershipEndDate"]),
                IsActive = Convert.ToBoolean(reader["MembershipIsActive"])
            };

            return membership;
        }

        private static ICollection<Entry> MapDataToEntries(SqlDataReader reader)
        {
            List<Entry> entries = new List<Entry>();

            do
            {
                Entry entry = new Entry
                {
                    Id = Convert.ToInt32(reader["EntryId"]),
                    CheckInTime = Convert.ToDateTime(reader["EntryCheckInTime"]),
                    Price = Convert.ToDecimal(reader["EntryPrice"]),
                    CustomerId = Convert.ToInt32(reader["CustomerId"]),
                    Items = MapDataToServiceItems(reader)
                };

                entries.Add(entry);

            } while (reader.Read() && Convert.ToInt32(reader["CustomerId"]) == entries[0].CustomerId);

            return entries;
        }

        private static ICollection<ServiceItem> MapDataToServiceItems(SqlDataReader reader)
        {
            List<ServiceItem> serviceItems = new List<ServiceItem>();

            do
            {
                ServiceItem serviceItem = new ServiceItem
                {
                    Id = Convert.ToInt32(reader["ServiceItemId"]),
                    Name = Convert.ToString(reader["ServiceItemName"]),
                    EntryId = Convert.ToInt32(reader["EntryId"])
                };

                serviceItems.Add(serviceItem);

            } while (reader.Read() && Convert.ToInt32(reader["EntryId"]) == serviceItems[0].EntryId);

            return serviceItems;
        }
    }
}

