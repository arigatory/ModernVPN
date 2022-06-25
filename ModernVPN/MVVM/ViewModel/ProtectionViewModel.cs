using ModernVPN.Core;
using ModernVPN.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ModernVPN.MVVM.ViewModel
{
    internal class ProtectionViewModel : ObservableObject
    {
        private string _connectionStatus;

        public ObservableCollection<ServerModel> Servers { get; set; }

        public GlobalViewModel Global { get;} = GlobalViewModel.Instance;

        public string ConnectionStatus
        {
            get => _connectionStatus;
            set
            {
                _connectionStatus = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand ConnectCommand { get; set; }
        public ProtectionViewModel()
        {
            Servers = new ObservableCollection<ServerModel>();
            for (int i = 0; i < 5; i++)
            {
                Servers.Add(new ServerModel
                {
                    Country = "USA"
                });
                Servers.Add(new ServerModel
                {
                    Country = "Russia"
                });
            }

            ConnectCommand = new RelayCommand(o =>
            {
                Task.Run(() =>
                {
                    ServerBuilder();
                    ConnectionStatus = "Connecting...";
                    var process = new Process();
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
                    // got from https://www.vpnbook.com/freevpn - blocked
                    // https://freevpn4you.net/locations/russia.php 
                    process.StartInfo.ArgumentList.Add(@"/c rasdial MyServer freevpn4you 2628155 /phonebook:./VPN/VPN.pbk");

                    process.Start();
                    process.WaitForExit();

                    switch (process.ExitCode)
                    {
                        case 0:
                            Debug.WriteLine("Success!");
                            ConnectionStatus = "Connected!";
                            break;
                        case 691:
                            Debug.WriteLine("Wrong credentials!");
                            ConnectionStatus = "Wrong credentials!";
                            break;
                        default:
                            Debug.WriteLine($"Error: {process.ExitCode}");
                            ConnectionStatus = $"Error: {process.ExitCode}";
                            break;
                    }
                });

            });
        }

        private void ServerBuilder()
        {
            var address = "rus1.freevpn4you.net";
            var FolderPath = $"{Directory.GetCurrentDirectory()}/VPN";
            var PbkPath = $"{FolderPath}/VPN.pbk";

            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }

            if (File.Exists(PbkPath))
            {
                MessageBox.Show("Connection already exists!");
                File.Delete(PbkPath);
            }

            var sb = new StringBuilder();
            sb.AppendLine("[MyServer]");
            sb.AppendLine("MEDIA=rastapi");
            sb.AppendLine("Port=VPN2-0");
            sb.AppendLine("Device=WAN Miniport (IKEv2)");
            sb.AppendLine("DEVICE=vpn");
            sb.AppendLine($"PhoneNumber={address}");
            File.WriteAllText(PbkPath, sb.ToString());
        }
    }
}
