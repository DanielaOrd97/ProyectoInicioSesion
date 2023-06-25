using System;
using System.Collections.Generic;

namespace Sistemas.Models;

public partial class Usuarios
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string Contrasena { get; set; } = null!;

    public int IdRol { get; set; }

    public virtual ICollection<Bitacoras> Bitacoras { get; set; } = new List<Bitacoras>();

    public virtual Roles IdRolNavigation { get; set; } = null!;
}
