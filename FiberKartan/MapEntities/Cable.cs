using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/*
The zlib/libpng License
Copyright (c) 2012 Henrik Östman

This software is provided 'as-is', without any express or implied warranty. In no event will the authors be held liable for any damages arising from the use of this software.
Permission is granted to anyone to use this software for any purpose, including commercial applications, and to alter it and redistribute it freely, subject to the following restrictions:

1. The origin of this software must not be misrepresented; you must not claim that you wrote the original software. If you use this software in a product, an acknowledgment in the product documentation would be appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/
namespace FiberKartan.MapEntities
{
    public class Cable
    {
        /// <summary>
        /// Unikt Id, är olika mellan linjerna MEN ÄVEN mellan versionerna. 
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Unikt Id, är olika mellan linjerna MEN SAMMA mellan versionerna. 
        /// </summary>
        public int Uid { get; set; }

        /// <summary>
        /// Namn på linjen. Behöver inte vara unikt.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Färg på linjen.
        /// </summary>
        public string LineColor { get; set; }

        /// <summary>
        /// Bredd på linjen.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// En sammansatt sträng som innehåller linjens samtliga punkter.
        /// </summary>
        public string Coordinates { get; set; }

        /// <summary>
        /// Linjens typ.
        /// </summary>
        public int Type { get; set; }
    }
}