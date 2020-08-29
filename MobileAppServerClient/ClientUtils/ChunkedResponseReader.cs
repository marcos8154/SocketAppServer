using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketAppServerClient
{
    internal class ChunkedResponseReader
    {
        public string ResponseId { get; }
        public int Length { get; }
        public Client Client { get; }
        private StringBuilder ResponseStorage { get; set; }

        public ChunkedResponseReader(string responseId, int length,
            Client client)
        {
            ResponseId = responseId;
            Length = length;
            Client = client;
            ResponseStorage = new StringBuilder(length * 10);
        }

        private void ReadToEnd()
        {
            try
            {
                bool canRead = true;

                while (canRead)
                {
                    Client c = new Client(Client.Server,
                        Client.Port, Client.Encoding, Client.BufferSize);

                    RequestBody rb = RequestBody.Create("ResponseStorageController",
                        "ReadContent")
                        .AddParameter("storageId", ResponseId)
                        .AddParameter("length", Length);
                    c.SendRequest(rb);

                    OperationResult result = c.GetResult();
                    if (result.Entity == null)
                        canRead = false;

                    if (canRead)
                        ResponseStorage.Append(result.Entity);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Reading of the partitioned response was interrupted for the following reason: {ex.Message}");
            }
        }

        public OperationResult GetResult()
        {
            ReadToEnd();
            using (StringReader sr = new StringReader(ResponseStorage.ToString()))
            {
                using (JsonReader jr = new JsonTextReader(sr))
                {
                    JsonSerializer js = new JsonSerializer();
                    OperationResult entityResult = js.Deserialize<OperationResult>(jr);
                    return entityResult;
                }
            }
        }
    }
}
