using Microsoft.EntityFrameworkCore;
using Sistemas.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistemas.Catalagos
{
    public class RolesCatalago
    {
        SistemasContext contenedor = new();


        public IEnumerable<Roles> GetAllRoles()
        {
            return contenedor.Roles.OrderBy(x => x.Id);
        }

        //GetAllRoles
        //Al agregar usuario, utilizar comando GuardarCommand, que implementa el metodo
        // Guardar.
        // Si es 0 lo crea, si es diferente, edita.
        //Invocamos al metodo create y se le pasa como parametro el usuario, va a generar exepcion.
        //Se le manda un 1 o un 2.


        //En viewModel
        //Usuario.IdRol = r.Next(1,3)


    }
}
