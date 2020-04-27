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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketAppServer.Security
{
    public sealed class ServerUser
    {
        public ServerUser(string identifier, string name, 
            string email, string organization)
        {
            Identifier = identifier;
            Name = name;
            Email = email;
            Organization = organization;
            Roles = new List<UserRole>();
        }

        public string Identifier { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string Organization { get; private set; }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Organization))
                return Name;
            else
                return $"{Name}@{Organization}";
        }

        internal List<UserRole> Roles { get; set; }

        internal bool IsRolesAccessControllerEnabled
        {
            get
            {
                return Roles.Count > 0;
            }
        }

        public void AddRole(string controller, string action, bool enableAccess)
        {
            if (string.IsNullOrEmpty(controller))
                throw new Exception("It is mandatory to enter a name of a valid Controller on the server");
            if (string.IsNullOrEmpty(action))
                throw new Exception("It is not allowed to use empty strings for Action. If your intention is to cover all Controller Actions, enter the character '*' instead of an empty string");

            Roles.Add(new UserRole(controller, action, enableAccess));
        }
    }
}
