using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs
{
    public class JudetDto
    {
        public int IdJudet { get; set; }
        public string Nume { get; set; } = string.Empty;
        public string CodAuto { get; set; } = string.Empty;
    }
}
