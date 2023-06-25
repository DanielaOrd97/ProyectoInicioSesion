using GalaSoft.MvvmLight.Command;
using Sistemas.Views;
using Sistemas.Catalagos;
using Sistemas.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO.Packaging;
using System.Net.Mail;
using System.Xml.Serialization;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using System.Collections.ObjectModel;

namespace Sistemas.ViewModels
{
    public class PrincipalViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Roles> ListaRoles { get; set; } = new();
        //Propiedades largas cuando se requieran validaciones en el get. Estan governadas por el campo.
        public Usuarios? usuario { get; set; }
        public Roles? Rol { get; set; }
        public UsuariosCatalago catalagousuarios = new();
        public RolesCatalago catalagoroles = new();
         UsuariosViewModel usuariosviewm = new();

        //NO
        public RolesCatalago catalogoroles = new();
        public string Error { get; set; }
        public string Modo { get; set; }
        public bool EstaConectado => usuario.Id != 0;

        public UserControl Vista { get; set; }

        LoginView view;

        public ICommand CerrarSesionCommand { get; set; }
        public ICommand IniciarSesionCommand { get; set; }
        public ICommand RegistrarUsuarioCommand { get; set; }
        public ICommand VerRegistrarCommand { get; set; }
        public ICommand RegresarCommand { get; set; }

        public PrincipalViewModel()
        {
            // catalogoroles.GetAllRoles();
            // usuariosviewm.ActualizarRoles();
            VerRegistrarCommand = new RelayCommand(VerRegistrarse);
            CerrarSesionCommand = new RelayCommand(CerrarSesion);
            IniciarSesionCommand = new RelayCommand(IniciarSesion);
            RegistrarUsuarioCommand = new RelayCommand(Registrar);
            RegresarCommand = new RelayCommand(Regresar);
            usuario = new();
            view = new LoginView()
            {
                DataContext = this    ///Referencia para ingresar los datos del usuario.
            };
            Vista = view;  //Se le asigna el valor de view a Vista.

            Notificar();
        }

        Random r = new();
        private void Registrar()
        {
           if(usuario != null)
            {

                if(catalagousuarios.Validar(usuario, out List<string> Errores))
                {
                    //    if (usuario.Id != 0)
                    //    {
                    //        //Esta dentro de la coleccion
                    //        Error = "Este usuario ya se encuentra registrado.";
                    //    }
                    usuario.IdRol = r.Next(1, 3);
                    catalagousuarios.Agregar(usuario);
                    Correo();
                    usuariosviewm.ActualizarBD();
                    Modo = "login";
                    Notificar();
                }
                else
                {
                    foreach (var item in Errores)
                    {
                        Error = $"{Error}{item}{Environment.NewLine}";
                    }
                    Notificar();
                }
                Error = "";
            }
        }

        public void CargarListaRoles()
        {
            ListaRoles.Clear();

            foreach (var i in catalagoroles.GetAllRoles())
            {
                ListaRoles.Add(i);
            }
            Notificar();
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

        private void Regresar()
        {
            Modo = "login";
            Notificar();
        }

        private void VerRegistrarse()
        {
            usuario = new();
            CargarListaRoles();
            Modo = "Registrar";
            Notificar();
        }

        private void IniciarSesion()
        {
            if (usuario != null)
            {
             //   if (catalagousuarios.Validar(usuario, out List<string> Errores))
              //  {

                    var inicio = catalagousuarios.spIniciarSesion(usuario.Correo, usuario.Contrasena);

                    if (inicio == 1)
                    {
                        //Existe el usuario, ahora lo buscamos en la bd para establecer que el es el conectado.
                        var usuarioconectado = catalagousuarios.GetUsuarioConectado(usuario.Correo);
                        usuario = usuarioconectado;

                    if(Thread.CurrentPrincipal != null)
                    {
                        if (Thread.CurrentPrincipal.IsInRole("Administrador"))
                        {
                            AccionesUsuarioAdmin();
                        }
                        if (Thread.CurrentPrincipal.IsInRole("Capturista"))
                        {
                            AccionesUsuarioCapturista();
                        }
                    }



                      //  Vista = new UsuariosView();  ////????
                        Notificar();
                    }
                    else if(inicio == 2)
                    {
                        Error = "Usuario no encontrado.";
                    }
                    else
                    {
                        Error = "Contraseña incorrecta.";
                    }
                    Notificar();
                Error = "";
              //  }
            }

            //Validar que la cajas de texto tengan datos.
            //Validar que el correo sea valido (regex).

        }

        [Authorize(Roles = "Administrador")]   //Indica que el metodo solo puede ser ejecutado si es un rol en especifico.
                                               //Es un atributo.
        private void AccionesUsuarioAdmin()
        {
            Vista = new UsuariosView();  //Acceso a todos los registros.
        }

        [Authorize(Roles = "Capturista")]
        private void AccionesUsuarioCapturista()
        {
            Vista = new BienvenidoView();  //Solo a la vista de bienvenida.
        }

        private void CerrarSesion()
        {
            usuario = new();
            Vista = view;   //Se regresa a la ventana de log in.
            Notificar();

        }

        //private bool estaconectado;

        //public bool EstaConectado1
        //{
        //    get { return usuario.Id != 0; }
        //} --------------- El lambda sustituye esta propiedad larga.



        void Notificar(string? propiedad = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propiedad));
        }



        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
