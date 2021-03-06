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

namespace SocketAppServer.CoreServices
{
    /// <summary>
    /// Obtains basic hardware information about the current server process
    /// </summary>
    public interface IHardwareServices
    {
        /// <summary>
        /// Gets the average % CPU usage in the last X hours
        /// </summary>
        /// <param name="lastMinutes"></param>
        /// <returns></returns>
        double AverageCPUUsage(double lastMinutes = 3);

        /// <summary>
        /// Gets the average MB memory usage in the last X hours
        /// </summary>
        /// <param name="lastMinutes"></param>
        /// <returns></returns>
        double AverageMemoryUsage(double lastMinutes = 3);

        /// <summary>
        /// Waits for current processes to end on the server to force 
        /// immediate freeing of memory in the current server process
        /// </summary>
        void ReleaseMemory();
    }
}
