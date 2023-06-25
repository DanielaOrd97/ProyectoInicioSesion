using Microsoft.EntityFrameworkCore;
using Sistemas.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;

namespace Sistemas.Catalagos
{
    public class UsuariosCatalago
    {

        SistemasContext contenedor = new();

        public IEnumerable<Usuarios> GetAllUsuarios()
        {
            return contenedor.Usuarios.OrderBy(x => x.Nombre);
        }

        public Usuarios? GetIdUsuario(int id)
        {
            return contenedor.Usuarios.Find(id);
        }

        public void Recargar(Usuarios u)
        {
            contenedor.Entry(u).Reload();
        }
        public Usuarios? GetUsuarioConectado(string correo)
        {
            return contenedor.Usuarios.Include(x => x.Bitacoras.OrderBy(x => x.Fecha)).FirstOrDefault(x => x.Correo == correo);
        }


        public int spIniciarSesion(string correo, string password)
        {
            ////El parametro de correo y password es para indicar que parte de la cadena es el correo y que parte es el password.
            ////Asincrona = mucha carga de datos. Para que no se bloquee.
            ////Sincrona = IEnumerable.

            string cadena = $"select fnIniciarSesion('{correo}','{password}')";

            var y = ((IEnumerable<int>)contenedor.Database.SqlQueryRaw<int>(cadena, correo, password).
                AsAsyncEnumerable<int>()).First();

            if (y == 1)
            {
                //Busca en la tabla usuarios y se trae al tipo de usuario(Rol).
                //var us = contenedor.Usuarios.
                //    Include(x => x.IdRolNavegation).
                //    FirstOrDefault(x => x.Correo == correo);

                var us = contenedor.Usuarios.
                    Include(x => x.IdRolNavigation).
                    FirstOrDefault(x => x.Correo == correo);

                if (us != null)
                {
                    EstablecerTipoUsuario(us);
                    //entra como parametro el tipo de usuario encontrado.
                }

                //1:Credenciales correctas y dice quien es el usuario identificado con rol estblecido.

            }


            return y;

           

        }

        private void EstablecerTipoUsuario(Usuarios u)  //El hilo esta corriendo como usuario u.
        {
            GenericIdentity user = new GenericIdentity(u.Nombre);  //Es el usuario identificado, lo toma a partir del nombre.
            if(u != null)
            {
                string[] roles = new string[] { u.IdRolNavigation.Nombre };  //Necesita la lista de roles, por eso el array.
                GenericPrincipal usprincipal = new GenericPrincipal(user, roles);  //Es el usuario en si, con nombre y rol.
                Thread.CurrentPrincipal = usprincipal;  //Este es el usuario que corre en este momento.
            }
        }

        public void Agregar(Usuarios u)
        {
            //contenedor.Add(u);
            contenedor.Usuarios.Add(u);
            contenedor.SaveChanges();
        }

        public void Eliminar(Usuarios u)
        {
            contenedor.Usuarios.Remove(u);
            contenedor.SaveChanges();
        }

        public void Editar(Usuarios u)
        {
            contenedor.Usuarios.Update(u);
            contenedor.SaveChanges();
        }

        public bool Validar(Usuarios? u, out List<string>Errores)
        {
            Errores = new List<string>();

            if(u != null)
            {
                string patronNombre = @"[a-zA-z]";
                Regex regular = new Regex(patronNombre);

                string patronCorreo = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

                if (string.IsNullOrWhiteSpace(u.Nombre))
                {
                    Errores.Add("Ingrese el nombre del usuario.");
                }
                else if(regular.IsMatch(u.Nombre) == false)
                {
                    Errores.Add("El nombre del usuario debe estar en el formato correcto.");
                }
                if (string.IsNullOrWhiteSpace(u.Correo))
                {
                    Errores.Add("Ingrese el correo correspondiente.");
                }
                else if(regular.IsMatch(patronCorreo) == false)
                {
                    Errores.Add("Escriba el correo electronico en el formato correcto.");
                }
                if (string.IsNullOrWhiteSpace(u.Contrasena))
                {
                    Errores.Add("Escriba la contraseña.");
                }
                if (contenedor.Usuarios.Any(x => x.Correo == u.Correo && x.Id != u.Id))
                {
                    Errores.Add("El correo electrónico ya se encuentra registrado.");
                }
                if (u.IdRol == 0 || u.IdRol == null)
                {
                    Errores.Add("Indique que tipo de rol desempeñara.");
                }
            }

            return Errores.Count == 0;
        }

    }
}
