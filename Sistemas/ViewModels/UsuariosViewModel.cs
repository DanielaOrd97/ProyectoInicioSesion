using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Sistemas.Views;
using Sistemas;
using Sistemas.Catalagos;
using Sistemas.Models;
using System.Net.Mail;
using System.IO;

namespace Sistemas.ViewModels
{
    public class UsuariosViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Usuarios> Usuarios1 { get; set; } = new ObservableCollection<Usuarios>();
        public ObservableCollection<Roles> ListaRoles { get; set; } = new ObservableCollection<Roles>();
        public Usuarios? usuario { get; set; }
        public Roles? rol { get; set; }

        public string? Error { get; set; }

        public UsuariosCatalago catalagousuario = new();
        public RolesCatalago rolescatalogo = new(); 
        public string Modo { get; set; }

        public ICommand AgregarCommand { get; set; }
        public ICommand EliminarCommand { get; set; }
        public ICommand AceptarCommand { get; set;}
        public ICommand EditarCommand { get; set; }
        public ICommand GuardarCommand { get; set; }
        public ICommand CancelarCommand { get; set; }
        public ICommand VerBitacorasCommand { get; set; }
        public ICommand RegresarUsuariosCommand { get; set; }

        //Aceptar Command para confirmar.
        public UsuariosViewModel()
        {
           // CargarListaRoles();
            ActualizarBD();
            Modo = "Ver";
            AgregarCommand = new RelayCommand(AgregarUsuario);
            EliminarCommand = new RelayCommand<Usuarios>(EliminarUsuario);
            AceptarCommand = new RelayCommand(Aceptar);
            EditarCommand = new RelayCommand<Usuarios>(EditarUsuario);
            GuardarCommand = new RelayCommand(GuardarUsuario);
            CancelarCommand = new RelayCommand(CancelarUsuario);
            RegresarUsuariosCommand = new RelayCommand(Regresar);
            VerBitacorasCommand = new RelayCommand<Usuarios>(VerBitacoras);
        }

        //private void CambiarContra(string[] entradas)
        //{
        //    string contra1 = entradas[0];
        //    string contra2 = entradas[1];

        //    if(contra1 == contra1)
        //    {
        //        catalagousuario.CambiarContrasena(usuario);
        //        ActualizarBD();
        //        Notificar();
        //    }
        //    else
        //    {
        //        Error = "Las contrasenas no coinciden";
        //        Notificar();
        //    }

        //}

        private void VerCambiar()
        {
            Modo = "Cambio";
            Notificar();
        }

        private void VerBitacoras(Usuarios u)
        {
            // usuario = catalagousuario.GetIdUsuario(id);
            if(u != null)
            {
                usuario = catalagousuario.GetUsuarioConectado(u.Correo);
                Modo = "VerBitacoras";
                Notificar();
            }
        }

        private void Regresar()
        {
            Modo = "Ver";
            Notificar();
        }

        private void Aceptar()
        {
            if(usuario != null)
            {
                catalagousuario.Eliminar(usuario);
                ActualizarBD();
                Modo = "Ver";
                Notificar();
            }
        }

        private void EliminarUsuario(Usuarios u)
        {
            if(u != null)
            {
                Error = "";
                usuario = u; //Misma referencia
                Modo = "Eliminar";
                Notificar();
            }
        }

        public void ActualizarBD()
        {
           Usuarios1.Clear();

          foreach (var i in catalagousuario.GetAllUsuarios())
          {
            Usuarios1.Add(i);
          }
          Notificar();
            
        }

        public void CargarListaRoles()
        {
            ListaRoles.Clear();

            foreach (var i in rolescatalogo.GetAllRoles())
            {
                ListaRoles.Add(i);
            }
            Notificar();
        }
        private void EditarUsuario(Usuarios u)
        {
            
            if(u != null)
            {
                Error = "";
                usuario = u;
                Modo = "Editar";
                Notificar();
            }
           

        }

        private void AgregarUsuario()
        {
            usuario = new();
            CargarListaRoles();
            Modo = "Agregar";
            Error = "";
            Notificar();
        }

        private void GuardarUsuario()
        {
            /*
             - Controlar las excepciones y mostrarlas en la varible error. (Enviroment)
             - Nombre y Contrasena no deben de estar vacios.
             - Que correo no se duplique.
             - El formato del correo (hotmail, gmail, yahoo, ITESRC).
             - Formato de la contrasena.
             - Validar la longitud de la contrasena
             */

            Random r = new();

            if(usuario != null)
            {
                Error = "";

                if(catalagousuario.Validar(usuario, out List<string> errores))
                {
                    if (usuario.Id != 0)
                    {
                        //Esta dentro de la coleccion
                        catalagousuario.Editar(usuario);
                    }
                    else
                    {
                        //Apenas estoy agregando (Id = 0).
                        //usuario.IdRol = r.Next(1, 2); 
                        usuario.IdRol = r.Next(1, 3);
                        catalagousuario.Agregar(usuario);
                    }
                    Correo();
                    ActualizarBD();
                    Modo = "Ver";
                    Notificar();

                }
                else
                {
                    foreach (var i in errores)
                    {
                        Error = $"{Error}{i}{Environment.NewLine}";
                    }
                    Notificar();
                }


                //if (usuario.Id != 0)
                //{
                //    //Esta dentro de la coleccion
                //}
                //else
                //{
                //    //Apenas estoy agregando (Id = 0).
                //    catalagousuario.Agregar(usuario);
                //    ActualizarBD();
                //    Modo = "Ver";
                //    Notificar();
                    
                //}
            }
        }

        public void Correo()
        {

            try
            {

                MailMessage mail = new()
                {
                    //From es de quien se envia el correo
                    From = new MailAddress("201G0272@rcarbonifera.tecnm.mx", "Registro sistema de usuarios."),
                    Subject = "Bienvenido al sistema de registro de usuarios."
                };
                mail.IsBodyHtml = true;
                //  mail.Body = "Esta es una prueba.";
                // string html = File.ReadAllText("C:\\Users\\danie\\Documents\\ProyectoSistemas\\Sistemas\\Views\\pag.html");
                string html = File.ReadAllText("C:\\Users\\danie\\Documents\\ProyectoSistemas\\Sistemas\\Views\\CuerpoCorre.html");
                mail.Body = html;
                // mail.Bcc.Add("iamdaniordz@gmail.com");  //Define a quien se lo enviara.
                mail.Bcc.Add(usuario.Correo);
                SmtpClient client = new SmtpClient("Smtp.outlook.office365.com");   //Servidor de protocolo de comunicacion entre servidor y....
                client.Port = 587;
                client.EnableSsl = true;   //Por seguridad, da la contrasena.
                client.UseDefaultCredentials = false;
                System.Net.NetworkCredential cred = new System.Net.NetworkCredential("201G0272@rcarbonifera.tecnm.mx", "soyALUMNAnueva2002");
                client.Credentials = cred;
                client.Send(mail);
            }
            catch (Exception e)
            {
                Error = "Ha ocurrido un error inseperado.";
            }
            // Asi para html y ancla: \""
        }


        private void CancelarUsuario()
        {
            if(usuario != null)
            {
                catalagousuario.Recargar(usuario);
                Modo = "Ver";
                Notificar();
            }
           

        }



        //Registro de cuenta

         public void Notificar(string? propiedad = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propiedad));
        }


        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
