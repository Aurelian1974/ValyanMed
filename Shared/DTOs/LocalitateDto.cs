using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs;

public class LocalitateDto
{
    public int IdOras { get; set; }
    public Guid LocalitateGuid { get; set; }
    public int IdJudet { get; set; }
    public string Nume { get; set; }
    public int Siruta { get; set; }
}
