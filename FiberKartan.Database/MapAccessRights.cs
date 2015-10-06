using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiberKartan.Database
{
    /// <summary>
    /// Vilka tillträdesnivåer som finns för en karta.
    /// </summary>
    [Flags]
    public enum MapAccessRights : int
    {
        None = 0,
        Read = 1,   // Kan granska en karta och alla dess versioner.
        Export = 2, // Kan exportera en karta och alla dess versioner.
        Write = 4,  // Kan modifiera en karta och skapa nya versioner.
        FullAccess = 8  // Fulla rättigheter, kan även bjuda in och ge rättigheter till andra användare.
    }
}
