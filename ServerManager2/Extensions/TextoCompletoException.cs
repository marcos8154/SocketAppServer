using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerManager2.Shared
{
    public static class TextoCompletoException
    {
        public static string TextoCompleto(this Exception ex)
        {
            string msg = ex.Message;
            if (ex.InnerException != null)
            {
                msg += $"\n{ex.InnerException}";
                if (ex.InnerException.InnerException != null)
                {
                    msg += $"\n{ex.InnerException.InnerException}";

                    if (ex.InnerException.InnerException.InnerException != null)
                        msg += $"\n{ex.InnerException.InnerException.InnerException}";
                }
            }

            return msg;
        }
    }
}
