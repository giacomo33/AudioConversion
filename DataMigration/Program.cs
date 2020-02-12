using System;

namespace DataMigration
{
    class Program
    {
        static void Main(string[] args)
        {
            // Copy all data from the existing monolithic test database table(s) to this microservices isolated database.
            Console.WriteLine("Reading existing database");


            // Read this!
            // https://robertheaton.com/2015/08/31/migrating-bajillions-of-database-records-at-stripe/

            Console.WriteLine("Writing to new database");

        }
    }
}
