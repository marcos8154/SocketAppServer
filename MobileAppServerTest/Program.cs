using SocketAppServerClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MobileAppServerTest
{
    static class Program
    {
        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread]
        static void Main()
        {
            SocketConnectionFactory.SetDefaultSettings(new SocketClientSettings
            (
                "localhost",
                2500,
                Encoding.UTF8
            ));

            using (ISocketClientConnection connection = SocketConnectionFactory.GetConnection())
            {
                connection.SendRequest("controller", "action", new
                {
                    nome = "XPTO",
                    idade = 39,
                    outroParametroQualquer = Guid.NewGuid()
                });

                var response = connection.ReadResponse();
            }










            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        static void Xpto(object obj)
        {
            var props = obj.GetType().GetProperties();
        }
    }
}
