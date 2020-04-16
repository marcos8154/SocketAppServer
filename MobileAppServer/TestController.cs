/*
MIT License

Copyright (c) 2020 Marcos Vinícius

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using SocketAppServer.CoreServices;
using SocketAppServer.ManagedServices;
using SocketAppServer.ServerObjects;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketAppServer
{
    internal class TestController : IController
    {
        private SqlConnection GetConnection()
        {
            SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder();
            sb.DataSource = "localhost";
            sb.UserID = "sa";
            sb.Password = "81547686";
            sb.InitialCatalog = "sas_tests";

            SqlConnection conn = new SqlConnection(sb.ConnectionString);
            conn.Open();
            return conn;
        }

        public ActionResult SimpleAction(SocketRequest request)
        {
            return ActionResult.Json(new OperationResult(true, 600, $""));
        }

        public ActionResult SaveEntity(Entity entity, Address address,
            List<Entity> entities,
            bool active, string alias,
            string anotherParameter)
        {
            string sqlEntity = $@"insert into Entities(Name, Phone) values ('{entity.Name}', '{entity.Phone}')";
            string sqlAddress = $"insert into Addresses(Street, City) values ('{address.Street}', '{address.City}')";

            using (SqlConnection conn = GetConnection())
            {
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    using (SqlCommand cmdEntity = new SqlCommand(sqlEntity, conn, tx))
                        cmdEntity.ExecuteNonQuery();

                    using (SqlCommand cmdAddress = new SqlCommand(sqlAddress, conn, tx))
                        cmdAddress.ExecuteNonQuery();

                    tx.Commit();
                }

                conn.Close();
            }

            return ActionResult.Json(new OperationResult(new { Entity = entity, EntityAddress = address }, 600, "Entity Saved.")); ;
        }
    }

    internal class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
    }

    internal class Entity
    {
        public string Name { get; set; }

        public string Phone { get; set; }
    }
}
