using System;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using Signum.Entities.Authorization;
using Signum.Services;
using Signum.Utilities;
using Signum.Windows;
using Signum.Windows.Authorization;
using Music.Windows.Properties;
using Music.Test;

namespace Music.Windows
{
    public class Program
    {


        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                ThemeManager.ChangeTheme(ThemeManager.AeroNormalcolor);

                Server.SetNewServerCallback(NewServer);

                Server.Connect();

                App.StartApplication();

                App app = new App() { ShutdownMode = ShutdownMode.OnMainWindowClose };

                app.Run(new Main());
            }
            catch (Exception e)
            {
                HandleException("Start-up error", e, null);
            }
            finally
            {
                Server.Disconnect();
            }
        }

        public static event Action<string, Exception> OverrideExceptionHandling;

        public static void HandleException(string errorTitle, Exception e, Window win)
        {
            if (OverrideExceptionHandling != null)
                OverrideExceptionHandling(errorTitle, e);
            else
            {
                string message = e.Follow(ex => ex.InnerException).ToString(ex => "{0} : {1}".Formato(
                          ex.GetType().Name != "FaultException" ? ex.GetType().Name : "Server Error",
                          ex.Message), "\r\n\r\n");

                if (win != null)
                    MessageBox.Show(win, message, errorTitle + ":", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    MessageBox.Show(message, errorTitle + ":", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        static ChannelFactory<IServerSample> channelFactory;

        public static IBaseServer NewServer()
        {
            if (channelFactory == null)
                channelFactory = new ChannelFactory<IServerSample>("server");

            IServerSample result = channelFactory.CreateChannel();
            string auto = Settings.Default.Autologin;
            if (auto.HasText())
            {
                string[] usernamePassword = auto.Split('/');
                result.Login(usernamePassword[0], Security.EncodePassword(usernamePassword[1]));
                UserDN.Current = result.GetCurrentUser();

                return result;
            }

            if (Application.Current == null || Application.Current.CheckAccess())
                return Login(result);
            else
                return Application.Current.Dispatcher.Return(() => Login(result));       
        }

        private static IServerSample Login(IServerSample result)
        {
            Login login = new Login
            {
                Title = "Login on Music Database",
                UserName = Settings.Default.Autologin,
                Password = "",
                ProductName = "Music Database",
                CompanyName = "Signum Software"
            };

            login.LoginClicked += (o, e) =>
            {
                try
                {
                    result.Login(login.UserName, Security.EncodePassword(login.Password));
                    Settings.Default.UserName = login.UserName;
                    Settings.Default.Save();

                    login.DialogResult = true;
                }
                catch (FaultException ex)
                {
                    login.Error = ex.Message;

                    if (ex.Message == "El usuario no existe")
                    {
                        login.FocusUserName();
                    }

                    if (ex.Message == "Password incorrecto")
                    {
                        login.FocusPassword();
                    }
                }
            };

            login.FocusUserName();

            bool? dialogResult = login.ShowDialog();
            if (dialogResult == true)
            {
                UserDN.Current = result.GetCurrentUser();

                return result;
            }

            return null;
        }
    }
}
