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

using SocketAppServer.ServerObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SocketAppServer.Security
{
    internal class TokenRepository
    {
        private static TokenRepository _instance;

        public static TokenRepository Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new TokenRepository();
                return _instance;
            }
        }

        public TokenRepository()
        {
            Tokens = new List<ServerToken>();
        }

        internal List<ServerToken> Tokens { get; set; }

        public bool IsValid(string token, ref SocketRequest request)
        {
            ServerToken serverToken = Tokens.FirstOrDefault(t => t.UserToken.Equals(token));
            if (serverToken == null)
                return false;

            if (serverToken.RemoteIP != request.RemoteEndPoint.Address.ToString())
                return false;

            if (serverToken.HasExpired())
            {
                Tokens.Remove(serverToken);
                return false;
            }

            return true;
        }

        public ServerToken GetToken(string token)
        {
            return Tokens.FirstOrDefault(t => t.UserToken.Equals(token));
        }

        public ServerToken AddToken(ServerUser user,
           ref SocketRequest request)
        {
            ServerToken token = new ServerToken(user, ref request);
            Tokens.Add(token);
            return token;
        }

        internal ServerToken GetToken(Guid sessionId)
        {
            return Tokens.FirstOrDefault(token => token.SessionId.Equals(sessionId));
        }
    }
}
