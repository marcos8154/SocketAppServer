﻿using SocketAppServer.ServerObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetFxTestServer
{
    public class ImageController : IController
    {
        [ServerAction]
        public void Upload(int produtoId, string base64Image)
        {

        }
    }
}
