using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.Management;
using System.Collections.ObjectModel;
using System.Configuration;
using System.ComponentModel;
using System.Threading;
using System.Text.RegularExpressions;

namespace RelayControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<ItemDevice> Devices { get; }

        private List<RelayButton> RelayButtons;

        public RelayController arduino;

        int SelectedBtn = 0;

        public MainWindow()
        {
            Devices = new ObservableCollection<ItemDevice>();
            RelayButtons = new List<RelayButton>(8);


            InitializeComponent();
            UpdateDevices();

        }

        private void UpdateDevices()
        {
            ItemDevice selected = ItemDevice.NullDevice;
            var aux = cmbPorts.SelectedItem;
            if (aux != null && aux != ItemDevice.NullDevice && aux.GetType() == typeof(ItemDevice))
                selected = (ItemDevice)aux;

            Devices.Clear();
            Devices.Add(ItemDevice.NullDevice);

            using (ManagementClass entities = new ManagementClass("WIN32_PnPEntity"))
            {
                foreach (ManagementObject device in entities.GetInstances())
                {
                    Object classGuid = device.GetPropertyValue("ClassGuid");
                    if (classGuid == null || classGuid.ToString().ToUpper() != "{4D36E978-E325-11CE-BFC1-08002BE10318}")
                        continue; // Skip all devices except device class "PORTS"

                    string caption = device.GetPropertyValue("Caption").ToString();
                    string name = caption.Substring(0, caption.LastIndexOf(" (COM")).Trim();
                    int start = caption.LastIndexOf('(') + 1;
                    int end = caption.LastIndexOf(')');
                    string port = caption.Substring(start, end - start);

                    Devices.Add(new ItemDevice(name, port));
                }
            }


            if (selected != null && selected != ItemDevice.NullDevice && Devices.Contains(selected))
            {  // If there was a selected device
                cmbPorts.SelectedItem = selected;
            }
            else
            {
                cmbPorts.SelectedItem = ItemDevice.NullDevice;
            }
        }

        private void DeployButtons()
        {
            this.GrdRelays.Children.Clear();
            this.GrdRelays.RowDefinitions.Clear();
            this.RelayButtons.Clear();

            int nRelays = arduino.relays.Count;

            int rows = (int)Math.Ceiling((double)nRelays / 2.0);
            for (int i = 0; i < rows; i++)
            {
                GrdRelays.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star)});
            }

            for (int i = 0; i < nRelays; i++)
            {
                var newBtn = new RelayButton(i+1);
                newBtn.btn.Click += RelayButton_Click;
                RelayButtons.Add(newBtn);
                Grid.SetColumn(newBtn, i % 2);
                Grid.SetRow(newBtn, i / 2);
                GrdRelays.Children.Add(newBtn);
            }
            btnSelectAll.IsEnabled = true;
            btnSend.IsEnabled = true;
            UpdateButtons();
        }

        private void DestroyButtons()
        {
            btnSelectAll.IsEnabled = false;
            btnSend.IsEnabled = false;
            this.GrdRelays.Children.Clear();
            this.GrdRelays.RowDefinitions.Clear();
            this.RelayButtons.Clear();
            this.btnSend.IsEnabled= false;
            if (btnSelectAllSelected.Visibility == Visibility.Visible) { this.btnSelectAllSelected.Visibility= Visibility.Collapsed; }
            SelectedBtn = 0;
        }

        private void CheckUpdates(object sender)
        {
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            arduino.Poll();
            var relays = arduino.relays;
            for (int i = relays.Count - 1; i >= 0; i--)
            {
                RelayButtons[i].RelayState = relays[i].state;
            }
        }

        

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            UpdateDevices();
        }

        private void RelayButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedBtn == -1)
            {
                btnSelectAllSelected.Visibility = Visibility.Collapsed;
            } else if (SelectedBtn > 0)
            {
                RelayButtons[SelectedBtn - 1].RelaySelected = false;
            }

            if (sender == btnSelectAll)
            {
                btnSelectAllSelected.Visibility= Visibility.Visible;
                SelectedBtn = -1;
                return;
            }

            RelayButton btn = (sender as Button).Parent as RelayButton;
            SelectedBtn = btn.RelayNum;
            btn.RelaySelected = true;

        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedBtn != 0)
                {
                    if (RadON.IsChecked == true)
                    {
                        arduino.Turn(SelectedBtn, true);
                        UpdateButtons();
                        return;
                    }
                    if (RadOFF.IsChecked == true)
                    {
                        arduino.Turn(SelectedBtn, false);
                        UpdateButtons();
                        return;
                    }
                    if (RadToggle.IsChecked == true)
                    {
                        arduino.Toggle(SelectedBtn);
                        UpdateButtons();
                        return;
                    }
                    if (RadPulse.IsChecked == true)
                    {
                        uint duration = UInt32.Parse(TxtPulseDuration.Text);

                        if (ChkPulseCounting.IsChecked == true)
                        {
                            int count = Int32.Parse(TxtPulseCount.Text);

                            arduino.Pulse(SelectedBtn, duration, count);
                        }
                        else
                        {
                            arduino.Pulse(SelectedBtn, duration);
                        }
                        UpdateButtons();
                        return;
                    }

                }
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void CmbPorts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemDevice selected = ItemDevice.NullDevice;
            var aux = cmbPorts.SelectedItem;
            if (aux != null && aux != ItemDevice.NullDevice && aux.GetType() == typeof(ItemDevice))
            {
                selected = (ItemDevice)aux;

                try
                {
                    arduino = new RelayController(selected.Port);

                    DeployButtons();
                }
                catch (FormatException)
                {
                    MessageBox.Show(this, "The selected device is not compatible.", "Connection error", MessageBoxButton.OK, MessageBoxImage.Error);
                    cmbPorts.SelectedItem = ItemDevice.NullDevice;
                    if (arduino != null)
                    {
                        try { arduino.Disconnect(); }
                        catch (Exception) { }
                        finally { arduino = null; DestroyButtons(); };
                    }
                    return;
                }
                catch (CommandNotAcknowledgedException)
                {
                    MessageBox.Show(this, "The selected device is not compatible.", "Connection error", MessageBoxButton.OK, MessageBoxImage.Error);
                    cmbPorts.SelectedItem = ItemDevice.NullDevice;
                    if (arduino != null)
                    {
                        try { arduino.Disconnect(); }
                        catch (Exception) { }
                        finally { arduino = null; DestroyButtons(); };
                    }
                    return;
                }
                catch (TimeoutException)
                {
                    MessageBox.Show(this, "The selected device is not responding or the Timeout setting is too low.", "Connection error", MessageBoxButton.OK, MessageBoxImage.Error);
                    cmbPorts.SelectedItem = ItemDevice.NullDevice;
                    if (arduino != null)
                    {
                        try { arduino.Disconnect(); }
                        catch (Exception) { }
                        finally { arduino = null; DestroyButtons(); };
                    }
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "There was an unknown error: " + ex.ToString(), "Connection error", MessageBoxButton.OK, MessageBoxImage.Error);
                    cmbPorts.SelectedItem = ItemDevice.NullDevice;
                    if (arduino != null)
                    {
                        try { arduino.Disconnect(); }
                        catch (Exception) { }
                        finally { arduino = null; DestroyButtons(); };
                    }
                    return;
                }
            }
            else if (arduino != null)
            {
                arduino.Disconnect();
                arduino = null;
                DestroyButtons();
            }
        }

        // Validator for Textboxes to only accept numeric input
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        




        private class RelayButton : DockPanel
        {
            public int RelayNum { get; }
            private bool _relaySelected;
            public bool RelaySelected
            {
                get { return _relaySelected; }

                set
                {
                    _relaySelected = value;
                    selectedTxt.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            private bool _relayState;
            public bool RelayState
            {
                get { return _relayState; }

                set
                {
                    _relayState = value;
                    state.Fill = value ? Brushes.Green : Brushes.Red;
                    //state.UpdateLayout();
                }
            }

            private Rectangle state;
            public Button btn;
            private StackPanel btnTextPanel;
            private TextBlock selectedTxt;


            public RelayButton(int relayNum)
            {
                this.RelayNum = relayNum;
                this.MaxHeight = 120;
                this.Margin = new Thickness(1, 0, 0, 1);
                this.LastChildFill = true;

                // State rectangle
                state = new Rectangle()
                {
                    Width = 12,
                    Margin = new Thickness(0, 0, 4, 0)
                };
                this.Children.Add(state);
                SetDock(state, Dock.Left);

                // Button
                TextBlock relayNameTxt = new TextBlock() { Text = $"RLA{relayNum}" };
                selectedTxt = new TextBlock()
                {
                    Text = "Selected",
                    Foreground = Brushes.Gray,
                    FontStyle = FontStyles.Italic,
                    Margin = new Thickness(10, 0, 0, 0)
                };

                btnTextPanel = new StackPanel() { Orientation = Orientation.Horizontal };
                btnTextPanel.Children.Add(relayNameTxt);
                btnTextPanel.Children.Add(selectedTxt);
                this.RelayState = false;
                this.RelaySelected = false;

                btn = new Button() { FontSize = 14, Content = btnTextPanel };
                this.Children.Add(btn);
            }
        }      
    }

    public class ItemDevice
    {
        public String Name { get; }
        public String Port { get; }

        public static readonly ItemDevice NullDevice = new ItemDevice(null, null);

        public ItemDevice(string name, string port)
        {
            Name = name;
            Port = port;
        }

        public ItemDevice(ItemDevice it)
        {
            this.Name = it.Name;
            this.Port = it.Port;
        }

        public override string ToString()
        {
            if (Port == null)
                return "--- Select a device ---";

            return $"{Name} - ({Port})";
        }

        public override bool Equals(object obj)
        {
            return obj is ItemDevice device &&
                   Name == device.Name &&
                   Port == device.Port;
        }

        public override int GetHashCode()
        {
            int hashCode = 48670396;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Port);
            return hashCode;
        }
    }


}
