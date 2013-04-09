﻿namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// The behavior used to control how the database is handled for test fixtures inheriting from BaseDatabaseFactoryTest
    /// </summary>
    public enum DatabaseBehavior
    {
        /// <summary>
        /// A database is not required whatsoever for the fixture
        /// </summary>
        NoDatabasePerFixture,

        /// <summary>
        /// For each test a new database file and schema will be created
        /// </summary>
        NewDbFileAndSchemaPerTest,
        
        /// <summary>
        /// For each test a new schema will be created on the existing database file
        /// </summary>
        NewSchemaPerTest,

        /// <summary>
        /// Creates a new database file and schema for the whole fixture, each test will use the pre-existing one
        /// </summary>
        NewDbFileAndSchemaPerFixture,

        /// <summary>
        /// Creates a new schema based on the existing database file for the whole fixture, each test will use the pre-existing one
        /// </summary>
        NewSchemaPerFixture,
    }
}